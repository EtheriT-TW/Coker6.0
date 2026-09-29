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

export interface SystemAdministratorPage {
  Administrators: SystemAdministrator[];
  Users: SystemAdministratorUserOption[];
  Roles: SystemAdministratorRoleOption[];
}
