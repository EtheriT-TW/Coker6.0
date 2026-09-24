<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { createProvisioningTask, fetchProvisioningAgents, fetchProvisioningServers, fetchProvisioningTasks } from "@/services/provisioning-api";
import {
  ProvisioningTaskStatus,
  ProvisioningTaskType,
  type CreateProvisioningTaskRequest,
  type ProvisioningAgentStatus,
  type ProvisioningServer,
  type ProvisioningTask
} from "@/types/provisioning";

const servers = ref<ProvisioningServer[]>([]);
const route = useRoute();
const agents = ref<ProvisioningAgentStatus[]>([]);
const targetServer = ref("");
const siteName = ref("");
const zoneName = ref("");
const recordName = ref("");
const ipAddress = ref("");
const sslHostNames = ref("");
const tasks = ref<ProvisioningTask[]>([]);
const loading = ref(false);
const submitting = ref(false);
const errorMessage = ref("");
let refreshTimer: number | undefined;

const typeText: Record<ProvisioningTaskType, string> = {
  [ProvisioningTaskType.RestartIis]: "重新啟動 IIS",
  [ProvisioningTaskType.SetIisSiteState]: "IIS 單站操作",
  [ProvisioningTaskType.CreateDnsARecord]: "建立 DNS A 記錄",
  [ProvisioningTaskType.DeleteDnsARecord]: "刪除 DNS A 記錄",
  [ProvisioningTaskType.InstallSsl]: "安裝 SSL"
};

const statusText: Record<ProvisioningTaskStatus, string> = {
  [ProvisioningTaskStatus.Pending]: "等待中",
  [ProvisioningTaskStatus.Running]: "執行中",
  [ProvisioningTaskStatus.Succeeded]: "成功",
  [ProvisioningTaskStatus.Failed]: "失敗"
};

async function load(silent = false): Promise<void> {
  if (!silent) loading.value = true;
  try {
    const [serverResult, agentResult, taskResult] = await Promise.all([
      fetchProvisioningServers(), fetchProvisioningAgents(), fetchProvisioningTasks()
    ]);
    servers.value = serverResult;
    agents.value = agentResult;
    tasks.value = taskResult;
    if (!servers.value.some(x => x.Id === targetServer.value))
      targetServer.value = servers.value[0]?.Id ?? "";
    errorMessage.value = "";
  }
  catch (error) {
    console.error(error);
    if (!silent) errorMessage.value = "無法讀取任務清單。";
  }
  finally {
    if (!silent) loading.value = false;
  }
}

async function submit(request: CreateProvisioningTaskRequest, confirmation: string): Promise<void> {
  if (!window.confirm(confirmation) || submitting.value) return;
  submitting.value = true;
  errorMessage.value = "";
  try {
    await createProvisioningTask(request);
    await load(true);
  }
  catch (error) {
    console.error(error);
    errorMessage.value = error instanceof Error ? error.message : "建立任務失敗。";
  }
  finally {
    submitting.value = false;
  }
}

function restartIis(): void {
  void submit({ TargetServerId: targetServer.value, Type: ProvisioningTaskType.RestartIis },
    `確定重新啟動 ${targetServer.value} 的整個 IIS？該伺服器上的所有網站都會短暫中斷。`);
}

function setSiteState(start: boolean): void {
  void submit({ TargetServerId: targetServer.value, Type: ProvisioningTaskType.SetIisSiteState, SiteName: siteName.value, StartSite: start },
    `確定要${start ? "啟動" : "停止"} ${targetServer.value} 的 IIS 網站「${siteName.value}」？`);
}

function changeDns(deleteRecord: boolean): void {
  const dnsServer = servers.value.find(x => x.IsDnsServer)?.Id;
  if (!dnsServer) {
    errorMessage.value = "目前設定中沒有 DNS Server。";
    return;
  }
  void submit({
    TargetServerId: dnsServer,
    Type: deleteRecord ? ProvisioningTaskType.DeleteDnsARecord : ProvisioningTaskType.CreateDnsARecord,
    ZoneName: zoneName.value,
    RecordName: recordName.value,
    IPv4Address: ipAddress.value
  }, `確定要${deleteRecord ? "刪除" : "建立"} DNS A 記錄 ${recordName.value}.${zoneName.value} → ${ipAddress.value}？`);
}

