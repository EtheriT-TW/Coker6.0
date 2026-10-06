import { requestAlert, requestConfirm } from '@/core/coker';
import { createProvisioningTask } from '@/services/provisioning-api';
import { ProvisioningTaskType } from '@/types/provisioning';

export async function installWebsiteTls(serverId: string, siteName: string, hostNames: string[], dryRun = false): Promise<void> {
  const hosts = [...new Set(hostNames.map(host => host.trim().toLowerCase()).filter(Boolean))];
  if (!serverId || !siteName || !hosts.length) return;
  if (!await requestConfirm({ title: '安裝／更新網站 TLS',
    message: `為「${siteName}」處理 ${hosts.join('、')}？已有續期設定時立即沿用並強制續期；多站共用設定會一起續期。${dryRun ? '目前為 Dry Run，不會實際安裝。' : ''}`,
    icon: 'lock', confirmText: '安裝／更新 TLS' })) return;
  await createProvisioningTask({ TargetServerId: serverId, Type: ProvisioningTaskType.InstallSsl,
    SiteName: siteName, HostNames: hosts });
  await requestAlert({ title: '已送出 TLS 任務', message: `「${siteName}」將由 ${serverId} 的 Worker 處理；執行結果請至任務紀錄查看。`, icon: 'task_alt', tone: 'primary' });
}
