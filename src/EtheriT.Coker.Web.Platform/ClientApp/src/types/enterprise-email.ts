/** 對應 C# 的 EnterpriseEmailTypeEnum。 */
export const EnterpriseEmail = {
    Starter: 1,
    Standard: 2,
    Other: 3
} as const;

export type EnterpriseEmailValue = (typeof EnterpriseEmail)[keyof typeof EnterpriseEmail];

export const enterpriseEmailOption: Array<{ Value: EnterpriseEmailValue; Text: string }> = [
    { Value: EnterpriseEmail.Starter, Text: "Google Workspace Business Starter" },
    { Value: EnterpriseEmail.Standard, Text: "Google Workspace Business Standard" },
    { Value: EnterpriseEmail.Other, Text: "其他" }
];

/** 客戶的企業信箱，一個客戶最多一筆。 */
export interface EnterpriseEmailDetail {
    AccountType: EnterpriseEmailValue;
    TotalAccounts: number;
    OtherType: string | null;
    ExpirationDate: string;
    Remark: string | null;
}