function installSsl(): void {
  const hosts = sslHostNames.value.split(/[,;\r\n]+/).map(x => x.trim()).filter(Boolean);
  void submit({ TargetServerId: targetServer.value, Type: ProvisioningTaskType.InstallSsl, HostNames: hosts },
    `確定要在 ${targetServer.value} 依 IIS binding 搜尋站台，並一次申請／安裝以下 SSL？\n${hosts.join("\n")}`);
}

function formatTime(value: string | null): string {
  return value ? new Date(value).toLocaleString("zh-TW") : "—";
}

function formatBytes(value: number | null): string {
  if (value === null || value < 0) return "—";
  const gib = value / 1024 / 1024 / 1024;
  return `${gib.toFixed(1)} GB`;
}

onMounted(() => {
  if (typeof route.query.server === "string") targetServer.value = route.query.server;
  void load();
  refreshTimer = window.setInterval(() => void load(true), 5000);
});
onBeforeUnmount(() => {
  if (refreshTimer !== undefined) window.clearInterval(refreshTimer);
});
</script>

<template>
  <section class="page-heading">
    <div><h1>主機操作測試</h1><p>建立受控任務，由 JSON 設定中的 Provisioning Worker 各自領取並執行。</p></div>
    <button class="ui-button" type="button" :disabled="loading" @click="load()">重新整理</button>
  </section>

  <p class="alert alert-warning" role="status">Worker 的 DryRun=false 時，此頁會執行真實的 IIS、DNS 與 SSL 操作；研發環境請先使用 DryRun=true。</p>
  <p v-if="errorMessage" class="alert alert-error" role="alert">{{ errorMessage }}</p>

  <section class="agent-grid" aria-label="伺服器環境監控">
    <article v-for="agent in agents" :key="agent.ServerId" class="data-card agent-card">
      <header>
        <div><h2>{{ agent.DisplayName }}</h2><small>{{ agent.ServerId }} · {{ agent.MachineName ?? "尚未連線" }}</small></div>
        <span class="agent-state" :class="agent.IsOnline ? 'online' : 'offline'">{{ agent.IsOnline ? "在線" : "離線" }}</span>
      </header>
      <div class="agent-metrics">
        <div><small>CPU</small><strong>{{ agent.CpuUsagePercent === null ? "—" : `${agent.CpuUsagePercent.toFixed(1)}%` }}</strong></div>
        <div><small>記憶體</small><strong>{{ agent.MemoryUsagePercent === null ? "—" : `${agent.MemoryUsagePercent.toFixed(1)}%` }}</strong></div>
      </div>
      <p>{{ formatBytes(agent.MemoryUsedBytes) }} / {{ formatBytes(agent.MemoryTotalBytes) }}</p>
      <footer><span>版本 {{ agent.AgentVersion ?? "—" }}</span><span>{{ agent.DryRun === null ? "尚未回報模式" : agent.DryRun ? "Dry Run" : "實際執行" }}</span><span>最後回報 {{ formatTime(agent.LastSeenAtUtc) }}</span></footer>
    </article>
  </section>

  <section class="provisioning-target data-card">
    <label><span>目標伺服器</span><select v-model="targetServer"><option v-for="server in servers" :key="server.Id" :value="server.Id">{{ server.DisplayName }}（{{ server.Id }}）</option></select></label>
  </section>

  <section class="operation-grid">
    <article class="data-card operation-card">
      <h2>整個 IIS</h2><p>重新啟動目標伺服器的 IIS，會影響該主機上的所有網站。</p>
      <button class="ui-button danger-button" type="button" :disabled="submitting" @click="restartIis">重新啟動 IIS</button>
    </article>

    <article class="data-card operation-card">
      <h2>IIS 單站</h2>
      <label><span>IIS 網站名稱</span><input v-model.trim="siteName" placeholder="例如 Default Web Site"></label>
      <div class="button-row"><button class="ui-button" type="button" :disabled="submitting || !siteName" @click="setSiteState(true)">啟動</button><button class="ui-button danger-button" type="button" :disabled="submitting || !siteName" @click="setSiteState(false)">停止</button></div>
    </article>

    <article class="data-card operation-card">
      <h2>Windows DNS（固定由 .31 執行）</h2>
      <label><span>Zone</span><input v-model.trim="zoneName" placeholder="example.com"></label>
      <label><span>記錄名稱</span><input v-model.trim="recordName" placeholder="www 或 @"></label>
      <label><span>IPv4</span><input v-model.trim="ipAddress" placeholder="210.65.132.30"></label>
      <div class="button-row"><button class="ui-button" type="button" :disabled="submitting || !zoneName || !recordName || !ipAddress" @click="changeDns(false)">建立 A 記錄</button><button class="ui-button danger-button" type="button" :disabled="submitting || !zoneName || !recordName || !ipAddress" @click="changeDns(true)">刪除 A 記錄</button></div>
    </article>

    <article class="data-card operation-card">
      <h2>win-acme SSL</h2>
      <p>依 IIS 現有 binding 的主機名稱尋找站台，不需要輸入 Site ID；可用換行、逗號或分號一次輸入多個網域。</p>
      <label><span>完整主機名稱（可多筆）</span><textarea v-model.trim="sslHostNames" rows="5" placeholder="example.com&#10;www.example.com"></textarea></label>
      <button class="ui-button" type="button" :disabled="submitting || !targetServer || !sslHostNames.trim()" @click="installSsl">申請並安裝 SSL</button>
    </article>
  </section>

  <section class="data-card task-card">
    <h2>最近任務</h2>
    <div class="task-table-wrap"><table><thead><tr><th>ID</th><th>伺服器</th><th>操作</th><th>狀態</th><th>建立時間</th><th>嘗試</th><th>結果</th></tr></thead>
      <tbody><tr v-for="task in tasks" :key="task.Id"><td>{{ task.Id }}</td><td>{{ task.TargetServerId }}</td><td>{{ typeText[task.Type] }}</td><td><span class="task-status" :class="`status-${task.Status}`">{{ statusText[task.Status] }}</span></td><td>{{ formatTime(task.CreatedAtUtc) }}</td><td>{{ task.AttemptCount }}</td><td class="result-cell">{{ task.ResultMessage ?? "—" }}</td></tr><tr v-if="!tasks.length"><td colspan="7">{{ loading ? "載入中…" : "目前沒有任務" }}</td></tr></tbody>
    </table></div>
  </section>
