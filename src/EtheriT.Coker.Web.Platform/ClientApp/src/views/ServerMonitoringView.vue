<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";
import { RouterLink } from "vue-router";
import MetricLineChart from "@/components/MetricLineChart.vue";
import { requestAlert, requestConfirm } from "@/core/coker";
import { createProvisioningTask, fetchProvisioningAgents, fetchProvisioningMetricHistory, fetchProvisioningServers } from "@/services/provisioning-api";
import { ProvisioningTaskType, type ProvisioningAgentStatus, type ProvisioningAppPoolMetric, type ProvisioningIisSiteBinding, type ProvisioningMetricHistory, type ProvisioningServer } from "@/types/provisioning";

const servers = ref<ProvisioningServer[]>([]);
const agents = ref<ProvisioningAgentStatus[]>([]);
const selectedServer = ref("");
const selectedDays = ref(1);
const metricHistory = ref<ProvisioningMetricHistory | null>(null);
const loading = ref(false);
const historyLoading = ref(false);
const submittingSite = ref("");
const errorMessage = ref("");
let refreshTimer: number | undefined;

const currentAgent = computed(() => agents.value.find(x => x.ServerId === selectedServer.value) ?? null);
const applicationPools = computed(() => [...(currentAgent.value?.ApplicationPools ?? [])]
  .sort((a, b) => b.PrivateMemoryBytes - a.PrivateMemoryBytes));
const alerts = computed(() => {
  const agent = currentAgent.value;
  if (!agent) return [] as string[];
  const output: string[] = [];
  if (!agent.IsOnline) output.push("Worker 已離線，資料可能不是目前狀態。");
  if ((agent.CpuUsagePercent ?? 0) >= 90) output.push(`CPU 使用率 ${formatPercent(agent.CpuUsagePercent)}。`);
  if ((agent.MemoryUsagePercent ?? 0) >= 90) output.push(`記憶體使用率 ${formatPercent(agent.MemoryUsagePercent)}。`);
  for (const disk of agent.Disks.filter(x => x.UsagePercent >= 90))
    output.push(`磁碟 ${disk.Name} 使用率 ${formatPercent(disk.UsagePercent)}，剩餘 ${formatBytes(disk.FreeBytes)}。`);
  return output;
});
const serverSeries = computed(() => [
  { Name: "CPU", Color: "#2563eb", Points: (metricHistory.value?.ServerMetrics ?? []).map(x => ({ At: x.SampledAtUtc, Value: x.CpuUsagePercent })) },
  { Name: "記憶體", Color: "#d97706", Points: (metricHistory.value?.ServerMetrics ?? []).map(x => ({ At: x.SampledAtUtc, Value: x.MemoryUsagePercent })) }
]);
const diskSeries = computed(() => {
  const colors = ["#059669", "#7c3aed", "#dc2626", "#0891b2", "#c026d3", "#4d7c0f"];
  return (metricHistory.value?.Disks ?? []).map((disk, index) => ({
    Name: `${disk.Name}${disk.VolumeLabel ? ` ${disk.VolumeLabel}` : ""}`,
    Color: colors[index % colors.length],
    Points: disk.Points.map(x => ({ At: x.SampledAtUtc, Value: x.UsagePercent }))
  }));
});

async function loadOverview(silent = false): Promise<void> {
  if (!silent) loading.value = true;
  try {
    const [serverResult, agentResult] = await Promise.all([fetchProvisioningServers(), fetchProvisioningAgents()]);
    servers.value = serverResult;
    agents.value = agentResult;
    if (!servers.value.some(x => x.Id === selectedServer.value)) selectedServer.value = servers.value[0]?.Id ?? "";
    errorMessage.value = "";
  }
  catch (error) {
    console.error(error);
    if (!silent) errorMessage.value = "無法讀取伺服器監控狀態。";
  }
  finally { if (!silent) loading.value = false; }
}

async function loadHistory(): Promise<void> {
  if (!selectedServer.value) { metricHistory.value = null; return; }
  historyLoading.value = true;
  try {
    metricHistory.value = await fetchProvisioningMetricHistory(selectedServer.value, selectedDays.value);
  }
  catch (error) {
    console.error(error);
    errorMessage.value = "無法讀取歷史監控資料。";
  }
  finally { historyLoading.value = false; }
}

async function refreshAll(): Promise<void> {
  await loadOverview();
  await loadHistory();
}

