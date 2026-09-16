<script setup lang="ts">
    import { computed, onMounted, ref } from "vue";
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

        // beforeSave 是在 useManagedForm 的 try/catch 之外被呼叫的，
        // 這裡不自己接住例外的話，整個儲存會靜默失敗、畫面完全沒反應。
        let matches;
        try {
            matches = await lookupByTaxId(
                values.TaxId.trim(),
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

    const form = useManagedForm<CustomerForm, { Id: number } | void>({
        id: "platform-customer-editor",
        initialValue: emptyForm,
        validation: {
            Name: [rules.required("請輸入公司名稱。"), rules.maxLength(200)],
            TaxId: [
                rules.required("請輸入統一編號。"),
                rules.pattern(/^\d{8}$/, "統一編號需為 8 碼數字。")
            ],
            Email: [rules.email("公司 Email 格式不正確。")],
            PrimaryContactEmail: [rules.email("主要聯絡人 Email 格式不正確。")]
        },
        validate: values => {
            const errors: Record<string, string[]> = {};

            if (values.CustomerType === CustomerType.其他 &&
                !values.CustomerTypeOther.trim()) {
                errors.CustomerTypeOther = ["選擇「其他」時請輸入客戶屬性內容。"];
            }

            values.SubContacts.forEach((contact, index) => {
                if (!contact.Name.trim()) {
                    errors[`SubContacts[${index}].Name`] = ["請輸入聯絡人姓名。"];
                }
            });

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

    <form class="form-card" @submit.prevent="form.save('button')">
        <fieldset class="form-section" :disabled="loading || form.isSaving.value">
            <legend class="form-section-heading">客戶資料</legend>
            <div class="form-grid">
                <div class="form-field">
                    <label>
                        <span>公司名稱 <i class="form-required">*</i></span>
                        <input v-model="form.model.value.Name" type="text" maxlength="200" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('Name')" />
                </div>

                <div class="form-field">
                    <label>
                        <span>統一編號 <i class="form-required">*</i></span>
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

                <div class="form-field">
                    <label>
                        <span>客戶屬性 <i class="form-required">*</i></span>
                        <select v-model.number="form.model.value.CustomerType">
                            <option v-for="option in customerTypeOptions"
                                    :key="option.Value"
                                    :value="option.Value">
                                {{ option.Text }}
                            </option>
                        </select>
                    </label>
                    <FormFieldErrors :errors="form.getErrors('CustomerType')" />
                </div>

                <div v-if="form.model.value.CustomerType === CustomerType.其他"
                     class="form-field form-field-wide">
                    <label>
                        <span>其他屬性內容 <i class="form-required">*</i></span>
                        <input v-model="form.model.value.CustomerTypeOther" type="text" maxlength="50" />
                    </label>
                    <FormFieldErrors :errors="form.getErrors('CustomerTypeOther')" />
                </div>
            </div>
        </fieldset>

        <fieldset class="form-section" :disabled="loading || form.isSaving.value">
            <legend class="form-section-heading">主要聯絡人資料</legend>
            <div class="form-grid">
                <div class="form-field">
                    <label>
                        <span>姓名</span>
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

        <fieldset class="form-section" :disabled="loading || form.isSaving.value">
            <legend class="form-section-heading">
                <span>次要聯絡人資料</span>
                <button class="ui-button ui-button-ghost" type="button" @click="addSubContact">
                    <span class="material-symbols-outlined">add</span>
                    <span>新增一筆</span>
                </button>
            </legend>

            <p v-if="!form.model.value.SubContacts.length" class="repeater-empty">
                尚未新增次要聯絡人。
            </p>

            <div v-else class="repeater">
                <article v-for="(contact, index) in form.model.value.SubContacts"
                         :key="contact.Id"
                         class="repeater-item">
                    <header class="repeater-item-heading">
                        <strong>聯絡人 {{ index + 1 }}</strong>
                        <button class="text-button text-button-danger"
                                type="button"
                                title="移除這筆"
                                @click="removeSubContact(index)">
                            <span class="material-symbols-outlined">close</span>
                        </button>
                    </header>

                    <div class="form-grid">
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