import { toLocaleInput } from "@/services/website-api";
import { toDateInput } from "@/utils/date-input";
import {
    websiteLevelOptions,
    websiteLocaleOptions,
    type WebsiteForm,
    type WebsiteSiteOption
} from "@/types/website";

/** 綁定後會寫進後台 Website 的欄位（與後端 SyncLinkedSiteAsync 一致）。 */
export const BINDING_FIELDS = ["Name", "Level", "Locale", "ServiceStartDate", "ServiceEndDate", "Url"] as const;
export type BindingField = (typeof BINDING_FIELDS)[number];

/** 差異彈窗的欄位：表單欄位之外再加上客戶（客戶不在 WebsiteForm 裡，要另外處理）。 */
export type BindingDiffField = BindingField | "Customer";

export interface BindingDiff {
    Field: BindingDiffField;
    Label: string;
    FormText: string;
    SiteText: string;
}

const FIELD_LABELS: Record<BindingField, string> = {
    Name: "網站名稱",
    Level: "網站版本",
    Locale: "語系",
    ServiceStartDate: "網站開通日期",
    ServiceEndDate: "網站到期日期",
    Url: "網址"
};

type BindingValues = Pick<WebsiteForm, BindingField>;

/** 站台資料轉成表單格式；比對與套用共用，確保兩邊用同一套轉換。 */
export function siteToFormValues(site: WebsiteSiteOption): BindingValues {
    return {
        Name: site.Title,
        Level: site.Level,
        Locale: toLocaleInput(site.Locale),
        ServiceStartDate: toDateInput(site.StartDate),
        ServiceEndDate: toDateInput(site.EndDate),
        Url: site.DefaultUrl ?? ""
    };
}

function isBlank(value: string | number | null): boolean {
    return value === null || (typeof value === "string" && value.trim() === "");
}

/** 網址比對忽略大小寫與結尾斜線：https://A.com/ 與 https://a.com 視為相同。 */
function isSame(field: BindingField, a: string | number | null, b: string | number | null): boolean {
    if (field === "Url") return String(a).trim().replace(/\/+$/, "").toLowerCase()
        === String(b).trim().replace(/\/+$/, "").toLowerCase();
    if (field === "Name") return String(a).trim() === String(b).trim();
    return a === b;
}

function toText(field: BindingField, value: string | number | null): string {
    if (isBlank(value)) return "（未填）";
    if (field === "Level") return websiteLevelOptions.find(option => option.Value === value)?.Text ?? String(value);
    if (field === "Locale") return websiteLocaleOptions.find(option => option.Value === value)?.Text ?? String(value);
    if (field === "ServiceStartDate" || field === "ServiceEndDate") return String(value).replaceAll("-", "/");
    return String(value);
}

/** 待申請時不寫後台網址，網址就不用比對。 */
function comparableFields(form: WebsiteForm): BindingField[] {
    return BINDING_FIELDS.filter(field => field !== "Url" || !form.IsDomainPending);
}

/** 表單還沒填、站台有值的欄位：直接用站台的，不必問。 */
export function blankFieldsToFill(form: WebsiteForm, site: WebsiteSiteOption): BindingField[] {
    const siteValues = siteToFormValues(site);
    return comparableFields(form).filter(field => isBlank(form[field]) && !isBlank(siteValues[field]));
}

/** 兩邊都有值但不一樣的欄位：要讓使用者選。 */
export function buildBindingDiffs(form: WebsiteForm, site: WebsiteSiteOption): BindingDiff[] {
    const siteValues = siteToFormValues(site);
    return comparableFields(form)
        .filter(field => !isBlank(form[field]) && !isSame(field, form[field], siteValues[field]))
        .map(field => ({
            Field: field,
            Label: FIELD_LABELS[field],
            FormText: toText(field, form[field]),
            SiteText: toText(field, siteValues[field])
        }));
}

/** CustomerLookup 與 WebsiteCustomer 共同的欄位。 */
interface CustomerRef {
    Id: number;
    Name: string;
    TaxId: string;
}

function customerText(customer: CustomerRef): string {
    return customer.TaxId ? `${customer.Name}（${customer.TaxId}）` : customer.Name;
}

/**
 * 兩邊都有客戶、但不是同一家才要問。
 * 本頁還沒帶客戶 → 直接用站台的；站台沒綁公司 → 存檔時由本頁的客戶寫入後台。
 */
export function buildCustomerDiff(current: CustomerRef | null, site: WebsiteSiteOption): BindingDiff | null {
    const siteCompany = site.Company;
    if (!current || !siteCompany || current.Id === siteCompany.Id) return null;
    return {
        Field: "Customer",
        Label: "客戶",
        FormText: customerText(current),
        SiteText: customerText(siteCompany)
    };
}