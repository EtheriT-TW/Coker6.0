<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { fetchProvisioningServers, fetchServerTlsStatus } from "@/services/provisioning-api";
import type { ProvisioningServer, ServerTlsStatus, TlsCertificate } from "@/types/provisioning";

const route = useRoute();
const servers = ref<ProvisioningServer[]>([]);
const selectedServer = ref("");
const inventory = ref<ServerTlsStatus | null>(null);
const loading = ref(false);
const error = ref("");
const search = ref("");
const statusFilter = ref("");
const now = ref(Date.now());
let timer: number | undefined;
let requestVersion = 0;
let disposed = false;
const certificates = computed(() => inventory.value?.Snapshot?.Certificates ?? []);
const stale = computed(() => {
  const collected = inventory.value?.Snapshot?.CollectedAtUtc;
  return !!collected && now.value - Date.parse(collected) > 26 * 60 * 60 * 1000;
});
function status(certificate: TlsCertificate): string {
  if (certificate.Error || !certificate.NotAfterUtc || !certificate.NotBeforeUtc) return "無法確認";
  if (Date.parse(certificate.NotAfterUtc) <= now.value) return "已到期";
  if (Date.parse(certificate.NotBeforeUtc) > now.value) return "尚未生效";
  if (Date.parse(certificate.NotAfterUtc) - now.value <= 30 * 86400000) return "即將到期";
  return "有效期內";
}
function remaining(certificate: TlsCertificate): string {
  if (!certificate.NotAfterUtc) return "—";
  const days = Math.ceil((Date.parse(certificate.NotAfterUtc) - now.value) / 86400000);
  return Date.parse(certificate.NotAfterUtc) <= now.value ? "已到期" : `${days} 天`;
}
const visibleCertificates = computed(() => certificates.value.filter(certificate => {
  const text = [certificate.Subject, certificate.Issuer, certificate.Thumbprint, certificate.StoreName,
    ...certificate.DnsNames, ...certificate.IisBindings].join(" ").toLowerCase();
  return (!statusFilter.value || status(certificate) === statusFilter.value) && text.includes(search.value.trim().toLowerCase());
}));
const expiringCount = computed(() => certificates.value.filter(certificate => status(certificate) === "即將到期").length);
const expiredCount = computed(() => certificates.value.filter(certificate => status(certificate) === "已到期").length);
function formatTime(value: string | null | undefined): string {
  return value ? new Date(value).toLocaleString("zh-TW") : "—";
}
async function refresh(): Promise<void> {
  const server = selectedServer.value;
  if (!server || disposed) return;
  const version = ++requestVersion;
  loading.value = true;
  error.value = "";
  try {
    const result = await fetchServerTlsStatus(server);
    if (version !== requestVersion) return;
    inventory.value = result;
    now.value = Date.now();
  }
  catch (cause) {
    if (version === requestVersion) error.value = cause instanceof Error ? cause.message : "讀取憑證資料失敗。";
  }
  finally { if (version === requestVersion) loading.value = false; }
}
watch(selectedServer, () => {
  inventory.value = null;
  statusFilter.value = "";
  search.value = "";
  void refresh();
});
onMounted(async () => {
  try {
    const result = await fetchProvisioningServers();
    if (disposed) return;
    servers.value = result;
    const requested = typeof route.query.server === "string" ? route.query.server : "";
    selectedServer.value = servers.value.some(server => server.Id === requested) ? requested : servers.value[0]?.Id ?? "";
    timer = window.setInterval(() => { now.value = Date.now(); if (!loading.value) void refresh(); }, 15000);
  }
  catch (cause) { error.value = cause instanceof Error ? cause.message : "讀取伺服器清單失敗。"; }
});
onBeforeUnmount(() => { disposed = true; ++requestVersion; if (timer !== undefined) window.clearInterval(timer); });
</script>

