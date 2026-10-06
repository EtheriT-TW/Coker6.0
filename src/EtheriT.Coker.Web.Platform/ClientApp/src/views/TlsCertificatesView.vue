<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";
import { DxColumn, DxDataGrid, DxPaging, DxPager, DxSearchPanel, DxFilterRow, DxSorting, DxColumnChooser } from "devextreme-vue/data-grid";
import { installWebsiteTls } from '@/services/site-tls';
import { getPlatformContext } from '@/services/platform-context';
import { useRoute } from "vue-router";
import { fetchProvisioningServers, fetchServerTlsStatus } from "@/services/provisioning-api";
import type { ProvisioningServer, ServerTlsStatus } from "@/types/provisioning";

const canControlServers = ref(false);
const installingSite = ref('');
async function installTls(siteName: string): Promise<void> {
  if (installingSite.value || !inventory.value?.IsOnline) return;
  const serverId = selectedServer.value;
  const site = websites.value.find(item => item.SiteName === siteName);
  const hosts = site?.HostNames ?? (site?.HttpsUrls ?? []).flatMap(url => {
    try { return [new URL(url).hostname]; } catch { return []; }
  });
  installingSite.value = siteName;
  try { await installWebsiteTls(serverId, siteName, hosts); }
  catch (cause) { error.value = cause instanceof Error ? cause.message : '無法送出 TLS 任務。'; }
  finally { installingSite.value = ''; }
}
const route = useRoute();
const servers = ref<ProvisioningServer[]>([]);
const selectedServer = ref("");
const inventory = ref<ServerTlsStatus | null>(null);
const loading = ref(false);
const error = ref("");
type SummaryFilter = 'all' | 'attention' | 'valid' | 'expiring' | 'expired' | 'renewalFailed' | 'uncertain' | 'renewalMissing';
const selectedFilter = ref<SummaryFilter>('all');
const filterLabels: Record<SummaryFilter, string> = {
  all: '全部網站', attention: '需處理網站', valid: '憑證有效期正常的網站',
  expiring: '憑證即將到期的網站', expired: '憑證已到期的網站',
  renewalFailed: '自動更新失敗網站', uncertain: '憑證檢查異常網站', renewalMissing: '未發現 renewal 的網站'
};
function toggleSummaryFilter(filter: SummaryFilter): void {
  selectedFilter.value = selectedFilter.value === filter ? 'all' : filter;
}


