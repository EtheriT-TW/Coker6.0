import { api } from "@/core/coker";
import { fromDateInput, toDateInput } from "@/utils/date-input";
import { toBindingHost } from "@/utils/domain-name";
import type { DomainSummary } from "@/types/domain";
import {
    websiteLocaleOptions,
    EnterpriseEmail,
    type IisBindingResult,
    type WebsiteDetail,
    type WebsiteForm,
    type WebsiteListResult,
    type WebsiteOptions,
    type WebsiteUrlFormItem
} from "@/types/website";
import type { WebsiteBindingComparison } from "@/types/website";

const baseUrl = "/api/websites";

let nextUrlKey = 1;

function blankToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed === "" ? null : trimmed;
}

function toEnterpriseEmailRequest(form: WebsiteForm){
    if(form.EnterpriseEmailAccountType === null)
        return null;

        return{
            AccountType: form.EnterpriseEmailAccountType,
            TotalAccounts: Number(form.EnterpriseEmailTotalAccounts),
            OtherType: form.EnterpriseEmailAccountType === EnterpriseEmail.Other ? blankToNull(form.EnterpriseEmailOtherType) : null,
            ExpirationDate: fromDateInput(form.EnterpriseEmailExpirationDate),
            Remark: blankToNull(form.EnterpriseEmailRemark)
        }
}

/**
 * 後台舊資料可能存成 "zh-TW"；轉小寫後不在選項內就當作未選擇，
 * 讓下拉顯示「未選擇」、由必填驗證提醒使用者重選，而不是送出一個後端不認得的值。
 */
export function toLocaleInput(value: string | null | undefined): string {
    const normalized = value?.trim().toLowerCase() ?? "";
    return websiteLocaleOptions.some(option => option.Value === normalized) ? normalized : "";
}

/** 建立表單上的一筆其他網址；Key 遞增，在頁面存活期間不重複即可。 */
export function newWebsiteUrlItem(url = "", domain: DomainSummary | null = null): WebsiteUrlFormItem {
    return { Key: nextUrlKey++, Url: url, InitialDomain: domain };
}

/** 與後端產生 DefaultUrl 的規則一致：https://{OrgName，_ 轉 -}{後綴}；站台代碼或後綴空白時回傳 ""。 */
export function toDefaultUrl(orgName: string, suffix: string): string {
    const name = orgName.trim().toLowerCase().replace(/_/g, "-");
    return name && suffix ? `https://${name}${suffix}` : "";
}

function toSaveRequest(form: WebsiteForm) {
    return {
        FK_CompanyId: form.FK_CompanyId,
        Name: form.Name.trim(),
        Level: form.Level,
        Locale: blankToNull(form.Locale),
        HostLocation: blankToNull(form.HostLocation),
        ServiceStartDate: fromDateInput(form.ServiceStartDate),
        ServiceEndDate: fromDateInput(form.ServiceEndDate),
        Status: form.Status,
        SuspendedDate: fromDateInput(form.SuspendedDate),
        TerminatedDate: fromDateInput(form.TerminatedDate),
        // 只送其他網址；主要網址由後台管理（新增時後端依 OrgName 產生）。Key、InitialDomain 只是畫面用
        Urls: form.Urls.map(item => ({ Url: item.Url.trim() })),
        Remark: blankToNull(form.Remark),
        EnterpriseEmail: toEnterpriseEmailRequest(form)
    };
}

/** API 回應轉成表單模型，日期欄位都要經過 toDateInput。 */
export function toWebsiteForm(detail: WebsiteDetail): WebsiteForm {
    // 同一主機的HTTP／HTTPS僅顯示一列；舊資料有兩列時優先顯示HTTPS。
    const primaryHost = toBindingHost(detail.DefaultUrl ?? '');
    const urls = new Map<string, WebsiteDetail['Urls'][number]>();
    for (const item of detail.Urls) {
        const host = toBindingHost(item.Url);
        if (host && host === primaryHost) continue;
        const key = host || item.Url.trim().toLowerCase();
        const previous = urls.get(key);
        if (!previous || (item.Url.toLowerCase().startsWith('https://') && !previous.Url.toLowerCase().startsWith('https://')))
            urls.set(key, item);
    }
    return {
        OrgName: detail.OrgName,
        FK_CompanyId: detail.FK_CompanyId,
        Name: detail.Name,
        Level: detail.Level,
        Locale: toLocaleInput(detail.Locale),
        HostLocation: detail.HostLocation ?? "",
        ServiceStartDate: toDateInput(detail.ServiceStartDate),
        ServiceEndDate: toDateInput(detail.ServiceEndDate),
        Status: detail.Status,
        SuspendedDate: toDateInput(detail.SuspendedDate),
        TerminatedDate: toDateInput(detail.TerminatedDate),
        Urls: [...urls.values()].map(item => newWebsiteUrlItem(item.Url, item.Domain)),
        Remark: detail.Remark ?? "",
        EnterpriseEmailAccountType: detail.EnterpriseEmail?.AccountType ?? null,
        EnterpriseEmailTotalAccounts: detail.EnterpriseEmail?.TotalAccounts.toString() ?? "",
        EnterpriseEmailOtherType: detail.EnterpriseEmail?.OtherType ?? "",
        EnterpriseEmailExpirationDate: toDateInput(detail.EnterpriseEmail?.ExpirationDate),
        EnterpriseEmailRemark: detail.EnterpriseEmail?.Remark ?? ""
    };
}

export function fetchWebsites(): Promise<WebsiteListResult> {
    return api.get<WebsiteListResult>(baseUrl);
}

export function fetchWebsite(id: number): Promise<WebsiteDetail> {
    return api.get<WebsiteDetail>(`${baseUrl}/${id}`);
}

export function fetchWebsiteOptions(): Promise<WebsiteOptions> {
    return api.get<WebsiteOptions>(`${baseUrl}/options`);
}

/** hosts 會展開成重複的 query：?hostLocation=x&hosts=a&hosts=b */
export function fetchIisBindings(hostLocation: string, hosts: string[]): Promise<IisBindingResult> {
    return api.get<IisBindingResult>(`${baseUrl}/iis-bindings`, { query: { hostLocation, hosts } });
}

/** 新增站台：只有這裡會送 OrgName。 */
export function createWebsite(form: WebsiteForm): Promise<WebsiteDetail> {
    return api.post<WebsiteDetail>(baseUrl, {
        ...toSaveRequest(form),
        OrgName: form.OrgName.trim().toLowerCase()
    });
}

export function updateWebsite(id: number, form: WebsiteForm, synchronizeBindings = false): Promise<WebsiteDetail> {
    return api.put<WebsiteDetail>(`${baseUrl}/${id}`, { ...toSaveRequest(form), SynchronizeBindings: synchronizeBindings });
}

export function fetchWebsiteBindingComparison(id: number): Promise<WebsiteBindingComparison> {
    return api.get<WebsiteBindingComparison>(`${baseUrl}/${id}/iis-comparison`);
}

export function refreshWebsiteUrls(id: number): Promise<WebsiteBindingComparison> {
    return api.post<WebsiteBindingComparison>(`${baseUrl}/${id}/refresh-urls`, {});
}
