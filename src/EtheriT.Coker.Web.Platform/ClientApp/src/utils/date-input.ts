const DATE_PREFIX = /^(\d{4}-\d{2}-\d{2})/;

/**
 * API 回傳 "2026-09-15T00:00:00"，<input type="date"> 只吃 "yyyy-MM-dd"，
 * 格式不符會顯示空白，使用者再存檔就把原本的日期洗成 null。
 * 不要用 new Date().toISOString()：會轉成 UTC，台灣時間 00:00 會變前一天。
 */
export function toDateInput(value: string | null | undefined): string {
    if (!value) return "";
    return DATE_PREFIX.exec(value)?.[1] ?? "";
}

/** 空字串 "" 送給 DateTime? 會轉換失敗，必須轉成 null。 */
export function fromDateInput(value: string): string | null {
    return value === "" ? null : value;
}


function pad2(value: number): string {
    return String(value).padStart(2, "0");
}

/** 本地日期轉 "yyyy-MM-dd"；不用 toISOString，理由同 toDateInput。 */
function formatDateInput(date: Date): string {
    return `${date.getFullYear()}-${pad2(date.getMonth() + 1)}-${pad2(date.getDate())}`;
}

/** 今天（使用者電腦的本地日期）。 */
export function todayDateInput(): string {
    return formatDateInput(new Date());
}