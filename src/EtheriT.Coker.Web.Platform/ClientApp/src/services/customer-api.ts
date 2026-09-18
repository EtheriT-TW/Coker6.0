import { api, ApiError } from "@/core/coker";
import type {
    CustomerDetail,
    CustomerForm,
    CustomerListItem,
    CustomerLookup,
    CustomerWebsite
} from "@/types/customer";

const baseUrl = "/api/companies";

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
    return api.post<{ Id: number }>(baseUrl, form);
}

export function updateCustomer(id: number, form: CustomerForm): Promise<void> {
    return api.put<void>(`${baseUrl}/${id}`, form);
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