export interface PlatformContext {
  UserName: string;
  MvcUrl: string;
  SessionActivityIntervalSeconds: number;
  AntiforgeryToken: string;
  ReauthenticationTicket: string;
  CanManagePlatformData: boolean;
  CanControlServers: boolean;
  CanManagePlatformRoles: boolean;
}
