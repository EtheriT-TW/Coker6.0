import { api } from "@/core/coker";
import { fromDateInput, toDateInput } from "@/utils/date-input";
import type {
    WebsiteDetail,
    WebsiteForm,
    WebsiteListResult,
    WebsiteSiteOption,
    WebsiteSiteOptionResult
} from "@/types/website";

const baseUrl = "/api/websites";

function blankToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed === "" ? null : trimmed;
}

function toSaveRequest(form: WebsiteForm) {
    return {
        FK_CompanyId: form.FK_CompanyId,
        FK_WebsiteId: form.FK_WebsiteId,
        Name: form.Name.trim(),
        Level: form.Level,
        HostLocation: blankToNull(form.HostLocation),
        ServiceStartDate: fromDateInput(form.ServiceStartDate),
        ServiceEndDate: fromDateInput(form.ServiceEndDate),
        Status: form.Status,
        TerminatedDate: fromDateInput(form.TerminatedDate),
        IsDomainPending: form.IsDomainPending,
        Url: form.IsDomainPending ? null : blankToNull(form.Url),
        Remark: blankToNull(form.Remark)
    };
}

/** API 回應轉成表單模型，六個日期欄位都要經過 toDateInput。 */
export function toWebsiteForm(detail: WebsiteDetail): WebsiteForm {
    return {
        FK_CompanyId: detail.FK_CompanyId,
        FK_WebsiteId: detail.FK_WebsiteId,
        Name: detail.Name,
        Level: detail.Level,
        HostLocation: detail.HostLocation ?? "",
        ServiceStartDate: toDateInput(detail.ServiceStartDate),
        ServiceEndDate: toDateInput(detail.ServiceEndDate),
        Status: detail.Status,
        TerminatedDate: toDateInput(detail.TerminatedDate),
        IsDomainPending: detail.IsDomainPending,
        Url: detail.Url ?? "",
        Remark: detail.Remark ?? ""
    };
}

export function fetchWebsites(): Promise<WebsiteListResult> {
    return api.get<WebsiteListResult>(baseUrl);
}

export function fetchWebsite(id: number): Promise<WebsiteDetail> {
    return api.get<WebsiteDetail>(`${baseUrl}/${id}`);
}

export function createWebsite(form: WebsiteForm): Promise<WebsiteDetail> {
    return api.post<WebsiteDetail>(baseUrl, toSaveRequest(form));
}

export function updateWebsite(id: number, form: WebsiteForm): Promise<WebsiteDetail> {
    return api.put<WebsiteDetail>(`${baseUrl}/${id}`, toSaveRequest(form));
}

/** 搜尋可綁定的實際站台；keyword 留空時回傳前 20 筆。 */
export function fetchSiteOptions(keyword: string): Promise<WebsiteSiteOptionResult> {
    return api.get<WebsiteSiteOptionResult>(`${baseUrl}/site-options`, { query: { keyword } });
}

/** 依 Id 取單筆站台；清單上尚未建合約的列會直接指定站台，不受搜尋筆數上限影響。 */
export function fetchSiteOption(id: number): Promise<WebsiteSiteOption> {
    return api.get<WebsiteSiteOption>(`${baseUrl}/site-options/${id}`);
}
