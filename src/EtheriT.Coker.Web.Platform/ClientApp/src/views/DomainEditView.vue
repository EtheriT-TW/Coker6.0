<script setup lang="ts">
    import { computed, onMounted, ref, watch } from "vue";
    import { useRoute, useRouter } from "vue-router";
    import { ApiError, FormFieldErrors, useManagedForm } from "@/core/coker";
    import {
        createDomain,
        domainValidation,
        emptyDomainForm,
        fetchDomain,
        fetchDomainPassword,
        toDomainForm,
        updateDomain
    } from "@/services/domain-api";
    import type { DomainDetail, DomainForm } from "@/types/domain";

    const route = useRoute();
    const router = useRouter();

    const domainId = computed(() => {
        const value = Number(route.params.id);
        return Number.isInteger(value) && value > 0 ? value : 0;
    });
    const isEdit = computed(() => domainId.value > 0);

    const loading = ref(false);
    const pageError = ref("");
    const websiteCount = ref(0);

    // ── 網域密碼 ──
    // 固定長度遮罩，不透露實際密碼長度
    const PASSWORD_MASK = "********";
    const hasStoredPassword = ref(false);
    const revealedPassword = ref<string | null>(null);   // 從後端讀回的原密碼，只用來顯示
    const isPasswordVisible = ref(true);
    const isPasswordEdited = ref(false);                 // 使用者動過輸入框才算要改密碼
    const passwordNotice = ref("");
    const revealing = ref(false);

    const form = useManagedForm<DomainForm, DomainDetail>({
        id: "platform-domain-editor",
        initialValue: () => emptyDomainForm(),
        validation: domainValidation,
        beforeSave: () => {
            pageError.value = "";
        },
        save: values => isEdit.value
            ? updateDomain(domainId.value, values)
            : createDomain(values),
        afterSave: () => {
            void router.push("/domains");
        },
        onError: error => {
            console.error(error);
            pageError.value = error instanceof ApiError && Object.keys(error.fieldErrors).length > 0
                ? "部分欄位有誤，請依紅字提示修正。"
                : "儲存失敗，請稍後再試。";
        }
    });

    const isFormLocked = computed(() => loading.value || form.isSaving.value);
    // 已有網站使用時，改名會讓網站網址對不上
    const isNameLocked = computed(() => websiteCount.value > 0);

    const isClearingPassword = computed(() => form.model.value.ClearPassword);
    const currentPassword = computed(() =>
        isPasswordEdited.value ? form.model.value.Password : (revealedPassword.value ?? ""));
    const hasPasswordValue = computed(() =>
        !isClearingPassword.value &&
        (isPasswordEdited.value ? form.model.value.Password !== "" : hasStoredPassword.value));
    // 隱藏時有值就顯示固定遮罩；顯示時才放實際內容
    const passwordDisplay = computed(() => {
        if (isClearingPassword.value) return "";
        if (isPasswordVisible.value) return currentPassword.value;
        return hasPasswordValue.value ? PASSWORD_MASK : "";
    });
    // 遮罩狀態不可編輯，否則會把 ******** 當成新密碼送出
    const isPasswordReadonly = computed(() => !isPasswordVisible.value && hasPasswordValue.value);

    // 勾選「清除密碼」時清空新密碼輸入（兩者互斥）
    watch(isClearingPassword, clear => {
        if (!clear) return;
        form.model.value.Password = "";
        isPasswordEdited.value = false;
        isPasswordVisible.value = false;
        form.clearErrors("Password");
    });

    function applyDetail(detail: DomainDetail): void {
        // 用 reset 載入，才不會一進頁面就被標成「尚未儲存」
        form.reset(toDomainForm(detail));
        websiteCount.value = detail.WebsiteCount;
        hasStoredPassword.value = detail.HasPassword;
        revealedPassword.value = null;
        isPasswordEdited.value = false;
        // 有存密碼 → 預設遮罩；沒有 → 直接可輸入
        isPasswordVisible.value = !detail.HasPassword;
        passwordNotice.value = "";
    }

    function onPasswordInput(event: Event): void {
        form.model.value.Password = (event.target as HTMLInputElement).value;
        isPasswordEdited.value = true;
        // 從空白開始打字時保持明文，打完再用眼睛遮起來
        isPasswordVisible.value = true;
    }

    async function togglePassword(): Promise<void> {
        if (isPasswordVisible.value) {
            isPasswordVisible.value = false;
            return;
        }

        // 自己輸入的新密碼，或已經讀過的原密碼，不必再打 API
        if (isPasswordEdited.value || revealedPassword.value !== null) {
            isPasswordVisible.value = true;
            return;
        }

        revealing.value = true;
        passwordNotice.value = "";
        try {
            const result = await fetchDomainPassword(domainId.value);
            // 讀不到時也切到明文狀態（空白），讓使用者可以直接重新輸入
            revealedPassword.value = result.State === "Ok" ? result.Password ?? "" : "";
            isPasswordVisible.value = true;
            if (result.State === "Empty") passwordNotice.value = "這筆尚未記錄網域密碼。";
            else if (result.State === "Unreadable") passwordNotice.value = "此註記已無法讀取，請重新輸入。";
        }
        catch (error) {
            console.error(error);
            passwordNotice.value = "讀取網域密碼失敗，請稍後再試。";
        }
        finally {
            revealing.value = false;
        }
    }

    function cancel(): void {
        void router.push("/domains");
    }

    onMounted(async () => {
        if (!isEdit.value) return;

        loading.value = true;
        try {
            applyDetail(await fetchDomain(domainId.value));
        }
        catch (error) {
            console.error(error);
            if (error instanceof ApiError && error.status === 404) {
                void router.replace("/domains");
                return;
            }
            pageError.value = "網域資料載入失敗，請重新整理再試。";
        }
        finally {
            loading.value = false;
        }
    });
