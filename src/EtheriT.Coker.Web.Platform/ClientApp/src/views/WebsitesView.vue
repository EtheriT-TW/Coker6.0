<script setup lang="ts">
    import { computed, onMounted, ref } from "vue";
    import { useRouter } from "vue-router";
    import {
        DxColumn,
        DxDataGrid,
        DxPaging,
        DxSearchPanel
    } from "devextreme-vue/data-grid";
    import { fetchWebsites } from "@/services/website-api";
    import { WebsiteStatus, type WebsiteListItem } from "@/types/website";


    /** 剩餘天數在 0～此值之間視為「即將到期」 */
    const EXPIRING_WITHIN_DAYS = 60;

    const router = useRouter();

    const websites = ref<WebsiteListItem[]>([]);
    const isTruncated = ref(false);
    const loading = ref(false);
    const pageError = ref("");

    // RemainingDays 為 null（未填到期日）只算進總數；先排除 null，否則 null >= 0 會成立
    const summary = computed(() => {
        const days = websites.value
            .map(site => site.RemainingDays)
            .filter((value): value is number => value !== null);

        return {
            Total: websites.value.length,
            Normal: days.filter(value => value > EXPIRING_WITHIN_DAYS).length,
            Expiring: days.filter(value => value >= 0 && value <= EXPIRING_WITHIN_DAYS).length,
            Expired: days.filter(value => value < 0).length
        };
    });

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

    /** 只放行 http/https；沒寫協定的補 https://，其他協定（如 javascript:）不產生連結 */
    function toHref(url: string | null): string | null {
        const value = url?.trim();
        if (!value)
            return null;
        if (/^https?:\/\//i.test(value))
            return value;
        if (/^[a-z][a-z0-9+.-]*:/i.test(value))
            return null;
        return `https://${value}`;
    }

    function remainingDaysClass(days: number): string {
        return days <= EXPIRING_WITHIN_DAYS ? "days-expiring" : "days-normal";
    }

        interface StatusView {
        Text: string;
        CssClass: string;
    }

    /** 狀態為「正常」時依剩餘天數改顯示；暫停／註銷維持原狀態 */
    function statusView(site: WebsiteListItem): StatusView {
        if (site.Status === WebsiteStatus.正常 && site.RemainingDays !== null) {
            if (site.RemainingDays < 0)
                return { Text: "已過期", CssClass: "status-pill-expired" };
            if (site.RemainingDays <= EXPIRING_WITHIN_DAYS)
                return { Text: "即將到期", CssClass: "status-pill-expiring" };
        }
        return { Text: site.StatusText, CssClass: `status-pill-${site.Status}` };
    }

    /** 給 DataGrid 排序／搜尋用，讓「即將到期」「已過期」也搜得到 */
    function statusText(site: WebsiteListItem): string {
        return statusView(site).Text;
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
            <h1>網站管理</h1>
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

    <section class="metric-grid website-metrics" aria-label="網站摘要">
        <article class="metric-card metric-card-inline">
            <span class="metric-icon material-symbols-outlined">language</span>
            <div class="metric-body">
                <small>網站總數</small>
                <strong :class="{ 'pending-value': loading }">{{ loading ? "—" : summary.Total }}</strong>
                <p class="metric-note">所有網站</p>
            </div>
        </article>
        <article class="metric-card metric-card-inline metric-card-normal">
            <span class="metric-icon material-symbols-outlined">check_circle</span>
            <div class="metric-body">
                <small>正常網站</small>
                <strong :class="{ 'pending-value': loading }">{{ loading ? "—" : summary.Normal }}</strong>
                <p class="metric-note">剩餘天數大於 {{ EXPIRING_WITHIN_DAYS }} 天</p>
            </div>
        </article>
        <article class="metric-card metric-card-inline metric-card-expiring">
            <span class="metric-icon material-symbols-outlined">event_upcoming</span>
            <div class="metric-body">
                <small>即將到期</small>
                <strong :class="{ 'pending-value': loading }">{{ loading ? "—" : summary.Expiring }}</strong>
                <p class="metric-note">剩餘 0～{{ EXPIRING_WITHIN_DAYS }} 天</p>
            </div>
        </article>
        <article class="metric-card metric-card-inline metric-card-expired">
            <span class="metric-icon material-symbols-outlined">event_busy</span>
            <div class="metric-body">
                <small>已到期</small>
                <strong :class="{ 'pending-value': loading }">{{ loading ? "—" : summary.Expired }}</strong>
                <p class="metric-note">已超過到期日</p>
            </div>
        </article>
    </section>

    <section class="data-card">
        <DxDataGrid :data-source="websites"
                    key-expr="Id"
                    :show-borders="true"
                    :column-auto-width="true"
                    :column-hiding-enabled="true"
                    :hover-state-enabled="true"
                    :allow-column-resizing="true"
                    no-data-text="目前沒有網站資料">
            <DxSearchPanel :visible="true" :width="260" placeholder="搜尋網站、客戶、統編、網址" />
            <DxPaging :page-size="20" />

            <!-- hiding-priority：數字越小越早隱藏，不可重複 -->
            <DxColumn data-field="Name" caption="網站名稱" :min-width="200" :hiding-priority="8" />
            <DxColumn data-field="CustomerName"
                      caption="公司名稱"
                      :min-width="180"
                      cell-template="customerCell"
                      :hiding-priority="6" />
            <DxColumn data-field="CustomerTaxId" caption="統一編號" :min-width="110" :hiding-priority="1" />
            <DxColumn data-field="LevelText" caption="版本" :min-width="80" :hiding-priority="0" />
            <DxColumn data-field="Url"
                      caption="網址"
                      :min-width="160"
                      cell-template="urlCell"
                      :hiding-priority="3" />
            <DxColumn data-field="ServiceStartDate"
                      caption="開通日"
                      data-type="date"
                      format="yyyy/MM/dd"
                      :min-width="110"
                      :hiding-priority="2" />
            <DxColumn data-field="ServiceEndDate"
                      caption="到期日"
                      data-type="date"
                      format="yyyy/MM/dd"
                      :min-width="110"
                      :hiding-priority="4" />
            <DxColumn data-field="RemainingDays"
                      caption="剩餘天數"
                      data-type="number"
                      alignment="right"
                      :min-width="90"
                      cell-template="remainingDaysCell"
                      :hiding-priority="7" />
            <DxColumn data-field="StatusText"
                      caption="狀態"
                      :min-width="80"
                      :calculate-cell-value="statusText"
                      cell-template="statusCell"
                      :hiding-priority="5" />
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

            <template #urlCell="{ data }">
                <a v-if="toHref(data.data.Url)"
                   class="grid-link"
                   :href="toHref(data.data.Url) ?? undefined"
                   target="_blank"
                   rel="noopener noreferrer">
                    {{ data.data.Url }}
                </a>
                <span v-else-if="data.data.Url">{{ data.data.Url }}</span>
                <span v-else class="grid-muted">—</span>
            </template>

            <template #remainingDaysCell="{ data }">
                <span v-if="data.data.RemainingDays !== null && data.data.RemainingDays >= 0"
                      class="days-value"
                      :class="remainingDaysClass(data.data.RemainingDays)">
                    {{ data.data.RemainingDays }} 天
                </span>
                <span v-else class="grid-muted">—</span>
            </template>

            <template #statusCell="{ data }">
                <span class="status-pill" :class="statusView(data.data).CssClass">
                    {{ statusView(data.data).Text }}
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