import type { ApiFieldErrors } from "@/core/api/api-error";

export type ValidationRule<TModel, TValue = unknown> = (
  value: TValue,
  model: TModel
) => string | null | undefined | Promise<string | null | undefined>;

export type ValidationSchema<TModel extends object> = Partial<
  Record<keyof TModel, Array<ValidationRule<TModel>>>
>;

function isEmpty(value: unknown): boolean {
  return value === null ||
    value === undefined ||
    (typeof value === "string" && value.trim() === "") ||
    (Array.isArray(value) && value.length === 0);
}

export const rules = {
  required: <TModel>(message = "此欄位為必填。"):
    ValidationRule<TModel> => value => isEmpty(value) ? message : null,

  email: <TModel>(message = "Email 格式不正確。"):
    ValidationRule<TModel> => value => {
      if (isEmpty(value)) return null;
      return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(value)) ? null : message;
    },

  minLength: <TModel>(length: number, message = `至少需要 ${length} 個字元。`):
    ValidationRule<TModel> => value => {
      if (isEmpty(value)) return null;
      return String(value).length >= length ? null : message;
    },

  maxLength: <TModel>(length: number, message = `不可超過 ${length} 個字元。`):
    ValidationRule<TModel> => value => {
      if (isEmpty(value)) return null;
      return String(value).length <= length ? null : message;
    },

  pattern: <TModel>(pattern: RegExp, message = "欄位格式不正確。"):
    ValidationRule<TModel> => value => {
      if (isEmpty(value)) return null;
      pattern.lastIndex = 0;
      return pattern.test(String(value)) ? null : message;
    },

  custom: <TModel, TValue = unknown>(rule: ValidationRule<TModel, TValue>) => rule
};

export async function validateSchema<TModel extends object>(
  model: TModel,
  schema: ValidationSchema<TModel>
): Promise<ApiFieldErrors> {
  const errors: ApiFieldErrors = {};

  for (const field of Object.keys(schema) as Array<keyof TModel>) {
    for (const rule of schema[field] ?? []) {
      const message = await rule(model[field], model);
      if (message) (errors[String(field)] ??= []).push(message);
    }
  }

  return errors;
}
