<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";
import { DxColumn, DxDataGrid, DxPaging, DxPager, DxSearchPanel, DxFilterRow, DxSorting, DxColumnChooser } from "devextreme-vue/data-grid";
import { useRoute } from "vue-router";
import { fetchProvisioningServers, fetchServerTlsStatus } from "@/services/provisioning-api";
import type { ProvisioningServer, ServerTlsStatus } from "@/types/provisioning";

const route = useRoute();
const servers = ref<ProvisioningServer[]>([]);
const selectedServer = ref("");
const inventory = ref<ServerTlsStatus | null>(null);
const loading = ref(false);
const error = ref("");


const now = ref(Date.now());
let timer: number | undefined;
let requestVersion = 0;
let disposed = false;
const websites = computed(() => inventory.value?.Snapshot?.Websites ?? []);
const centralStore = computed(() => inventory.value?.Snapshot?.CentralStore);
const wacsLogs = computed(() => inventory.value?.Snapshot?.WacsLogs);
function executionSites(taskName: string): { names: string[]; basis: string } {
  // Only explicit site names and exact host tokens are usable. Never expand "(+N other)" or "any site".
  const siteLabel = taskName.match(/^\[IIS(?:Site)?\]\s*([^,]+)(?:,|$)/i)?.[1]?.trim();
  if (siteLabel && !/\(any site\)|\(\+\d+ other/i.test(siteLabel)) {
    const site = websites.value.find(value => value.SiteName.toLowerCase() === siteLabel.toLowerCase());
    if (site) return { names: [site.SiteName], basis: '日誌任務中的 IIS 網站名稱' };
  }
  const tokens = (taskName.toLowerCase().match(/[a-z0-9_-]+(?:\.[a-z0-9_-]+)+/g) ?? [])
    .map(value => value.replace(/\.$/, ''));
  const names = websites.value.filter(site => site.HttpsUrls.some(url => {
    try { return tokens.includes(new URL(url).hostname.toLowerCase()); }
    catch { return false; }
  })).map(site => site.SiteName);
  return { names, basis: names.length ? '日誌任務網域符合網站 HTTPS 主機名稱' : '無明確網站或網域對應' };
}
const wacsExecutions = computed(() => (wacsLogs.value?.Executions ?? []).map(record => {
  const match = executionSites(record.TaskName);
  return {
    ...record,
    StartedAt: record.StartedAtUtc ? new Date(record.StartedAtUtc) : null,
    CompletedAt: record.CompletedAtUtc ? new Date(record.CompletedAtUtc) : null,
    SourceLines: `${record.StartLine}–${record.EndLine}`,
    WebsiteNames: match.names,
    WebsiteMatch: match.names.join('、') || '未對應網站',
    MatchBasis: match.basis
  };
}));
type PfxInfo = NonNullable<NonNullable<ServerTlsStatus['Snapshot']>['CentralStore']>['Certificates'];
interface WebsiteRow {
  Id: string; Website: string; IisState: string; Urls: string; Files: string;
  Subject: string; Issuer: string; Thumbprint: string; Status: string; Match: string;
  NotBefore: Date | null; NotAfter: Date | null; RemainingDays: number | null; Error: string;
}
const gridRows = computed(() => rows.value.map(row => {
  const records = wacsExecutions.value.filter(record => record.WebsiteNames.includes(row.Website))
    .sort((left, right) => {
      const time = (record: typeof left) => (record.CompletedAt ?? record.StartedAt)?.getTime() ?? -Infinity;
      const leftTime = time(left);
      const rightTime = time(right);
      return (leftTime === rightTime ? 0 : rightTime > leftTime ? 1 : -1)
        || right.FileName.localeCompare(left.FileName) || right.EndLine - left.EndLine;
    });
  const latest = records[0];
  return {
    ...row,
    RenewalResult: latest?.Result ?? (wacsLogs.value?.Error ? '日誌讀取失敗'
      : !wacsLogs.value?.Executions ? '尚無日誌解析資料' : '沒有可對應紀錄'),
    RenewalTime: latest?.CompletedAt ?? latest?.StartedAt ?? null,
    RenewalDetails: latest?.Details ?? '',
    RenewalTask: latest?.TaskName ?? '',
    RenewalSource: latest ? `${latest.FileName}:${latest.SourceLines}` : '',
    RenewalMatch: latest?.MatchBasis ?? '未對應'
  };
}));
const unmatchedCount = computed(() => wacsExecutions.value.filter(record => !record.WebsiteNames.length).length);
const rows = computed(() => {
  const result: WebsiteRow[] = [];
  const certificates: NonNullable<PfxInfo> = centralStore.value?.Certificates ?? [];
  for (const site of websites.value) {
    const groups = new Map<string, WebsiteRow>();
    for (const binding of site.HttpsBindings) {
      const separator = binding.lastIndexOf(':');
      const host = binding.slice(separator + 1).toLowerCase().replace(/\.$/, '');
      const port = binding.slice(0, separator).split(':').at(-1);
      const url = host ? `https://${host}${port === '443' ? '' : ':' + port}` : binding;
      const exactName = host + '.pfx';
      const wildcardName = host.includes('.') ? '_' + host.slice(host.indexOf('.')) + '.pfx' : '';
      const names = centralStore.value?.PfxFileNames ?? [];
      const file = names.find(name => name.toLowerCase() === exactName)
        ?? names.find(name => wildcardName && name.toLowerCase() === wildcardName);
      const candidates = host && file ? certificates.filter(item => item.FileName === file) : [];
      const items = candidates.length ? candidates : [null];
      for (const certificate of items) {
        const key = certificate?.Thumbprint ?? certificate?.FileName ?? file ?? binding;
        const before = certificate?.NotBeforeUtc ? new Date(certificate.NotBeforeUtc) : null;
        const after = certificate?.NotAfterUtc ? new Date(certificate.NotAfterUtc) : null;
        const message = certificate?.Error ?? (!host ? '綁定未指定主機名稱' : !centralStore.value ? '尚無集中式憑證資料'
          : centralStore.value.Error ?? (!file ? '沒有符合檔名的 PFX；可能使用本機憑證' : !candidates.length ? '尚未收到 PFX 解析結果' : ''));
        const row = groups.get(key);
        if (row) {
          if (!row.Urls.split('\n').includes(url)) row.Urls += '\n' + url;
          if (file && !row.Files.split('\n').includes(file)) row.Files += '\n' + file;
          continue;
        }
        groups.set(key, {
          Id: JSON.stringify([site.SiteName, key]), Website: site.SiteName, IisState: site.State,
          Urls: url, Files: file ?? '—', Subject: certificate?.Subject ?? '—',
          Issuer: certificate?.Issuer ?? '—', Thumbprint: certificate?.Thumbprint ?? '',
          NotBefore: before, NotAfter: after,
          RemainingDays: after ? Math.ceil((after.getTime() - now.value) / 86400000) : null,
          Status: message || !after || !before ? '無法確認' : after.getTime() <= now.value ? '已到期'
            : before.getTime() > now.value ? '尚未生效' : after.getTime() - now.value <= 30 * 86400000 ? '30 天內到期' : '有效期內',
          Match: file ? '依 CCS 檔名推定，尚未確認實際綁定' : '未對應', Error: message
        });
      }
    }
    if (!site.HttpsBindings.length) groups.set('http-only', {
      Id: JSON.stringify([site.SiteName, 'http-only']), Website: site.SiteName, IisState: site.State,
      Urls: '—', Files: '—', Subject: '—', Issuer: '—', Thumbprint: '', Status: '沒有 HTTPS',
      Match: '不適用', Error: '', NotBefore: null, NotAfter: null, RemainingDays: null
    });
    result.push(...groups.values());
  }
  return result;
});
const stale = computed(() => {
  const collected = inventory.value?.Snapshot?.CollectedAtUtc;
  return !!collected && now.value - Date.parse(collected) > 26 * 60 * 60 * 1000;
});
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
    <div><h1>TLS 憑證管理</h1><p>查詢本系統對應的 IIS 網站、HTTPS 網址與集中式憑證有效期。</p></div>
    <button class="ui-button" type="button" :disabled="loading || !selectedServer" @click="refresh">重新整理</button>
  </section>
  <section class="data-card tls-toolbar">
    <label><span>伺服器</span><select v-model="selectedServer"><option v-if="!servers.length" value="">沒有可用伺服器</option><option v-for="server in servers" :key="server.Id" :value="server.Id">{{ server.DisplayName }}（{{ server.Id }}）</option></select></label>
    <div v-if="inventory" class="snapshot-meta">
      <span>{{ inventory.IsOnline ? 'Worker 在線' : 'Worker 離線' }}</span>
      <span>採集：{{ formatTime(inventory.Snapshot?.CollectedAtUtc) }}</span>
      <span>{{ websites.length }} 個網站 · {{ centralStore?.PfxFileNames.length ?? 0 }} 個 PFX</span>
    </div>
  </section>
  <p v-if="error" class="alert alert-error" role="alert">{{ error }}</p>
  <p v-if="inventory && !inventory.IsOnline" class="alert alert-warning">Worker 離線，以下為最後回報資料。</p>
  <p v-if="inventory?.Snapshot?.Error" class="alert alert-error" role="alert">{{ inventory.Snapshot.Error }}</p>
  <p v-if="centralStore?.Error" class="alert alert-error" role="alert">{{ centralStore.Error }}</p>
  <p v-if="wacsLogs?.Error" class="alert alert-error" role="alert">{{ wacsLogs.Error }}</p>
  <p v-if="stale" class="alert alert-warning">快照已超過 26 小時，請確認 Worker 採集設定與連線。</p>
  <section class="data-card tls-list">
    <DxDataGrid :key="selectedServer" :data-source="gridRows" key-expr="Id" :show-borders="true"
      :column-auto-width="true" :allow-column-resizing="true" :allow-column-reordering="true"
      :word-wrap-enabled="true" :hover-state-enabled="true" no-data-text="沒有符合本系統綁定的 HTTPS 網站，請確認網站網址設定與 Worker 回報。">
      <DxSearchPanel :visible="true" :width="280" placeholder="搜尋網站、網址、憑證、續期結果" />
      <DxFilterRow :visible="true" />
      <DxSorting mode="multiple" />
      <DxPaging :page-size="10" />
      <DxPager :visible="true" :show-info="true" :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" />
      <DxColumnChooser :enabled="true" mode="select" />
      <DxColumn data-field="Website" caption="網站" :min-width="140" sort-order="asc" />
      <DxColumn data-field="IisState" caption="IIS 狀態" :width="100" />
      <DxColumn data-field="Urls" caption="HTTPS 網址" :min-width="250" cell-template="multiline" />
      <DxColumn data-field="Status" caption="憑證狀態" :width="120" />
      <DxColumn data-field="NotAfter" caption="到期時間" data-type="datetime" format="yyyy/MM/dd HH:mm" :width="160" />
      <DxColumn data-field="RemainingDays" caption="剩餘天數" data-type="number" :width="100" />
      <DxColumn data-field="RenewalResult" caption="最近續期結果" :width="150" />
      <DxColumn data-field="RenewalTime" caption="續期紀錄時間" data-type="datetime" format="yyyy/MM/dd HH:mm" :width="160" />
      <DxColumn data-field="RenewalDetails" caption="續期訊息" :min-width="220" cell-template="message" />
      <DxColumn data-field="Error" caption="憑證讀取說明" :min-width="180" cell-template="message" />
      <DxColumn data-field="Files" caption="PFX 檔名" :visible="false" cell-template="multiline" />
      <DxColumn data-field="Match" caption="憑證對應依據" :visible="false" />
      <DxColumn data-field="RenewalTask" caption="續期任務" :visible="false" />
      <DxColumn data-field="RenewalSource" caption="續期來源日誌／行號" :visible="false" />
      <DxColumn data-field="RenewalMatch" caption="續期對應依據" :visible="false" />
      <DxColumn data-field="Subject" caption="憑證主體" :visible="false" />
      <DxColumn data-field="Issuer" caption="發行者" :visible="false" />
      <DxColumn data-field="Thumbprint" caption="憑證指紋" :visible="false" />
      <DxColumn data-field="NotBefore" caption="生效時間" data-type="datetime" format="yyyy/MM/dd HH:mm" :visible="false" />
      <template #multiline="{ data }"><span class="multiline">{{ data.value }}</span></template>
      <template #message="{ data }"><span class="message" :title="data.value || ''">{{ data.value || '—' }}</span></template>
    </DxDataGrid>
    <p class="scope-note">同網站中具有相同憑證指紋的網址合併顯示；不同憑證分列。PFX 依主機名稱或萬用字元檔名推定，尚未讀取 IIS 的集中式憑證綁定旗標，因此不代表網站實際使用此憑證。沒有對應或解析失敗顯示「無法確認」。</p>
    <p class="scope-note">最近續期結果依日誌中的明確網站名稱或網域對應，屬網站層級紀錄，不代表此張憑證安裝驗證。僅涵蓋最近讀取的日誌；沒有可對應紀錄不代表從未執行。「any site」或「+N other」不推測其他網站，未對應內容保留於下方詳細紀錄。</p>
    <details><summary>採集與診斷資訊</summary>
      <p>集中式憑證：{{ centralStore?.Enabled === true ? '已啟用' : centralStore?.Enabled === false ? '未啟用' : '無法確認' }} · 目錄：{{ centralStore?.DirectoryPath || '—' }}</p>
      <p>已加入 win-acme 本機日誌讀取；不啟動 win-acme、PowerShell 或讀取事件檢視器。每次 Worker 啟動採集一次，之後每日採集；重新整理只讀最近回報。</p>
    </details>
  </section>
  <section class="data-card tls-list">
    <details><summary>win-acme 詳細紀錄與未對應任務（{{ unmatchedCount }} 筆未對應）</summary>
    <p v-if="!wacsLogs">尚未收到第四步資料，請更新並啟動 Worker。</p>
    <template v-else>
      <p>採集：{{ formatTime(wacsLogs.CollectedAtUtc) }} · 目錄：{{ wacsLogs.DirectoryPath }}</p>
      <p v-if="wacsLogs.Error" class="alert alert-error">{{ wacsLogs.Error }}</p>
      <details v-else><summary>已讀取 {{ wacsLogs.Files.length }} 個日誌（展開檢視）</summary>
        <DxDataGrid :data-source="wacsLogs.Files" key-expr="FileName" :show-borders="true" :column-auto-width="true" :word-wrap-enabled="true" no-data-text="目錄內沒有 .log 或 .txt 日誌。">
          <DxColumn data-field="FileName" caption="日誌檔名" />
          <DxColumn data-field="LastWriteAtUtc" caption="檔案最後修改時間" data-type="datetime" format="yyyy/MM/dd HH:mm" />
          <DxColumn data-field="ErrorEntries" caption="錯誤標記筆數" data-type="number" />
          <DxColumn data-field="WarningEntries" caption="警告標記筆數" data-type="number" />
          <DxColumn data-field="Error" caption="讀取錯誤" />
        </DxDataGrid>
      </details>
      <p class="scope-note">僅讀最近修改的 7 個日誌，各檔上限 5 MB。標記筆數不等於網站安裝結果；最後修改時間不等於執行時間。</p>
      <h3>win-acme 執行結果</h3>
      <p v-if="!wacsLogs.Executions">尚未收到日誌內容解析結果，請更新 Worker 與 Platform 後端。</p>
      <DxDataGrid v-else :key="`wacs-${selectedServer}`" :data-source="wacsExecutions" key-expr="Id"
        :show-borders="true" :column-auto-width="true" :word-wrap-enabled="true" :allow-column-resizing="true"
        no-data-text="此次日誌沒有可辨識的續期任務或錯誤／警告紀錄；不代表執行成功。">
        <DxSearchPanel :visible="true" :width="280" placeholder="搜尋任務、結果、原因、檔名" />
        <DxFilterRow :visible="true" /><DxSorting mode="multiple" />
        <DxPaging :page-size="10" />
        <DxPager :visible="true" :show-info="true" :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" />
        <DxColumn data-field="StartedAt" caption="紀錄開始時間" data-type="datetime" format="yyyy/MM/dd HH:mm:ss" sort-order="desc" />
        <DxColumn data-field="CompletedAt" caption="任務結束時間" data-type="datetime" format="yyyy/MM/dd HH:mm:ss" />
        <DxColumn data-field="TaskName" caption="續期任務／網域" :min-width="200" />
        <DxColumn data-field="Result" caption="結果" :width="140" />
        <DxColumn data-field="Details" caption="區段訊息／錯誤摘要" :min-width="300" />
        <DxColumn data-field="WebsiteMatch" caption="網站對應" :width="130" />
        <DxColumn data-field="MatchBasis" caption="對應依據" />
        <DxColumn data-field="FileName" caption="來源日誌" />
        <DxColumn data-field="SourceLines" caption="來源行號" :width="100" />
      </DxDataGrid>
      <p class="scope-note">每檔最多保留最後 300 筆。只有明確的續期結束訊息才判定成功或失敗；一般錯誤與不完整紀錄標示「無法確認」。缺少時間時留空；未附時區依 Worker 當地時區解讀。敏感內容遮蔽；尚未提供續期排程與失敗通知。</p>
    </template>
    </details>
  </section>
</template>

<style scoped>
.tls-toolbar, .tls-list { min-height: 0; margin-bottom: 1rem; }
.tls-toolbar { display: flex; flex-wrap: wrap; align-items: end; gap: 1rem; }
label { display: grid; gap: .4rem; min-width: 260px; }
select { min-height: 38px; padding: .5rem .7rem; border: 1px solid #cfd6e2; border-radius: 6px; background: #fff; font: inherit; }
.snapshot-meta { display: flex; flex-wrap: wrap; gap: .5rem 1rem; color: #687386; font-size: .85rem; }
.multiline { white-space: pre-line; overflow-wrap: anywhere; }
.message { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; overflow-wrap: anywhere; }
.scope-note, details { margin-top: 1rem; color: #687386; font-size: .85rem; line-height: 1.6; }
@media (max-width: 600px) { label { width: 100%; min-width: 0; } }
</style>
