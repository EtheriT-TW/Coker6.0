<script setup lang="ts">
    import { ApiError } from "@/core/coker";
    import { onMounted, ref } from "vue";
    import { useRouter } from "vue-router";
    import {
        DxColumn,
        DxDataGrid,
        DxPaging,
        DxSearchPanel
    } from "devextreme-vue/data-grid";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import { deleteCustomer, fetchCustomers } from "@/services/customer-api";
    import type { CustomerListItem } from "@/types/customer";

    const router = useRouter();

    const customers = ref<CustomerListItem[]>([]);
    const loading = ref(false);
    const pageError = ref("");

    const pendingDelete = ref<CustomerListItem | null>(null);
    const deleting = ref(false);

    async function load(): Promise<void> {
        loading.value = true;
        pageError.value = "";
        try {
            customers.value = await fetchCustomers();
        }
        catch (error) {
            console.error(error);
            pageError.value = "客戶資料載入失敗，請重新整理再試。";
        }
        finally {
            loading.value = false;
        }
    }

    function goCreate(): void {
        void router.push("/companies/new");
    }

    function goEdit(id: number): void {
        void router.push(`/companies/${id}`);
    }

    type PresenceField = "HasEnterpriseEmail" | "Urls" | "Domains";

    /** 清單上這三欄只分有無；欄位的 name 就是 CustomerListItem 的屬性名稱。 */
    function hasValue(row: CustomerListItem, field: PresenceField): boolean {
        return Boolean(row[field]);
    }

    function enterpriseEmailText(row: CustomerListItem): string {
        return row.HasEnterpriseEmail ? "已設定" : "未設定";
    }

    function urlText(row: CustomerListItem): string {
        return row.Urls ? "已設定" : "未設定";
    }

    function domainText(row: CustomerListItem): string {
        return row.Domains ? "已設定" : "未設定";
    }

    async function confirmDelete(): Promise<void> {
        if (!pendingDelete.value) return;

        deleting.value = true;
        try {
            await deleteCustomer(pendingDelete.value.Id);
            await load();
        }
        catch (error) {
            console.error(error);
            pageError.value = error instanceof ApiError && error.status === 409
                ? error.message
                : "刪除失敗，請稍後再試。";
        }
        finally {
            // 不論成功失敗都要關掉對話框，否則錯誤訊息會被遮罩蓋住看不到。
            pendingDelete.value = null;
            deleting.value = false;
        }
    }

    onMounted(load);
</script>

<template>
    <section class="page-heading">
        <div>
            <h1>客戶資料</h1>
            <p>管理公司基本資料，以及公司與站台之間的關係。</p>
        </div>
        <button class="ui-button ui-button-primary" type="button" @click="goCreate">
            <span class="material-symbols-outlined">add</span>
            <span>新增客戶</span>
        </button>
    </section>

    <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>

    <section class="data-card">
        <DxDataGrid :data-source="customers"
                    key-expr="Id"
                    :show-borders="true"
                    :column-auto-width="true"
                    :column-hiding-enabled="true"
                    :hover-state-enabled="true"
                    :allow-column-resizing="true"
                    no-data-text="目前沒有客戶資料">
            <DxSearchPanel :visible="true" :width="260" placeholder="搜尋公司名稱、統編" />
            <DxPaging :page-size="20" />

            <DxColumn data-field="Name" caption="公司名稱" :min-width="220" />
            <DxColumn data-field="TaxId" caption="統一編號" :min-width="120" :hiding-priority="3" />
            <DxColumn data-field="CustomerTypeText" caption="客戶屬性" :min-width="110" :hiding-priority="2" />
            <DxColumn data-field="PrimaryContactName" caption="主要聯絡人" :min-width="120" :hiding-priority="1" />
            <DxColumn data-field="Phone" caption="公司電話" :min-width="130" :hiding-priority="0" />
            <DxColumn name="HasEnterpriseEmail"
                      caption="企業信箱"
                      :width="110"
                      :hiding-priority="5"
                      :calculate-cell-value="enterpriseEmailText"
                      cell-template="presenceCell" />
            <DxColumn name="Urls"
                      caption="網址"
                      :width="110"
                      :hiding-priority="6"
                      :calculate-cell-value="urlText"
                      cell-template="presenceCell" />
            <DxColumn name="Domains"
                      caption="網域"
                      :width="110"
                      :hiding-priority="4"
                      :calculate-cell-value="domainText"
                      cell-template="presenceCell" />
            <DxColumn caption="操作"
                      :width="120"
                      :fixed="true"
                      fixed-position="right"
                      :allow-sorting="false"
                      :allow-hiding="false"
                      cell-template="rowActions" />

            <template #rowActions="{ data }">
                <div class="grid-actions">
                    <button class="text-button"
                            type="button"
                            title="編輯"
                            @click="goEdit(data.data.Id)">
                        <span class="material-symbols-outlined">edit</span>
                    </button>
                    <button class="text-button text-button-danger"
                            type="button"
                            title="刪除"
                            @click="pendingDelete = data.data">
                        <span class="material-symbols-outlined">delete</span>
                    </button>
                </div>
            </template>

            <template #presenceCell="{ data }">
                <span class="status-pill"
                      :class="hasValue(data.data, data.column.name) ? 'status-pill-0' : 'status-pill-2'">
                    {{ data.value }}
                </span>
            </template>
        </DxDataGrid>
    </section>

    <ConfirmDialog :open="pendingDelete !== null"
                   icon="delete"
                   tone="danger"
                   title="確定要刪除這筆客戶嗎？"
                   :message="`「${pendingDelete?.Name ?? ''}」及其次要聯絡人都會一併移除。`"
                   confirm-text="刪除"
                   :busy="deleting"
                   @confirm="confirmDelete"
                   @cancel="pendingDelete = null" />
</template>