<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { RouterLink, useRoute } from 'vue-router';
import { fetchProvisioningServers, fetchProvisioningTasks } from '@/services/provisioning-api';
import { ProvisioningTaskType, type ProvisioningServer, type ProvisioningTask } from '@/types/provisioning';

const route = useRoute();
const serverId = ref(typeof route.query.server === 'string' ? route.query.server : '');
const status = ref('');
const servers = ref<ProvisioningServer[]>([]);
const tasks = ref<ProvisioningTask[]>([]);
const loading = ref(false);
const errorMessage = ref('');
let timer: number | undefined;
let request = 0;
let disposed = false;
const types: Record<ProvisioningTaskType, string> = {
  [ProvisioningTaskType.RestartIis]: '重新啟動 IIS',
  [ProvisioningTaskType.SetIisSiteState]: 'IIS 單站操作',
  [ProvisioningTaskType.CreateDnsARecord]: '建立 DNS A 記錄',
  [ProvisioningTaskType.DeleteDnsARecord]: '刪除 DNS A 記錄',
  [ProvisioningTaskType.InstallSsl]: '安裝 SSL',
  [ProvisioningTaskType.CreateDnsRecord]: '建立 DNS 記錄',
  [ProvisioningTaskType.DeleteDnsRecord]: '刪除 DNS 記錄',
  [ProvisioningTaskType.SyncWebsiteBindings]: '同步網址繫結並更新 TLS'
};
const statuses: Record<number, string> = { 0: '等待中', 1: '執行中', 2: '成功', 3: '失敗' };
function formatTime(value: string | null): string { return value ? new Date(value).toLocaleString('zh-TW') : '—'; }
async function load(silent = false): Promise<void> {
  if (disposed || (silent && loading.value)) return;
  const current = ++request;
  loading.value = true;
  try {
    const result = await fetchProvisioningTasks({
      ...(serverId.value ? { serverId: serverId.value } : {}),
      ...(status.value !== '' ? { status: Number(status.value) } : {})
    });
    if (current !== request || disposed) return;
    tasks.value = result;
    errorMessage.value = '';
  } catch (error) {
    if (current === request && !disposed) errorMessage.value = error instanceof Error ? error.message : '無法讀取任務紀錄。';
  } finally { if (current === request && !disposed) loading.value = false; }
}
watch([serverId, status], () => { tasks.value = []; void load(); });
onMounted(async () => {
  void load();
  timer = window.setInterval(() => void load(true), 5000);
  try { servers.value = await fetchProvisioningServers(); }
  catch { if (!disposed) errorMessage.value = '無法讀取伺服器選項，仍可查看任務紀錄。'; }
});
onBeforeUnmount(() => { disposed = true; ++request; if (timer !== undefined) window.clearInterval(timer); });
</script>

<template>
  <section class="page-heading">
    <div><h1>任務紀錄</h1><p>查看 IIS、DNS、TLS 與網站網址同步任務；每 5 秒更新，顯示篩選後最近 100 筆。</p></div>
    <button class="ui-button" type="button" :disabled="loading" @click="load()">重新整理</button>
  </section>
  <p v-if="errorMessage" class="alert alert-error" role="alert">{{ errorMessage }}</p>
  <section class="data-card tasks-card">
    <div class="task-filters">
      <label>伺服器<select v-model="serverId"><option value="">全部伺服器</option><option v-for="server in servers" :key="server.Id" :value="server.Id">{{ server.DisplayName }}（{{ server.Id }}）</option></select></label>
      <label>狀態<select v-model="status"><option value="">全部狀態</option><option v-for="(label, value) in statuses" :key="value" :value="String(value)">{{ label }}</option></select></label>
      <RouterLink class="ui-button" to="/provisioning-test">主機操作測試</RouterLink>
    </div>
    <div class="task-table-wrap"><table><thead><tr><th>ID</th><th>伺服器／網站</th><th>操作</th><th>狀態</th><th>建立／完成時間</th><th>嘗試</th><th>結果</th></tr></thead>
      <tbody><tr v-for="task in tasks" :key="task.Id">
        <td>{{ task.Id }}</td><td>{{ task.TargetServerId }}<small>{{ task.Payload.SiteName || '—' }}</small></td>
        <td>{{ types[task.Type] || `未知操作（${task.Type}）` }}<details v-if="task.Payload.HostNames?.length"><summary>網址</summary><div class="hosts">{{ task.Payload.HostNames.join('\n') }}</div></details></td>
        <td><span class="task-status" :class="`status-${task.Status}`">{{ statuses[task.Status] || task.Status }}</span></td>
        <td>{{ formatTime(task.CreatedAtUtc) }}<small>{{ formatTime(task.CompletedAtUtc) }}</small></td>
        <td>{{ task.AttemptCount }}</td><td class="result-cell"><details v-if="task.ResultMessage"><summary>{{ task.ResultMessage.split('\n')[0] }}</summary><pre>{{ task.ResultMessage }}</pre></details><span v-else>—</span></td>
      </tr><tr v-if="!tasks.length"><td colspan="7">{{ loading ? '載入中…' : '目前沒有符合條件的任務' }}</td></tr></tbody>
    </table></div>
  </section>
</template>

<style scoped>
.tasks-card { padding: 1.25rem; min-height: 0; }.task-filters { display: flex; flex-wrap: wrap; align-items: end; gap: 1rem; }label { display: grid; gap: .4rem; font-size: .9rem; }select { min-height: 40px; padding: .5rem; border: 1px solid #cfd6e2; border-radius: 6px; background: white; }.task-table-wrap { overflow-x: auto; margin-top: 1rem; }table { width: 100%; border-collapse: collapse; font-size: .9rem; }th, td { text-align: left; vertical-align: top; padding: .7rem; border-bottom: 1px solid #e8ebf0; }th { white-space: nowrap; }small { display: block; margin-top: .3rem; color: #687386; }.task-status { display: inline-block; padding: .15rem .5rem; border-radius: 999px; background: #eef1f5; white-space: nowrap; }.status-1 { background: #fff2cc; }.status-2 { background: #d9f4e5; color: #17633a; }.status-3 { background: #fde2e0; color: #9d241d; }.result-cell { min-width: 240px; max-width: 480px; }summary { cursor: pointer; overflow-wrap: anywhere; }pre, .hosts { white-space: pre-wrap; overflow-wrap: anywhere; font: inherit; }
</style>
