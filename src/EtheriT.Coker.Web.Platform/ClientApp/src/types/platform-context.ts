export interface PlatformContext {
  UserName: string;
  MvcUrl: string;
  SessionActivityIntervalSeconds: number;
  AntiforgeryToken: string;
  ReauthenticationTicket: string;
}
