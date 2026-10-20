import { api, ApiError } from "@/core/coker";
import { EnterpriseEmail } from "@/types/enterprise-email";
import { fromDateInput } from "@/utils/date-input";
import type {
    CustomerDetail,
    CustomerForm,
    CustomerListItem,
    CustomerLookup,
    CustomerWebsite
} from "@/types/customer";

const baseUrl = "/api/companies";

function blankToNull(value: string): string | null {
    const trimmed = value.trim();
    return trimmed === "" ? null : trimmed;
}

/** 企業信箱五個欄位攤平在表單上，送出時收成一個物件；未使用＝null，後端會軟刪除既有那筆。 */
function toSaveRequest(form: CustomerForm) {
    const {
        EnterpriseEmailAccountType: accountType,
        EnterpriseEmailTotalAccounts: totalAccounts,
        EnterpriseEmailOtherType: otherType,
        EnterpriseEmailExpirationDate: expirationDate,
        EnterpriseEmailRemark: remark,
        ...customer
    } = form;

    return {
        ...customer,
        EnterpriseEmail: accountType === null
            ? null
            : {
                AccountType: accountType,
                TotalAccounts: Number(totalAccounts),
                OtherType: accountType === EnterpriseEmail.Other ? blankToNull(otherType) : null,
                ExpirationDate: fromDateInput(expirationDate),
                Remark: blankToNull(remark)
            }
    };
}

export function fetchCustomers(): Promise<CustomerListItem[]> {
    return api.get<CustomerListItem[]>(baseUrl);
}

export function fetchCustomer(id: number): Promise<CustomerDetail> {
    return api.get<CustomerDetail>(`${baseUrl}/${id}`);
}

/** 客戶底下的站台（唯讀）：Platform 網站資料 ∪ 後台公司綁定。 */
export function fetchCustomerWebsites(id: number): Promise<CustomerWebsite[]> {
    return api.get<CustomerWebsite[]>(`${baseUrl}/${id}/websites`);
}

export function createCustomer(form: CustomerForm): Promise<{ Id: number }> {
    return api.post<{ Id: number }>(baseUrl, toSaveRequest(form));
}

export function updateCustomer(id: number, form: CustomerForm): Promise<void> {
    return api.put<void>(`${baseUrl}/${id}`, toSaveRequest(form));
}

export function deleteCustomer(id: number): Promise<void> {
    return api.delete<void>(`${baseUrl}/${id}`);
}

/** 以統編或完整公司名稱帶出客戶；兩者都不可重複，最多一筆。查無回 null。 */
export async function lookupCustomer(keyword: string): Promise<CustomerLookup | null> {
    try {
        return await api.get<CustomerLookup>(`${baseUrl}/lookup`, { query: { keyword } });
    }
    catch (error) {
        if (error instanceof ApiError && error.status === 404) return null;
        throw error;
    }
}