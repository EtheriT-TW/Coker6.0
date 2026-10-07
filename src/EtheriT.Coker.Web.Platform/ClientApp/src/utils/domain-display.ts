import { EXPIRING_WITHIN_DAYS, type StatusView } from "@/utils/website-display";

export function domainStatusView(remainingDays: number | null): StatusView {
    if(remainingDays === null)
        return {Text: "未設定", CssClass: ""};
    if(remainingDays < 0 )
        return {Text: "已過期", CssClass: "status-pill-expired"};
    if(remainingDays <= EXPIRING_WITHIN_DAYS)
        return {Text: "即將到期", CssClass: "status-pill-expiring"};
    return {Text: "正常", CssClass: "status-pill-0"};
}