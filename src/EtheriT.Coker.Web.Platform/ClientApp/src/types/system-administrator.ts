export interface SystemAdministrator {
  MappingId: number;
  UserId: number;
  Name: string;
  Account: string;
  Email: string | null;
  RoleId: number;
  RoleCode: string;
  RoleName: string;
  IsCurrentUser: boolean;
}

export interface SystemAdministratorUserOption {
  Id: number;
  Name: string;
  Account: string;
  Email: string | null;
}

export interface SystemAdministratorRoleOption {
  Id: number;
  Code: string;
  Name: string;
  Description: string;
}

export interface SystemAdministratorMvcRoleOption {
  Id: number;
  Name: string;
}

export interface MvcSystemAdministrator {
  UserId: number;
  Name: string;
  Account: string;
  Email: string | null;
  RoleNames: string;
  IsCurrentUser: boolean;
}

export interface CreateSystemAdministratorForm {
  Name: string;
  Email: string;
  MvcRoleId: number | null;
  PlatformRoleId: number | null;
}

export interface SystemAdministratorInvitation {
  Id: number;
  UserId: number;
  Name: string;
  Email: string;
  Account: string | null;
  MvcRoleId: number | null;
  MvcRoleName: string | null;
  PlatformRoleId: number | null;
  PlatformRoleName: string | null;
  ExpiresAtUtc: string;
  EmailVerifiedAtUtc: string | null;
}

export interface CreateSystemAdministratorInvitationResponse {
  Invitation: SystemAdministratorInvitation;
  EmailSent: boolean;
  EmailError: string | null;
}

export interface SystemAdministratorPage {
  Administrators: SystemAdministrator[];
  Users: SystemAdministratorUserOption[];
  Roles: SystemAdministratorRoleOption[];
  MvcRoles: SystemAdministratorMvcRoleOption[];
  Invitations: SystemAdministratorInvitation[];
  MvcAdministrators: MvcSystemAdministrator[];
}
