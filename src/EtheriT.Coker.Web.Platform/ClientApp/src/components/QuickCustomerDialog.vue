<script setup lang="ts">
    import { computed, ref, watch } from "vue";
    import {
        ApiError,
        FormFieldErrors,
        rules,
        validateSchema,
        type ApiFieldErrors,
        type ValidationSchema
    } from "@/core/coker";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import { createCustomer } from "@/services/customer-api";
    import {
        CustomerType,
        customerTypeOptions,
        type CustomerForm,
        type CustomerLookup
    } from "@/types/customer";

    const props = defineProps<{
        open: boolean;
        initialTaxId: string;
        initialName: string;
    }>();

    const emit = defineEmits<{
        created: [customer: CustomerLookup];
        cancel: [];
    }>();

    function emptyForm(taxId: string, name: string): CustomerForm {
        return {
            Name: name,
            TaxId: taxId,
            Phone: "",
            Email: "",
            Address: "",
            InvoiceInfo: "",
            SalesOwner: "",
            CustomerType: CustomerType.一般客戶,
            CustomerTypeOther: "",
            PrimaryContactName: "",
            PrimaryContactJobTitle: "",
            PrimaryContactPhone: "",
            PrimaryContactEmail: "",
            SubContacts: []
        };
    }

    // 與 CompanyEditView 相同的規則子集；後端 CustomerSaveRequest 仍會再驗一次
    const validation: ValidationSchema<CustomerForm> = {
        Name: [rules.required("請輸入公司名稱。"), rules.maxLength(200)],
        TaxId: [rules.pattern(/^\d{8,10}$/, "統一編號需為 8～10 碼數字。")],
        PrimaryContactName: [rules.required("請輸入主要聯絡人姓名。"), rules.maxLength(100)],
        Email: [rules.email("公司 Email 格式不正確。")],
        CustomerTypeOther: [
            rules.custom<CustomerForm>((value, model) =>
                model.CustomerType === CustomerType.其他 && !String(value ?? "").trim()
                    ? "選擇「其他」時請輸入客戶屬性內容。"
                    : null)
        ]
    };

    const model = ref<CustomerForm>(emptyForm(props.initialTaxId, props.initialName));
    const errors = ref<ApiFieldErrors>({});
    const busy = ref(false);
    const submitError = ref("");

    const isOtherType = computed(() => model.value.CustomerType === CustomerType.其他);

    // 每次打開都重置，帶入最新的統編
    watch(() => props.open, isOpen => {
        if (!isOpen) return;
        model.value = emptyForm(props.initialTaxId, props.initialName);
        errors.value = {};
        submitError.value = "";
    });

    function fieldErrors(field: string): string[] {
        const wanted = field.toLowerCase();
        return Object.entries(errors.value)
            .find(([name]) => name.toLowerCase() === wanted)?.[1] ?? [];
    }

    function toNullable(value: string): string | null {
        const trimmed = value.trim();
        return trimmed === "" ? null : trimmed;
    }

    async function submit(): Promise<void> {
        if (busy.value) return;

        errors.value = await validateSchema(model.value, validation);
        if (Object.keys(errors.value).length > 0) return;

        busy.value = true;
        submitError.value = "";
        try {
            const { Id } = await createCustomer(model.value);
            emit("created", {
                Id,
                Name: model.value.Name.trim(),
                TaxId: model.value.TaxId.trim(),
                Phone: toNullable(model.value.Phone),
                Email: toNullable(model.value.Email),
                PrimaryContactName: toNullable(model.value.PrimaryContactName)
            });
        }
        catch (error) {
            console.error(error);
            if (error instanceof ApiError && Object.keys(error.fieldErrors).length > 0) {
                errors.value = error.fieldErrors;
                return;
            }
            submitError.value = "客戶建立失敗，請稍後再試。";
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
                   title="建立客戶資料"
                   message="只需填寫必要欄位，其餘資料之後可到「客戶資料」補齊。"
                   cancel-text="取消"
                   confirm-text="建立並帶入"
                   :busy="busy"
                   @cancel="emit('cancel')"
                   @confirm="submit">
        <div v-if="submitError" class="alert alert-error dialog-alert" role="alert">{{ submitError }}</div>

        <form novalidate @submit.prevent="submit">
            <fieldset class="dialog-fieldset" :disabled="busy">
                <div class="form-grid">
                    <div class="form-field">
                        <label>
                            <span>公司名稱 <i class="form-required">*</i></span>
                            <input v-model="model.Name" type="text" maxlength="200" />
                        </label>
                        <FormFieldErrors :errors="fieldErrors('Name')" />
                    </div>

                    <div class="form-field">
                        <label>
                            <span>統一編號</span>
                            <input v-model="model.TaxId" type="text" inputmode="numeric" maxlength="10" />
                        </label>
                        <FormFieldErrors :errors="fieldErrors('TaxId')" />
                    </div>

                    <div class="form-field">
                        <label>
                            <span>客戶屬性 <i class="form-required">*</i></span>
                            <select v-model.number="model.CustomerType">
                                <option v-for="option in customerTypeOptions" :key="option.Value" :value="option.Value">
                                    {{ option.Text }}
                                </option>
                            </select>
                        </label>
                        <FormFieldErrors :errors="fieldErrors('CustomerType')" />
                    </div>

                    <div v-if="isOtherType" class="form-field">
                        <label>
                            <span>其他屬性內容 <i class="form-required">*</i></span>
                            <input v-model="model.CustomerTypeOther" type="text" maxlength="50" />
                        </label>
                        <FormFieldErrors :errors="fieldErrors('CustomerTypeOther')" />
                    </div>

                    <div class="form-field">
                        <label>
                            <span>主要聯絡人姓名 <i class="form-required">*</i></span>
                            <input v-model="model.PrimaryContactName" type="text" maxlength="100" />
                        </label>
                        <FormFieldErrors :errors="fieldErrors('PrimaryContactName')" />
                    </div>

                    <div class="form-field">
                        <label>
                            <span>公司電話</span>
                            <input v-model="model.Phone" type="tel" maxlength="50" />
                        </label>
                        <FormFieldErrors :errors="fieldErrors('Phone')" />
                    </div>

                    <div class="form-field form-field-wide">
                        <label>
                            <span>公司 Email</span>
                            <input v-model="model.Email" type="email" maxlength="150" />
                        </label>
                        <FormFieldErrors :errors="fieldErrors('Email')" />
                    </div>
                </div>
            </fieldset>

            <!-- 讓欄位內按 Enter 也能送出；看得到的按鈕在 ConfirmDialog 裡 -->
            <button type="submit" class="visually-hidden" tabindex="-1">建立並帶入</button>
        </form>
    </ConfirmDialog>
</template>