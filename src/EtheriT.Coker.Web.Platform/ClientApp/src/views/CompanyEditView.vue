<script setup lang="ts">
    import { computed, onMounted, ref, watch } from "vue";
    import { useRoute, useRouter } from "vue-router";
    import { ApiError, FormFieldErrors, rules, useManagedForm } from "@/core/coker";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import {
        createCustomer,
        fetchCustomer,
        lookupByTaxId,
        updateCustomer
    } from "@/services/customer-api";
    import {
        CustomerType,
        customerTypeOptions,
        type CustomerForm
    } from "@/types/customer";

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

    // 統編重複確認：beforeSave 會卡在這個 Promise，等使用者回答。
    const duplicateNames = ref<string[]>([]);
    let duplicateResolver: ((confirmed: boolean) => void) | null = null;

    // 新增的次要聯絡人用遞減負數當暫時 key，v-for 的 key 才會穩定。
    let nextTempContactId = -1;

    function emptyForm(): CustomerForm {
        return {
            Name: "",
            // 從「網站與站台」頁跳過來時會帶 ?taxId=
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

    async function checkDuplicateTaxId(values: CustomerForm): Promise<boolean> {
        pageError.value = "";

        // 防重入：beforeSave 跑在 isSaving 變 true 之前，儲存鍵與 Ctrl+S 都還活著。
        // 連按兩次會蓋掉 resolver，讓前一個 Promise 永遠不 resolve、畫面卡死。
        duplicateResolver?.(false);
        duplicateResolver = null;

        // 統編選填，沒填就不用檢查重複
        const taxId = values.TaxId.trim();
        if (!taxId) return true;

        // beforeSave 是在 useManagedForm 的 try/catch 之外被呼叫的，
        // 這裡不自己接住例外的話，整個儲存會靜默失敗、畫面完全沒反應。
        let matches;
        try {
            matches = await lookupByTaxId(
                taxId,
                isEdit.value ? customerId.value : undefined
            );
        }
        catch (error) {
            console.error(error);
            pageError.value = "無法檢查統一編號是否重複，請稍後再試。";
            return false;
        }

        if (matches.length === 0) return true;

        duplicateNames.value = matches.map(item => item.Name);
        return await new Promise<boolean>(resolve => {
            duplicateResolver = resolve;
        });
    }

    function resolveDuplicate(confirmed: boolean): void {
        duplicateNames.value = [];
        const resolve = duplicateResolver;
        duplicateResolver = null;
        resolve?.(confirmed);
    }

    const subContactEmailRule = rules.email<CustomerForm>("聯絡人 Email 格式不正確。");

    const form = useManagedForm<CustomerForm, { Id: number } | void>({
        id: "platform-customer-editor",
        initialValue: emptyForm,
        validation: {
            Name: [rules.required("請輸入公司名稱。"), rules.maxLength(200)],
            // 統編選填；rules.pattern 遇到空值會直接放行，有填才檢查 8 碼
            TaxId: [rules.pattern(/^\d{8}$/, "統一編號需為 8 碼數字。")],
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
        beforeSave: checkDuplicateTaxId,
        save: values => isEdit.value
            ? updateCustomer(customerId.value, values)
            : createCustomer(values),
        afterSave: () => {
            // 需求：新增完後返回客戶管理目錄
            void router.push("/companies");
        },
        onError: error => {
            console.error(error);
            pageError.value = "儲存失敗，請確認欄位內容後再試。";
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

    onMounted(async () => {
        if (!isEdit.value) return;

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
                               maxlength="8" />
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
                        <input v-model="form.model.value.Address" type="text" maxlength="250" />
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

    <ConfirmDialog :open="duplicateNames.length > 0"
                   icon="warning"
                   title="這組統一編號已被使用"
                   :message="`已被「${duplicateNames.join('」、「')}」使用，是否確認繼續？`"
                   confirm-text="確認新增"
                   cancel-text="返回修改"
                   @confirm="resolveDuplicate(true)"
                   @cancel="resolveDuplicate(false)" />
</template>