export enum ProvisioningTaskType {
  RestartIis = 1,
  SetIisSiteState = 2,
  CreateDnsARecord = 3,
  DeleteDnsARecord = 4,
  InstallSsl = 5,
  CreateDnsRecord = 6,
  DeleteDnsRecord = 7
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
  DnsRecordType?: string | null;
  DnsRecordValue?: string | null;
  MxPreference?: number | null;
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
  DnsRecordType?: string;
  DnsRecordValue?: string;
  MxPreference?: number;
}

export interface ProvisioningServer {
  Id: string;
  DisplayName: string;
  IsDnsServer: boolean;
  AllowedDnsZones: string[];
}

export interface TlsCertificate {
  Thumbprint: string;
  StoreName: string;
  Subject: string | null;
  Issuer: string | null;
  DnsNames: string[];
  NotBeforeUtc: string | null;
  NotAfterUtc: string | null;
  HasPrivateKey: boolean;
  IisBindings: string[];
  Error: string | null;
}

export interface ServerTlsStatus {
  ServerId: string;
  IsOnline: boolean;
  LastSeenAtUtc: string | null;
  Snapshot: { CollectedAtUtc: string | null; Certificates: TlsCertificate[]; Error: string | null;
    WacsLogs?: { CollectedAtUtc: string | null; DirectoryPath: string; Error: string | null;
      Executions?: { Id: string; StartedAtUtc: string | null; CompletedAtUtc: string | null;
        TaskName: string; Result: string; Details: string; FileName: string; StartLine: number; EndLine: number }[] | null;
      Files: { FileName: string; LastWriteAtUtc: string; ErrorEntries: number | null; WarningEntries: number | null; Error: string | null }[] } | null;
    Websites?: { SiteName: string; State: string; HttpsBindings: string[]; HttpsUrls: string[] }[] | null;
    CentralStore?: { CollectedAtUtc: string | null; Enabled: boolean | null; DirectoryPath: string | null;
      PfxFileNames: string[]; Error: string | null;
      Certificates?: { FileName: string; Subject: string | null; Issuer: string | null; Thumbprint: string | null;
        NotBeforeUtc: string | null; NotAfterUtc: string | null; Error: string | null }[] | null } | null } | null;
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

export interface ProvisioningIisSiteBinding {
  SiteName: string;
  HostNames: string[];
  State?: string | null;
}

export interface ProvisioningAppPoolMetric {
  ApplicationPoolName: string;
  SiteNames: string[];
  HostNames: string[] | null;
  State: string;
  ProcessIds: number[];
  CpuUsagePercent: number | null;
  WorkingSetBytes: number;
  PrivateMemoryBytes: number;
  WebsiteNames: string[] | null;
  SiteBindings?: ProvisioningIisSiteBinding[] | null;
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
