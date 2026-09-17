export interface DomainListItem {
    Id: number;
    DomainName: string;
    Registrar: string | null;
    StartDate: string | null;
    EndDate: string | null;
    WebsiteCount: number;
}

export interface DomainListResult {
    Items: DomainListItem[];
    IsTruncated: boolean;
}

export interface DomainDetail {
    Id: number;
    DomainName: string;
    Registrar: string | null;
    StartDate: string | null;
    EndDate: string | null;
    HasPassword: boolean;
    Remark: string | null;
    /** 大於 0 時後端不允許修改網域名稱。 */
    WebsiteCount: number;
}

export interface DomainSummary {
    Id: number;
    DomainName: string;
    Registrar: string | null;
    EndDate: string | null;
}

/** Host＝null：網址格式不正確；Domain＝null：網域尚未建立。 */
export interface DomainMatch {
    Host: string | null;
    SuggestedDomainName: string | null;
    Domain: DomainSummary | null;
}

/** 表單模型：字串與日期一律用 ""。 */
export interface DomainForm {
    DomainName: string;
    Registrar: string;
    StartDate: string;
    EndDate: string;
    Password: string;
    ClearPassword: boolean;
    Remark: string;
}

export interface DomainPasswordResult {
    State: "Ok" | "Empty" | "Unreadable";
    Password: string | null;
}
