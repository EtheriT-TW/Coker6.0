export enum ProvisioningTaskType {
  RestartIis = 1,
  SetIisSiteState = 2,
  CreateDnsARecord = 3,
  DeleteDnsARecord = 4,
  InstallSsl = 5
}

export enum ProvisioningTaskStatus {
  Pending = 0,
  Running = 1,
  Succeeded = 2,
  Failed = 3
}

export interface ProvisioningTaskPayload {
  SiteName: string | null;
  StartSite: boolean | null;
  ZoneName: string | null;
  RecordName: string | null;
  IPv4Address: string | null;
  HostNames: string[] | null;
}

export interface ProvisioningTask {
  Id: number;
  TargetServerId: string;
  Type: ProvisioningTaskType;
  Status: ProvisioningTaskStatus;
  Payload: ProvisioningTaskPayload;
  AttemptCount: number;
  CreatedAtUtc: string;
  StartedAtUtc: string | null;
  CompletedAtUtc: string | null;
  ResultMessage: string | null;
}

export interface CreateProvisioningTaskRequest {
  TargetServerId: string;
  Type: ProvisioningTaskType;
  SiteName?: string;
  StartSite?: boolean;
  ZoneName?: string;
  RecordName?: string;
  IPv4Address?: string;
  HostNames?: string[];
}

export interface ProvisioningServer {
  Id: string;
  DisplayName: string;
  IsDnsServer: boolean;
}