const now = ref(Date.now());
let timer: number | undefined;
let requestVersion = 0;
let disposed = false;
const websites = computed(() => (inventory.value?.Snapshot?.Websites ?? []).filter(site => site.State.toLowerCase() !== 'stopped'));
const urlIssues = computed(() => (inventory.value?.UrlIssues ?? []).map((issue, index) => ({
  ...issue, Id: index,
  EscapedUrl: JSON.stringify(issue.OriginalUrl),
  CharacterHints: [...issue.OriginalUrl].flatMap((character, position) => {
    const code = character.codePointAt(0)!;
    return code <= 32 || code >= 127
      ? [`第 ${position + 1} 字元 ${JSON.stringify(character)}（U+${code.toString(16).toUpperCase().padStart(4, '0')}）`] : [];
  }).join('、') || '未發現空白、控制或非 ASCII 字元；請檢查網址格式。'
})));
const centralStore = computed(() => inventory.value?.Snapshot?.CentralStore);
const wacsLogs = computed(() => inventory.value?.Snapshot?.WacsLogs);
const wacsSchedule = computed(() => inventory.value?.Snapshot?.WacsSchedule);
const scheduleRows = computed(() => (wacsSchedule.value?.Tasks ?? []).map(task => ({
  ...task,
  NextRun: task.NextRunAtUtc ? new Date(task.NextRunAtUtc) : null,
  LastRun: task.LastRunAtUtc ? new Date(task.LastRunAtUtc) : null,
  ResultCode: `0x${(task.LastResult >>> 0).toString(16).padStart(8, '0').toUpperCase()}`
})));
const nextWacsRun = computed(() => scheduleRows.value.filter(task => task.Enabled && task.NextRun)
  .sort((left, right) => left.NextRun!.getTime() - right.NextRun!.getTime())[0]?.NextRun?.toISOString());
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
function renewalExplanation(details: string, failed: boolean): string {
  if (/no bindings matched|did not generate a source/i.test(details)) return '更新程式找不到符合設定的網站網址。請管理員修正網站與自動更新設定。';
  if (/access (?:is )?denied|unauthorizedaccess|permission denied/i.test(details)) return '更新程式沒有讀取或寫入所需資料的權限。請管理員檢查服務帳號權限。';
  if (/NXDOMAIN|no such host|DNS problem/i.test(details)) return '網站網址的名稱解析失敗。請管理員檢查網址與網域設定。';
  if (/rate limit|too many requests/i.test(details)) return '憑證申請次數已達限制。請管理員確認允許再次申請的時間。';
  if (/timed? ?out|timeout/i.test(details)) return '更新時連線逾時。請管理員檢查網路與網站連線。';
  if (/connection refused/i.test(details)) return '更新時連線遭拒絕。請管理員檢查網站服務與防火牆設定。';
  if (/password.*incorrect|incorrect.*password/i.test(details)) return '憑證檔案密碼不符。請管理員確認憑證密碼設定。';
  return failed ? '已記錄自動更新失敗，但紀錄未提供可辨識的原因。請通知管理員處理此網站的憑證更新。'
    : '自動更新已完成，但同次作業仍有錯誤紀錄。請通知管理員確認此網站的更新結果。';
}
function certificateExplanation(row: WebsiteRow): string {
  if (/沒有符合檔名/.test(row.Error)) return '沒有找到此網址對應的憑證檔案，無法取得到期日。請管理員確認網站使用的憑證位置。';
  if (/尚未收到|尚無/.test(row.Error)) return '尚未取得此網站的憑證資料。請管理員確認監測服務已完成檢查。';
  if (/password|密碼/i.test(row.Error)) return '讀取憑證時密碼驗證失敗。請管理員確認監測服務的憑證密碼。';
  if (/denied|permission|權限/i.test(row.Error)) return '監測服務無權讀取憑證。請管理員調整憑證目錄的讀取權限。';
  return '憑證資料讀取不完整，無法取得到期日。請通知管理員檢查憑證檔案與監測設定。';
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
  const recordTime = latest?.CompletedAt ?? latest?.StartedAt;
  const historical = !!recordTime && !!row.NotBefore && recordTime.getTime() < row.NotBefore.getTime();
  // Site-level logs are not proof that the currently loaded certificate failed to install.
  const current = historical ? undefined : latest;
  const renewalMissing = row.Status !== '沒有 HTTPS' && websites.value.find(site => site.SiteName === row.Website)?.RenewalDetected === false;
  const attention = [
    ...(renewalMissing ? ['未設定續期'] : []),
    ...(['已到期', '30 天內到期', '尚未生效'].includes(row.Status) ? [row.Status] : []),
    ...(row.Status === '無法確認' ? ['憑證待確認'] : []),
    ...(current?.Result === '失敗' ? ['自動更新作業失敗'] : []),
    ...(current?.Result === '成功但有錯誤' ? ['更新完成但有異常'] : []),
    ...(current?.Result === '無法確認' ? ['更新紀錄不完整'] : [])
  ];
  const explanations: string[] = [];
  const renewalAdvice = websites.value.find(site => site.SiteName === row.Website)?.RenewalAdvice;
  if (renewalAdvice) explanations.push(renewalAdvice);
  if (row.Status === '已到期') explanations.push('憑證已到期，訪客可能看到安全警告。請立即通知管理員更換憑證。');
  if (row.Status === '30 天內到期') explanations.push(`憑證將在 ${row.RemainingDays} 天內到期。請管理員確認自動更新能在到期前完成。`);
  if (row.Status === '尚未生效') explanations.push('憑證尚未開始生效。請管理員確認生效日期與伺服器時間。');
  if (row.Status === '無法確認') explanations.push(certificateExplanation(row));
  if (current?.Result === '失敗' || current?.Result === '成功但有錯誤') {
    explanations.push(`更新作業紀錄：${formatTime(recordTime?.toISOString())}。${renewalExplanation(current.Details, current.Result === '失敗')}`);
    if (row.Status === '有效期內') explanations.push('目前憑證有效期仍超過 30 天；此為網站更新作業異常，不代表目前憑證到期或安裝失敗。');
  }
  if (current?.Result === '無法確認') explanations.push('最近一次更新紀錄缺少完成結果，無法確認更新是否結束。請通知管理員確認更新作業。');
  if (!explanations.length) explanations.push(row.Status === '沒有 HTTPS' ? '網站尚未設定安全連線。' : '目前讀取的憑證有效期超過 30 天，暫無到期提醒。');
  if (historical && latest?.Result !== '成功') explanations.push(`歷史更新紀錄（${formatTime(recordTime?.toISOString())}）早於目前憑證生效時間，未列入目前更新異常。`);
  return {
    ...row,
    RenewalMissing: renewalMissing,
    Attention: attention.join('、'),
    Overview: row.Status === '已到期' ? '已到期' : current?.Result === '失敗' ? '更新作業異常'
      : row.Status === '無法確認' ? '檢查異常' : row.Status === '30 天內到期' ? '即將到期'
      : row.Status === '尚未生效' ? '尚未生效' : current?.Result === '成功但有錯誤' ? '更新有異常'
      : current?.Result === '無法確認' ? '更新需確認'
      : renewalMissing ? '未設定續期' : row.Status === '沒有 HTTPS' ? '未設定安全連線' : '有效期正常',
    Explanation: explanations.join('\n'),
    SiteState: row.IisState === 'Started' ? '運作中' : row.IisState === 'Stopped' ? '已停止' : row.IisState,
    RenewalResult: historical ? `歷史紀錄：${latest?.Result}` : latest?.Result ?? (wacsLogs.value?.Error ? '日誌讀取失敗'
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
const websiteCategories = computed(() => {
  const sitesWithStatus = (status: string) => new Set(gridRows.value.filter(row => row.Status === status)
    .map(row => row.Website));
  const validWebsites = new Set(gridRows.value.filter(row => row.Status === '有效期內').map(row => row.Website));
  for (const row of gridRows.value) {
    if (row.Status !== '有效期內') validWebsites.delete(row.Website);
  }
  return {
    valid: validWebsites,
    expired: sitesWithStatus('已到期'),
    expiring: sitesWithStatus('30 天內到期'),
    uncertain: sitesWithStatus('無法確認'),
    renewalMissing: new Set(gridRows.value.filter(row => row.RenewalMissing).map(row => row.Website)),
    renewalFailed: new Set(gridRows.value.filter(row => row.RenewalResult === '失敗').map(row => row.Website)),
    attention: new Set(gridRows.value.filter(row => row.Attention).map(row => row.Website))
  };
});
const visibleRows = computed(() => selectedFilter.value === 'all' ? gridRows.value
  : gridRows.value.filter(row => websiteCategories.value[selectedFilter.value as Exclude<SummaryFilter, 'all'>].has(row.Website)));
const summary = computed(() => ({
    valid: websiteCategories.value.valid.size,
    expired: websiteCategories.value.expired.size,
    expiring: websiteCategories.value.expiring.size,
    uncertain: websiteCategories.value.uncertain.size,
    renewalMissing: websiteCategories.value.renewalMissing.size,
    renewalFailed: websiteCategories.value.renewalFailed.size,
    attention: websiteCategories.value.attention.size,
    scheduleNonzero: scheduleRows.value.filter(task => task.LastRun && task.LastResult !== 0 && task.State !== '執行中').length
}));
const serviceNotice = computed(() => {
  if (inventory.value && !inventory.value.IsOnline) return '監測服務目前未連線，顯示的是先前的檢查結果。請通知管理員恢復監測服務。';
  if (inventory.value?.Snapshot?.Error) return '網站清單檢查失敗，部分網站可能未顯示。請通知管理員修復監測服務。';
  if (centralStore.value?.Error) return '憑證資料檢查失敗，到期資訊可能不完整。請通知管理員檢查憑證目錄與讀取權限。';
  if (wacsLogs.value?.Error) return '無法讀取自動更新紀錄，目前不能判斷更新是否完成。請通知管理員檢查紀錄目錄與讀取權限。';
  if (wacsLogs.value?.Files.some(file => file.Error)) return '部分自動更新紀錄讀取失敗，更新資訊可能不完整。請通知管理員修復紀錄讀取問題。';
  if (wacsSchedule.value?.Error) return '無法取得下次自動檢查時間。請通知管理員檢查自動更新任務與讀取權限。';
  if (stale.value) return '檢查結果超過一天未更新。請通知管理員確認監測服務，勿以舊資料判定網站正常。';
  if (summary.value.scheduleNonzero) return `自動更新作業上次回報異常狀態。${summary.value.renewalFailed ? `已辨識 ${summary.value.renewalFailed} 個網站更新失敗，原因與處理建議見下方列表。` : '目前紀錄未指出受影響的本系統網站，請通知管理員檢查自動更新作業。'}`;
  if (wacsSchedule.value && !wacsSchedule.value.Tasks.some(task => task.Enabled)) return '沒有找到已啟用的自動更新作業。請管理員確認憑證是否已設定自動更新。';
  return '';
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
  selectedFilter.value = 'all';


  void refresh();
});
onMounted(async () => {
  try {
    const [result, context] = await Promise.all([fetchProvisioningServers(), getPlatformContext()]);
    canControlServers.value = context.CanControlServers;
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
    <div><h1>網站憑證監測</h1><p>查看哪些網站憑證即將到期、更新失敗，以及需要處理的原因。</p></div>
    <button class="ui-button" type="button" :disabled="loading || !selectedServer" @click="refresh">重新整理</button>
  </section>
  <section class="data-card tls-toolbar">
    <label><span>伺服器</span><select v-model="selectedServer"><option v-if="!servers.length" value="">沒有可用伺服器</option><option v-for="server in servers" :key="server.Id" :value="server.Id">{{ server.DisplayName }}（{{ server.Id }}）</option></select></label>
    <div v-if="inventory" class="snapshot-meta">
      <span>{{ inventory.IsOnline ? '監測服務已連線' : '監測服務未連線' }}</span>
      <span>最近檢查：{{ formatTime(inventory.Snapshot?.CollectedAtUtc) }}</span>
      <span>{{ websites.length }} 個網站</span>
      <span>下次自動更新檢查：{{ formatTime(nextWacsRun) }}</span>
    </div>
  </section>
  <p v-if="error" class="alert alert-error" role="alert">{{ error }}</p>
  <p v-if="serviceNotice" class="alert alert-warning operator-notice" role="alert">{{ serviceNotice }}</p>
  <section v-if="urlIssues.length" class="data-card tls-list">
    <h3>有 {{ urlIssues.length }} 筆網址無法比對，需手動確認</h3>
    <p class="scope-note">這些網址未能完成監控比對，請管理員核對原始設定。系統網站設定無法判定所屬伺服器時會另外標示；伺服器回報僅列未停止的網站。</p>
    <DxDataGrid :data-source="urlIssues" key-expr="Id" :show-borders="true" :column-auto-width="true" :word-wrap-enabled="true">
      <DxSearchPanel :visible="true" placeholder="搜尋網站或網址" />
      <DxPaging :page-size="10" />
      <DxPager :visible="true" :show-info="true" />
      <DxColumn data-field="Website" caption="網站" />
      <DxColumn data-field="Source" caption="設定來源" />
      <DxColumn data-field="OriginalUrl" caption="原始網址" />
      <DxColumn data-field="Reason" caption="無法比對原因" />
      <DxColumn data-field="EscapedUrl" caption="隱藏字元檢視" />
      <DxColumn data-field="CharacterHints" caption="字元位置與代碼" />
    </DxDataGrid>
    <p class="scope-note">字元位置從 1 開始。空白、控制與非 ASCII 字元列供核對；合法中文網域也含非 ASCII 字元，此清單不表示每個列出的字元都不合法。已停止網站不列入主要監測列表與摘要。</p>
  </section>
  <section v-if="inventory?.Snapshot" class="data-card tls-list" aria-label="TLS 狀態摘要">
    <div class="tls-summary">
      <button type="button" class="summary-good" :class="{ selected: selectedFilter === 'valid' }" :aria-pressed="selectedFilter === 'valid'" @click="toggleSummaryFilter('valid')"><span>憑證有效期正常的網站</span><strong>{{ summary.valid }} <small>個網站</small></strong><small>所有已列出憑證皆超過 30 天</small></button>
      <button type="button" class="summary-warning" :class="{ selected: selectedFilter === 'expiring' }" :aria-pressed="selectedFilter === 'expiring'" @click="toggleSummaryFilter('expiring')"><span>憑證即將到期的網站</span><strong>{{ summary.expiring }} <small>個網站</small></strong><small>有憑證將於 30 天內到期</small></button>
      <button type="button" class="summary-danger" :class="{ selected: selectedFilter === 'expired' }" :aria-pressed="selectedFilter === 'expired'" @click="toggleSummaryFilter('expired')"><span>憑證已到期的網站</span><strong>{{ summary.expired }} <small>個網站</small></strong><small>有憑證已到期，需立即處理</small></button>
      <button type="button" class="summary-danger" :class="{ selected: selectedFilter === 'renewalFailed' }" :aria-pressed="selectedFilter === 'renewalFailed'" @click="toggleSummaryFilter('renewalFailed')"><span>自動更新失敗網站</span><strong>{{ summary.renewalFailed }} <small>個網站</small></strong><small>最近一次更新失敗</small></button>
      <button type="button" class="summary-warning" :class="{ selected: selectedFilter === 'renewalMissing' }" :aria-pressed="selectedFilter === 'renewalMissing'" @click="toggleSummaryFilter('renewalMissing')"><span>未發現 renewal</span><strong>{{ summary.renewalMissing }} <small>個網站</small></strong><small>缺少對應續期設定</small></button>
      <button type="button" class="summary-warning" :class="{ selected: selectedFilter === 'uncertain' }" :aria-pressed="selectedFilter === 'uncertain'" @click="toggleSummaryFilter('uncertain')"><span>憑證檢查異常網站</span><strong>{{ summary.uncertain }} <small>個網站</small></strong><small>未能取得到期資訊</small></button>
    </div>
    <p class="scope-note">點擊卡片可查看對應網站。同一網站可能同時有更新失敗與檢查異常，請勿將各卡片數字相加；下方「需處理網站」已去除重複。此頁尚未驗證網站實際連線。</p>
  </section>
  <section class="data-card tls-list">
    <div class="list-filters">
      <button type="button" class="ui-button" :aria-pressed="selectedFilter === 'attention'" @click="toggleSummaryFilter('attention')">需處理網站：{{ summary.attention }} 個（不重複）</button>
      <button v-if="selectedFilter !== 'all'" type="button" class="ui-button" @click="selectedFilter = 'all'">顯示全部</button>
      <span>目前顯示：{{ filterLabels[selectedFilter] }}</span>
    </div>
    <DxDataGrid :key="`${selectedServer}-${selectedFilter}`" :data-source="visibleRows" key-expr="Id" :show-borders="true"
      :column-auto-width="true" :allow-column-resizing="true" :allow-column-reordering="true"
      :word-wrap-enabled="true" :hover-state-enabled="true" :no-data-text="selectedFilter !== 'all' ? '目前沒有符合此分類的網站。' : '尚未取得本系統網站資料，請通知管理員確認網站設定與監測服務。'">
      <DxSearchPanel :visible="true" :width="240" placeholder="搜尋網站、網址或問題" />
      <DxFilterRow :visible="true" />
      <DxSorting mode="multiple" />
      <DxPaging :page-size="10" />
      <DxPager :visible="true" :show-info="true" :show-page-size-selector="true" :allowed-page-sizes="[10, 20, 50]" />
      <DxColumnChooser :enabled="true" mode="select" />
      <DxColumn data-field="Website" caption="網站" :width="140" sort-order="asc" cell-template="website" />
      <DxColumn data-field="Urls" caption="網站網址" :min-width="180" cell-template="multiline" />
      <DxColumn data-field="Overview" caption="目前狀況" :width="120" cell-template="status" />
      <DxColumn data-field="NotAfter" caption="憑證到期日" data-type="datetime" format="yyyy/MM/dd" :width="115" />
      <DxColumn data-field="RemainingDays" caption="剩餘天數" data-type="number" :width="90" />
      <DxColumn v-if="canControlServers" caption="TLS 安裝" :width="145" cell-template="install-tls" :allow-filtering="false" :allow-sorting="false" />
      <DxColumn data-field="Explanation" caption="問題說明與處理建議" :min-width="270" cell-template="multiline" />
      <DxColumn data-field="SiteState" caption="網站運作狀態" :visible="false" />
      <DxColumn data-field="Status" caption="憑證有效期狀態" :visible="false" />
      <DxColumn data-field="Attention" caption="提醒項目" :visible="false" />
      <DxColumn data-field="RenewalResult" caption="最近更新結果" :visible="false" />
      <DxColumn data-field="RenewalTime" caption="最近更新紀錄時間" data-type="datetime" format="yyyy/MM/dd HH:mm" :visible="false" />
      <DxColumn data-field="RenewalDetails" caption="原始更新訊息" :visible="false" cell-template="message" />
      <DxColumn data-field="Error" caption="原始憑證讀取訊息" :visible="false" cell-template="message" />
      <DxColumn data-field="Files" caption="PFX 檔名" :visible="false" cell-template="multiline" />
      <DxColumn data-field="Match" caption="憑證對應依據" :visible="false" />
      <DxColumn data-field="RenewalTask" caption="續期任務" :visible="false" />
      <DxColumn data-field="RenewalSource" caption="續期來源日誌／行號" :visible="false" />
      <DxColumn data-field="RenewalMatch" caption="續期對應依據" :visible="false" />
      <DxColumn data-field="Subject" caption="憑證主體" :visible="false" />
      <DxColumn data-field="Issuer" caption="發行者" :visible="false" />
      <DxColumn data-field="Thumbprint" caption="憑證指紋" :visible="false" />
      <DxColumn data-field="NotBefore" caption="生效時間" data-type="datetime" format="yyyy/MM/dd HH:mm" :visible="false" />
      <template #install-tls="{ data }"><button type="button" :disabled="!inventory?.IsOnline || !!installingSite || !(websites.find(site => site.SiteName === data.data.Website)?.HostNames?.length || websites.find(site => site.SiteName === data.data.Website)?.HttpsUrls.length)" @click="installTls(data.data.Website)">安裝／更新 TLS</button></template>
      <template #multiline="{ data }"><span class="multiline">{{ data.value }}</span></template>
      <template #website="{ data }"><span>{{ data.value }}</span><small class="site-state">{{ data.data.SiteState }}</small></template>
      <template #status="{ data }"><span class="status-label" :class="{ 'status-good': data.value === '有效期正常', 'status-issue': data.data.Attention }">{{ data.value }}</span></template>
      <template #message="{ data }"><span class="message" :title="data.value || ''">{{ data.value || '—' }}</span></template>
    </DxDataGrid>
    <details><summary>管理員檢視：資料來源與檢查錯誤</summary>
      <p v-if="inventory?.Snapshot?.Error">網站清單：{{ inventory.Snapshot.Error }}</p>
      <p v-if="centralStore?.Error">憑證讀取：{{ centralStore.Error }}</p>
      <p v-if="wacsLogs?.Error">更新紀錄：{{ wacsLogs.Error }}</p>
      <p v-if="wacsSchedule?.Error">更新排程：{{ wacsSchedule.Error }}</p>
      <p>同網站相同指紋的憑證合併顯示。PFX 依網址檔名推定對應，尚未驗證網站實際使用的憑證。更新結果依最近日誌的明確網站名稱或網域對應；不推測 any site 或 +N other。沒有紀錄不代表從未更新，成功紀錄不代表已完成網站安裝驗證。</p>
      <p>集中式憑證：{{ centralStore?.Enabled === true ? '已啟用' : centralStore?.Enabled === false ? '未啟用' : '無法確認' }} · 目錄：{{ centralStore?.DirectoryPath || '—' }}</p>
      <p>讀取 win-acme 續期設定、本機日誌與工作排程；不啟動 win-acme、PowerShell 或讀取事件檢視器。每次 Worker 啟動採集一次，之後每日採集；重新整理只讀最近回報。</p>
    </details>
  </section>
  <section class="data-card tls-list">
    <details><summary>管理員檢視：自動更新排程（{{ scheduleRows.length }} 個任務）</summary>
      <p v-if="!wacsSchedule">尚未收到第五步資料，請更新並啟動 Worker。</p>
      <template v-else>
        <p>採集：{{ formatTime(wacsSchedule.CollectedAtUtc) }} · 排程資料夾：{{ wacsSchedule.Folder }}</p>
        <DxDataGrid :data-source="scheduleRows" key-expr="TaskPath" :show-borders="true" :column-auto-width="true" :word-wrap-enabled="true"
          no-data-text="此資料夾沒有符合 wacs.exe --renew 的任務；請確認 Worker 的 WacsTaskFolder 設定或排程動作。">
          <DxSorting mode="multiple" />
          <DxColumn data-field="TaskPath" caption="排程任務" />
          <DxColumn data-field="Enabled" caption="啟用" data-type="boolean" />
          <DxColumn data-field="State" caption="狀態" />
          <DxColumn data-field="NextRun" caption="下次啟動時間" data-type="datetime" format="yyyy/MM/dd HH:mm:ss" />
          <DxColumn data-field="LastRun" caption="上次啟動時間" data-type="datetime" format="yyyy/MM/dd HH:mm:ss" />
          <DxColumn data-field="ResultCode" caption="上次返回碼" />
        </DxDataGrid>
        <p class="scope-note">這是伺服器排程的啟動時間，不是每張憑證的續期或安裝時間。返回碼 0 不代表所有網站續期成功，請搭配日誌結果查看。只讀指定資料夾內直接執行 wacs.exe 且帶 --renew 的任務，不讀子資料夾或腳本包裝的任務；不修改或執行排程。</p>
      </template>
    </details>
    <details><summary>管理員檢視：更新原始紀錄（{{ unmatchedCount }} 筆未對應網站）</summary>
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
      <p class="scope-note">每檔最多保留最後 300 筆。只有明確的續期結束訊息才判定成功或失敗；一般錯誤與不完整紀錄標示「無法確認」。缺少時間時留空；未附時區依 Worker 當地時區解讀。敏感內容遮蔽；尚未提供失敗通知。</p>
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
.tls-summary { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 1rem; }
.tls-summary > button { display: grid; gap: .5rem; padding: 1rem; border: 1px solid #e2e7ef; border-radius: 10px; border-top: 3px solid #aeb9c9; background: #fff; color: inherit; font: inherit; text-align: left; cursor: pointer; }
.tls-summary > button:hover { background: #f5f8fc; }
.tls-summary > button.selected, .tls-summary > button:focus-visible { outline: 2px solid #2473bd; outline-offset: 2px; }
.tls-summary span { color: #687386; font-size: .85rem; }
.tls-summary strong { font-size: 1.8rem; line-height: 1.2; }
.tls-summary small { color: #687386; font-size: .75rem; }
.tls-summary .summary-good { border-top-color: #23835a; }
.tls-summary .summary-warning { border-top-color: #bb8219; }
.tls-summary .summary-danger { border-top-color: #c34b49; }
.operator-notice { padding: 1rem; line-height: 1.7; margin-bottom: 1rem; }
.site-state { display: block; color: #687386; margin-top: .4rem; }
.status-label { display: inline-block; padding: .25rem .5rem; border-radius: 5px; background: #eef1f5; }
.status-good { color: #1c6847; background: #e8f5ed; }
.status-issue { color: #954518; background: #fff2df; }
.list-filters { display: flex; flex-wrap: wrap; align-items: center; gap: .75rem; margin-bottom: .75rem; }
.list-filters > span { color: #687386; font-size: .85rem; }
.list-filters button[aria-pressed="true"] { outline: 2px solid #2473bd; }
.multiline { white-space: pre-line; overflow-wrap: anywhere; }
.message { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; overflow-wrap: anywhere; }
.scope-note, details { margin-top: 1rem; color: #687386; font-size: .85rem; line-height: 1.6; }
@media (max-width: 1000px) { .tls-summary { grid-template-columns: repeat(3, minmax(0, 1fr)); } }
@media (max-width: 600px) { label { width: 100%; min-width: 0; } .tls-summary { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
