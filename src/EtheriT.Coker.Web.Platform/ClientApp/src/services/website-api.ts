import { api } from "@/core/coker";
import { fromDateInput, toDateInput } from "@/utils/date-input";
import {
    websiteLocaleOptions,
    type WebsiteDetail,
    type WebsiteForm,
    type WebsiteListResult
} from "@/types/website";

const baseUrl = "/api/websites";

function blankToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed === "" ? null : trimmed;
}

/**
 * 後台舊資料可能存成 "zh-TW"；轉小寫後不在選項內就當作未選擇，
 * 讓下拉顯示「未選擇」、由必填驗證提醒使用者重選，而不是送出一個後端不認得的值。
 */
export function toLocaleInput(value: string | null | undefined): string {
    const normalized = value?.trim().toLowerCase() ?? "";
    return websiteLocaleOptions.some(option => option.Value === normalized) ? normalized : "";
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
        TerminatedDate: fromDateInput(form.TerminatedDate),
        IsDomainPending: form.IsDomainPending,
        Url: form.IsDomainPending ? null : blankToNull(form.Url),
        Remark: blankToNull(form.Remark)
    };
}

/** API 回應轉成表單模型，日期欄位都要經過 toDateInput。 */
export function toWebsiteForm(detail: WebsiteDetail): WebsiteForm {
    return {
        FK_CompanyId: detail.FK_CompanyId,
        Name: detail.Name,
        Level: detail.Level,
        Locale: toLocaleInput(detail.Locale),
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

export function updateWebsite(id: number, form: WebsiteForm): Promise<WebsiteDetail> {
    return api.put<WebsiteDetail>(`${baseUrl}/${id}`, toSaveRequest(form));
}