import type { WebsiteLevelValue, WebsiteStatusValue } from "@/types/website";

export const CustomerType = {
    未設定: 0,
    經銷商: 1,
    一般客戶: 2,
    其他: 9
} as const;

export type CustomerTypeValue = (typeof CustomerType)[keyof typeof CustomerType];

export const customerTypeOptions: Array<{ Value: CustomerTypeValue; Text: string }> = [
    { Value: CustomerType.經銷商, Text: "經銷商" },
    { Value: CustomerType.一般客戶, Text: "一般客戶" },
    { Value: CustomerType.其他, Text: "其他" }
];

export interface CustomerListItem {
    Id: number;
    Name: string;
    TaxId: string;
    Phone: string | null;
    Email: string | null;
    SalesOwner: string | null;
    CustomerTypeText: string;
    PrimaryContactName: string | null;
}
export interface CustomerContact {
    Id: number;
    Name: string;
    JobTitle: string | null;
    Phone: string | null;
    Email: string | null;
    Sort: number;
}

export interface CustomerDetail {
    Id: number;
    Name: string;
    TaxId: string;
    Phone: string | null;
    Email: string | null;
    Address: string | null;
    InvoiceInfo: string | null;
    SalesOwner: string | null;
    CustomerType: CustomerTypeValue;
    CustomerTypeOther: string | null;
    PrimaryContactName: string | null;
    PrimaryContactJobTitle: string | null;
    PrimaryContactPhone: string | null;
    PrimaryContactEmail: string | null;
    SubContacts: CustomerContact[];
}

export interface CustomerLookup {
    Id: number;
    Name: string;
    TaxId: string;
    Phone: string | null;
    Email: string | null;
    PrimaryContactName: string | null;
}

export interface CustomerContactForm {
    /** 大於 0 為既有列；負數為前端暫時 key（送到後端會走新增）。 */
    Id: number;
    Name: string;
    JobTitle: string;
    Phone: string;
    Email: string;
}

export interface CustomerForm {
    Name: string;
    TaxId: string;
    Phone: string;
    Email: string;
    Address: string;
    InvoiceInfo: string;
    SalesOwner: string;
    /** 用 number 而非字面值聯集，避免 v-model.number 在 vue-tsc 報型別錯。 */
    CustomerType: number;
    CustomerTypeOther: string;
    PrimaryContactName: string;
    PrimaryContactJobTitle: string;
    PrimaryContactPhone: string;
    PrimaryContactEmail: string;
    SubContacts: CustomerContactForm[];
}

/** 客戶底下的站台（唯讀）＝後台綁到這個客戶的站台。 */
export interface CustomerWebsite {
    /** 後台 Websites.Id，也是網站編輯頁路由的 id */
    WebsiteId: number;
    Name: string;
    OrgName: string;
    Level: WebsiteLevelValue;
    LevelText: string;
    Status: WebsiteStatusValue;
    StatusText: string;
    Url: string | null;
    ServiceStartDate: string | null;
    ServiceEndDate: string | null;
    RemainingDays: number | null;
}