import { api } from "@/core/coker";
import { fromDateInput, toDateInput } from "@/utils/date-input";
import type {
    DomainPasswordResult,
    WebsiteDetail,
    WebsiteForm,
    WebsiteListResult
} from "@/types/website";

const baseUrl = "/api/websites";

function blankToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed === "" ? null : trimmed;
}

function toSaveRequest(form: WebsiteForm) {
    return {
        FK_PlatformCustomerId: form.FK_PlatformCustomerId,
        Name: form.Name.trim(),
        Level: form.Level,
        HostLocation: blankToNull(form.HostLocation),
        ServiceStartDate: fromDateInput(form.ServiceStartDate),
        ServiceEndDate: fromDateInput(form.ServiceEndDate),
        Status: form.Status,
        TerminatedDate: fromDateInput(form.TerminatedDate),
        IsDomainPending: form.IsDomainPending,
        DomainName: blankToNull(form.DomainName),
        DomainRegistrar: blankToNull(form.DomainRegistrar),
        DomainStartDate: fromDateInput(form.DomainStartDate),
        DomainEndDate: fromDateInput(form.DomainEndDate),
        DomainPassword: form.ClearDomainPassword ? null : blankToNull(form.DomainPassword),
        ClearDomainPassword: form.ClearDomainPassword,
        Remark: blankToNull(form.Remark)
    };
}

/** API 回應轉成表單模型，六個日期欄位都要經過 toDateInput。 */
export function toWebsiteForm(detail: WebsiteDetail): WebsiteForm {
    return {
        FK_PlatformCustomerId: detail.FK_PlatformCustomerId,
        Name: detail.Name,
        Level: detail.Level,
        HostLocation: detail.HostLocation ?? "",
        ServiceStartDate: toDateInput(detail.ServiceStartDate),
        ServiceEndDate: toDateInput(detail.ServiceEndDate),
        Status: detail.Status,
        TerminatedDate: toDateInput(detail.TerminatedDate),
        IsDomainPending: detail.IsDomainPending,
        DomainName: detail.DomainName ?? "",
        DomainRegistrar: detail.DomainRegistrar ?? "",
        DomainStartDate: toDateInput(detail.DomainStartDate),
        DomainEndDate: toDateInput(detail.DomainEndDate),
        DomainPassword: "",
        ClearDomainPassword: false,
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

export function fetchDomainPassword(id: number): Promise<DomainPasswordResult> {
    return api.get<DomainPasswordResult>(`${baseUrl}/${id}/domain-password`);
}