</template>

<style scoped>
.provisioning-target { margin-bottom: 1rem; padding: 1rem 1.25rem; }
.agent-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1rem; margin-bottom: 1rem; }
.agent-card { padding: 1.1rem 1.25rem; }
.agent-card header { display: flex; justify-content: space-between; gap: 1rem; align-items: flex-start; }
.agent-card h2 { margin: 0 0 .25rem; font-size: 1rem; }.agent-card small, .agent-card p, .agent-card footer { color: #687386; }
.agent-state { padding: .2rem .55rem; border-radius: 999px; font-size: .8rem; font-weight: 700; }.agent-state.online { color: #17633a; background: #d9f4e5; }.agent-state.offline { color: #9d241d; background: #fde2e0; }
.agent-metrics { display: grid; grid-template-columns: 1fr 1fr; gap: .75rem; margin-top: 1rem; }.agent-metrics div { padding: .75rem; border-radius: 8px; background: #f7f9fc; }.agent-metrics small, .agent-metrics strong { display: block; }.agent-metrics strong { margin-top: .2rem; font-size: 1.25rem; }
.agent-card footer { display: flex; flex-wrap: wrap; gap: .4rem 1rem; font-size: .78rem; }
.provisioning-target label { max-width: 320px; }
.operation-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1rem; }
.operation-card { padding: 1.25rem; display: flex; flex-direction: column; gap: .8rem; }
.operation-card h2, .task-card h2 { margin: 0; font-size: 1.1rem; }
.operation-card p { margin: 0; color: #687386; }
label { display: grid; gap: .35rem; font-size: .9rem; font-weight: 600; }
input, select, textarea { min-height: 40px; padding: .5rem .7rem; border: 1px solid #cfd6e2; border-radius: 6px; background: #fff; font: inherit; }
.button-row { display: flex; flex-wrap: wrap; gap: .6rem; }
.danger-button { color: #b42318; border-color: #f0aaa5; }
.task-card { margin-top: 1rem; padding: 1.25rem; }
.task-table-wrap { overflow-x: auto; margin-top: 1rem; }
table { width: 100%; border-collapse: collapse; font-size: .9rem; }
th, td { padding: .65rem; text-align: left; border-bottom: 1px solid #e8ebf0; vertical-align: top; }
.task-status { display: inline-block; padding: .15rem .5rem; border-radius: 999px; background: #eef1f5; white-space: nowrap; }
.status-1 { background: #fff2cc; }.status-2 { background: #d9f4e5; color: #17633a; }.status-3 { background: #fde2e0; color: #9d241d; }
.result-cell { min-width: 240px; max-width: 480px; white-space: pre-wrap; word-break: break-word; }
@media (max-width: 900px) { .operation-grid { grid-template-columns: 1fr; } }
</style>
