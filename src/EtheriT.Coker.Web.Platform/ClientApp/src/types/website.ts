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

export const EnterpriseEmail = {
    Starter: 1,
    Standard: 2,
    Other: 3
} as const;

export type EnterpriseEmailValue = (typeof EnterpriseEmail)[keyof typeof EnterpriseEmail];

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

/** 值要與後端 WebsiteLocales.All、Website.Locale 一致（小寫文化代碼）。 */
export const websiteLocaleOptions: Array<{ Value: string; Text: string }> = [
    { Value: "zh-tw", Text: "中文（繁體）" },
    { Value: "en", Text: "英文" }
];

export const enterpriseEmailOption: Array<{ Value: EnterpriseEmailValue; Text: string }> = [
    {Value: EnterpriseEmail.Starter, Text:"Google Workspace Business Starter"},
    {Value: EnterpriseEmail.Standard, Text:"Google Workspace Business Standard"},
    {Value: EnterpriseEmail.Other, Text:"其他"}
]

/** GET /api/websites/options：主機清單與新增網站時的預設值，由後端設定提供，前端不寫死。 */
export interface WebsiteOptions {
    HostLocations: Array<{ Value: string; Text: string }>;
    /** key 是網站版本數值轉成的字串（JSON 物件的 key 一律是字串），value 是主機的 Value */
    DefaultHostByLevel: Record<string, string>;
    /** 例：".coker.ezsale.tw"；預設網址＝https://{OrgName 的 _ 轉 -}{後綴} */
    DefaultUrlSuffix: string;
}

/** Ok＝有最新資料；NoAgent＝主機沒裝回報程式；Offline＝回報中斷、資料過期；Unavailable＝查詢失敗 */
export type IisBindingStatus = "Ok" | "NoAgent" | "Offline" | "Unavailable";

export interface IisBindingItem {
    Host: string;
    /** null＝主機上沒有這個綁定 */
    SiteName: string | null;
}

/** GET /api/websites/iis-bindings */
export interface IisBindingResult {
    Status: IisBindingStatus;
    LastSeenAtUtc: string | null;
    Items: IisBindingItem[];
}

/** 一筆網址在畫面上的 IIS 綁定提示，只是參考資訊，不擋存檔。 */
export interface IisBindingHint {
    Tone: "ok" | "warning" | "muted";
    Text: string;
}

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

/** 網站的其他網址（存在 WebsiteAlternateUrls）；主要網址是 WebsiteDetail.DefaultUrl，不在這裡。 */
export interface WebsiteUrl {
    Url: string;
    /** null＝網域管理中沒有對應的網域（或後台舊資料沒比對過） */
    Domain: DomainSummary | null;
}

/** 表單上的一筆其他網址。Key 只給 v-for 與彈窗追蹤用，不送後端。 */
export interface WebsiteUrlFormItem {
    Key: number;
    Url: string;
    /** 載入時後端比對出的網域，給列元件當初始顯示；新增的列為 null */
    InitialDomain: DomainSummary | null;
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
    SuspendedDate: string | null;
    TerminatedDate: string | null;
    /** 後台 Website.DefaultUrl，Platform 只顯示不修改；null／""＝未設定 */
    DefaultUrl: string | null;
    /** 舊資料的「網域待申請」；載入時改成在其他網址補一筆預設網址 */
    IsDomainPending: boolean;
    /** 只有其他網址，可為空陣列 */
    Urls: WebsiteUrl[];
    Remark: string | null;
    /** null＝後台還沒綁公司 */
    Customer: WebsiteCustomer | null;
    EnterpriseEmail: WebsiteEnterpriseEmail | null;
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
    SuspendedDate: string;
    TerminatedDate: string;
    /** 只有其他網址；主要網址不在表單內 */
    Urls: WebsiteUrlFormItem[];
    Remark: string;
    EnterpriseEmailAccountType: number | null;
    EnterpriseEmailTotalAccounts: string;
    EnterpriseEmailOtherType: string;
    EnterpriseEmailExpirationDate: string;
    EnterpriseEmailRemark: string;
}

export interface WebsiteEnterpriseEmail{
    AccountType: EnterpriseEmailValue;
    TotalAccounts: number;
    OtherType: string | null;
    ExpirationDate: string;
    Remark: string | null;
}

export interface WebsiteBindingComparison {
    Status: string;
    Message: string | null;
    SiteName: string | null;
    LastSeenAtUtc: string | null;
    SiteHosts: string[];
    OtherBindings: { Host: string; SiteName: string | null }[];
}
