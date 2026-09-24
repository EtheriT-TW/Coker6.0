export interface SystemAdministrator {
  MappingId: number;
  UserId: number;
  Name: string;
  Account: string;
  Email: string | null;
  RoleId: number;
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
  Name: string;
  IsSuperUser: boolean;
}

export interface SystemAdministratorPage {
  Administrators: SystemAdministrator[];
  Users: SystemAdministratorUserOption[];
  Roles: SystemAdministratorRoleOption[];
}
