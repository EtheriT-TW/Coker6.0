<script setup lang="ts">
    import { onMounted, ref } from "vue";
    import { useRouter } from "vue-router";
    import {
        DxColumn,
        DxDataGrid,
        DxPaging,
        DxSearchPanel
    } from "devextreme-vue/data-grid";
    import { fetchWebsites } from "@/services/website-api";
    import type { WebsiteListItem } from "@/types/website";

    const router = useRouter();

    const websites = ref<WebsiteListItem[]>([]);
    const isTruncated = ref(false);
    const loading = ref(false);
    const pageError = ref("");

    async function load(): Promise<void> {
        loading.value = true;
        pageError.value = "";
        try {
            const result = await fetchWebsites();
            websites.value = result.Items;
            isTruncated.value = result.IsTruncated;
        }
        catch (error) {
            console.error(error);
            pageError.value = "網站資料載入失敗，請重新整理再試。";
        }
        finally {
            loading.value = false;
        }
    }

    function goCreate(): void {
        void router.push("/websites/new");
    }

    function goEdit(id: number): void {
        void router.push(`/websites/${id}`);
    }

    onMounted(load);
</script>

<template>
    <section class="page-heading">
        <div>
            <h1>網站與站台</h1>
            <p>查看各客戶擁有的網站、期限與目前運作狀態。</p>
        </div>
        <button class="ui-button ui-button-primary" type="button" @click="goCreate">
            <span class="material-symbols-outlined">add</span>
            <span>新增網站</span>
        </button>
    </section>

    <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>
    <p v-if="isTruncated" class="alert alert-warning" role="status">
        資料量超過顯示上限，清單只列出前 1000 筆。
    </p>

    <section class="data-card">
        <DxDataGrid :data-source="websites"
                    key-expr="Id"
                    :show-borders="true"
                    :column-auto-width="true"
                    :column-hiding-enabled="true"
                    :hover-state-enabled="true"
                    :allow-column-resizing="true"
                    no-data-text="目前沒有網站資料">
            <DxSearchPanel :visible="true" :width="260" placeholder="搜尋網站、客戶、統編、網域" />
            <DxPaging :page-size="20" />

            <DxColumn data-field="Name" caption="網站名稱" :min-width="200" :hiding-priority="6" />
            <DxColumn data-field="CustomerName"
                      caption="客戶"
                      :min-width="180"
                      cell-template="customerCell"
                      :hiding-priority="5" />
            <DxColumn data-field="CustomerTaxId" caption="統一編號" :min-width="110" :hiding-priority="1" />
            <DxColumn data-field="LevelText" caption="版本" :min-width="80" :hiding-priority="0" />
            <DxColumn data-field="DomainName" caption="網域" :min-width="160" :hiding-priority="2" />
            <DxColumn data-field="ServiceEndDate"
                      caption="到期日"
                      data-type="date"
                      format="yyyy/MM/dd"
                      :min-width="110"
                      :hiding-priority="3" />
            <DxColumn data-field="StatusText"
                      caption="狀態"
                      :min-width="80"
                      cell-template="statusCell"
                      :hiding-priority="4" />
            <DxColumn caption="操作"
                      :width="80"
                      :fixed="true"
                      fixed-position="right"
                      :allow-sorting="false"
                      :allow-hiding="false"
                      cell-template="rowActions" />

            <template #customerCell="{ data }">
                <span v-if="data.data.CustomerName">{{ data.data.CustomerName }}</span>
                <span v-else class="grid-muted">（客戶已刪除）</span>
            </template>

            <template #statusCell="{ data }">
                <span class="status-pill" :class="`status-pill-${data.data.Status}`">
                    {{ data.data.StatusText }}
                </span>
            </template>

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