async function setSiteState(siteName: string, start: boolean): Promise<void> {
  const serverId = selectedServer.value;
  if (!serverId || !siteName || submittingSite.value) return;

  const action = start ? "啟動" : "停止";
  const dryRunText = currentAgent.value?.DryRun ? "目前 Worker 為 Dry Run，不會實際變更 IIS。" : "";
  const confirmed = await requestConfirm({
    title: `${action} IIS 網站`,
    message: `確定要在 ${serverId} ${action}「${siteName}」？${dryRunText ? ` ${dryRunText}` : ""}`,
    icon: start ? "play_arrow" : "stop",
    confirmText: action,
    tone: start ? "primary" : "danger"
  });
  if (!confirmed) return;

  submittingSite.value = siteName;
  try {
    await createProvisioningTask({
      TargetServerId: serverId,
      Type: ProvisioningTaskType.SetIisSiteState,
      SiteName: siteName,
      StartSite: start
    });
    await requestAlert({
      title: `已送出${action}任務`,
      message: `「${siteName}」將由 ${serverId} 的 Worker 執行。`,
      icon: "task_alt",
      tone: "primary"
    });
  }
  catch (error) {
    console.error(error);
    await requestAlert({
      title: `無法送出${action}任務`,
      message: error instanceof Error ? error.message : `無法送出 IIS 網站${action}任務。`
    });
  }
  finally {
    submittingSite.value = "";
  }
}

function getSiteBindings(pool: ProvisioningAppPoolMetric): ProvisioningIisSiteBinding[] {
  if (pool.SiteBindings?.length) return pool.SiteBindings;
  return pool.SiteNames.map(siteName => ({
    SiteName: siteName,
    HostNames: pool.SiteNames.length === 1 ? (pool.HostNames ?? []) : [],
    State: pool.State
  }));
}

function isSiteStarted(site: ProvisioningIisSiteBinding): boolean {
  return site.State?.toLowerCase() === "started";
}

async function installSsl(siteName: string, hostNames: string[]): Promise<void> {
  const serverId = selectedServer.value;
  const hosts = [...new Set(hostNames.map(x => x.trim().toLowerCase()).filter(Boolean))];
  if (!serverId || !siteName || hosts.length === 0 || submittingSite.value) return;

  const confirmed = await requestConfirm({
    title: "申請並安裝 SSL",
    message: `確定要為 IIS 網站「${siteName}」處理以下主機名稱？ ${hosts.join("、")}${currentAgent.value?.DryRun ? " 目前 Worker 為 Dry Run，不會實際安裝憑證。" : ""}`,
    icon: "lock",
    confirmText: "安裝 SSL"
  });
  if (!confirmed) return;

  submittingSite.value = siteName;
  try {
    await createProvisioningTask({
      TargetServerId: serverId,
      Type: ProvisioningTaskType.InstallSsl,
      HostNames: hosts
    });
    await requestAlert({
      title: "已送出 SSL 安裝任務",
      message: `「${siteName}」的 ${hosts.length} 個主機名稱將由 ${serverId} 的 Worker 處理。`,
      icon: "task_alt",
      tone: "primary"
    });
  }
  catch (error) {
    console.error(error);
    await requestAlert({
      title: "無法送出 SSL 安裝任務",
      message: error instanceof Error ? error.message : "請稍後再試。"
    });
  }
  finally {
    submittingSite.value = "";
  }
}

function formatBytes(value: number | null | undefined): string {
  if (value === null || value === undefined || value < 0) return "—";
  const units = ["B", "KB", "MB", "GB", "TB"];
  let size = value;
  let unit = 0;
  while (size >= 1024 && unit < units.length - 1) { size /= 1024; unit++; }
  return `${size.toFixed(unit >= 3 ? 1 : 0)} ${units[unit]}`;
}

function formatPercent(value: number | null | undefined): string {
  return value === null || value === undefined ? "—" : `${value.toFixed(1)}%`;
}

function formatTime(value: string | null | undefined): string {
  return value ? new Date(value).toLocaleString("zh-TW") : "—";
}

watch([selectedServer, selectedDays], () => void loadHistory());
onMounted(() => {
  void loadOverview();
  refreshTimer = window.setInterval(() => void loadOverview(true), 15_000);
});
onBeforeUnmount(() => { if (refreshTimer !== undefined) window.clearInterval(refreshTimer); });
</script>

