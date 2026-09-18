import { WebsiteStatus, type WebsiteStatusValue } from "@/types/website";

/** 剩餘天數在 0～此值之間視為「即將到期」 */
export const EXPIRING_WITHIN_DAYS = 60;

/** 網站管理清單與客戶編輯頁共用；兩邊的列型別都符合這個形狀。 */
export interface WebsiteStatusSource {
    Status: WebsiteStatusValue | null;
    StatusText: string;
    RemainingDays: number | null;
}

export interface StatusView {
    Text: string;
    CssClass: string;
}

/** 只放行 http/https；沒寫協定的補 https://，其他協定（如 javascript:）不產生連結 */
export function toHref(url: string | null): string | null {
    const value = url?.trim();
    if (!value) return null;
    if (/^https?:\/\//i.test(value)) return value;
    if (/^[a-z][a-z0-9+.-]*:/i.test(value)) return null;
    return `https://${value}`;
}

export function remainingDaysClass(days: number): string {
    return days <= EXPIRING_WITHIN_DAYS ? "days-expiring" : "days-normal";
}

/** 狀態為「正常」時依剩餘天數改顯示；暫停／註銷維持原狀態 */
export function statusView(site: WebsiteStatusSource): StatusView {
    if (site.Status === WebsiteStatus.正常 && site.RemainingDays !== null) {
        if (site.RemainingDays < 0)
            return { Text: "已過期", CssClass: "status-pill-expired" };
        if (site.RemainingDays <= EXPIRING_WITHIN_DAYS)
            return { Text: "即將到期", CssClass: "status-pill-expiring" };
    }
    return { Text: site.StatusText, CssClass: `status-pill-${site.Status}` };
}