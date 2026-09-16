import { api } from "@/core/coker";
import type {
    CustomerDetail,
    CustomerForm,
    CustomerListItem,
    CustomerLookup
} from "@/types/customer";

const baseUrl = "/api/companies";

export function fetchCustomers(): Promise<CustomerListItem[]> {
    return api.get<CustomerListItem[]>(baseUrl);
}

export function fetchCustomer(id: number): Promise<CustomerDetail> {
    return api.get<CustomerDetail>(`${baseUrl}/${id}`);
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

/** 統編查詢。查無資料回空陣列。 */
export function lookupByTaxId(
    taxId: string,
    excludeId?: number
): Promise<CustomerLookup[]> {
    return api.get<CustomerLookup[]>(`${baseUrl}/by-tax-id`, {
        query: { taxId, excludeId }
    });
}