<script setup lang="ts">
    import { computed, onMounted, ref, watch } from "vue";
    import { useRoute, useRouter } from "vue-router";
    import { ApiError, FormFieldErrors, requestAlert, rules, useManagedForm } from "@/core/coker";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import QuickCustomerDialog from "@/components/QuickCustomerDialog.vue";
    import QuickDomainDialog from "@/components/QuickDomainDialog.vue";
    import { lookupCustomer } from "@/services/customer-api";
    import { fetchDomainPassword, matchDomain } from "@/services/domain-api";
    import { isSiteRoot } from "@/utils/url-parts";

    import {
        createWebsite,
        fetchSiteOption,
        fetchSiteOptions,
        fetchWebsite,
        toLocaleInput,
        toWebsiteForm,
        updateWebsite
    } from "@/services/website-api";
    import type { CustomerLookup } from "@/types/customer";
    import type { DomainSummary } from "@/types/domain";
    import {
        WebsiteStatus,
        websiteLevelOptions,
        websiteLocaleOptions,
        websiteStatusOptions,
        hostLocationOptions,
        type WebsiteDetail,
        type WebsiteForm,
        type WebsiteSiteOption
    } from "@/types/website";
    import { toDateInput } from "@/utils/date-input";
    import DateField from "@/components/DateField.vue";
    import UrlField from "@/components/UrlField.vue";

    import BindingDiffDialog from "@/components/BindingDiffDialog.vue";
    import {
        blankFieldsToFill,
        buildBindingDiffs,
        buildCustomerDiff,
        siteToFormValues,
        type BindingDiff,
        type BindingDiffField,
        type BindingField
    } from "@/utils/binding-diff";


    const TAX_ID_PATTERN = /^\d{8,10}$/;

    const route = useRoute();
    const router = useRouter();

    const websiteId = computed(() => {
        const value = Number(route.params.id);
        return Number.isInteger(value) && value > 0 ? value : 0;
    });
    const isEdit = computed(() => websiteId.value > 0);

    // 清單上尚未建合約的列會帶 ?siteId=，代表這筆合約對應的就是那個後台站台
    const presetSiteId = computed(() => {
        const value = Number(route.query.siteId);
        return !isEdit.value && Number.isInteger(value) && value > 0 ? value : 0;
    });
    // 站台由清單指定時不讓改，避免補錯對象
    const isSiteLocked = computed(() => presetSiteId.value > 0);

    const loading = ref(false);
    const pageError = ref("");

    // ── 對應實際站台 ──
    const siteKeyword = ref("");
    const siteOptions = ref<WebsiteSiteOption[]>([]);
    const searchingSite = ref(false);
    const siteSearchError = ref("");
    const siteSearchTruncated = ref(false);
    const hasSearchedSite = ref(false);

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
    // ── 網址 → 網域比對 ──
    const matchedDomain = ref<DomainSummary | null>(null);
    const matchedUrl = ref("");            // 最近一次比對成功送出的網址，沒變就不重查
    const unmatchedHost = ref("");
    const suggestedDomainName = ref("");
    const matchingDomain = ref(false);
    const domainMatchError = ref("");
    const isDomainNotFoundOpen = ref(false);
    const isQuickDomainOpen = ref(false);
    let matchSequence = 0;                 // 比對請求序號，舊回應晚到時丟棄

        // ── 網域密碼（唯讀；要改請到網域編輯頁） ──
    const PASSWORD_MASK = "********";
    const domainPassword = ref<string | null>(null);   // 按眼睛後讀回的明文；null＝遮罩中
    const revealingPassword = ref(false);
    const domainPasswordNotice = ref("");

    const domainPasswordDisplay = computed(() => {
        if (!matchedDomain.value?.HasPassword) return "";
        return domainPassword.value ?? PASSWORD_MASK;
    });

    // 比對到別的網域時清掉明文，避免顯示成上一個網域的密碼
    watch(() => matchedDomain.value?.Id, () => {
        domainPassword.value = null;
        domainPasswordNotice.value = "";
    });

    async function toggleDomainPassword(): Promise<void> {
        if (domainPassword.value !== null) {
            domainPassword.value = null;
            return;
        }

        const domainId = matchedDomain.value?.Id;
        if (!domainId) return;

        revealingPassword.value = true;
        domainPasswordNotice.value = "";
        try {
            const result = await fetchDomainPassword(domainId);
            // 等回應期間網址被改掉、換了網域，就丟掉這次結果
            if (matchedDomain.value?.Id !== domainId) return;

            if (result.State === "Ok") domainPassword.value = result.Password ?? "";
            else if (result.State === "Empty") domainPasswordNotice.value = "這筆網域尚未記錄密碼。";
            else domainPasswordNotice.value = "密碼已無法讀取，請到網域資料重新輸入。";
        }
        catch (error) {
            console.error(error);
            domainPasswordNotice.value = "讀取網域密碼失敗，請稍後再試。";
        }
        finally {
            revealingPassword.value = false;
        }
    }

    const isDomainMissing = computed(() =>
        unmatchedHost.value !== "" && matchedDomain.value === null && !matchingDomain.value);

    function emptyForm(): WebsiteForm {
        return {
            FK_CompanyId: 0,
            FK_WebsiteId: null,
            Name: "",
            Level: null,
            Locale: "",
            HostLocation: "",
            ServiceStartDate: "",
            ServiceEndDate: "",
            Status: WebsiteStatus.正常,
            TerminatedDate: "",
            IsDomainPending: false,
            Url: "",
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
            // 已綁定站台時這些欄位會寫進後台 Website，規則與後端 ValidateBoundSite 一致
            Level: [
                rules.custom<WebsiteForm>((value, model) =>
                    model.FK_WebsiteId !== null && value === null
                        ? "已綁定站台時必須選擇網站版本。"
                        : null)
            ],
            Locale: [
                rules.custom<WebsiteForm>((value, model) =>
                    model.FK_WebsiteId !== null && !value
                        ? "已綁定站台時必須選擇語系。"
                        : null)
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
            ],
            Url: [
                rules.maxLength(500, "網站網址不可超過 500 個字元。"),
                rules.custom<WebsiteForm>((value, model) => {
                    // 待申請時不寫後台網址，沿用站台原本的值
                    if (model.FK_WebsiteId === null || model.IsDomainPending) return null;
                    const url = String(value ?? "");
                    if (!isSiteRoot(url)) return "已綁定站台時，網址必須是網站根網址，例：https://www.example.com.tw（不可包含路徑）。";
                    return url.length > 255 ? "已綁定站台時，網址不可超過 255 個字元。" : null;
                })
            ]
        },
        confirmSave: {
            icon: "save",
            title: "確認儲存",
            message: "確定要儲存這筆網站資料嗎？儲存後會返回網站清單。"
        },
        beforeSave: () => {
            pageError.value = "";
            // 任一彈窗開著時，Ctrl+S 不能偷偷送出底下的網站表單
            return !isNotFoundDialogOpen.value && !isQuickCustomerOpen.value && !isDomainNotFoundOpen.value && !isQuickDomainOpen.value
                && bindingSite.value === null;

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

    // 依賴 form 的 computed 一律放在 form 宣告之後
    const isTerminated = computed(() => form.model.value.Status === WebsiteStatus.註銷);
    const isFormLocked = computed(() => loading.value || form.isSaving.value);

    // 顯示用：siteOptions 同時裝著搜尋結果與載入時帶回來的 LinkedSite
    const linkedSite = computed(() =>
        siteOptions.value.find(item => item.Id === form.model.value.FK_WebsiteId) ?? null);

    // 已綁定時，名稱／版本／語系／日期／網址直接讀寫後台 Website，不再有兩份資料要比對
    const isBound = computed(() => form.model.value.FK_WebsiteId !== null);

        // ── 綁定站台時的差異比對 ──
    const bindingSite = ref<WebsiteSiteOption | null>(null);
    const bindingDiffs = ref<BindingDiff[]>([]);
    let previousSiteId: number | null = null;       // 下拉變更前的站台
    let siteBeforeBinding: number | null = null;    // 彈窗取消時要還原成這個

    // flush: "sync"：要在 @change 處理之前就記下舊值；預設的非同步 watch 會來不及
    watch(() => form.model.value.FK_WebsiteId, (_next, prev) => {
        previousSiteId = prev ?? null;
    }, { flush: "sync" });

    /** 把站台的指定欄位寫進表單；網址換了就重新比對網域。 */
    async function applySiteFields(site: WebsiteSiteOption, fields: BindingField[]): Promise<void> {
        if (fields.length === 0) return;
        const values = siteToFormValues(site);
        const picked = Object.fromEntries(fields.map(field => [field, values[field]])) as Partial<WebsiteForm>;
        form.model.value = { ...form.model.value, ...picked };
        if (fields.includes("Url")) await checkDomain({ force: true, promptWhenMissing: false });
    }

        /** 改用站台在後台綁定的公司當客戶。 */
    function useSiteCustomer(site: WebsiteSiteOption): void {
        if (!site.Company) return;
        setCustomer(site.Company);
        lookupKeyword.value = site.Company.TaxId || site.Company.Name;
        lookupError.value = "";
    }

    /** 本頁還沒帶客戶時，直接用站台的公司，不必問。 */
    function fillCustomerFromSite(site: WebsiteSiteOption): void {
        if (!customer.value) useSiteCustomer(site);
    }

    /** 使用者在「對應站台」下拉選了站台（程式設定的不會觸發 change 事件）。 */
    async function onSiteSelected(): Promise<void> {
        const site = linkedSite.value;
        if (!site) return;   // 選回「未綁定」不用比對

        const customerDiff = buildCustomerDiff(customer.value, site);
        const diffs = [
            ...(customerDiff ? [customerDiff] : []),
            ...buildBindingDiffs(form.model.value, site)
        ];
        if (diffs.length === 0) {
            fillCustomerFromSite(site);
            await applySiteFields(site, blankFieldsToFill(form.model.value, site));
            return;
        }


        siteBeforeBinding = previousSiteId;
        bindingDiffs.value = diffs;
        bindingSite.value = site;
    }

    async function onBindingApply(siteFields: BindingDiffField[]): Promise<void> {
        const site = bindingSite.value;
        closeBindingDialog();
        if (!site) return;

        // 客戶不在表單模型裡，要先處理；setCustomer 會同步寫入 FK_CompanyId
        if (siteFields.includes("Customer")) useSiteCustomer(site);
        else fillCustomerFromSite(site);

        // 空白欄位補站台值＋使用者選「站台」的欄位，一次寫入
        const formFields = siteFields.filter((field): field is BindingField => field !== "Customer");
        await applySiteFields(site, [...blankFieldsToFill(form.model.value, site), ...formFields]);
    }


    function onBindingCancel(): void {
        form.model.value.FK_WebsiteId = siteBeforeBinding;
        closeBindingDialog();
    }

    function closeBindingDialog(): void {
        bindingSite.value = null;
        bindingDiffs.value = [];
    }

    // 狀態切離「註銷」時清空註銷日期，前後端規則一致
    watch(() => form.model.value.Status, status => {
        if (status === WebsiteStatus.註銷) return;
        form.model.value.TerminatedDate = "";
        form.clearErrors("TerminatedDate");
    });

    // 勾選「網域待申請」時清空網址與比對結果（兩者互斥，後端也會擋）
    watch(() => form.model.value.IsDomainPending, pending => {
        if (!pending) return;
        form.model.value.Url = "";
        form.clearErrors("Url");
        resetDomainMatch();
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
        matchedDomain.value = detail.Domain;
        matchedUrl.value = detail.Url ?? "";
        siteOptions.value = detail.LinkedSite ? [detail.LinkedSite] : [];
        hasSearchedSite.value = false;
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

    function resetDomainMatch(): void {
        matchSequence += 1;   // 讓還在路上的舊回應作廢
        matchedDomain.value = null;
        matchedUrl.value = "";
        unmatchedHost.value = "";
        domainMatchError.value = "";
        matchingDomain.value = false;
    }

    async function searchSites(): Promise<void> {
        if (searchingSite.value) return;

        searchingSite.value = true;
        siteSearchError.value = "";
        try {
            const result = await fetchSiteOptions(siteKeyword.value.trim());
            hasSearchedSite.value = true;
            siteSearchTruncated.value = result.IsTruncated;
            // 目前綁定的站台若不在搜尋結果裡要保留，否則下拉會突然變成「未綁定」
            const current = linkedSite.value;
            siteOptions.value = current && !result.Items.some(item => item.Id === current.Id)
                ? [current, ...result.Items]
                : result.Items;
        }
        catch (error) {
            console.error(error);
            siteSearchError.value = "搜尋站台失敗，請稍後再試。";
        }
        finally {
            searchingSite.value = false;
        }
    }

    function clearLinkedSite(): void {
        form.model.value.FK_WebsiteId = null;
        form.clearErrors("FK_WebsiteId");
    }

    /**
     * 網址欄位 change／Enter 時比對網域（由長到短逐段比對在後端做）。
     * promptWhenMissing＝false：剛建完網域後的重查用，避免彈窗無限循環。
     */
    async function checkDomain(options: { force?: boolean; promptWhenMissing?: boolean } = {}): Promise<void> {
        const { force = false, promptWhenMissing = true } = options;
        const url = form.model.value.Url.trim();
        if (!force && url === matchedUrl.value) return;

        resetDomainMatch();
        form.clearErrors("Url");
        if (!url) return;

        const sequence = matchSequence;
        matchingDomain.value = true;
        try {
            const result = await matchDomain(url);
            if (sequence !== matchSequence) return;

            matchedUrl.value = url;
            if (!result.Host) {
                domainMatchError.value = "網址格式不正確，例：https://www.example.com.tw";
                return;
            }

            matchedDomain.value = result.Domain;
            if (result.Domain) return;

            unmatchedHost.value = result.Host;
            suggestedDomainName.value = result.SuggestedDomainName ?? result.Host;
            if (promptWhenMissing) isDomainNotFoundOpen.value = true;
        }
        catch (error) {
            if (sequence !== matchSequence) return;
            console.error(error);
            domainMatchError.value = "比對網域失敗，請稍後再試。";
        }
        finally {
            if (sequence === matchSequence) matchingDomain.value = false;
        }
    }

    function openQuickDomain(): void {
        isDomainNotFoundOpen.value = false;
        isQuickDomainOpen.value = true;
    }

    async function onDomainCreated(): Promise<void> {
        isQuickDomainOpen.value = false;
        // 使用者可能在彈窗改過網域，重查一次確認真的對得上
        await checkDomain({ force: true, promptWhenMissing: false });
    }

    function cancel(): void {
        void router.push("/websites");
    }

    /** 依 ?siteId= 預先綁定後台站台，並用站台資料補上預設的網站名稱與網址。 */
    async function applyPresetSite(): Promise<void> {
        let site: WebsiteSiteOption;
        try {
            site = await fetchSiteOption(presetSiteId.value);
        }
        catch (error) {
            if (error instanceof ApiError && error.status === 404) {
                pageError.value = "找不到指定的後台站台，請回清單重新選擇。";
                return;
            }
            throw error;
        }

        siteOptions.value = [site];
        form.model.value.FK_WebsiteId = site.Id;
        form.model.value.Name = site.Title || site.OrgName;
        form.model.value.Level = site.Level;
        form.model.value.Locale = toLocaleInput(site.Locale);
        form.model.value.ServiceStartDate = toDateInput(site.StartDate);
        form.model.value.ServiceEndDate = toDateInput(site.EndDate);
        fillCustomerFromSite(site);

        if (site.DefaultUrl) {
            form.model.value.Url = site.DefaultUrl;
            await checkDomain({ force: true, promptWhenMissing: false });
        }
        // 預填不算使用者的修改，離開時不該跳「尚未儲存」
        form.reset({ ...form.model.value });
    }

    onMounted(async () => {
        if (!isEdit.value) {
            if (presetSiteId.value > 0) {
                loading.value = true;
                try {
                    await applyPresetSite();
                }
                catch (error) {
                    console.error(error);
                    pageError.value = "後台站台資料載入失敗，請重新整理再試。";
                }
                finally {
                    loading.value = false;
                }
            }
            return;
        }

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
            <p v-if="isSiteLocked">
                對應後台站台「{{ linkedSite?.OrgName ?? "—" }}」。
                輸入統一編號或公司名稱帶出客戶，再填寫期限與狀態。
            </p>
            <p v-else>輸入統一編號或公司名稱帶出客戶，再填寫網站、期限與網址。</p>
        </div>
    </section>

    <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>
    <p v-if="isBound" class="alert alert-info" role="status">
        已綁定後台站台「{{ linkedSite?.OrgName ?? "—" }}」：網站名稱、版本、語系、開通／到期日與網址會同步更新後台站台，儲存後立即生效。
    </p>

    <form class="form-stack" novalidate @submit.prevent="form.save('button')">
        <!-- ── 客戶與網站資料 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-customer-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-customer-title">客戶與網站資料</span>
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
                        <span>網站版本 <i v-if="isBound" class="form-required">*</i></span>
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
                        <span>語系 <i v-if="isBound" class="form-required">*</i></span>
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

                <div class="form-field">
                    <label>
                        <span>註銷日期 <i v-if="isTerminated" class="form-required">*</i></span>
                        <DateField v-model="form.model.value.TerminatedDate" :disabled="!isTerminated" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('TerminatedDate')" />
                    <p v-if="!isTerminated" class="field-note">狀態為「註銷」時才可填寫。</p>
                </div>
            </div>
        </fieldset>

        <!-- ── 網址 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-url-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-url-title">網址</span>
                <RouterLink v-if="isEdit && matchedDomain"
                            class="ui-button ui-button-secondary"
                            :to="{name:'domain-edit', params: {id: matchedDomain.Id}, query: { returnTo: route.fullPath } }">
                    <span class="material-symbols-outlined" aria-hidden="true">edit</span>
                    <span>編輯網域資料</span>
                </RouterLink>
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

                <div class="form-field form-field-wide">
                    <label for="website-url" class="form-label">網址 <i v-if="isBound && !form.model.value.IsDomainPending" class="form-required">*</i></label>
                    <UrlField v-model="form.model.value.Url"
                              input-id="website-url"
                              :disabled="form.model.value.IsDomainPending"
                              @change="checkDomain()"
                              @enter="checkDomain({ force: true })" />
                    <FormFieldErrors :errors="form.getErrors('Url')" />
                    <p v-if="form.model.value.IsDomainPending" class="field-note">網域待申請時不需填寫網址。</p>
                    <p v-if="matchingDomain" class="field-note" role="status">比對網域中…</p>
                    <p v-if="domainMatchError" class="field-note field-note-error" role="alert">{{ domainMatchError }}</p>
                    <p v-if="isDomainMissing" class="field-note field-note-error field-note-with-action" role="alert">
                        <span>網域管理中沒有「{{ unmatchedHost }}」對應的網域資料。</span>
                        <button class="field-note-action" type="button" @click="openQuickDomain">建立網域</button>
                    </p>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域</span>
                        <input :value="matchedDomain?.DomainName ?? ''" type="text" readonly />
                    </label>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域公司</span>
                        <input :value="matchedDomain?.Registrar ?? ''" type="text" readonly />
                    </label>
                </div>

                <div class="form-field">
                    <label for="website-domain-password" class="form-label">網域密碼</label>
                    <div class="input-with-action">
                        <input id="website-domain-password"
                               :value="domainPasswordDisplay"
                               type="text"
                               readonly
                               autocomplete="off"
                               spellcheck="false"
                               :placeholder="matchedDomain ? '尚未記錄' : ''" />
                        <button v-if="matchedDomain?.HasPassword"
                                class="outline-icon-button"
                                type="button"
                                :disabled="revealingPassword"
                                :title="domainPassword !== null ? '隱藏密碼' : '顯示密碼'"
                                :aria-label="domainPassword !== null ? '隱藏密碼' : '顯示密碼'"
                                :aria-pressed="domainPassword !== null"
                                @click="toggleDomainPassword">
                            <span class="material-symbols-outlined">
                                {{ domainPassword !== null ? "visibility_off" : "visibility" }}
                            </span>
                        </button>
                    </div>
                    <p v-if="domainPasswordNotice" class="field-note field-note-error">{{ domainPasswordNotice }}</p>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域到期日期</span>
                        <input :value="toDateInput(matchedDomain?.EndDate)" type="date" readonly />
                    </label>
                </div>
            </div>
        </fieldset>

        <!-- ── 對應站台 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-linked-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-linked-title">對應站台</span>
                <button v-if="form.model.value.FK_WebsiteId && !isSiteLocked"
                        class="ui-button ui-button-secondary"
                        type="button"
                        @click="clearLinkedSite">
                    解除綁定
                </button>
            </div>

            <div class="form-grid">
                <div v-if="!isSiteLocked" class="form-field form-field-wide">
                    <label for="website-site-keyword" class="form-label">站台搜尋</label>
                    <div class="input-with-action">
                        <input id="website-site-keyword"
                               v-model="siteKeyword"
                               type="text"
                               maxlength="200"
                               placeholder="輸入 OrgName、站名或網址；留空列出前 20 筆"
                               @keydown.enter.prevent="searchSites" />
                        <button class="ui-button ui-button-secondary"
                                type="button"
                                :disabled="searchingSite"
                                @click="searchSites">
                            <span class="material-symbols-outlined">search</span>
                            <span>搜尋站台</span>
                        </button>
                    </div>
                    <p v-if="siteSearchError" class="field-note field-note-error" role="alert">
                        {{ siteSearchError }}
                    </p>
                    <p v-else-if="siteSearchTruncated" class="field-note">
                        符合的站台超過 20 筆，請輸入更精確的關鍵字。
                    </p>
                    <p v-else-if="hasSearchedSite && siteOptions.length === 0" class="field-note">
                        找不到符合的站台。
                    </p>
                </div>

                <div class="form-field form-field-wide">
                    <label>
                        <span>對應站台</span>
                        <select v-model="form.model.value.FK_WebsiteId"
                                :disabled="isSiteLocked"
                                @change="onSiteSelected">
                            <option :value="null">未綁定</option>
                            <option v-for="option in siteOptions" :key="option.Id" :value="option.Id">
                                {{ option.OrgName }}（{{ option.Title || "未命名" }}）
                            </option>
                        </select>
                    </label>
                    <FormFieldErrors :errors="form.getErrors('FK_WebsiteId')" />
                    <p v-if="isSiteLocked" class="field-note">
                        由清單指定，不可變更。要改綁其他站台請先存檔，再從編輯頁調整。
                    </p>
                    <p v-else class="field-note">綁定後，架站／搬站流程才能對應到這筆網站資料。</p>
                </div>

                <div class="form-field">
                    <label>
                        <span>站台代碼（OrgName）</span>
                        <input :value="linkedSite?.OrgName ?? ''" type="text" readonly />
                    </label>
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

    <ConfirmDialog :open="isDomainNotFoundOpen"
                   icon="dns"
                   title="找不到網域"
                   :message="`網域管理中沒有「${unmatchedHost}」對應的網域資料，要現在建立嗎？`"
                   cancel-text="稍後再說"
                   confirm-text="填寫網域資料"
                   @cancel="isDomainNotFoundOpen = false"
                   @confirm="openQuickDomain" />

    <QuickDomainDialog :open="isQuickDomainOpen"
                       :initial-domain-name="suggestedDomainName"
                       @cancel="isQuickDomainOpen = false"
                       @created="onDomainCreated" />
    <BindingDiffDialog :open="bindingSite !== null"
                       :site-name="bindingSite?.OrgName ?? ''"
                       :diffs="bindingDiffs"
                       @apply="onBindingApply"
                       @cancel="onBindingCancel" />
</template>