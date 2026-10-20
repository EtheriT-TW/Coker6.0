<script setup lang="ts">
    import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";
    import { useRoute, useRouter } from "vue-router";
    import { ApiError, FormFieldErrors, requestAlert, requestConfirm, rules, useManagedForm } from "@/core/coker";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import DateField from "@/components/DateField.vue";
    import IisBindingNote from "@/components/IisBindingNote.vue";
    import QuickCustomerDialog from "@/components/QuickCustomerDialog.vue";
    import WebsiteUrlItem from "@/components/WebsiteUrlItem.vue";
    import { lookupCustomer } from "@/services/customer-api";
    import { useIisBindingLookup } from "@/services/iis-binding-lookup";
    import {
        createWebsite,
        fetchWebsite,
        fetchWebsiteOptions,
        fetchWebsiteBindingComparison,
        refreshWebsiteUrls,
        newWebsiteUrlItem,
        toDefaultUrl,
        toWebsiteForm,
        updateWebsite
    } from "@/services/website-api";
    import type { CustomerLookup } from "@/types/customer";
    import {
        WebsiteLevel,
        WebsiteStatus,
        EnterpriseEmail,
        websiteLevelOptions,
        websiteLocaleOptions,
        websiteStatusOptions,
        enterpriseEmailOption,
        type WebsiteDetail,
        type WebsiteForm,
        type WebsiteOptions
    } from "@/types/website";
    import type { WebsiteBindingComparison } from "@/types/website";
    import { toBindingHost } from "@/utils/domain-name";
    import { todayDateInput } from "@/utils/date-input";
    import { isSiteRoot } from "@/utils/url-parts";

    const TAX_ID_PATTERN = /^\d{8,10}$/;

    // 與後端 WebsitesController.MaxUrlCount 一致（只算其他網址，主要網址另計）
    const MAX_URL_COUNT = 10;
    const urlLimit = ref(MAX_URL_COUNT);
    const SITE_ROOT_MESSAGE = "請輸入網站根網址，例：https://www.example.com.tw（不可包含路徑）。";
    const DEFAULT_LOCALE = "zh-tw";
    const bindingComparison = ref<WebsiteBindingComparison | null>(null);
    const comparisonLoading = ref(false);
    const comparisonError = ref("");
    const savedHostLocation = ref("");
    const synchronizeRequested = ref(false);
    const defaultSaveMessage = '確定要儲存這筆網站資料嗎？網址的新增、修改與刪除會一併套用；完成狀態可至任務紀錄查看。';
    const saveConfirmation = { icon: 'save', title: '確認儲存', message: defaultSaveMessage };
    let comparisonSequence = 0;
    let comparisonDisposed = false;

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

    // options API：主機清單、各版本預設主機、預設網址後綴
    const websiteOptions = ref<WebsiteOptions | null>(null);
    // 編輯時的後台 Website.DefaultUrl，只顯示不修改
    const savedDefaultUrl = ref("");
    // 載入時 IsDomainPending 為 true，已自動在其他網址補一筆預設網址
    const isPendingUrlAdded = ref(false);

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

        /** 「其他」要跟填空框綁在一起單獨渲染，不走迴圈。 */
    const plainTypeOption = enterpriseEmailOption.filter(
        option => option.Value !== EnterpriseEmail.Other
    );

    // 各網址列目前開著彈窗的 Key；只在 beforeSave 讀，不需要響應式
    const openUrlDialogKeys = new Set<number>();

    function emptyForm(): WebsiteForm {
        const start = todayDateInput();
        return {
            OrgName: "",
            FK_CompanyId: 0,
            Name: "",
            Level: WebsiteLevel.形象,
            Locale: DEFAULT_LOCALE,
            HostLocation: "",
            ServiceStartDate: start,
            ServiceEndDate: "",
            Status: WebsiteStatus.正常,
            SuspendedDate: "",
            TerminatedDate: "",
            Urls: [],
            Remark: "",
            EnterpriseEmailAccountType: null,
            EnterpriseEmailTotalAccounts: "",
            EnterpriseEmailOtherType: "",
            EnterpriseEmailExpirationDate: "",
            EnterpriseEmailRemark: ""
        };
    }

    /** 其他網址逐列驗證，可以 0 筆；錯誤 key 與後端一致（Urls[i].Url），才能掛到對的那一列。 */
    function validateUrls(values: WebsiteForm): Record<string, string[]> {
        const errors: Record<string, string[]> = {};
        if (values.Urls.length > urlLimit.value) {
            errors.Urls = [`其他網址最多 ${urlLimit.value} 筆。`];
        }

        const primary = toBindingHost(primaryUrl.value);
        const seen = new Set<string>();
        for (const [index, item] of values.Urls.entries()) {
            const url = item.Url.trim();
            const normalized = toBindingHost(url);
            const key = `Urls[${index}].Url`;
            if (!isSiteRoot(url)) errors[key] = [SITE_ROOT_MESSAGE];
            else if (url.length > 255) errors[key] = ["網址不可超過 255 個字元。"];
            // 訊息與後端一致
            else if (primary && normalized === primary) errors[key] = [`網址「${url}」就是預設網址，不必再填。`];
            else if (seen.has(normalized)) errors[key] = ["這筆網址與前面的網址重複。"];
            seen.add(normalized);
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
            // 新增時必填且必須是清單內的值；編輯時沿用原值（含空白或清單外舊值），與後端規則一致
            HostLocation: [
                rules.custom<WebsiteForm>(value => {
                    if (isEdit.value) return null;
                    const host = String(value ?? "");
                    if (!host) return "請選擇主機位置。";
                    return websiteOptions.value?.HostLocations.some(option => option.Value === host)
                        ? null
                        : "請從清單選擇主機位置。";
                })
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
            ],
            EnterpriseEmailTotalAccounts:[
                rules.custom<WebsiteForm>((value, model) => {
                    if(!model.EnterpriseEmailAccountType) return null;
                    const number = String(value ?? "").trim();
                    if(!number) return "請輸入帳號總數。";
                    if(Number(value) < 1 || !Number.isInteger(Number(value))) return "帳號總數至少為1。"
                    return null;
                })
            ],
            EnterpriseEmailOtherType:[
                rules.custom<WebsiteForm>((value, model) => {
                    if(!model.EnterpriseEmailAccountType) return null;
                    if(model.EnterpriseEmailAccountType === EnterpriseEmail.Other){
                        const text = String(value ?? "").trim();
                        if(!text) return "請輸入自訂版本名稱。"
                        return null;
                    }
                    return null;
                })
            ],
            EnterpriseEmailExpirationDate:[
                rules.custom<WebsiteForm>((value, model) => {
                    if(model.EnterpriseEmailAccountType === null) return null;
                    if(!value) return "請輸入到期日期。";
                    return null;
                })
            ]
        },
        validate: values => validateUrls(values),
        beforeSave: async values => {
            synchronizeRequested.value = false;
            saveConfirmation.message = defaultSaveMessage;
            pageError.value = "";
            // 任一彈窗開著時，Ctrl+S 不能偷偷送出底下的網站表單
            if (isNotFoundDialogOpen.value || isQuickCustomerOpen.value || openUrlDialogKeys.size > 0) return false;
            if (isEdit.value && savedHostLocation.value === values.HostLocation) {
                await refreshComparison();
                if (comparisonUsable.value) {
                    const missing = values.Urls.filter(item => {
                        const host = toBindingHost(item.Url);
                        return host && !bindingComparison.value!.SiteHosts.includes(host);
                    });
                    if (missing.length) {
                        saveConfirmation.message = `以下網址尚未在主機啟用。是否儲存本次修改，並一併啟用這些網址？\n${missing.map(item => item.Url).join('\n')}`;
                        synchronizeRequested.value = true;
                    }
                }
            }
            return await requestConfirm(saveConfirmation);
        },
        save: values => isEdit.value
            ? updateWebsite(websiteId.value, values, synchronizeRequested.value)
            : createWebsite(values),
        afterSave: () => {
            synchronizeRequested.value = false;
            // 與客戶頁一致：存檔後返回清單
            void router.push("/websites");
        },
        onError: async error => {
            synchronizeRequested.value = false;
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
    const isSuspended = computed(() => form.model.value.Status === WebsiteStatus.暫停);
    const isTerminated = computed(() => form.model.value.Status === WebsiteStatus.註銷);
    const isFormLocked = computed(() => loading.value || form.isSaving.value);
    const hasEnterpriseEmail = computed(() => form.model.value.EnterpriseEmailAccountType !== null);
    const isOtherEmailType = computed (() => form.model.value.EnterpriseEmailAccountType === EnterpriseEmail.Other);

    // 主要網址：新增時跟著站台代碼即時產生（與後端規則一致）；編輯時是後台 DefaultUrl，這裡不能改
    const primaryUrl = computed(() => isEdit.value
        ? savedDefaultUrl.value
        : toDefaultUrl(form.model.value.OrgName, websiteOptions.value?.DefaultUrlSuffix ?? ""));

    // 主機下拉：舊資料的主機不在清單內時補一個選項，下拉才不會顯示空白
    const hostLocationOptions = computed(() => {
        const options = websiteOptions.value?.HostLocations ?? [];
        const current = form.model.value.HostLocation;
        return current && !options.some(option => option.Value === current)
            ? [...options, { Value: current, Text: `${current}（不在主機清單）` }]
            : options;
    });

    // IIS 綁定：主要網址＋所有其他網址一次查；主機或網址變更時重查
    const iisLookup = useIisBindingLookup(() => ({
        hostLocation: form.model.value.HostLocation,
        urls: [primaryUrl.value, ...form.model.value.Urls.map(item => item.Url)]
    }));

    const comparisonUsable = computed(() => bindingComparison.value?.Status === 'Ok'
        && savedHostLocation.value === form.model.value.HostLocation && !comparisonLoading.value);
    const comparisonRows = computed(() => comparisonUsable.value ? form.model.value.Urls.map(item => {
        const host = toBindingHost(item.Url);
        const other = bindingComparison.value!.OtherBindings.find(x => x.Host === host);
        return { key: item.Key, url: item.Url, state: !host ? '網址待填寫／格式待確認'
            : other ? '此網址已由其他網站使用，請先確認'
            : bindingComparison.value!.SiteHosts.includes(host) ? '主機已設定此網址' : '尚未啟用，儲存時將請您確認' };
    }) : []);
    async function refreshComparison(): Promise<void> {
        if (!isEdit.value || comparisonDisposed) return;
        const sequence = ++comparisonSequence;
        comparisonLoading.value = true;
        bindingComparison.value = null;
        comparisonError.value = '';
        try {
            const result = await fetchWebsiteBindingComparison(websiteId.value);
            if (sequence === comparisonSequence && !comparisonDisposed) bindingComparison.value = result;
        } catch {
            if (sequence === comparisonSequence && !comparisonDisposed) comparisonError.value = '無法取得網址狀態，仍可編輯一般資料；網址變更需待主機回報後儲存。';
        } finally { if (sequence === comparisonSequence && !comparisonDisposed) comparisonLoading.value = false; }
    }
    onBeforeUnmount(() => { comparisonDisposed = true; ++comparisonSequence; });

    // 暫停／註銷日期只在各自的狀態下有值，切走就清空，前後端規則一致。
    // 只在真的有值時才指派，避免載入資料時就被標成「尚未儲存」
    watch(() => form.model.value.Status, status => {
        if (status !== WebsiteStatus.暫停 && form.model.value.SuspendedDate) form.model.value.SuspendedDate = "";
        if (status !== WebsiteStatus.註銷 && form.model.value.TerminatedDate) form.model.value.TerminatedDate = "";
        form.clearErrors("SuspendedDate");
        form.clearErrors("TerminatedDate");
    });

    watch(() => form.model.value.EnterpriseEmailAccountType, accountType => {
        if(accountType !== EnterpriseEmail.Other && form.model.value.EnterpriseEmailOtherType) form.model.value.EnterpriseEmailOtherType = "";
        form.clearErrors("EnterpriseEmailOtherType");
    })

    // 新增時改版本連帶換成該版本的預設主機；編輯舊網站時不動主機。改主機不會反過來改版本
    watch(() => form.model.value.Level, level => {
        if (isEdit.value || level === null) return;
        const host = websiteOptions.value?.DefaultHostByLevel[String(level)];
        if (host && host !== form.model.value.HostLocation) form.model.value.HostLocation = host;
    });

    function addUrl(): void {
        const urls = form.model.value.Urls;
        if (urls.length >= urlLimit.value) return;
        urls.push(newWebsiteUrlItem());
        form.clearErrors("Urls");
    }

    function removeUrl(index: number): void {
        form.model.value.Urls.splice(index, 1);
        // 索引會位移，只清掉被刪那列的 key 會讓後面的錯誤訊息錯位到別列
        clearUrlErrors();
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

    /** 新增時帶入預設主機；用 reset 才不會一進頁面就被標成「尚未儲存」（載入期間表單鎖住，使用者還沒輸入）。 */
    function applyNewDefaults(options: WebsiteOptions): void {
        const value = emptyForm();
        form.reset({
            ...value,
            HostLocation: options.DefaultHostByLevel[String(value.Level)] ?? ""
        });
    }

    function applyDetail(detail: WebsiteDetail, defaultUrlSuffix: string): void {
        const value = toWebsiteForm(detail);
        savedDefaultUrl.value = detail.DefaultUrl ?? "";
        urlLimit.value = Math.max(MAX_URL_COUNT, detail.Urls.length);
        savedHostLocation.value = detail.HostLocation ?? "";

        // 舊資料標記「網域待申請」：改成在其他網址補一筆預設網址；主要網址或其他網址已經有了就不補
        const pendingUrl = detail.IsDomainPending ? toDefaultUrl(detail.OrgName, defaultUrlSuffix) : "";
        const existingUrls = [savedDefaultUrl.value, ...value.Urls.map(item => item.Url)].map(toBindingHost);
        const shouldAddPendingUrl = pendingUrl !== ""
            && !existingUrls.includes(toBindingHost(pendingUrl))
            && value.Urls.length < urlLimit.value;
        if (shouldAddPendingUrl) value.Urls.push(newWebsiteUrlItem(pendingUrl));

        // 用 reset 載入而非逐欄指派，才不會一進頁面就被標成「尚未儲存」
        form.reset(value);
        // 補了網址就是有變更要存：reset 後手動標成「尚未儲存」，離開頁面時會提醒
        if (shouldAddPendingUrl) form.isDirty.value = true;
        isPendingUrlAdded.value = shouldAddPendingUrl;

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
        loading.value = true;
        try {
            const options = await fetchWebsiteOptions();
            websiteOptions.value = options;
            if (isEdit.value) {
                try { bindingComparison.value = await refreshWebsiteUrls(websiteId.value); }
                catch { comparisonError.value = '目前無法更新網址清單，仍可編輯一般資料；請稍後重新開啟此頁。'; }
                applyDetail(await fetchWebsite(websiteId.value), options.DefaultUrlSuffix);
            }
            else applyNewDefaults(options);
        }
        catch (error) {
            console.error(error);
            // 站台會記住最後位置，網站不存在時直接導回清單（options 已載入，404 才是網站本身不存在）
            if (isEdit.value && websiteOptions.value && error instanceof ApiError && error.status === 404) {
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
        網站名稱、版本、語系、開通／到期日直接寫入後台站台「{{ form.model.value.OrgName || "—" }}」，儲存後立即生效。
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
                    <p v-if="!isEdit" class="field-note">改版本會自動換成該版本的預設主機。</p>
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
                        <span>主機位置 <i v-if="!isEdit" class="form-required">*</i></span>
                        <select v-model="form.model.value.HostLocation">
                            <option value="">未選擇</option>
                            <option v-for="option in hostLocationOptions" :key="option.Value" :value="option.Value">
                                {{ option.Text }}
                            </option>
                        </select>
                    </label>
                    <FormFieldErrors :errors="form.getErrors('HostLocation')" />
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
                <button class="ui-button ui-button-ghost"
                        type="button"
                        :disabled="form.model.value.Urls.length >= urlLimit"
                        @click="addUrl">
                    <span class="material-symbols-outlined">add</span>
                    <span>新增其他網址</span>
                </button>
            </div>

            <p v-if="isPendingUrlAdded" class="alert alert-warning" role="status">
                這個網站原本標記為「網域待申請」，已自動在其他網址補上預設網址，請確認後儲存。
            </p>

            <div class="form-grid">
                <!-- 主要網址：後台 Website.DefaultUrl，Platform 只顯示 -->
                <div v-if="isEdit" class="form-field form-field-wide" aria-live="polite">
                    <span class="form-label">網址狀態</span>
                    <p class="field-note">主機已有的網址會自動加入清單。新增、修改或刪除網址後，按「儲存」即可套用。</p>
                    <div class="form-section-heading">
                        <button class="ui-button ui-button-ghost" type="button" :disabled="comparisonLoading" @click="refreshComparison">{{ comparisonLoading ? '讀取中…' : '更新狀態' }}</button>
                    </div>
                    <p v-if="comparisonError" class="field-note" role="alert">{{ comparisonError }}</p>
                    <p v-else-if="savedHostLocation !== form.model.value.HostLocation" class="field-note">主機位置已變更，請先完成網站搬移，再重新開啟此頁修改網址。</p>
                    <p v-else-if="bindingComparison && bindingComparison.Status !== 'Ok'" class="field-note">{{ bindingComparison.Message }}一般資料仍可編輯。</p>
                    <template v-if="comparisonUsable">
                        <p class="field-note">最後更新：{{ bindingComparison?.LastSeenAtUtc ? new Date(bindingComparison.LastSeenAtUtc).toLocaleString('zh-TW') : '—' }}。</p>
                        <ul v-if="comparisonRows.length"><li v-for="row in comparisonRows" :key="row.key">{{ row.url || '尚未填寫網址' }}：{{ row.state }}</li></ul>
                        <p v-else class="field-note">目前沒有其他網址。</p>
                        <p class="field-note">尚未啟用的網址會在儲存時請您確認。儲存後需等待系統完成設定，可至「任務紀錄」查看結果。</p>
                    </template>
                </div>
                <div class="form-field form-field-wide">
                    <label>
                        <span>主要網址</span>
                        <input :value="primaryUrl"
                               type="text"
                               readonly
                               :placeholder="isEdit ? '未設定' : '輸入站台代碼後自動產生'" />
                    </label>
                    <p class="field-note">
                        {{ isEdit ? "主要網址請至後台修改。" : "依站台代碼自動產生，建立後請至後台修改。" }}
                    </p>
                    <IisBindingNote :hint="iisLookup.hintFor(primaryUrl)" />
                </div>

                <div class="form-field form-field-wide">
                    <span class="form-label">其他網址</span>
                    <FormFieldErrors :errors="form.getErrors('Urls')" />
                    <p class="field-note">
                        選填，最多 {{ urlLimit }} 筆，不可與主要網址重複。新增、修改或刪除後按「儲存」，系統會更新網站的網址設定；結果可至「任務紀錄」查看。
                    </p>
                </div>

                <div class="form-field form-field-wide">
                    <p v-if="!form.model.value.Urls.length" class="repeater-empty">尚未新增其他網址。</p>
                    <div v-else class="repeater">
                        <WebsiteUrlItem v-for="(item, index) in form.model.value.Urls"
                                        :key="item.Key"
                                        :url="item.Url"
                                        :index="index"
                                        :initial-domain="item.InitialDomain"
                                        :errors="form.getErrors(`Urls[${index}].Url`)"
                                        :can-edit-domain="isEdit"
                                        :iis-hint="iisLookup.hintFor(item.Url)"
                                        @update:url="value => updateUrl(index, value)"
                                        @remove="removeUrl(index)"
                                        @dialog-change="open => onUrlDialogChange(item.Key, open)" />
                    </div>
                </div>
            </div>
        </fieldset>
        
        <!-- ── 企業信箱 ── -->
        <fieldset class="form-card form-section"
                  aria-labelledby="section-website-email-title"
                  :disabled="isFormLocked">
            <div class="form-section-heading">
                <span id="section-website-email-title">企業信箱</span>
            </div>
            <div class="form-grid">
                <div class="form-field form-field-wide">
                    <span id="account-type-label" class="form-label">帳號版本</span>
                    <div class="choice-group" role="radiogroup" aria-labelledby="account-type-label">
                        <label class="choice">
                            <input type="radio"
                                   name="account-type"
                                   :value=null
                                   v-model="form.model.value.EnterpriseEmailAccountType" />
                            <span class="choice-box" aria-hidden="true"></span>
                            <span>未使用</span>
                        </label>
                        <label v-for="option in plainTypeOption" :key="option.Value" class="choice">
                            <input type="radio"
                                   name="account-type"
                                   :value="option.Value"
                                   v-model="form.model.value.EnterpriseEmailAccountType" />
                            <span class="choice-box" aria-hidden="true"></span>
                            <span>{{ option.Text }}</span>
                        </label>

                        <span class="choice-fill">
                            <label class="choice">
                                <input type="radio"
                                       name="account-type"
                                       :value="EnterpriseEmail.Other"
                                       v-model="form.model.value.EnterpriseEmailAccountType"/>
                                <span class="choice-box" aria-hidden="true"></span>
                                <span>其他</span>
                            </label>
                            <input v-model="form.model.value.EnterpriseEmailOtherType"
                                   class="choice-blank"
                                   type="text"
                                   maxlength="200"
                                   aria-label="自訂版本名稱"
                                   :disabled="!isOtherEmailType" />
                        </span>
                    </div>
                    <FormFieldErrors :errors="[...form.getErrors('EnterpriseEmailAccountType'), ...form.getErrors('EnterpriseEmail.AccountType')]"/>
                    <FormFieldErrors :errors="[...form.getErrors('EnterpriseEmailOtherType'), ...form.getErrors('EnterpriseEmail.OtherType')]"/>
                </div>

                <div class="form-field" v-if="hasEnterpriseEmail">
                    <label>
                        <span>帳號總數<i class="form-required">*</i></span>
                        <input type="number" min="1" v-model="form.model.value.EnterpriseEmailTotalAccounts"/>
                    </label>
                    <FormFieldErrors :errors="[...form.getErrors('EnterpriseEmailTotalAccounts'), ...form.getErrors('EnterpriseEmail.TotalAccounts')]"/>
                </div>

                <div class="form-field" v-if="hasEnterpriseEmail">
                    <label>
                        <span>到期日期<i class="form-required">*</i></span>
                        <DateField v-model="form.model.value.EnterpriseEmailExpirationDate" />
                    </label>
                    <FormFieldErrors :errors="[...form.getErrors('EnterpriseEmailExpirationDate'), ...form.getErrors('EnterpriseEmail.ExpirationDate')]" />
                </div>

                <div class="form-field form-field-wide" v-if="hasEnterpriseEmail">
                    <label>
                        <span>備註</span>
                        <textarea rows="4" maxlength="2000" v-model="form.model.value.EnterpriseEmailRemark"></textarea>
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
</template>
