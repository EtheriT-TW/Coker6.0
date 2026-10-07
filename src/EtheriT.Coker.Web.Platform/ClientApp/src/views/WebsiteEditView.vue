<script setup lang="ts">
    import { computed, onMounted, ref, watch } from "vue";
    import { useRoute, useRouter } from "vue-router";
    import { ApiError, FormFieldErrors, requestAlert, rules, useManagedForm } from "@/core/coker";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import QuickCustomerDialog from "@/components/QuickCustomerDialog.vue";
    import WebsiteUrlItem from "@/components/WebsiteUrlItem.vue";
    import { lookupCustomer } from "@/services/customer-api";
    import { isSiteRoot } from "@/utils/url-parts";

    import {
        createWebsite,
        fetchWebsite,
        newWebsiteUrlItem,
        toWebsiteForm,
        updateWebsite
    } from "@/services/website-api";
    import type { CustomerLookup } from "@/types/customer";
    import {
        WebsiteStatus,
        websiteLevelOptions,
        websiteLocaleOptions,
        websiteStatusOptions,
        hostLocationOptions,
        type WebsiteDetail,
        type WebsiteForm
    } from "@/types/website";
    import DateField from "@/components/DateField.vue";

    const TAX_ID_PATTERN = /^\d{8,10}$/;

    // 與後端 WebsitesController.MaxUrlCount 一致
    const MAX_URL_COUNT = 10;
    const SITE_ROOT_MESSAGE = "請輸入網站根網址，例：https://www.example.com.tw（不可包含路徑）；網域還沒申請請勾選「網域待申請」。";

    const route = useRoute();
    const router = useRouter();

    // 後台 Websites.Id；/websites/new 沒有 id，為 0
    const websiteId = computed(() => {
        const value = Number(route.params.id);
        return Number.isInteger(value) && value > 0 ? value : 0;
    });
    const isEdit = computed(() => websiteId.value > 0);

    // 與後端 WebsitesController.OrgNamePattern 一致
    const ORG_NAME_PATTERN = /^[a-z0-9][a-z0-9_-]{1,99}$/;

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

    // 各網址列目前開著彈窗的 Key；只在 beforeSave 讀，不需要響應式
    const openUrlDialogKeys = new Set<number>();

    function emptyForm(): WebsiteForm {
        return {
            OrgName: "",
            FK_CompanyId: 0,
            Name: "",
            Level: null,
            Locale: "",
            HostLocation: "",
            ServiceStartDate: "",
            ServiceEndDate: "",
            Status: WebsiteStatus.正常,
            SuspendedDate: "",
            TerminatedDate: "",
            IsDomainPending: false,
            Urls: [newWebsiteUrlItem(true)],
            Remark: ""
        };
    }

    /** 網址是陣列，逐列驗證；錯誤 key 與後端一致（Urls[i].Url），才能掛到對的那一列。 */
    function validateUrls(values: WebsiteForm): Record<string, string[]> {
        // 待申請時不寫後台網址，沿用站台原本的值
        if (values.IsDomainPending) return {};
        if (values.Urls.length === 0) {
            return { Urls: ["請至少輸入一筆網址；網域還沒申請請勾選「網域待申請」。"] };
        }

        const errors: Record<string, string[]> = {};
        if (values.Urls.filter(item => item.IsPrimary).length !== 1) {
            errors.Urls = ["請指定一筆主要網址。"];
        }

        const seen = new Set<string>();
        for (const [index, item] of values.Urls.entries()) {
            const url = item.Url.trim();
            const key = `Urls[${index}].Url`;
            if (!isSiteRoot(url)) errors[key] = [SITE_ROOT_MESSAGE];
            else if (url.length > 255) errors[key] = ["網址不可超過 255 個字元。"];
            else if (seen.has(url.toLowerCase())) errors[key] = ["這筆網址與前面的網址重複。"];
            seen.add(url.toLowerCase());
        }
        return errors;
    }

    const form = useManagedForm<WebsiteForm, WebsiteDetail>({
        id: "platform-website-editor",
        initialValue: emptyForm,
        validation: {
            FK_CompanyId: [
                rules.custom<WebsiteForm>(value =>
                    Number(value) > 0 ? null : "請先輸入統一編號或公司名稱並帶出客戶。")
            ],
            // 只有新增時要驗；編輯時 OrgName 唯讀、後端也不會讀
            OrgName: [
                rules.custom<WebsiteForm>(value => {
                    if (isEdit.value) return null;
                    const text = String(value ?? "").trim().toLowerCase();
                    if (!text) return "請輸入站台代碼。";
                    return ORG_NAME_PATTERN.test(text)
                        ? null
                        : "站台代碼只能使用小寫英文、數字、- 與 _，長度 2～100 字，且須以英文或數字開頭。";
                })
            ],
            Name: [rules.required("請輸入網站名稱。"), rules.maxLength(250)],
            // 這些欄位直接寫進後台 Website，規則與後端 WebsitesController 一致
            Level: [
                rules.custom<WebsiteForm>(value =>
                    value === null ? "請選擇網站版本。" : null)
            ],
            Locale: [
                rules.custom<WebsiteForm>(value =>
                    !value ? "請選擇語系。" : null)
            ],
            SuspendedDate: [
                rules.custom<WebsiteForm>((value, model) => {
                    const suspended = model.Status === WebsiteStatus.暫停;
                    if (suspended && !value) return "網站狀態為「暫停」時，必須填寫暫停日期。";
                    if (!suspended && value) return "只有網站狀態為「暫停」時才能填寫暫停日期。";
                    return null;
                })
            ],
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
            ]
        },
        validate: values => validateUrls(values),
        confirmSave: {
            icon: "save",
            title: "確認儲存",
            message: "確定要儲存這筆網站資料嗎？新增時會在後台建立站台，站台代碼建立後不可修改。儲存後會返回網站清單。"
        },
        beforeSave: () => {
            pageError.value = "";
            // 任一彈窗開著時，Ctrl+S 不能偷偷送出底下的網站表單
            return !isNotFoundDialogOpen.value && !isQuickCustomerOpen.value && openUrlDialogKeys.size === 0;
        },
        save: values => isEdit.value
            ? updateWebsite(websiteId.value, values)
            : createWebsite(values),
        afterSave: () => {
            // 與客戶頁一致：存檔後返回清單
            void router.push("/websites");
        },
        onError: async error => {
            console.error(error);
            const hasFieldErrors = error instanceof ApiError &&
                Object.keys(error.fieldErrors).length > 0;
            await requestAlert(hasFieldErrors
                ? {
                    title: "部分欄位有誤",
                    message: "請修正以下項目後再儲存：",
                    details: [...new Set(Object.values((error as ApiError).fieldErrors).flat())]
                }
                : { title: "儲存失敗", message: "儲存失敗，請稍後再試。" });
        }
    });

    // 依賴 form 的 computed 一律放在 form 宣告之後    const isSuspended = computed(() => form.model.value.Status === WebsiteStatus.暫停);
    const isSuspended = computed(() => form.model.value.Status === WebsiteStatus.暫停);
    const isTerminated = computed(() => form.model.value.Status === WebsiteStatus.註銷);
    const isFormLocked = computed(() => loading.value || form.isSaving.value);

    // 暫停／註銷日期只在各自的狀態下有值，切走就清空，前後端規則一致。
    // 只在真的有值時才指派，避免載入資料時就被標成「尚未儲存」
    watch(() => form.model.value.Status, status => {
        if (status !== WebsiteStatus.暫停 && form.model.value.SuspendedDate) form.model.value.SuspendedDate = "";
        if (status !== WebsiteStatus.註銷 && form.model.value.TerminatedDate) form.model.value.TerminatedDate = "";
        form.clearErrors("SuspendedDate");
        form.clearErrors("TerminatedDate");
    });

    // 勾選「網域待申請」時清空所有網址（兩者互斥，後端也會擋）；取消勾選時補一列空白主要網址。
    // 只在真的有變動時才指派，避免載入資料時就被標成「尚未儲存」
    watch(() => form.model.value.IsDomainPending, pending => {
        clearUrlErrors();
        if (pending) {
            if (form.model.value.Urls.length > 0) form.model.value.Urls = [];
            return;
        }
        if (form.model.value.Urls.length === 0) form.model.value.Urls = [newWebsiteUrlItem(true)];
    });

    function addUrl(): void {
        const urls = form.model.value.Urls;
        if (urls.length >= MAX_URL_COUNT) return;
        // 清單空的時候，第一筆自動當主要網址
        urls.push(newWebsiteUrlItem(urls.length === 0));
        form.clearErrors("Urls");
    }

    function removeUrl(index: number): void {
        const urls = form.model.value.Urls;
        const [removed] = urls.splice(index, 1);
        // 刪掉主要網址時改由剩下的第一筆接手，存檔時才不會沒有主要網址
        const first = urls[0];
        if (removed?.IsPrimary && first) first.IsPrimary = true;
        // 索引會位移，只清掉被刪那列的 key 會讓後面的錯誤訊息錯位到別列
        clearUrlErrors();
    }

    function setPrimaryUrl(index: number): void {
        form.model.value.Urls.forEach((item, position) => {
            item.IsPrimary = position === index;
        });
        form.clearErrors("Urls");
    }

    function updateUrl(index: number, value: string): void {
        const item = form.model.value.Urls[index];
        if (!item) return;
        item.Url = value;
        form.clearErrors(`Urls[${index}].Url`);
    }

    function clearUrlErrors(): void {
        for (const key of Object.keys(form.errors.value)) {
            if (key.startsWith("Urls")) form.clearErrors(key);
        }
    }

    function onUrlDialogChange(key: number, open: boolean): void {
        if (open) openUrlDialogKeys.add(key);
        else openUrlDialogKeys.delete(key);
    }

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
        // FK_CompanyId 為 0＝後台還沒綁公司；有 Id 卻帶不出客戶才是客戶被刪除
        isCustomerDeleted.value = detail.FK_CompanyId > 0 && detail.Customer === null;
        // 統編選填，沒統編的客戶改顯示公司名稱
        lookupKeyword.value = detail.Customer?.TaxId || detail.Customer?.Name || "";
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
            <p>輸入統一編號或公司名稱帶出客戶，再填寫網站、期限與網址。</p>
        </div>
    </section>

    <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>
    <p v-if="isEdit" class="alert alert-info" role="status">
        網站名稱、版本、語系、開通／到期日與主要網址直接寫入後台站台「{{ form.model.value.OrgName || "—" }}」，儲存後立即生效。
    </p>

    <form class="form-stack" novalidate autocomplete="off" @submit.prevent="form.save('button')">
        <!-- ── 客戶與網站資料 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-customer-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-customer-title">基本資料</span>
                <RouterLink v-if="isEdit && customer && !isCustomerDeleted"
                            class="ui-button ui-button-secondary"
                            :to="{name:'company-edit', params:{id: customer.Id}, query: { returnTo: route.fullPath } }">
                    <span class="material-symbols-outlined" aria-hidden="true">edit</span>
                    <span>編輯客戶資料</span>
                </RouterLink>
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
                    <!-- 只放確認用的摘要；聯絡方式等完整資料請到客戶頁看 -->
                    <div v-else-if="customer && !isCustomerDeleted" class="customer-summary" role="status">
                        <span class="customer-summary-icon" aria-hidden="true">
                            <span class="material-symbols-outlined">apartment</span>
                        </span>
                        <div class="customer-summary-body">
                            <span class="customer-summary-name">
                                <span>客戶名稱 </span>{{ customer.Name }}
                            </span>
                            <span v-if="customer.TaxId || customer.PrimaryContactName" class="customer-summary-meta">
                                <span v-if="customer.TaxId">統編 {{ customer.TaxId }}</span>
                                <span v-if="customer.PrimaryContactName">聯絡人 {{ customer.PrimaryContactName }}</span>
                            </span>
                        </div>
                    </div>
                </div>

                <div class="form-field">
                    <label>
                        <span>站台代碼（OrgName） <i v-if="!isEdit" class="form-required">*</i></span>
                        <input v-if="isEdit" :value="form.model.value.OrgName" type="text" readonly />
                        <input v-else
                               v-model="form.model.value.OrgName"
                               type="text"
                               maxlength="100"
                               autocomplete="off"
                               spellcheck="false"
                               placeholder="例：coker6" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('OrgName')" />
                    <p v-if="!isEdit" class="field-note">建立後不可修改；只能用小寫英文、數字、- 與 _。</p>
                </div>

                <div class="form-field">
                    <label>
                        <span>網站名稱 <i class="form-required">*</i></span>
                        <input v-if="isEdit" :value="form.model.value.Name" type="text" readonly/>
                        <input v-if="!isEdit" v-model="form.model.value.Name" type="text" maxlength="250" />
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
                        <span>網站版本 <i class="form-required">*</i></span>
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
                        <span>語系 <i class="form-required">*</i></span>
                        <select v-model="form.model.value.Locale">
                            <option value="">未選擇</option>
                            <option v-for="option in websiteLocaleOptions" :key="option.Value" :value="option.Value">
                                {{ option.Text }}
                            </option>
                        </select>
                    </label>
                    <FormFieldErrors :errors="form.getErrors('Locale')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>主機位置</span>
                        <select v-model="form.model.value.HostLocation">
                            <option value="">未選擇</option>
                            <option v-for="option in hostLocationOptions" :key="option.Value" :value="option.Value">
                                {{option.Text}}
                            </option>
                        </select>
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網站開通日期</span>
                        <DateField v-model="form.model.value.ServiceStartDate" />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網站到期日期</span>
                        <DateField v-model="form.model.value.ServiceEndDate" />
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
                <div v-if="isSuspended" class="form-field">
                    <label>
                        <span>暫停日期 <i class="form-required">*</i></span>
                        <DateField v-model="form.model.value.SuspendedDate" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('SuspendedDate')" />
                </div>

                <div v-if="isTerminated" class="form-field">
                    <label>
                        <span>註銷日期 <i class="form-required">*</i></span>
                        <DateField v-model="form.model.value.TerminatedDate" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('TerminatedDate')" />
                </div>
            </div>
        </fieldset>

        <!-- ── 網址 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-url-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-url-title">網址</span>
                <button v-if="!form.model.value.IsDomainPending"
                        class="ui-button ui-button-ghost"
                        type="button"
                        :disabled="form.model.value.Urls.length >= MAX_URL_COUNT"
                        @click="addUrl">
                    <span class="material-symbols-outlined">add</span>
                    <span>新增網址</span>
                </button>
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
                    <FormFieldErrors :errors="form.getErrors('Urls')" />
                    <p class="field-note">
                        主要網址會寫入後台站台網址，金流、物流回呼與通知信都使用它；其他網址只記錄在客戶管理平台。最多 {{ MAX_URL_COUNT }} 筆。
                    </p>
                </div>

                <div class="form-field form-field-wide">
                    <p v-if="form.model.value.IsDomainPending" class="repeater-empty">網域待申請時不需填寫網址。</p>
                    <p v-else-if="!form.model.value.Urls.length" class="repeater-empty">尚未新增網址。</p>
                    <div v-else class="repeater">
                        <WebsiteUrlItem v-for="(item, index) in form.model.value.Urls"
                                        :key="item.Key"
                                        :url="item.Url"
                                        :index="index"
                                        :is-primary="item.IsPrimary"
                                        :initial-domain="item.InitialDomain"
                                        :errors="form.getErrors(`Urls[${index}].Url`)"
                                        :can-remove="form.model.value.Urls.length > 1"
                                        :can-edit-domain="isEdit"
                                        @update:url="value => updateUrl(index, value)"
                                        @set-primary="setPrimaryUrl(index)"
                                        @remove="removeUrl(index)"
                                        @dialog-change="open => onUrlDialogChange(item.Key, open)" />
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