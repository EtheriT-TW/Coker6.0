<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { RouterLink, useRoute } from "vue-router";
import { createProvisioningTask, fetchProvisioningServers } from "@/services/provisioning-api";
import {
  ProvisioningTaskType,
  type CreateProvisioningTaskRequest,
  type ProvisioningServer,
} from "@/types/provisioning";

const servers = ref<ProvisioningServer[]>([]);
const route = useRoute();
const targetServer = ref("");
const siteName = ref("");
const zoneName = ref("");
const recordName = ref("");
const dnsRecordType = ref("A");
const dnsRecordValue = ref("");
const mxPreference = ref(10);
const dnsServer = computed(() => servers.value.find(server => server.IsDnsServer));
const allowedZones = computed(() => dnsServer.value?.AllowedDnsZones ?? []);
const dnsValueLabels: Record<string, string> = { A: "IPv4", TXT: "文字內容", MX: "郵件伺服器主機名稱", CNAME: "別名目標主機名稱" };
const dnsValueLabel = computed(() => dnsValueLabels[dnsRecordType.value] ?? "記錄值");
const canSubmitDns = computed(() => allowedZones.value.includes(zoneName.value) && !!recordName.value &&
  !!dnsRecordValue.value.trim() && (dnsRecordType.value !== "MX" ||
    (Number.isInteger(mxPreference.value) && mxPreference.value >= 0 && mxPreference.value <= 65535)));

const loading = ref(false);
const submitting = ref(false);
const errorMessage = ref("");

async function load(silent = false): Promise<void> {
  if (!silent) loading.value = true;
  try {
    const serverResult = await fetchProvisioningServers();
    servers.value = serverResult;
    if (!servers.value.some(x => x.Id === targetServer.value))
      targetServer.value = servers.value[0]?.Id ?? "";
    if (!allowedZones.value.includes(zoneName.value))
      zoneName.value = allowedZones.value[0] ?? "";
    errorMessage.value = "";
  }
  catch (error) {
    console.error(error);
    if (!silent) errorMessage.value = "無法讀取伺服器選項。";
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
  const dnsServerId = dnsServer.value?.Id;
  if (!dnsServerId || !canSubmitDns.value) {
    errorMessage.value = "請選擇允許的 Zone，並填寫 DNS 記錄資料。";
    return;
  }
  void submit({
    TargetServerId: dnsServerId,
    Type: deleteRecord ? ProvisioningTaskType.DeleteDnsRecord : ProvisioningTaskType.CreateDnsRecord,
    ZoneName: zoneName.value,
    RecordName: recordName.value,
    DnsRecordType: dnsRecordType.value,
    DnsRecordValue: dnsRecordValue.value,
    MxPreference: dnsRecordType.value === "MX" ? mxPreference.value : undefined
  }, `確定要${deleteRecord ? "刪除" : "建立"} DNS ${dnsRecordType.value} 記錄 ${recordName.value}.${zoneName.value} → ${dnsRecordValue.value}${dnsRecordType.value === "MX" ? `（優先序 ${mxPreference.value}）` : ""}？`);
}



onMounted(() => {
  if (typeof route.query.server === "string") targetServer.value = route.query.server;
  void load();
});
</script>

<template>
  <section class="page-heading">
    <div><h1>主機操作測試</h1><p>此頁用於驗證 IIS 與 DNS 操作；伺服器狀態請至伺服器監控查看。</p></div>
    <RouterLink class="ui-button" :to="{ path: '/provisioning-tasks', query: { server: targetServer } }">任務紀錄</RouterLink>
  </section>

  <p class="alert alert-warning" role="status">Worker 的 DryRun=false 時，此頁會執行真實的 IIS 與 DNS 操作；研發環境請先使用 DryRun=true。</p>
  <p v-if="errorMessage" class="alert alert-error" role="alert">{{ errorMessage }}</p>

  <section class="provisioning-target data-card">
    <label><span>目標伺服器</span><select v-model="targetServer"><option v-for="server in servers" :key="server.Id" :value="server.Id">{{ server.DisplayName }}（{{ server.Id }}）</option></select></label>
  </section>

  <section class="operation-grid">
    <article class="data-card operation-card">
      <h2>整個 IIS</h2><p>重新啟動目標伺服器的 IIS，會影響該主機上的所有網站。</p>
      <button class="ui-button danger-button" type="button" :disabled="submitting || !targetServer" @click="restartIis">重新啟動 IIS</button>
    </article>

    <article class="data-card operation-card">
      <h2>IIS 單站</h2>
      <label><span>IIS 網站名稱</span><input v-model.trim="siteName" placeholder="例如 Default Web Site"></label>
      <div class="button-row"><button class="ui-button" type="button" :disabled="submitting || !siteName" @click="setSiteState(true)">啟動</button><button class="ui-button danger-button" type="button" :disabled="submitting || !siteName" @click="setSiteState(false)">停止</button></div>
    </article>

    <article class="data-card operation-card">
      <h2>Windows DNS</h2>
      <p>由 {{ dnsServer?.DisplayName || "未設定的 DNS Server" }} 執行，只能操作既有 Zone，不會建立 Zone。</p>
      <label><span>Zone</span><select v-model="zoneName" :disabled="!allowedZones.length"><option v-if="!allowedZones.length" value="">尚未設定允許的 Zone</option><option v-for="zone in allowedZones" :key="zone" :value="zone">{{ zone }}</option></select></label>
      <label><span>記錄類型</span><select v-model="dnsRecordType" @change="dnsRecordValue = ''"><option v-for="type in ['A', 'TXT', 'MX', 'CNAME']" :key="type" :value="type">{{ type }}</option></select></label>
      <label><span>記錄名稱</span><input v-model.trim="recordName" placeholder="www 或 @"></label>
      <label><span>{{ dnsValueLabel }}</span><textarea v-if="dnsRecordType === 'TXT'" v-model="dnsRecordValue" rows="3" placeholder="TXT 內容（最多 255 UTF-8 bytes）"></textarea><input v-else v-model.trim="dnsRecordValue" :placeholder="dnsRecordType === 'A' ? '192.0.2.1' : 'mail.example.com'"></label>
      <label v-if="dnsRecordType === 'MX'"><span>優先序（數字越小越優先）</span><input v-model.number="mxPreference" type="number" min="0" max="65535"></label>
      <div class="button-row"><button class="ui-button" type="button" :disabled="submitting || !canSubmitDns" @click="changeDns(false)">建立記錄</button><button class="ui-button danger-button" type="button" :disabled="submitting || !canSubmitDns" @click="changeDns(true)">刪除記錄</button></div>
    </article>


  </section>


</template>

<style scoped>
.provisioning-target { min-height: 0; margin-bottom: 1rem; padding: 1rem 1.25rem; }
.provisioning-target label { max-width: 320px; }
.operation-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1rem; align-items: stretch; }
.operation-card { min-height: 0; padding: 1.25rem; display: flex; flex-direction: column; gap: .8rem; }
.operation-card h2 { margin: 0; font-size: 1.1rem; }
.operation-card p { margin: 0; color: #687386; }
label { display: grid; gap: .35rem; font-size: .9rem; font-weight: 600; }
input, select, textarea { width: 100%; min-width: 0; min-height: 40px; padding: .5rem .7rem; border: 1px solid #cfd6e2; border-radius: 6px; background: #fff; font: inherit; }
.button-row { display: flex; flex-wrap: wrap; gap: .6rem; }
.danger-button { color: #b42318; border-color: #f0aaa5; }
@media (max-width: 900px) { .operation-grid { grid-template-columns: 1fr; } }
</style>
