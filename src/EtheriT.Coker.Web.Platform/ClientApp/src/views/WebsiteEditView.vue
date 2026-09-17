<script setup lang="ts">
    import { computed, onMounted, ref, watch } from "vue";
    import { useRoute, useRouter } from "vue-router";
    import { ApiError, FormFieldErrors, rules, useManagedForm } from "@/core/coker";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import QuickCustomerDialog from "@/components/QuickCustomerDialog.vue";
    import { lookupCustomer } from "@/services/customer-api";
    import {
        createWebsite,
        fetchDomainPassword,
        fetchWebsite,
        toWebsiteForm,
        updateWebsite
    } from "@/services/website-api";
    import type { CustomerLookup } from "@/types/customer";
    import {
        WebsiteStatus,
        websiteLevelOptions,
        websiteStatusOptions,
        type WebsiteDetail,
        type WebsiteForm
    } from "@/types/website";

    const TAX_ID_PATTERN = /^\d{8,10}$/;

    const route = useRoute();
    const router = useRouter();

    const websiteId = computed(() => {
        const value = Number(route.params.id);
        return Number.isInteger(value) && value > 0 ? value : 0;
    });
    const isEdit = computed(() => websiteId.value > 0);

    const loading = ref(false);
    const pageError = ref("");

    // ── 以統編或公司名稱帶出客戶（兩者都不可重複，最多一筆） ──
    const lookupKeyword = ref("");
    const customer = ref<CustomerLookup | null>(null);
    const isCustomerDeleted = ref(false);
    const lookingUp = ref(false);
    const lookupError = ref("");
    const isNotFoundDialogOpen = ref(false);
    const isQuickCustomerOpen = ref(false);

    // 查無資料時開快速建立：輸入的是 8～10 碼數字就帶進統編，否則當公司名稱
    const quickCustomerTaxId = computed(() => {
        const keyword = lookupKeyword.value.trim();
        return TAX_ID_PATTERN.test(keyword) ? keyword : "";
    });
    const quickCustomerName = computed(() =>
        quickCustomerTaxId.value ? "" : lookupKeyword.value.trim());
    // ── 網域密碼 ──
    const hasStoredPassword = ref(false);
    const revealedPassword = ref<string | null>(null);
    const passwordNotice = ref("");
    const revealing = ref(false);

    function emptyForm(): WebsiteForm {
        return {
            FK_CompanyId: 0,
            Name: "",
            Level: null,
            HostLocation: "",
            ServiceStartDate: "",
            ServiceEndDate: "",
            Status: WebsiteStatus.正常,
            TerminatedDate: "",
            IsDomainPending: false,
            DomainName: "",
            DomainRegistrar: "",
            DomainStartDate: "",
            DomainEndDate: "",
            DomainPassword: "",
            ClearDomainPassword: false,
            Remark: ""
        };
    }

    const form = useManagedForm<WebsiteForm, WebsiteDetail>({
        id: "platform-website-editor",
        initialValue: emptyForm,
        validation: {
            FK_CompanyId: [
                rules.custom<WebsiteForm>(value =>
                    Number(value) > 0 ? null : "請先輸入統一編號或公司名稱並帶出客戶。")
            ],
            Name: [rules.required("請輸入網站名稱。"), rules.maxLength(250)],
            TerminatedDate: [
                rules.custom<WebsiteForm>((value, model) => {
                    const terminated = model.Status === WebsiteStatus.註銷;
                    if (terminated && !value) return "網站狀態為「註銷」時，必須填寫註銷日期。";
                    if (!terminated && value) return "只有網站狀態為「註銷」時才能填寫註銷日期。";
                    return null;
                })
            ],
            ServiceEndDate: [
                rules.custom<WebsiteForm>((value, model) =>
                    value && model.ServiceStartDate && String(value) < model.ServiceStartDate
                        ? "網站到期日期不可早於開通日期。"
                        : null)
            ],
            DomainEndDate: [
                rules.custom<WebsiteForm>((value, model) =>
                    value && model.DomainStartDate && String(value) < model.DomainStartDate
                        ? "網域到期日期不可早於起始日期。"
                        : null)
            ],
            DomainPassword: [rules.maxLength(200)]
        },
        beforeSave: () => {
            pageError.value = "";
            // 任一彈窗開著時，Ctrl+S 不能偷偷送出底下的網站表單
            return !isNotFoundDialogOpen.value && !isQuickCustomerOpen.value;
        },
        save: values => isEdit.value
            ? updateWebsite(websiteId.value, values)
            : createWebsite(values),
        afterSave: () => {
            // 與客戶頁一致：存檔後返回清單
            void router.push("/websites");
        },
        onError: error => {
            console.error(error);
            pageError.value = error instanceof ApiError && Object.keys(error.fieldErrors).length > 0
                ? "部分欄位有誤，請依紅字提示修正。"
                : "儲存失敗，請稍後再試。";
        }
    });

    // 依賴 form 的 computed 一律放在 form 宣告之後
    const isTerminated = computed(() => form.model.value.Status === WebsiteStatus.註銷);
    const isFormLocked = computed(() => loading.value || form.isSaving.value);

    // 狀態切離「註銷」時清空註銷日期，前後端規則一致
    watch(() => form.model.value.Status, status => {
        if (status === WebsiteStatus.註銷) return;
        form.model.value.TerminatedDate = "";
        form.clearErrors("TerminatedDate");
    });

    // 勾選「清除密碼」時清空新密碼輸入（兩者互斥）
    watch(() => form.model.value.ClearDomainPassword, clear => {
        if (!clear) return;
        form.model.value.DomainPassword = "";
        form.clearErrors("DomainPassword");
    });

    function setCustomer(value: CustomerLookup | null): void {
        customer.value = value;
        isCustomerDeleted.value = false;
        form.model.value.FK_CompanyId = value?.Id ?? 0;
        form.clearErrors("FK_CompanyId");
    }

    function applyDetail(detail: WebsiteDetail): void {
        // 用 reset 載入而非逐欄指派，才不會一進頁面就被標成「尚未儲存」
        form.reset(toWebsiteForm(detail));
        customer.value = detail.Customer;
        isCustomerDeleted.value = detail.Customer === null;
        // 統編選填，沒統編的客戶改顯示公司名稱
        lookupKeyword.value = detail.Customer?.TaxId || detail.Customer?.Name || "";
        hasStoredPassword.value = detail.HasDomainPassword;
        revealedPassword.value = null;
        passwordNotice.value = "";
    }

    async function lookupByKeyword(): Promise<void> {
        if (lookingUp.value) return;

        const keyword = lookupKeyword.value.trim();
        lookupError.value = "";
        if (!keyword) {
            lookupError.value = "請輸入統一編號或公司名稱。";
            return;
        }

        lookingUp.value = true;
        try {
            const match = await lookupCustomer(keyword);
            setCustomer(match);
            if (!match) isNotFoundDialogOpen.value = true;
        }
        catch (error) {
            console.error(error);
            lookupError.value = "查詢客戶失敗，請稍後再試。";
        }
        finally {
            lookingUp.value = false;
        }
    }

    function openQuickCustomer(): void {
        isNotFoundDialogOpen.value = false;
        isQuickCustomerOpen.value = true;
    }

    function onCustomerCreated(created: CustomerLookup): void {
        isQuickCustomerOpen.value = false;
        lookupError.value = "";
        lookupKeyword.value = created.TaxId || created.Name;
        setCustomer(created);
    }
    async function toggleDomainPassword(): Promise<void> {
        if (revealedPassword.value !== null) {
            revealedPassword.value = null;
            return;
        }

        revealing.value = true;
        passwordNotice.value = "";
        try {
            const result = await fetchDomainPassword(websiteId.value);
            if (result.State === "Ok") revealedPassword.value = result.Password ?? "";
            else if (result.State === "Empty") passwordNotice.value = "這筆尚未記錄網域密碼。";
            else passwordNotice.value = "此註記已無法讀取，請重新輸入。";
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
        void router.push("/websites");
    }

    onMounted(async () => {
        if (!isEdit.value) return;

        loading.value = true;
        try {
            applyDetail(await fetchWebsite(websiteId.value));
        }
        catch (error) {
            console.error(error);
            // 站台會記住最後位置，網站不存在時直接導回清單
            if (error instanceof ApiError && error.status === 404) {
                void router.replace("/websites");
                return;
            }
            pageError.value = "網站資料載入失敗，請重新整理再試。";
        }
        finally {
            loading.value = false;
        }
    });
</script>

<template>
    <section class="page-heading">
        <div>
            <h1>{{ isEdit ? "編輯網站" : "新增網站" }}</h1>
            <p>輸入統一編號或公司名稱帶出客戶，再填寫網站、期限與網域資訊。</p>
        </div>
    </section>

    <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>

    <form class="form-stack" novalidate @submit.prevent="form.save('button')">
        <!-- ── 客戶與網站資料 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-customer-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-customer-title">客戶與網站資料</span>
            </div>

            <p v-if="isCustomerDeleted" class="alert alert-warning" role="status">
                原本的客戶資料已被刪除，請重新以統一編號或公司名稱帶出客戶後再儲存。
            </p>

            <div class="form-grid">
                <div class="form-field form-field-wide">
                    <label for="website-customer-keyword" class="form-label">
                        統一編號／公司名稱 <i class="form-required">*</i>
                    </label>
                    <div class="input-with-action">
                        <input id="website-customer-keyword"
                               v-model="lookupKeyword"
                               type="text"
                               maxlength="200"
                               placeholder="輸入統編或完整公司名稱"
                               @keydown.enter.prevent="lookupByKeyword" />
                        <button class="ui-button ui-button-secondary"
                                type="button"
                                :disabled="lookingUp"
                                @click="lookupByKeyword">
                            <span class="material-symbols-outlined">search</span>
                            <span>帶出客戶</span>
                        </button>
                    </div>
                    <FormFieldErrors :errors="form.getErrors('FK_CompanyId')" />
                    <p v-if="lookupError" class="field-note field-note-error" role="alert">{{ lookupError }}</p>
                </div>

                <div class="form-field">
                    <label>
                        <span>公司名稱</span>
                        <input :value="customer?.Name ?? ''" type="text" readonly />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>主要聯絡人姓名</span>
                        <input :value="customer?.PrimaryContactName ?? ''" type="text" readonly />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>公司 Email</span>
                        <input :value="customer?.Email ?? ''" type="text" readonly />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>公司電話</span>
                        <input :value="customer?.Phone ?? ''" type="text" readonly />
                    </label>
                </div>

                <div class="form-field form-field-wide">
                    <label>
                        <span>網站名稱 <i class="form-required">*</i></span>
                        <input v-model="form.model.value.Name" type="text" maxlength="250" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('Name')" />
                </div>
            </div>
        </fieldset>

        <!-- ── 版本與期限 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-period-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-period-title">版本與期限</span>
            </div>

            <div class="form-grid">
                <div class="form-field">
                    <label>
                        <span>網站版本</span>
                        <select v-model.number="form.model.value.Level">
                            <option :value="null">未選擇</option>
                            <option v-for="option in websiteLevelOptions" :key="option.Value" :value="option.Value">
                                {{ option.Text }}
                            </option>
                        </select>
                    </label>
                    <FormFieldErrors :errors="form.getErrors('Level')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>主機位置</span>
                        <input v-model="form.model.value.HostLocation" type="text" maxlength="200" />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網站開通日期</span>
                        <input v-model="form.model.value.ServiceStartDate" type="date" />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網站到期日期</span>
                        <input v-model="form.model.value.ServiceEndDate" type="date" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('ServiceEndDate')" />
                </div>

                <div class="form-field form-field-wide">
                    <span id="website-status-label" class="form-label">
                        網站狀態 <i class="form-required">*</i>
                    </span>
                    <div class="choice-group" role="radiogroup" aria-labelledby="website-status-label">
                        <label v-for="option in websiteStatusOptions" :key="option.Value" class="choice">
                            <input type="radio"
                                   name="website-status"
                                   :value="option.Value"
                                   v-model="form.model.value.Status" />
                            <span class="choice-box" aria-hidden="true"></span>
                            <span>{{ option.Text }}</span>
                        </label>
                    </div>
                    <FormFieldErrors :errors="form.getErrors('Status')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>註銷日期 <i v-if="isTerminated" class="form-required">*</i></span>
                        <input v-model="form.model.value.TerminatedDate"
                               type="date"
                               :disabled="!isTerminated" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('TerminatedDate')" />
                    <p v-if="!isTerminated" class="field-note">狀態為「註銷」時才可填寫。</p>
                </div>
            </div>
        </fieldset>

        <!-- ── 網址／網域 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-domain-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-domain-title">網址／網域</span>
            </div>

            <div class="form-grid">
                <div class="form-field form-field-wide">
                    <div class="choice-group">
                        <label class="choice">
                            <input type="checkbox" v-model="form.model.value.IsDomainPending" />
                            <span class="choice-box" aria-hidden="true"></span>
                            <span>網域待申請</span>
                        </label>
                    </div>
                </div>

                <div class="form-field">
                    <label>
                        <span>網址／網域</span>
                        <input v-model="form.model.value.DomainName" type="text" maxlength="255" />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域公司</span>
                        <input v-model="form.model.value.DomainRegistrar" type="text" maxlength="200" />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域起始日期</span>
                        <input v-model="form.model.value.DomainStartDate" type="date" />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域到期日期</span>
                        <input v-model="form.model.value.DomainEndDate" type="date" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('DomainEndDate')" />
                </div>

                <div class="form-field form-field-wide">
                    <label for="website-domain-password" class="form-label">網域密碼</label>
                    <div class="input-with-action">
                        <input id="website-domain-password"
                               v-model="form.model.value.DomainPassword"
                               type="text"
                               maxlength="200"
                               autocomplete="off"
                               :disabled="form.model.value.ClearDomainPassword"
                               :placeholder="hasStoredPassword ? '已儲存，留空則不變更' : '尚未記錄'" />
                        <button v-if="isEdit && hasStoredPassword"
                                class="outline-icon-button"
                                type="button"
                                :disabled="revealing"
                                :title="revealedPassword === null ? '顯示目前密碼' : '隱藏目前密碼'"
                                :aria-label="revealedPassword === null ? '顯示目前密碼' : '隱藏目前密碼'"
                                @click="toggleDomainPassword">
                            <span class="material-symbols-outlined">
                                {{ revealedPassword === null ? "visibility" : "visibility_off" }}
                            </span>
                        </button>
                    </div>
                    <FormFieldErrors :errors="form.getErrors('DomainPassword')" />
                    <p v-if="revealedPassword !== null" class="field-note field-note-reveal">
                        目前密碼：{{ revealedPassword }}
                    </p>
                    <p v-if="passwordNotice" class="field-note field-note-error">{{ passwordNotice }}</p>
                    <div v-if="hasStoredPassword" class="choice-group choice-group-compact">
                        <label class="choice">
                            <input type="checkbox" v-model="form.model.value.ClearDomainPassword" />
                            <span class="choice-box" aria-hidden="true"></span>
                            <span>清除已儲存的網域密碼</span>
                        </label>
                    </div>
                </div>
            </div>
        </fieldset>

        <!-- ── 備註 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-remark-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-remark-title">備註</span>
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
    <ConfirmDialog :open="isNotFoundDialogOpen"
                   icon="search_off"
                   title="找不到客戶"
                   :message="`系統內沒有統一編號或公司名稱為「${lookupKeyword.trim()}」的客戶資料。`"
                   cancel-text="關閉"
                   confirm-text="填寫資料"
                   @cancel="isNotFoundDialogOpen = false"
                   @confirm="openQuickCustomer" />

    <QuickCustomerDialog :open="isQuickCustomerOpen"
                         :initial-tax-id="quickCustomerTaxId"
                         :initial-name="quickCustomerName"
                         @cancel="isQuickCustomerOpen = false"
                         @created="onCustomerCreated" />
</template>