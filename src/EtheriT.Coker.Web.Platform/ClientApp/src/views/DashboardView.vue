<script setup lang="ts">
    import { onMounted, ref } from "vue";
    import { useRouter } from "vue-router";
    import { api } from "@/core/coker";
    import { toDateInput } from "@/utils/date-input";

    interface ExpiringWebsite {
        Id: number;
        Name: string;
        CustomerName: string | null;
        ServiceEndDate: string | null;
    }

    interface DashboardSummary {
        CustomerCount: number;
        WebsiteCount: number;
        ExpiringCount: number;
        ExpiringWithinDays: number;
        ExpiringWebsites: ExpiringWebsite[];
    }

    const router = useRouter();
    const summary = ref<DashboardSummary | null>(null);
    const loadFailed = ref(false);

    /** "2026-09-15T00:00:00" → "2026/09/15"，不經過 new Date() 避免時區位移。 */
    function formatDate(value: string | null): string {
        return toDateInput(value).replaceAll("-", "/") || "—";
    }

    function openWebsite(id: number): void {
        void router.push(`/websites/${id}`);
    }

    onMounted(async () => {
        try {
            summary.value = await api.get<DashboardSummary>("/api/dashboard/summary");
        }
        catch (error) {
            console.error(error);
            loadFailed.value = true;
        }
    });
</script>

<template>
    <section class="page-heading">
        <div>
            <h1>客戶營運總覽</h1>
            <p>集中查看 Coker 客戶、站台授權與營運狀態。</p>
        </div>
    </section>

    <p v-if="loadFailed" class="alert alert-error" role="alert">總覽資料載入失敗，請重新整理再試。</p>

    <section class="metric-grid" aria-label="平台摘要">
        <article class="metric-card">
            <span class="metric-icon material-symbols-outlined">domain</span>
            <small>客戶數</small>
            <strong :class="{ 'pending-value': !summary }">{{ summary?.CustomerCount ?? "—" }}</strong>
        </article>
        <article class="metric-card">
            <span class="metric-icon material-symbols-outlined">language</span>
            <small>管理站台</small>
            <strong :class="{ 'pending-value': !summary }">{{ summary?.WebsiteCount ?? "—" }}</strong>
        </article>
        <article class="metric-card">
            <span class="metric-icon material-symbols-outlined">event_upcoming</span>
            <small>{{ summary?.ExpiringWithinDays ?? 30 }} 天內到期</small>
            <strong :class="{ 'pending-value': !summary }">{{ summary?.ExpiringCount ?? "—" }}</strong>
        </article>
        <!-- 本月交易尚無資料來源，維持佔位 -->
        <article class="metric-card">
            <span class="metric-icon material-symbols-outlined">monitoring</span>
            <small>本月交易</small>
            <strong class="pending-value">—</strong>
        </article>
    </section>

    <section class="dashboard-grid">
        <article class="content-card">
            <div class="card-heading"><h2>客戶營運趨勢</h2><span class="status-badge">版面預留</span></div>
            <div class="empty-state">
                <div>
                    <span class="material-symbols-outlined">insert_chart</span>
                    <p>資料服務完成後顯示跨站台營運趨勢</p>
                </div>
            </div>
        </article>

        <article class="content-card">
            <div class="card-heading">
                <h2>即將到期</h2>
                <span class="status-badge">{{ summary?.ExpiringWithinDays ?? 30 }} 天內</span>
            </div>

            <ul v-if="summary && summary.ExpiringWebsites.length > 0" class="expiring-list">
                <li v-for="site in summary.ExpiringWebsites" :key="site.Id">
                    <a :href="`/websites/${site.Id}`" @click.prevent="openWebsite(site.Id)">
                        <strong>{{ site.Name }}</strong>
                        <small>{{ site.CustomerName ?? "（客戶已刪除）" }}</small>
                    </a>
                    <span class="expiring-date">{{ formatDate(site.ServiceEndDate) }}</span>
                </li>
            </ul>

            <div v-else class="empty-state">
                <div>
                    <span class="material-symbols-outlined">task_alt</span>
                    <p>{{ summary ? "近期沒有即將到期的站台" : "載入中…" }}</p>
                </div>
            </div>
        </article>
    </section>
</template>