<template>
  <section class="page-heading">
    <div><h1>伺服器監控</h1><p>遠端查看主機、磁碟與 IIS 網站資源使用狀況。</p></div>
    <div class="heading-actions"><RouterLink class="ui-button" :to="{ path: '/provisioning-test', query: { server: selectedServer } }">前往主機操作</RouterLink><button class="ui-button" type="button" :disabled="loading || historyLoading" @click="refreshAll()">重新整理</button></div>
  </section>

  <p v-if="errorMessage" class="alert alert-error" role="alert">{{ errorMessage }}</p>
  <div v-if="alerts.length" class="alert alert-error monitor-alert" role="alert"><strong>偵測到需要注意的狀況</strong><ul><li v-for="message in alerts" :key="message">{{ message }}</li></ul></div>
  <section class="monitor-toolbar data-card">
    <label><span>伺服器</span><select v-model="selectedServer"><option v-for="server in servers" :key="server.Id" :value="server.Id">{{ server.DisplayName }}（{{ server.Id }}）</option></select></label>
    <label><span>歷史範圍</span><select v-model.number="selectedDays"><option :value="1">近 24 小時</option><option :value="7">近 7 天</option><option :value="30">近 30 天</option></select></label>
    <div v-if="currentAgent" class="connection-state"><span :class="currentAgent.IsOnline ? 'online' : 'offline'"></span>{{ currentAgent.IsOnline ? "Worker 在線" : "Worker 離線" }}<small>最後回報 {{ formatTime(currentAgent.LastSeenAtUtc) }}</small></div>
  </section>

  <section v-if="currentAgent" class="summary-grid">
    <article class="data-card summary-card" :class="{ critical: (currentAgent.CpuUsagePercent ?? 0) >= 90 }"><small>CPU</small><strong>{{ formatPercent(currentAgent.CpuUsagePercent) }}</strong></article>
    <article class="data-card summary-card" :class="{ critical: (currentAgent.MemoryUsagePercent ?? 0) >= 90 }"><small>記憶體</small><strong>{{ formatPercent(currentAgent.MemoryUsagePercent) }}</strong><span>{{ formatBytes(currentAgent.MemoryUsedBytes) }} / {{ formatBytes(currentAgent.MemoryTotalBytes) }}</span></article>
    <article v-for="disk in currentAgent.Disks" :key="disk.Name" class="data-card summary-card" :class="{ critical: disk.UsagePercent >= 90 }"><small>磁碟 {{ disk.Name }} {{ disk.VolumeLabel }}</small><strong>{{ formatPercent(disk.UsagePercent) }}</strong><span>剩餘 {{ formatBytes(disk.FreeBytes) }} / {{ formatBytes(disk.TotalBytes) }}</span></article>
  </section>

  <section class="chart-grid">
    <article class="data-card chart-card"><header><h2>CPU／記憶體趨勢</h2><small>每 {{ metricHistory?.BucketMinutes ?? "—" }} 分鐘彙整</small></header><MetricLineChart :series="serverSeries" :empty-text="historyLoading ? '讀取中…' : '尚未累積歷史資料'" /></article>
    <article class="data-card chart-card"><header><h2>磁碟使用率趨勢</h2><small>保存最近 30 天</small></header><MetricLineChart :series="diskSeries" :empty-text="historyLoading ? '讀取中…' : '尚未累積磁碟資料'" /></article>
  </section>

  <section class="data-card pool-card">
    <header><div><h2>IIS 網站資源使用量</h2><p>依 Private Memory 由高至低排序；回收期間會加總同一 Application Pool 的所有 PID。</p></div><span>{{ applicationPools.length }} 個網站集區</span></header>
    <div class="table-wrap"><table><thead><tr><th>IIS 網站／綁定網址</th><th>Application Pool</th><th>集區狀態</th><th>PID</th><th>CPU</th><th>Private Memory</th><th>Working Set</th><th>操作</th></tr></thead>
      <tbody><tr v-for="pool in applicationPools" :key="pool.ApplicationPoolName"><td><strong>{{ pool.WebsiteNames?.join("、") || pool.SiteNames.join("、") || "未綁定網站" }}</strong><small v-if="pool.WebsiteNames?.length && pool.SiteNames.length" class="binding-hosts">IIS：{{ pool.SiteNames.join("、") }}</small><small v-if="pool.HostNames?.length" class="binding-hosts">{{ pool.HostNames.join("、") }}</small></td><td>{{ pool.ApplicationPoolName }}</td><td><span class="pool-state" :class="pool.State.toLowerCase() === 'started' ? 'started' : ''">{{ pool.State }}</span></td><td>{{ pool.ProcessIds.join(", ") || "—" }}</td><td>{{ formatPercent(pool.CpuUsagePercent) }}</td><td><strong>{{ formatBytes(pool.PrivateMemoryBytes) }}</strong></td><td>{{ formatBytes(pool.WorkingSetBytes) }}</td><td><div v-if="getSiteBindings(pool).length" class="site-controls"><div v-for="site in getSiteBindings(pool)" :key="site.SiteName" class="site-control"><small v-if="getSiteBindings(pool).length > 1">{{ site.SiteName }}</small><div><button v-if="!isSiteStarted(site)" class="table-action icon-action" type="button" title="啟動 IIS 網站" :aria-label="`啟動 IIS 網站 ${site.SiteName}`" :disabled="!currentAgent?.IsOnline || !!submittingSite" @click="setSiteState(site.SiteName, true)"><span class="material-symbols-outlined">play_arrow</span></button><button v-else class="table-action icon-action danger-action" type="button" title="停止 IIS 網站" :aria-label="`停止 IIS 網站 ${site.SiteName}`" :disabled="!currentAgent?.IsOnline || !!submittingSite" @click="setSiteState(site.SiteName, false)"><span class="material-symbols-outlined">stop</span></button><button class="table-action icon-action ssl-action" type="button" :title="site.HostNames.length ? `安裝 SSL：${site.HostNames.join('、')}` : '此 IIS 網站沒有可申請 SSL 的主機名稱綁定'" :aria-label="`為 IIS 網站 ${site.SiteName} 安裝 SSL`" :disabled="!currentAgent?.IsOnline || !!submittingSite || !site.HostNames.length" @click="installSsl(site.SiteName, site.HostNames)"><span class="material-symbols-outlined">lock</span></button></div></div></div><span v-else>—</span></td></tr><tr v-if="!applicationPools.length"><td colspan="8">{{ currentAgent?.IsOnline ? "Worker 未回報 IIS Application Pool；請檢查執行帳號是否有權限讀取 IIS 設定" : "Worker 離線，無法取得 IIS 資料" }}</td></tr></tbody>
    </table></div>
  </section>