<template>
  <section class="page-heading">
    <div><h1>TLS 憑證管理</h1><p>查詢伺服器本機 My／WebHosting 存放區的憑證與 IIS HTTPS 綁定。</p></div>
    <button class="ui-button" type="button" :disabled="loading || !selectedServer" @click="refresh">重新整理</button>
  </section>
  <section class="data-card tls-toolbar">
    <label><span>伺服器</span><select v-model="selectedServer"><option v-if="!servers.length" value="">沒有可用伺服器</option><option v-for="server in servers" :key="server.Id" :value="server.Id">{{ server.DisplayName }}（{{ server.Id }}）</option></select></label>
    <div v-if="inventory" class="snapshot-meta">
      <span>{{ inventory.IsOnline ? "Worker 在線" : "Worker 離線" }}</span>
      <span>憑證採集：{{ formatTime(inventory.Snapshot?.CollectedAtUtc) }}</span>
      <span>最近回報：{{ formatTime(inventory.LastSeenAtUtc) }}</span>
    </div>
  </section>
  <p v-if="error" class="alert alert-error" role="alert">{{ error }}</p>
  <p v-if="inventory && !inventory.IsOnline" class="alert alert-warning">Worker 離線，以下為最後回報資料。</p>
  <p v-if="inventory?.Snapshot?.Error" class="alert alert-error" role="alert">採集異常：{{ inventory.Snapshot.Error }}。保留的資料可能不是最新狀態。</p>
  <p v-if="stale" class="alert alert-warning">憑證快照已超過 26 小時，請確認 Worker 的每日採集設定與連線。</p>
  <section class="data-card tls-list">
    <header>
      <div><h2>憑證清單</h2><p>共 {{ certificates.length }} 筆 · 30 天內到期 {{ expiringCount }} 筆 · 已到期 {{ expiredCount }} 筆</p></div>
      <div class="filters"><input v-model="search" aria-label="搜尋憑證" placeholder="搜尋網域、網站、發行者、指紋"><select v-model="statusFilter" aria-label="篩選狀態"><option value="">所有狀態</option><option v-for="state in ['有效期內', '即將到期', '已到期', '尚未生效', '無法確認']" :key="state">{{ state }}</option></select></div>
    </header>
    <div class="table-wrap"><table>
      <thead><tr><th>憑證／網域</th><th>IIS 綁定</th><th>狀態</th><th>生效時間</th><th>到期時間</th><th>剩餘時間</th><th>發行者／存放區</th></tr></thead>
      <tbody>
        <tr v-for="certificate in visibleCertificates" :key="`${certificate.StoreName}:${certificate.Thumbprint}:${certificate.IisBindings.join('|')}`">
          <td><strong>{{ certificate.Subject || '無法讀取憑證' }}</strong><small>{{ certificate.DnsNames.join('、') || '—' }}</small><small class="thumbprint">{{ certificate.Thumbprint || '未指定指紋' }}</small></td>
          <td><div v-for="binding in certificate.IisBindings" :key="binding">{{ binding }}</div><span v-if="!certificate.IisBindings.length">未綁定 IIS</span></td>
          <td><span class="certificate-status" :class="{ danger: ['已到期', '無法確認'].includes(status(certificate)), warning: ['即將到期', '尚未生效'].includes(status(certificate)) }">{{ status(certificate) }}</span><small v-if="certificate.Error">{{ certificate.Error }}</small><small v-else-if="!certificate.HasPrivateKey">本機沒有私鑰</small></td>
          <td>{{ formatTime(certificate.NotBeforeUtc) }}</td><td>{{ formatTime(certificate.NotAfterUtc) }}</td><td>{{ remaining(certificate) }}</td><td>{{ certificate.Issuer || '—' }}<small>{{ certificate.StoreName }}</small></td>
        </tr>
        <tr v-if="!visibleCertificates.length"><td colspan="7">{{ loading ? '載入中…' : !inventory?.Snapshot ? '尚未收到 TLS 資料，請確認 Worker 已更新並回報。' : inventory.Snapshot.Error && !inventory.Snapshot.CollectedAtUtc ? '採集失敗，尚無可用憑證資料。' : !certificates.length ? '此次快照沒有憑證。' : '沒有符合條件的憑證。' }}</td></tr>
      </tbody>
    </table></div>
    <p class="scope-note">顯示 Worker 每日採集的本機快照，預設於伺服器當地時間凌晨 03:00 更新；重新整理只讀取最近回報。Worker 啟動時若缺少快照或錯過最近排定時段，會補採集一次。有效期內不代表憑證鏈、網域或對外 HTTPS 驗證通過。此頁尚未提供 win-acme 續期排程與失敗通知。</p>
  </section>
</template>

<style scoped>
.tls-toolbar, .tls-list { min-height: 0; margin-bottom: 1rem; }
.tls-toolbar { display: flex; flex-wrap: wrap; align-items: end; gap: 1rem; }
label { display: grid; gap: .4rem; min-width: 260px; }
input, select { min-height: 38px; padding: .5rem .7rem; border: 1px solid #cfd6e2; border-radius: 6px; background: #fff; font: inherit; }
.snapshot-meta { display: flex; flex-wrap: wrap; gap: .5rem 1rem; color: #687386; font-size: .85rem; }
header { display: flex; flex-wrap: wrap; justify-content: space-between; align-items: center; gap: 1rem; margin-bottom: 1rem; }
h2 { margin: 0; font-size: 1.1rem; } header p, .scope-note { color: #687386; font-size: .85rem; }
.filters { display: flex; flex-wrap: wrap; gap: .6rem; }
.table-wrap { overflow-x: auto; } table { width: 100%; border-collapse: collapse; font-size: .86rem; }
th, td { padding: .75rem; text-align: left; vertical-align: top; border-bottom: 1px solid #e8ebf0; min-width: 100px; }
td small { display: block; margin-top: .3rem; color: #687386; } .thumbprint { overflow-wrap: anywhere; }
.certificate-status { display: inline-block; padding: .2rem .5rem; border-radius: 6px; background: #d9f4e5; color: #17633a; white-space: nowrap; }
.certificate-status.danger { background: #fde2e0; color: #9d241d; } .certificate-status.warning { background: #fff2cc; color: #8a6100; }
.scope-note { margin: 1rem 0 0; line-height: 1.6; }
@media (max-width: 600px) { label, .filters, .filters input, .filters select { width: 100%; min-width: 0; } }
</style>