</script>

<template>
    <section class="page-heading">
        <div>
            <h1>{{ isEdit ? "編輯網域" : "新增網域" }}</h1>
            <p>填寫網域、申請公司、期限與密碼。可直接貼上網址，系統會自動取出網域。</p>
        </div>
    </section>

    <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>

    <form class="form-stack" novalidate @submit.prevent="form.save('button')">
        <fieldset class="form-card form-section"
                  aria-labelledby="section-domain-info-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-domain-info-title">網域資料</span>
            </div>

            <div class="form-grid">
                <div class="form-field form-field-wide">
                    <label>
                        <span>網域 <i class="form-required">*</i></span>
                        <input v-model="form.model.value.DomainName"
                               type="text"
                               inputmode="url"
                               maxlength="255"
                               placeholder="例：example.com.tw"
                               :readonly="isNameLocked" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('DomainName')" />
                    <p v-if="isNameLocked" class="field-note">
                        已有 {{ websiteCount }} 個網站使用此網域，網域名稱不可修改。
                    </p>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域公司</span>
                        <input v-model="form.model.value.Registrar" type="text" maxlength="200" />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域起始日期</span>
                        <input v-model="form.model.value.StartDate" type="date" />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域到期日期</span>
                        <input v-model="form.model.value.EndDate" type="date" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('EndDate')" />
                </div>

                <div class="form-field form-field-wide">
                    <label for="domain-password" class="form-label">網域密碼</label>
                    <div class="input-with-action">
                        <input id="domain-password"
                               :value="passwordDisplay"
                               type="text"
                               maxlength="200"
                               autocomplete="off"
                               spellcheck="false"
                               :readonly="isPasswordReadonly"
                               :disabled="isClearingPassword"
                               :placeholder="isClearingPassword ? '' : '尚未記錄'"
                               @input="onPasswordInput" />
                        <button v-if="hasPasswordValue"
                                class="outline-icon-button"
                                type="button"
                                :disabled="revealing"
                                :title="isPasswordVisible ? '隱藏密碼' : '顯示密碼'"
                                :aria-label="isPasswordVisible ? '隱藏密碼' : '顯示密碼'"
                                :aria-pressed="isPasswordVisible"
                                @click="togglePassword">
                            <span class="material-symbols-outlined">
                                {{ isPasswordVisible ? "visibility_off" : "visibility" }}
                            </span>
                        </button>
                    </div>
                    <FormFieldErrors :errors="form.getErrors('Password')" />
                    <p v-if="isPasswordReadonly" class="field-note">點擊眼睛顯示密碼後即可修改。</p>
                    <p v-if="passwordNotice" class="field-note field-note-error">{{ passwordNotice }}</p>
                    <div v-if="hasStoredPassword" class="choice-group choice-group-compact">
                        <label class="choice">
                            <input type="checkbox" v-model="form.model.value.ClearPassword" />
                            <span class="choice-box" aria-hidden="true"></span>
                            <span>清除已儲存的網域密碼</span>
                        </label>
                    </div>
                </div>
            </div>
        </fieldset>

        <fieldset class="form-card form-section"
                  aria-labelledby="section-domain-remark-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-domain-remark-title">備註</span>
            </div>
            <div class="form-field">
                <label>
                    <span class="visually-hidden">備註</span>
                    <textarea v-model="form.model.value.Remark" rows="4" maxlength="2000"></textarea>
                </label>
            </div>
        </fieldset>

        <div class="form-actions">
            <button class="ui-button ui-button-secondary"
                    type="button"
                    :disabled="form.isSaving.value"
                    @click="cancel">
                取消
            </button>
            <button class="ui-button ui-button-primary"
                    type="submit"
                    :disabled="isFormLocked">
                {{ form.isSaving.value ? "儲存中…" : "儲存" }}
            </button>
        </div>
    </form>
</template>