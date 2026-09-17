<script setup lang="ts">
    import { ref, watch } from "vue";
    import {
        ApiError,
        FormFieldErrors,
        validateSchema,
        type ApiFieldErrors
    } from "@/core/coker";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import { createDomain, domainValidation, emptyDomainForm } from "@/services/domain-api";
    import type { DomainDetail, DomainForm } from "@/types/domain";

    const props = defineProps<{
        open: boolean;
        initialDomainName: string;
    }>();

    const emit = defineEmits<{
        created: [domain: DomainDetail];
        cancel: [];
    }>();

    const model = ref<DomainForm>(emptyDomainForm(props.initialDomainName));
    const errors = ref<ApiFieldErrors>({});
    const busy = ref(false);
    const submitError = ref("");

    // 每次打開都重置，帶入最新的建議網域
    watch(() => props.open, isOpen => {
        if (!isOpen) return;
        model.value = emptyDomainForm(props.initialDomainName);
        errors.value = {};
        submitError.value = "";
    });

    function fieldErrors(field: string): string[] {
        const wanted = field.toLowerCase();
        return Object.entries(errors.value)
            .find(([name]) => name.toLowerCase() === wanted)?.[1] ?? [];
    }

    async function submit(): Promise<void> {
        if (busy.value) return;

        errors.value = await validateSchema(model.value, domainValidation);
        if (Object.keys(errors.value).length > 0) return;

        busy.value = true;
        submitError.value = "";
        try {
            emit("created", await createDomain(model.value));
        }
        catch (error) {
            console.error(error);
            if (error instanceof ApiError && Object.keys(error.fieldErrors).length > 0) {
                errors.value = error.fieldErrors;
                return;
            }
            submitError.value = "網域建立失敗，請稍後再試。";
        }
        finally {
            busy.value = false;
        }
    }
</script>

<template>
    <ConfirmDialog class="app-dialog-form"
                   :open="open"
                   icon="dns"
                   title="建立網域資料"
                   message="請確認網域是否正確（例：shop.example.com.tw 的網域通常是 example.com.tw），其餘資料之後可到「網域管理」補齊。"
                   cancel-text="取消"
                   confirm-text="建立並帶入"
                   :busy="busy"
                   @cancel="emit('cancel')"
                   @confirm="submit">
        <div v-if="submitError" class="alert alert-error dialog-alert" role="alert">{{ submitError }}</div>

        <form novalidate @submit.prevent="submit">
            <fieldset class="dialog-fieldset" :disabled="busy">
                <div class="form-grid">
                    <div class="form-field form-field-wide">
                        <label>
                            <span>網域 <i class="form-required">*</i></span>
                            <input v-model="model.DomainName" type="text" inputmode="url" maxlength="255" />
                        </label>
                        <FormFieldErrors :errors="fieldErrors('DomainName')" />
                    </div>

                    <div class="form-field">
                        <label>
                            <span>網域公司</span>
                            <input v-model="model.Registrar" type="text" maxlength="200" />
                        </label>
                    </div>

                    <div class="form-field">
                        <label>
                            <span>網域密碼</span>
                            <input v-model="model.Password" type="text" maxlength="200" autocomplete="off" />
                        </label>
                        <FormFieldErrors :errors="fieldErrors('Password')" />
                    </div>

                    <div class="form-field">
                        <label>
                            <span>網域起始日期</span>
                            <input v-model="model.StartDate" type="date" />
                        </label>
                    </div>

                    <div class="form-field">
                        <label>
                            <span>網域到期日期</span>
                            <input v-model="model.EndDate" type="date" />
                        </label>
                        <FormFieldErrors :errors="fieldErrors('EndDate')" />
                    </div>
                </div>
            </fieldset>

            <!-- 讓欄位內按 Enter 也能送出；看得到的按鈕在 ConfirmDialog 裡 -->
            <button type="submit" class="visually-hidden" tabindex="-1">建立並帶入</button>
        </form>
    </ConfirmDialog>
</template>