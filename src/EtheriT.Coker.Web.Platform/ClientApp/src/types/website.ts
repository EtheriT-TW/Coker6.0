/** 對應 C# 的 PlatformWebsiteStatusEnum。 */
export const WebsiteStatus = {
    正常: 0,
    暫停: 1,
    註銷: 2
} as const;

export type WebsiteStatusValue = (typeof WebsiteStatus)[keyof typeof WebsiteStatus];

/** 對應 C# 的 WebsiteLevelEnum（0 是真實選項「簡約」，所以欄位可為 null）。 */
export const WebsiteLevel = {
    簡約: 0,
    形象: 1,
    會員: 2,
    購物: 3
} as const;

export type WebsiteLevelValue = (typeof WebsiteLevel)[keyof typeof WebsiteLevel];

export const websiteStatusOptions: Array<{ Value: WebsiteStatusValue; Text: string }> = [
    { Value: WebsiteStatus.正常, Text: "正常" },
    { Value: WebsiteStatus.暫停, Text: "暫停" },
    { Value: WebsiteStatus.註銷, Text: "註銷" }
];

export const websiteLevelOptions: Array<{ Value: WebsiteLevelValue; Text: string }> = [
    { Value: WebsiteLevel.簡約, Text: "簡約" },
    { Value: WebsiteLevel.形象, Text: "形象" },
    { Value: WebsiteLevel.會員, Text: "會員" },
    { Value: WebsiteLevel.購物, Text: "購物" }
];

export interface WebsiteListItem {
    Id: number;
    Name: string;
    FK_CompanyId: number;
    CustomerName: string | null;
    CustomerTaxId: string | null;
    Level: WebsiteLevelValue | null;
    LevelText: string;
    Status: WebsiteStatusValue;
    StatusText: string;
    DomainName: string | null;
    ServiceEndDate: string | null;
    DomainEndDate: string | null;
}

export interface WebsiteListResult {
    Items: WebsiteListItem[];
    IsTruncated: boolean;
}

export interface WebsiteCustomer {
    Id: number;
    Name: string;
    TaxId: string;
    Phone: string | null;
    Email: string | null;
    PrimaryContactName: string | null;
}

export interface WebsiteDetail {
    Id: number;
    FK_CompanyId: number;
    Name: string;
    Level: WebsiteLevelValue | null;
    HostLocation: string | null;
    ServiceStartDate: string | null;
    ServiceEndDate: string | null;
    Status: WebsiteStatusValue;
    TerminatedDate: string | null;
    IsDomainPending: boolean;
    DomainName: string | null;
    DomainRegistrar: string | null;
    DomainStartDate: string | null;
    DomainEndDate: string | null;
    HasDomainPassword: boolean;
    Remark: string | null;
    Customer: WebsiteCustomer | null;
}

/** 表單模型：字串與日期一律用 ""；Status／Level 用 number，避免 v-model 在 vue-tsc 報型別錯。 */
export interface WebsiteForm {
    FK_CompanyId: number;
    Name: string;
    Level: number | null;
    HostLocation: string;
    ServiceStartDate: string;
    ServiceEndDate: string;
    Status: number;
    TerminatedDate: string;
    IsDomainPending: boolean;
    DomainName: string;
    DomainRegistrar: string;
    DomainStartDate: string;
    DomainEndDate: string;
    DomainPassword: string;
    ClearDomainPassword: boolean;
    Remark: string;
}

export interface DomainPasswordResult {
    State: "Ok" | "Empty" | "Unreadable";
    Password: string | null;
}