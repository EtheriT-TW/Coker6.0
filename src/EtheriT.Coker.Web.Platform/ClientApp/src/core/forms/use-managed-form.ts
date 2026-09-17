import { computed, onBeforeUnmount, onMounted, ref, toRaw, watch, type Ref } from "vue";
import { onBeforeRouteLeave } from "vue-router";
import { ApiError, type ApiFieldErrors } from "@/core/api/api-error";
import { ensureAuthenticatedForSave } from "@/core/auth/reauthentication";
import {
  runAfterSaveHooks,
  runBeforeSaveHooks,
  runSaveErrorHooks,
  type SavePipelineContext,
  type SaveTrigger
} from "@/core/forms/save-pipeline";
import { registerSaveShortcut } from "@/core/forms/save-shortcut";
import { validateSchema, type ValidationSchema } from "@/core/forms/validation";
import { requestConfirm } from "@/core/dialogs/confirm-request";

export type SaveStatus = "saved" | "invalid" | "cancelled" | "unauthorized" | "failed";

export interface ManagedFormOptions<TModel extends object, TResult> {
  id: string;
  initialValue: TModel | (() => TModel);
  validation?: ValidationSchema<TModel>;
  validate?: (values: TModel) => ApiFieldErrors | Promise<ApiFieldErrors>;
  beforeSave?: (values: TModel) => boolean | void | Promise<boolean | void>;
  save: (values: TModel) => Promise<TResult>;
  afterSave?: (result: TResult, values: TModel) => void | Promise<void>;
  onError?: (error: unknown) => void | Promise<void>;
  enableSaveShortcut?: boolean;
  protectUnsavedChanges?: boolean;
  unsavedChangesMessage?: string;
}

function clone<T>(value: T): T {
    return structuredClone(toRaw(value));
}

function createInitialValue<TModel extends object>(
  source: TModel | (() => TModel)
): TModel {
  return clone(typeof source === "function" ? (source as () => TModel)() : source);
}

export function useManagedForm<TModel extends object, TResult = unknown>(
  options: ManagedFormOptions<TModel, TResult>
) {
  const model = ref<TModel>(createInitialValue(options.initialValue)) as Ref<TModel>;
  const errors = ref<ApiFieldErrors>({});
  const isDirty = ref(false);
  const isSaving = ref(false);
  const lastSavedAt = ref<Date | null>(null);
  let trackingChanges = true;
  let unregisterShortcut: (() => void) | undefined;

  watch(model, () => {
    if (trackingChanges) isDirty.value = true;
  }, { deep: true });

  const isValid = computed(() => Object.keys(errors.value).length === 0);

  function setErrors(nextErrors: ApiFieldErrors): void {
    errors.value = nextErrors;
  }

  function clearErrors(field?: keyof TModel | string): void {
    if (field === undefined) {
      errors.value = {};
      return;
    }

    const nextErrors = { ...errors.value };
    delete nextErrors[String(field)];
    errors.value = nextErrors;
  }

  function getErrors(field: keyof TModel | string): string[] {
    const requestedField = String(field).toLowerCase();
    const match = Object.entries(errors.value).find(
      ([name]) => name.toLowerCase() === requestedField
    );
    return match?.[1] ?? [];
  }

  async function validate(): Promise<boolean> {
    const schemaErrors = options.validation
      ? await validateSchema(model.value, options.validation)
      : {};
    const customErrors = options.validate
      ? await options.validate(model.value)
      : {};

    errors.value = { ...schemaErrors, ...customErrors };
    return Object.keys(errors.value).length === 0;
  }

  async function save(trigger: SaveTrigger = "programmatic"): Promise<SaveStatus> {
    if (isSaving.value) return "cancelled";
    if (!await validate()) return "invalid";

    const values = clone(model.value);
    const context: SavePipelineContext<TModel, TResult> = {
      formId: options.id,
      trigger,
      values
    };

    if (await options.beforeSave?.(values) === false ||
        !await runBeforeSaveHooks(context)) {
      return "cancelled";
    }

    isSaving.value = true;
    try {
      if (!await ensureAuthenticatedForSave()) return "unauthorized";

      let result: TResult;
      try {
        result = await options.save(values);
      }
      catch (error) {
        // Handles the narrow race where the session expires after the pre-save
        // check but before the actual write request reaches the server.
        if (!(error instanceof ApiError) ||
            error.status !== 401 ||
            !await ensureAuthenticatedForSave()) {
          throw error;
        }
        result = await options.save(values);
      }

      context.result = result;
      await options.afterSave?.(result, values);
      await runAfterSaveHooks(context);
      isDirty.value = false;
      lastSavedAt.value = new Date();
      return "saved";
    }
    catch (error) {
      context.error = error;
      if (error instanceof ApiError && Object.keys(error.fieldErrors).length > 0) {
        errors.value = error.fieldErrors;
      }
      await options.onError?.(error);
      await runSaveErrorHooks(context);
      return "failed";
    }
    finally {
      isSaving.value = false;
    }
  }

  function reset(value: TModel = createInitialValue(options.initialValue)): void {
    trackingChanges = false;
    model.value = clone(value);
    errors.value = {};
    isDirty.value = false;
    queueMicrotask(() => { trackingChanges = true; });
  }

  function markClean(): void {
    isDirty.value = false;
  }

  onMounted(() => {
    if (options.enableSaveShortcut !== false) {
        unregisterShortcut = registerSaveShortcut(() => { void save("shortcut"); });
    }
    window.addEventListener("beforeunload", handleBeforeUnload);
  });

  function handleBeforeUnload(event: BeforeUnloadEvent): void {
    if (!isDirty.value || isSaving.value || options.protectUnsavedChanges === false) return;
    event.preventDefault();
  }

  onBeforeRouteLeave(() => {
    if (!isDirty.value || isSaving.value || options.protectUnsavedChanges === false) return true;
    return requestConfirm({
      icon: "warning",
      tone: "danger",
      title: "尚有未儲存的變更",
      message: options.unsavedChangesMessage ?? "確定要離開此頁面嗎？未儲存的內容會遺失。",
      confirmText: "離開",
      cancelText: "留在此頁"
    });
  });

  onBeforeUnmount(() => {
    unregisterShortcut?.();
    window.removeEventListener("beforeunload", handleBeforeUnload);
  });

  return {
    model,
    errors,
    isDirty,
    isSaving,
    isValid,
    lastSavedAt,
    validate,
    save,
    reset,
    markClean,
    setErrors,
    clearErrors,
    getErrors
  };
}
