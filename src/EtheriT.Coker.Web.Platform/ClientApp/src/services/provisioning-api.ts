import { api } from "@/core/api/api-client";
import type { CreateProvisioningTaskRequest, ProvisioningAgentStatus, ProvisioningMetricHistory, ProvisioningServer, ProvisioningTask } from "@/types/provisioning";

export function fetchProvisioningServers(): Promise<ProvisioningServer[]> {
  return api.get<ProvisioningServer[]>("/api/provisioning/tasks/servers");
}

export function fetchProvisioningAgents(): Promise<ProvisioningAgentStatus[]> {
  return api.get<ProvisioningAgentStatus[]>("/api/provisioning/tasks/agents");
}

export function fetchProvisioningMetricHistory(serverId: string, days: number): Promise<ProvisioningMetricHistory> {
  return api.get<ProvisioningMetricHistory>(
    `/api/provisioning/tasks/agents/${encodeURIComponent(serverId)}/metrics`,
    { query: { days } }
  );
}

export function fetchProvisioningTasks(): Promise<ProvisioningTask[]> {
  return api.get<ProvisioningTask[]>("/api/provisioning/tasks");
}

export function createProvisioningTask(request: CreateProvisioningTaskRequest): Promise<ProvisioningTask> {
  return api.post<ProvisioningTask>("/api/provisioning/tasks", request);
}