</template>

<style scoped>
.heading-actions { display: flex; gap: .6rem; }.monitor-alert ul { margin: .4rem 0 0 1.1rem; }.monitor-toolbar { display: flex; flex-wrap: wrap; gap: 1rem 1.5rem; align-items: end; min-height: 0; padding: 1rem 1.25rem; margin-bottom: 1rem; }.monitor-toolbar label { display: grid; gap: .35rem; min-width: 250px; font-size: .85rem; font-weight: 600; }.monitor-toolbar select { min-height: 40px; padding: .45rem .65rem; border: 1px solid #cfd6e2; border-radius: 6px; background: white; }.connection-state { display: grid; grid-template-columns: auto 1fr; align-items: center; gap: .15rem .5rem; margin-left: auto; }.connection-state span { width: .65rem; height: .65rem; border-radius: 50%; background: #b42318; }.connection-state span.online { background: #159455; }.connection-state small { grid-column: 2; color: #718096; }.summary-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(210px, 1fr)); gap: 1rem; margin-bottom: 1rem; }.summary-card { display: grid; align-content: center; gap: .45rem; min-height: 145px; padding: 1rem 1.2rem; }.summary-card.critical { border-color: #e76f68; background: #fff7f6; }.summary-card small, .summary-card span { color: #687386; }.summary-card strong { font-size: 1.7rem; }.chart-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1rem; }.chart-card { padding: 1.2rem; }.chart-card header, .pool-card > header { display: flex; justify-content: space-between; gap: 1rem; align-items: flex-start; }.chart-card h2, .pool-card h2 { margin: 0; font-size: 1.05rem; }.chart-card header small { color: #718096; }.pool-card { min-height: 0; margin-top: 1rem; padding: 1.2rem; }.pool-card p { margin: .35rem 0 0; color: #687386; }.binding-hosts { display: block; margin-top: .25rem; color: #687386; }.table-wrap { overflow-x: auto; margin-top: 1rem; }table { width: 100%; border-collapse: collapse; font-size: .87rem; }th, td { padding: .65rem; text-align: left; border-bottom: 1px solid #e8ebf0; vertical-align: top; }th { white-space: nowrap; }.pool-state { display: inline-block; padding: .15rem .5rem; border-radius: 999px; background: #eef1f5; }.pool-state.started { color: #17633a; background: #d9f4e5; }.site-controls, .site-control { display: grid; gap: .35rem; }.site-control > div { display: flex; gap: .35rem; }.table-action { border: 1px solid #b9c4d3; border-radius: 6px; background: #fff; color: #234; cursor: pointer; }.icon-action { display: inline-grid; width: 31px; height: 31px; padding: 0; place-items: center; }.icon-action .material-symbols-outlined { font-size: 19px; }.table-action:hover:not(:disabled) { background: #f4f7fb; }.table-action:disabled { opacity: .55; cursor: not-allowed; }.danger-action { color: #b42318; border-color: #efaaa5; }.ssl-action { color: #17633a; border-color: #9acfb3; }@media (max-width: 1000px) { .chart-grid { grid-template-columns: 1fr; }.connection-state { margin-left: 0; } }
</style>
