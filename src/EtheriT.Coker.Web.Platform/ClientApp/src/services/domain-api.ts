import { api, rules, type ValidationSchema } from "@/core/coker";
import { fromDateInput, toDateInput } from "@/utils/date-input";
import type {
    DomainDetail,
    DomainForm,
    DomainListResult,
    DomainMatch,
    DomainPasswordResult
} from "@/types/domain";

const baseUrl = "/api/domains";

function blankToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed === "" ? null : trimmed;
}

export function emptyDomainForm(domainName = ""): DomainForm {
    return {
        DomainName: domainName,
        Registrar: "",
        StartDate: "",
        EndDate: "",
        Password: "",
        ClearPassword: false,
        Remark: ""
    };
}

/** 編輯頁與快速建立彈窗共用。網域格式由後端正規化後再驗，前端只擋必填與長度。 */
export const domainValidation: ValidationSchema<DomainForm> = {
    DomainName: [rules.required("請輸入網域。"), rules.maxLength(255)],
    EndDate: [
        rules.custom<DomainForm>((value, model) =>
            value && model.StartDate && String(value) < model.StartDate
                ? "網域到期日期不可早於起始日期。"
                : null)
    ],
    Password: [rules.maxLength(200)]
};

function toSaveRequest(form: DomainForm) {
    return {
        DomainName: form.DomainName.trim(),
        Registrar: blankToNull(form.Registrar),
        StartDate: fromDateInput(form.StartDate),
        EndDate: fromDateInput(form.EndDate),
        Password: form.ClearPassword ? null : blankToNull(form.Password),
        ClearPassword: form.ClearPassword,
        Remark: blankToNull(form.Remark)
    };
}

export function toDomainForm(detail: DomainDetail): DomainForm {
    return {
        DomainName: detail.DomainName,
        Registrar: detail.Registrar ?? "",
        StartDate: toDateInput(detail.StartDate),
        EndDate: toDateInput(detail.EndDate),
        Password: "",
        ClearPassword: false,
        Remark: detail.Remark ?? ""
    };
}

export function fetchDomains(): Promise<DomainListResult> {
    return api.get<DomainListResult>(baseUrl);
}

export function fetchDomain(id: number): Promise<DomainDetail> {
    return api.get<DomainDetail>(`${baseUrl}/${id}`);
}

export function createDomain(form: DomainForm): Promise<DomainDetail> {
    return api.post<DomainDetail>(baseUrl, toSaveRequest(form));
}

export function updateDomain(id: number, form: DomainForm): Promise<DomainDetail> {
    return api.put<DomainDetail>(`${baseUrl}/${id}`, toSaveRequest(form));
}

export function fetchDomainPassword(id: number): Promise<DomainPasswordResult> {
    return api.get<DomainPasswordResult>(`${baseUrl}/${id}/password`);
}

export function matchDomain(url: string): Promise<DomainMatch> {
    return api.get<DomainMatch>(`${baseUrl}/match`, { query: { url } });
}
