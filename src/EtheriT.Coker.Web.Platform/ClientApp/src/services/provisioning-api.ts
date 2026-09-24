import { api } from "@/core/api/api-client";
import type { CreateProvisioningTaskRequest, ProvisioningServer, ProvisioningTask } from "@/types/provisioning";

export function fetchProvisioningServers(): Promise<ProvisioningServer[]> {
  return api.get<ProvisioningServer[]>("/api/provisioning/tasks/servers");
}

export function fetchProvisioningTasks(): Promise<ProvisioningTask[]> {
  return api.get<ProvisioningTask[]>("/api/provisioning/tasks");
}

export function createProvisioningTask(request: CreateProvisioningTaskRequest): Promise<ProvisioningTask> {
  return api.post<ProvisioningTask>("/api/provisioning/tasks", request);
}
