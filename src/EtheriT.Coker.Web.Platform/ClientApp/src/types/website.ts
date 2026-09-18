import type { DomainSummary } from "@/types/domain";

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

export const HostLocation = {
    主機28: "210.65.132.28",
    主機30: "210.65.132.30",
    主機31: "210.65.132.31",
    NAS: "NAS封存"
} as const;

export type HostLocationValue = (typeof HostLocation)[keyof typeof HostLocation];

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

export const hostLocationOptions: Array<{ Value: HostLocationValue; Text: string }> = [
    { Value: HostLocation.主機28, Text: "210.65.132.28" },
    { Value: HostLocation.主機30, Text: "210.65.132.30" },
    { Value: HostLocation.主機31, Text: "210.65.132.31" },
    { Value: HostLocation.NAS, Text: "NAS封存" }
];

/**
 * 清單是「後台站台」與「僅合約」的聯集，所以兩個 Id 都可能為 null：
 * - 兩個都有值＝後台站台，且已建立合約資料
 * - 只有 WebsiteId＝後台站台，合約還沒建（IsPending 為 true）
 * - 只有 PlatformWebsiteId＝合約已簽，但還沒對應到有效站台
 */
export interface WebsiteListItem {
    /** DataGrid 的 key；聯集後單一 Id 不再唯一，改用 "W{id}"／"P{id}" */
    RowKey: string;
    PlatformWebsiteId: number | null;
    WebsiteId: number | null;
    Name: string;
    OrgName: string | null;
    FK_CompanyId: number | null;
    CustomerName: string | null;
    CustomerTaxId: string | null;
    Level: WebsiteLevelValue | null;
    LevelText: string;
    /** null＝尚未建立合約資料 */
    Status: WebsiteStatusValue | null;
    StatusText: string;
    Url: string | null;
    ServiceEndDate: string | null;
    DomainEndDate: string | null;
    ServiceStartDate: string | null;
    /** 到期日 − 今天；未填到期日為 null */
    RemainingDays: number | null;
    /** true＝後台有站台但合約資料還沒建；不直接顯示，只用於導向與空值判斷 */
    IsPending: boolean;
}

export interface WebsiteListResult {
    Items: WebsiteListItem[];
    IsTruncated: boolean;
}

/** 後台 Websites 的唯讀投影；對應 C# 的 WebsiteSiteOptionDto。 */
export interface WebsiteSiteOption {
    Id: number;
    OrgName: string;
    Title: string;
    DefaultUrl: string | null;
    /** Website.Level 不可為 null，與本頁的 WebsiteForm.Level 不同 */
    Level: WebsiteLevelValue;
    Locale: string;
    StartDate: string | null;
    EndDate: string | null;
}

export interface WebsiteSiteOptionResult {
    Items: WebsiteSiteOption[];
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
    Url: string | null;
    Domain: DomainSummary | null;
    Remark: string | null;
    /** null＝尚未綁定實際站台 */
    FK_WebsiteId: number | null;
    /** null＝尚未綁定，或綁定的站台已被刪除 */
    LinkedSite: WebsiteSiteOption | null;
    Customer: WebsiteCustomer | null;
}

/** 表單模型：字串與日期一律用 ""；Status／Level 用 number，避免 v-model 在 vue-tsc 報型別錯。 */
export interface WebsiteForm {
    FK_CompanyId: number;
    FK_WebsiteId: number | null;
    Name: string;
    Level: number | null;
    HostLocation: string;
    ServiceStartDate: string;
    ServiceEndDate: string;
    Status: number;
    TerminatedDate: string;
    IsDomainPending: boolean;
    Url: string;
    Remark: string;
}