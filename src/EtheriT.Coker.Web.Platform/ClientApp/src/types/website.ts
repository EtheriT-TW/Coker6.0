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

/** 值要與後端 WebsiteLocales.All、Website.Locale 一致（小寫文化代碼）。 */
export const websiteLocaleOptions: Array<{ Value: string; Text: string }> = [
    { Value: "zh-tw", Text: "中文（繁體）" },
    { Value: "en", Text: "英文" }
];

/**
 * 網站管理清單的一列，一列＝一個後台站台（Websites）。
 * 客戶取後台綁定的公司；沒綁公司時 FK_CompanyId／Customer* 為 null。
 */
export interface WebsiteListItem {
    /** 後台 Websites.Id，也是編輯頁路由的 id */
    Id: number;
    Name: string;
    OrgName: string;
    FK_CompanyId: number | null;
    CustomerName: string | null;
    CustomerTaxId: string | null;
    Level: WebsiteLevelValue;
    LevelText: string;
    Status: WebsiteStatusValue;
    StatusText: string;
    Url: string | null;
    ServiceEndDate: string | null;
    DomainEndDate: string | null;
    ServiceStartDate: string | null;
    /** 到期日 − 今天；未填到期日為 null */
    RemainingDays: number | null;
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
    /** 後台 Websites.Id */
    Id: number;
    /** 站台代碼，唯讀 */
    OrgName: string;
    /** 0＝後台還沒綁公司 */
    FK_CompanyId: number;
    Name: string;
    Level: WebsiteLevelValue;
    Locale: string;
    HostLocation: string | null;
    ServiceStartDate: string | null;
    ServiceEndDate: string | null;
    Status: WebsiteStatusValue;
    TerminatedDate: string | null;
    IsDomainPending: boolean;
    Url: string | null;
    Domain: DomainSummary | null;
    Remark: string | null;
    /** null＝後台還沒綁公司 */
    Customer: WebsiteCustomer | null;
}

/** 表單模型：字串與日期一律用 ""；Status／Level 用 number，避免 v-model 在 vue-tsc 報型別錯。 */
export interface WebsiteForm {
    /** 站台代碼：新增時可填，編輯時唯讀（送出時後端也不會讀） */
    OrgName: string;
    FK_CompanyId: number;
    Name: string;
    Level: number | null;
    Locale: string;
    HostLocation: string;
    ServiceStartDate: string;
    ServiceEndDate: string;
    Status: number;
    TerminatedDate: string;
    IsDomainPending: boolean;
    Url: string;
    Remark: string;
}