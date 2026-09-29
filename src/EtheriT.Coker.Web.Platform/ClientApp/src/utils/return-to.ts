/**
 * 從網址參數取回「從哪裡來」，只接受站內路徑。
 * 擋掉 https://evil.com、//evil.com、/\evil.com 這類會跳到外部網站的值（開放式重導向）。
 */
export function safeReturnTo(value: unknown): string | null {
    if (typeof value !== "string") return null;
    if (!value.startsWith("/") || value.startsWith("//") || value.startsWith("/\\")) return null;
    return value;
}