<script setup lang="ts">
import { ref, watch } from "vue";
import ConfirmDialog from "@/components/ConfirmDialog.vue";
import {
  ApiError,
  FormFieldErrors,
  requestAlert,
  rules,
  validateSchema,
  type ApiFieldErrors,
  type ValidationSchema
} from "@/core/coker";
import { createSystemAdministrator } from "@/services/system-administrator-api";
import type {
  CreateSystemAdministratorForm,
  CreateSystemAdministratorInvitationResponse,
  SystemAdministratorMvcRoleOption,
  SystemAdministratorRoleOption
} from "@/types/system-administrator";

const props = defineProps<{
  open: boolean;
  mvcRoles: SystemAdministratorMvcRoleOption[];
  platformRoles: SystemAdministratorRoleOption[];
}>();

const emit = defineEmits<{
  created: [response: CreateSystemAdministratorInvitationResponse];
  cancel: [];
}>();

function emptyForm(): CreateSystemAdministratorForm {
  return {
    Name: "",
    Email: "",
    MvcRoleId: props.mvcRoles[0]?.Id ?? null,
    PlatformRoleId: props.platformRoles[0]?.Id ?? null
  };
}

const model = ref<CreateSystemAdministratorForm>(emptyForm());
const errors = ref<ApiFieldErrors>({});
const busy = ref(false);

const validation: ValidationSchema<CreateSystemAdministratorForm> = {
  Name: [rules.required("請輸入姓名。"), rules.maxLength(150)],
  Email: [rules.required("請輸入 Email。"), rules.email(), rules.maxLength(150)],
  MvcRoleId: [rules.required("請選擇 MVC 系統角色。")],
  PlatformRoleId: [rules.required("請選擇 Platform 角色。")]
};

watch(() => props.open, open => {
  if (!open) return;
  model.value = emptyForm();
  errors.value = {};
});

function fieldErrors(field: string): string[] {
  const wanted = field.toLowerCase();
  return Object.entries(errors.value)
    .find(([name]) => name.toLowerCase() === wanted)?.[1] ?? [];
}

async function submit(): Promise<void> {
  if (busy.value) return;

  errors.value = await validateSchema(model.value, validation);
  if (Object.keys(errors.value).length > 0) {
    await requestAlert({
      title: "表單尚未填寫完整",
      message: "請修正以下項目後再建立：",
      details: [...new Set(Object.values(errors.value).flat())]
    });
    return;
  }

  busy.value = true;
  try {
    const response = await createSystemAdministrator({
      ...model.value,
      Name: model.value.Name.trim(),
      Email: model.value.Email.trim()
    });
    emit("created", response);
  }
  catch (error) {
    console.error(error);
    if (error instanceof ApiError && Object.keys(error.fieldErrors).length > 0) {
      errors.value = error.fieldErrors;
      await requestAlert({
        title: "部分欄位有誤",
        message: "請修正以下項目後再建立：",
        details: [...new Set(Object.values(error.fieldErrors).flat())]
      });
      return;
    }
    await requestAlert({
      title: "新增使用者失敗",
      message: error instanceof ApiError ? error.message : "新增使用者失敗，請稍後再試。"
    });
  }
  finally {
    busy.value = false;
  }
}
</script>

<template>
  <ConfirmDialog class="app-dialog-form"
                 :open="open"
                 icon="person_add"
                 title="邀請系統管理者"
                 message="系統會寄出 Email 驗證信；受邀者設定帳號與密碼後，仍需由 Platform 總管理者核准才會取得權限。"
                 confirm-text="寄出邀請"
                 :busy="busy"
                 @cancel="emit('cancel')"
                 @confirm="submit">
    <form novalidate @submit.prevent="submit">
      <fieldset class="dialog-fieldset" :disabled="busy">
        <div class="form-grid">
          <div class="form-field">
            <label><span>姓名 <i class="form-required">*</i></span><input v-model="model.Name" type="text" maxlength="150" autocomplete="name" /></label>
            <FormFieldErrors :errors="fieldErrors('Name')" />
          </div>
          <div class="form-field">
            <label><span>Email <i class="form-required">*</i></span><input v-model="model.Email" type="email" maxlength="150" autocomplete="email" /></label>
            <FormFieldErrors :errors="fieldErrors('Email')" />
          </div>
          <div class="form-field">
            <label>
              <span>MVC 系統角色 <i class="form-required">*</i></span>
              <select v-model.number="model.MvcRoleId">
                <option v-for="role in mvcRoles" :key="role.Id" :value="role.Id">{{ role.Name }}</option>
              </select>
            </label>
            <FormFieldErrors :errors="fieldErrors('MvcRoleId')" />
          </div>
          <div class="form-field">
            <label>
              <span>Platform 角色 <i class="form-required">*</i></span>
              <select v-model.number="model.PlatformRoleId">
                <option v-for="role in platformRoles" :key="role.Id" :value="role.Id">{{ role.Name }}</option>
              </select>
            </label>
            <FormFieldErrors :errors="fieldErrors('PlatformRoleId')" />
          </div>
        </div>
        <p class="invitation-note">角色只會在 Email 驗證完成且管理員核准後生效。</p>
      </fieldset>
    </form>
  </ConfirmDialog>
</template>

<style scoped>
.invitation-note { margin: .75rem 0 0; color: #687386; font-size: .82rem; line-height: 1.5; }
</style>
