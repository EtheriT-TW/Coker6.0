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

export interface ProvisioningAgentStatus {
  ServerId: string;
  DisplayName: string;
  IsDnsServer: boolean;
  IsOnline: boolean;
  WorkerId: string | null;
  MachineName: string | null;
  AgentVersion: string | null;
  DryRun: boolean | null;
  CpuUsagePercent: number | null;
  MemoryUsedBytes: number | null;
  MemoryTotalBytes: number | null;
  MemoryUsagePercent: number | null;
  Disks: ProvisioningDiskMetric[];
  ApplicationPools: ProvisioningAppPoolMetric[];
  LastSeenAtUtc: string | null;
}

export interface ProvisioningDiskMetric {
  Name: string;
  VolumeLabel: string;
  UsedBytes: number;
  TotalBytes: number;
  FreeBytes: number;
  UsagePercent: number;
}

export interface ProvisioningAppPoolMetric {
  ApplicationPoolName: string;
  SiteNames: string[];
  State: string;
  ProcessIds: number[];
  CpuUsagePercent: number | null;
  WorkingSetBytes: number;
  PrivateMemoryBytes: number;
}

export interface ProvisioningMetricHistory {
  ServerId: string;
  FromUtc: string;
  ToUtc: string;
  BucketMinutes: number;
  ServerMetrics: ProvisioningServerMetricPoint[];
  Disks: ProvisioningDiskMetricSeries[];
}

export interface ProvisioningServerMetricPoint {
  SampledAtUtc: string;
  CpuUsagePercent: number | null;
  MemoryUsagePercent: number | null;
  MemoryUsedBytes: number | null;
  MemoryTotalBytes: number | null;
}

export interface ProvisioningDiskMetricSeries {
  Name: string;
  VolumeLabel: string;
  Points: ProvisioningDiskMetricPoint[];
}

export interface ProvisioningDiskMetricPoint {
  SampledAtUtc: string;
  UsagePercent: number;
  UsedBytes: number;
  TotalBytes: number;
  FreeBytes: number;
}
