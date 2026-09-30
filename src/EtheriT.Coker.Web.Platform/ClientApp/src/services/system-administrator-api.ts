import { api } from "@/core/api/api-client";
import type { CreateSystemAdministratorForm, CreateSystemAdministratorInvitationResponse, SystemAdministrator, SystemAdministratorPage } from "@/types/system-administrator";

export function fetchSystemAdministrators(): Promise<SystemAdministratorPage> {
  return api.get<SystemAdministratorPage>("/api/system-administrators");
}

export function addSystemAdministrator(userId: number, roleId: number): Promise<SystemAdministrator> {
  return api.post<SystemAdministrator>("/api/system-administrators", { UserId: userId, RoleId: roleId });
}

export function createSystemAdministrator(request: CreateSystemAdministratorForm): Promise<CreateSystemAdministratorInvitationResponse> {
  return api.post<CreateSystemAdministratorInvitationResponse>("/api/system-administrators/users", request);
}

export function resendSystemAdministratorInvitation(invitationId: number): Promise<void> {
  return api.post<void>(`/api/system-administrators/invitations/${invitationId}/resend`);
}

export function approveSystemAdministratorInvitation(invitationId: number): Promise<SystemAdministrator> {
  return api.post<SystemAdministrator>(`/api/system-administrators/invitations/${invitationId}/approve`);
}

export function revokeSystemAdministratorInvitation(invitationId: number): Promise<void> {
  return api.delete<void>(`/api/system-administrators/invitations/${invitationId}`);
}

export function removeSystemAdministrator(mappingId: number): Promise<void> {
  return api.delete<void>(`/api/system-administrators/${mappingId}`);
}
