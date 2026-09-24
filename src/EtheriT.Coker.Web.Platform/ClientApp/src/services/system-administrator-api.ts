import { api } from "@/core/api/api-client";
import type { SystemAdministrator, SystemAdministratorPage } from "@/types/system-administrator";

export function fetchSystemAdministrators(): Promise<SystemAdministratorPage> {
  return api.get<SystemAdministratorPage>("/api/system-administrators");
}

export function addSystemAdministrator(userId: number, roleId: number): Promise<SystemAdministrator> {
  return api.post<SystemAdministrator>("/api/system-administrators", { UserId: userId, RoleId: roleId });
}

export function removeSystemAdministrator(mappingId: number): Promise<void> {
  return api.delete<void>(`/api/system-administrators/${mappingId}`);
}
