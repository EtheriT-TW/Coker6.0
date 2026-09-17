<script setup lang="ts">
    import { onMounted, ref } from "vue";
    import { useRouter } from "vue-router";
    import {
        DxColumn,
        DxDataGrid,
        DxPaging,
        DxSearchPanel
    } from "devextreme-vue/data-grid";
    import { fetchDomains } from "@/services/domain-api";
    import type { DomainListItem } from "@/types/domain";

    const router = useRouter();

    const domains = ref<DomainListItem[]>([]);
    const isTruncated = ref(false);
    const loading = ref(false);
    const pageError = ref("");

    async function load(): Promise<void> {
        loading.value = true;
        pageError.value = "";
        try {
            const result = await fetchDomains();
            domains.value = result.Items;
            isTruncated.value = result.IsTruncated;
        }
        catch (error) {
            console.error(error);
            pageError.value = "網域資料載入失敗，請重新整理再試。";
        }
        finally {
            loading.value = false;
        }
    }

    function goCreate(): void {
        void router.push("/domains/new");
    }

    function goEdit(id: number): void {
        void router.push(`/domains/${id}`);
    }

    onMounted(load);
</script>

<template>
    <section class="page-heading">
        <div>
            <h1>網域管理</h1>
            <p>管理網域的申請公司、期限與密碼，網站會依網址自動對應到這裡的網域。</p>
        </div>
        <button class="ui-button ui-button-primary" type="button" @click="goCreate">
            <span class="material-symbols-outlined">add</span>
            <span>新增網域</span>
        </button>
    </section>

    <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>
    <p v-if="isTruncated" class="alert alert-warning" role="status">
        資料量超過顯示上限，清單只列出前 1000 筆。
    </p>

    <section class="data-card">
        <DxDataGrid :data-source="domains"
                    key-expr="Id"
                    :show-borders="true"
                    :column-auto-width="true"
                    :column-hiding-enabled="true"
                    :hover-state-enabled="true"
                    :allow-column-resizing="true"
                    no-data-text="目前沒有網域資料">
            <DxSearchPanel :visible="true" :width="260" placeholder="搜尋網域、網域公司" />
            <DxPaging :page-size="20" />

            <DxColumn data-field="DomainName" caption="網域" :min-width="200" :hiding-priority="5" />
            <DxColumn data-field="Registrar" caption="網域公司" :min-width="140" :hiding-priority="2" />
            <DxColumn data-field="StartDate"
                      caption="起始日"
                      data-type="date"
                      format="yyyy/MM/dd"
                      :min-width="110"
                      :hiding-priority="0" />
            <DxColumn data-field="EndDate"
                      caption="到期日"
                      data-type="date"
                      format="yyyy/MM/dd"
                      :min-width="110"
                      :hiding-priority="4" />
            <DxColumn data-field="WebsiteCount"
                      caption="使用網站數"
                      data-type="number"
                      :min-width="90"
                      :hiding-priority="1" />
            <DxColumn caption="操作"
                      :width="80"
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
                </div>
            </template>
        </DxDataGrid>
    </section>
</template>