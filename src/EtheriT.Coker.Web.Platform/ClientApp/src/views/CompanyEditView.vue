<script setup lang="ts">
    import { computed, onMounted, ref, watch } from "vue";
    import { useRoute, useRouter } from "vue-router";
    import { ApiError, FormFieldErrors, requestAlert, rules, useManagedForm } from "@/core/coker";
    import {
        createCustomer,
        fetchCustomer,
        fetchCustomerWebsites,
        updateCustomer
    } from "@/services/customer-api";
    import {
        CustomerType,
        customerTypeOptions,
        type CustomerForm,
        type CustomerWebsite

    } from "@/types/customer";

    import { remainingDaysClass, statusView, toHref } from "@/utils/website-display";


    /** 「其他」要跟填空框綁在一起單獨渲染，不走迴圈。 */
    const plainTypeOptions = customerTypeOptions.filter(
        option => option.Value !== CustomerType.其他
    );

    const route = useRoute();
    const router = useRouter();

    const customerId = computed(() => {
        const value = Number(route.params.id);
        return Number.isInteger(value) && value > 0 ? value : 0;
    });
    const isEdit = computed(() => customerId.value > 0);

    const loading = ref(false);
    const pageError = ref("");

    // 唯讀區塊，跟表單完全分開：塞進 form.model 會被離開攔截與 CtrlS 存檔誤收。
    const websites = ref<CustomerWebsite[]>([]);
    const websitesLoading = ref(false);
    const websitesError = ref("");

    // 新增的次要聯絡人用遞減負數當暫時 key，v-for 的 key 才會穩定。
    let nextTempContactId = -1;

    function emptyForm(): CustomerForm {
        return {
            Name: "",
            // 從「網站管理」頁跳過來時會帶 ?taxId=
            TaxId: typeof route.query.taxId === "string" ? route.query.taxId : "",
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

    const subContactEmailRule = rules.email<CustomerForm>("聯絡人 Email 格式不正確。");

    const form = useManagedForm<CustomerForm, { Id: number } | void>({
        id: "platform-customer-editor",
        initialValue: emptyForm,
        validation: {
            Name: [rules.required("請輸入公司名稱。"), rules.maxLength(200)],
            // 統編選填；rules.pattern 遇到空值會直接放行，有填才檢查 8～10 碼
            TaxId: [rules.pattern(/^\d{8,10}$/, "統一編號需為 8～10 碼數字。")],
            Address: [rules.maxLength(150, "公司地址不可超過 150 個字元。")],
            Email: [rules.email("公司 Email 格式不正確。")],
            PrimaryContactName: [rules.required("請輸入主要聯絡人姓名。")],
            PrimaryContactEmail: [rules.email("主要聯絡人 Email 格式不正確。")]
        },
        validate: async values => {
            const errors: Record<string, string[]> = {};

            if (values.CustomerType === CustomerType.其他 &&
                !values.CustomerTypeOther.trim()) {
                errors.CustomerTypeOther = ["選擇「其他」時請輸入客戶屬性內容。"];
            }

            for (const [index, contact] of values.SubContacts.entries()) {
                if (!contact.Name.trim()) {
                    errors[`SubContacts[${index}].Name`] = ["請輸入聯絡人姓名。"];
                }

                const emailError = await subContactEmailRule(contact.Email, values);
                if (emailError) {
                    errors[`SubContacts[${index}].Email`] = [emailError];
                }
            }


            return errors;
        },
        save: values => isEdit.value
            ? updateCustomer(customerId.value, values)
            : createCustomer(values),
        afterSave: () => {
            // 需求：新增完後返回客戶管理目錄
            void router.push("/companies");
        },
        onError: async error => {
            console.error(error);
            if (error instanceof ApiError && error.status === 409) {
                await requestAlert({ title: "無法儲存", message: error.message });
                return;
            }
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

    const isOtherType = computed(
        () => form.model.value.CustomerType === CustomerType.其他
    );

    // 保留使用者打過的字，但把殘留的錯誤訊息清掉，
    // 免得紅字掛在已經切走的欄位下面。
    watch(isOtherType, (isOther) => {
        if (!isOther) form.clearErrors("CustomerTypeOther");
    });

    function addSubContact(): void {
        form.model.value.SubContacts.push({
            Id: nextTempContactId--,
            Name: "",
            JobTitle: "",
            Phone: "",
            Email: ""
        });
    }

    function removeSubContact(index: number): void {
        form.model.value.SubContacts.splice(index, 1);
        // 索引會位移，只清掉被刪那列的 key 會讓後面的錯誤訊息錯位到別列。
        clearSubContactErrors();
    }

    function clearSubContactErrors(): void {
        for (const key of Object.keys(form.errors.value)) {
            if (key.startsWith("SubContacts[")) form.clearErrors(key);
        }
    }

    function cancel(): void {
        void router.push("/companies");
    }

    /** 已建網站資料就開該筆；只有後台綁定時，開新增頁並預先帶入站台（與網站管理清單一致）。 */
    function siteLink(site: CustomerWebsite) {
        if (site.PlatformWebsiteId !== null)
            return `/websites/${site.PlatformWebsiteId}`;
        if (site.WebsiteId !== null)
            return { path: "/websites/new", query: { siteId: String(site.WebsiteId) } };
        return null;
    }

    /** 後台與 Platform 兩邊對不起來時要標出來，否則使用者會以為資料漏了。 */
    function siteFlag(site: CustomerWebsite): string {
        if (site.PlatformWebsiteId === null) return "尚未建立網站資料";
        if (site.IsLinkedToOtherCustomer) return "網站資料掛在其他客戶";
        return "";
    }

    async function loadCustomer(): Promise<void> {
        loading.value = true;
        try {
            const detail = await fetchCustomer(customerId.value);
            form.reset({
                Name: detail.Name,
                TaxId: detail.TaxId,
                Phone: detail.Phone ?? "",
                Email: detail.Email ?? "",
                Address: detail.Address ?? "",
                InvoiceInfo: detail.InvoiceInfo ?? "",
                SalesOwner: detail.SalesOwner ?? "",
                CustomerType: detail.CustomerType,
                CustomerTypeOther: detail.CustomerTypeOther ?? "",
                PrimaryContactName: detail.PrimaryContactName ?? "",
                PrimaryContactJobTitle: detail.PrimaryContactJobTitle ?? "",
                PrimaryContactPhone: detail.PrimaryContactPhone ?? "",
                PrimaryContactEmail: detail.PrimaryContactEmail ?? "",
                SubContacts: detail.SubContacts.map(contact => ({
                    Id: contact.Id,
                    Name: contact.Name,
                    JobTitle: contact.JobTitle ?? "",
                    Phone: contact.Phone ?? "",
                    Email: contact.Email ?? ""
                }))
            });
        }
        catch (error) {
            console.error(error);
            // 站台會記住最後位置，客戶被刪掉後再登入會停在死掉的編輯頁。
            if (error instanceof ApiError && error.status === 404) {
                void router.replace("/companies");
                return;
            }
            pageError.value = "客戶資料載入失敗。";
        }
        finally {
            loading.value = false;
        }
    }

    /** 唯讀區塊自己處理錯誤：站台載不出來不該把整張表單鎖死。 */
    async function loadWebsites(): Promise<void> {
        websitesLoading.value = true;
        websitesError.value = "";
        try {
            websites.value = await fetchCustomerWebsites(customerId.value);
        }
        catch (error) {
            console.error(error);
            websitesError.value = "站台資料載入失敗。";
        }
        finally {
            websitesLoading.value = false;
        }
    }

    // 同一個 tick 內發出，兩支請求平行跑；不用 Promise.all 是為了讓錯誤各自獨立。
    onMounted(() => {
        if (!isEdit.value) return;
        void loadCustomer();
        void loadWebsites();
    });
</script>

<template>
    <section class="page-heading">
        <div>
            <h1>{{ isEdit ? "編輯客戶" : "新增客戶" }}</h1>
            <p>填寫客戶資料、主要聯絡人與次要聯絡人。</p>
        </div>
    </section>

    <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>

    <form class="form-stack" @submit.prevent="form.save('button')">
        <fieldset class="form-card form-section"
                  aria-labelledby="section-customer-title"
                  :disabled="loading || form.isSaving.value">
            <div class="form-section-heading">
                <span id="section-customer-title">公司基本資料</span>
            </div>
            <div class="form-grid">

                <div class="form-field form-field-wide form-field-right">
                    <span id="customer-type-label" class="form-label">
                        客戶屬性 <i class="form-required">*</i>
                    </span>
                    <div class="choice-group" role="radiogroup" aria-labelledby="customer-type-label">
                        <label v-for="option in plainTypeOptions" :key="option.Value" class="choice">
                            <input type="radio"
                                   name="customer-type"
                                   :value="option.Value"
                                   v-model="form.model.value.CustomerType" />
                            <span class="choice-box" aria-hidden="true"></span>
                            <span>{{ option.Text }}</span>
                        </label>

                        <!-- 其他 ＋ 填空線綁在同一個容器，換行時不會被拆開 -->
                        <span class="choice-fill">
                            <label class="choice">
                                <input type="radio"
                                       name="customer-type"
                                       :value="CustomerType.其他"
                                       v-model="form.model.value.CustomerType" />
                                <span class="choice-box" aria-hidden="true"></span>
                                <span>其他</span>
                            </label>
                            <input v-model="form.model.value.CustomerTypeOther"
                                   class="choice-blank"
                                   type="text"
                                   maxlength="50"
                                   aria-label="其他屬性內容"
                                   :disabled="!isOtherType" />
                        </span>
                    </div>
                    <FormFieldErrors :errors="form.getErrors('CustomerType')" />
                    <FormFieldErrors :errors="form.getErrors('CustomerTypeOther')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>公司名稱 <i class="form-required">*</i></span>
                        <input v-model="form.model.value.Name" type="text" maxlength="200" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('Name')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>統一編號</span>
                        <input v-model="form.model.value.TaxId"
                               type="text"
                               inputmode="numeric"
                               maxlength="10" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('TaxId')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>公司電話</span>
                        <input v-model="form.model.value.Phone" type="tel" maxlength="50" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('Phone')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>公司 Email</span>
                        <input v-model="form.model.value.Email" type="email" maxlength="150" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('Email')" />
                </div>

                <div class="form-field form-field-wide">
                    <label>
                        <span>公司地址</span>
                        <input v-model="form.model.value.Address" type="text" maxlength="150" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('Address')" />
                </div>

                <div class="form-field form-field-wide">
                    <label>
                        <span>發票資訊</span>
                        <textarea v-model="form.model.value.InvoiceInfo" rows="2" maxlength="500"></textarea>
                    </label>
                    <FormFieldErrors :errors="form.getErrors('InvoiceInfo')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>負責業務</span>
                        <input v-model="form.model.value.SalesOwner" type="text" maxlength="50" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('SalesOwner')" />
                </div>
            </div>
        </fieldset>

        <!-- 唯讀區塊：用 section 不用 fieldset，才不會被表單的 disabled 一起關掉 -->
        <section v-if="isEdit"
                 class="form-card form-section"
                 aria-labelledby="section-websites-title">
            <div class="form-section-heading">
                <span id="section-websites-title">
                    所屬站台
                    <template v-if="!websitesLoading && !websitesError">
                        （{{ websites.length }}）
                    </template>
                </span>
                <RouterLink class="ui-button ui-button-ghost" to="/websites">
                    <span class="material-symbols-outlined">grid_view</span>
                    <span>網站管理</span>
                </RouterLink>
            </div>

            <p v-if="websitesLoading" class="repeater-empty">載入中…</p>
            <p v-else-if="websitesError" class="alert alert-error" role="alert">{{ websitesError }}</p>
            <p v-else-if="!websites.length" class="repeater-empty">這個客戶底下還沒有站台。</p>

            <ul v-else class="site-list">
                <li v-for="site in websites" :key="site.RowKey">
                    <div class="site-list-main">
                        <RouterLink v-if="siteLink(site)" :to="siteLink(site)!">
                            <strong>{{ site.Name }}</strong>
                        </RouterLink>
                        <strong v-else>{{ site.Name }}</strong>

                        <div class="site-list-meta">
                            <span v-if="site.OrgName">{{ site.OrgName }}</span>
                            <span v-if="site.LevelText">{{ site.LevelText }}版</span>
                            <a v-if="toHref(site.Url)"
                               class="grid-link"
                               :href="toHref(site.Url)!"
                               target="_blank"
                               rel="noopener noreferrer">{{ site.Url }}</a>
                            <span v-if="siteFlag(site)" class="site-flag">{{ siteFlag(site) }}</span>
                        </div>
                    </div>

                    <div class="site-list-side">
                        <span v-if="site.Status === null" class="grid-muted">—</span>
                        <span v-else class="status-pill" :class="statusView(site).CssClass">
                            {{ statusView(site).Text }}
                        </span>

                        <div class="site-list-date">
                            <template v-if="site.ServiceEndDate">
                                {{ site.ServiceEndDate.slice(0, 10).replaceAll("-", "/") }}
                                <small v-if="site.RemainingDays !== null && site.RemainingDays >= 0"
                                       class="days-value"
                                       :class="remainingDaysClass(site.RemainingDays)">
                                    剩 {{ site.RemainingDays }} 天
                                </small>
                            </template>
                            <span v-else class="grid-muted">—</span>
                        </div>
                    </div>
                </li>
            </ul>
        </section>

        <fieldset class="form-card form-section"
                  aria-labelledby="section-primary-title"
                  :disabled="loading || form.isSaving.value">
            <div class="form-section-heading">
                <span id="section-primary-title">主要聯絡人資料</span>
            </div>
            <div class="form-grid form-grid-4">
                <div class="form-field">
                    <label>
                        <span>姓名 <i class="form-required">*</i></span>
                        <input v-model="form.model.value.PrimaryContactName" type="text" maxlength="100" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('PrimaryContactName')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>職稱</span>
                        <input v-model="form.model.value.PrimaryContactJobTitle" type="text" maxlength="100" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('PrimaryContactJobTitle')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>連絡電話</span>
                        <input v-model="form.model.value.PrimaryContactPhone" type="tel" maxlength="50" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('PrimaryContactPhone')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>Email</span>
                        <input v-model="form.model.value.PrimaryContactEmail" type="email" maxlength="150" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('PrimaryContactEmail')" />
                </div>
            </div>
        </fieldset>

        <fieldset class="form-card form-section"
                  aria-labelledby="section-sub-title"
                  :disabled="loading || form.isSaving.value">
            <div class="form-section-heading">
                <span id="section-sub-title">次要聯絡人資料</span>
                <button class="ui-button ui-button-ghost" type="button" @click="addSubContact">
                    <span class="material-symbols-outlined">add</span>
                    <span>新增一筆</span>
                </button>
            </div>

            <p v-if="!form.model.value.SubContacts.length" class="repeater-empty">
                尚未新增次要聯絡人。
            </p>

            <div v-else class="repeater">
                <article v-for="(contact, index) in form.model.value.SubContacts"
                         :key="contact.Id"
                         class="repeater-item">
                    <header class="repeater-item-heading">
                        <strong>聯絡人 {{ index + 1 }}</strong>
                    </header>

                    <div class="repeater-row">
                        <div class="form-grid form-grid-4">
                            <div class="form-field">
                                <label>
                                    <span>姓名 <i class="form-required">*</i></span>
                                    <input v-model="contact.Name" type="text" maxlength="100" />
                                </label>
                                <FormFieldErrors :errors="form.getErrors(`SubContacts[${index}].Name`)" />
                            </div>

                            <div class="form-field">
                                <label>
                                    <span>職稱</span>
                                    <input v-model="contact.JobTitle" type="text" maxlength="100" />
                                </label>
                            </div>

                            <div class="form-field">
                                <label>
                                    <span>連絡電話</span>
                                    <input v-model="contact.Phone" type="tel" maxlength="50" />
                                </label>
                            </div>

                            <div class="form-field">
                                <label>
                                    <span>Email</span>
                                    <input v-model="contact.Email" type="email" maxlength="150" />
                                </label>
                                <FormFieldErrors :errors="form.getErrors(`SubContacts[${index}].Email`)" />
                            </div>
                        </div>

                        <!-- 空白標題佔位，讓按鈕與輸入框對齊在同一條線上 -->
                        <div class="repeater-action">
                            <span aria-hidden="true">&nbsp;</span>
                            <button class="outline-icon-button outline-icon-button-danger"
                                    type="button"
                                    title="移除這筆"
                                    aria-label="移除這筆聯絡人"
                                    @click="removeSubContact(index)">
                                <span class="material-symbols-outlined">delete</span>
                            </button>
                        </div>
                    </div>
                </article>
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
                    :disabled="loading || form.isSaving.value">
                {{ form.isSaving.value ? "儲存中…" : "儲存" }}
            </button>
        </div>
    </form>
</template>