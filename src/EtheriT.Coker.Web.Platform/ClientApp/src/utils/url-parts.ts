export const URL_SCHEMES = ["https", "http"] as const;
export type UrlScheme = (typeof URL_SCHEMES)[number];

const SCHEME_PREFIX = /^\s*(https?):\/\//i;

/** "https://www.a.com" → { scheme: "https", rest: "www.a.com" }；沒寫協定時 scheme 為 null */
export function splitUrl(value: string): { scheme: UrlScheme | null; rest: string } {
    const match = SCHEME_PREFIX.exec(value);
    if (!match) return { scheme: null, rest: value };
    return {
        scheme: match[1].toLowerCase() as UrlScheme,
        rest: value.slice(match[0].length)
    };
}

/** 輸入框空白時存空字串，不能存成 "https://"，否則選填的網址會被當成有填 */
export function joinUrl(scheme: UrlScheme, rest: string): string {
    const trimmed = rest.trim();
    return trimmed ? `${scheme}://${trimmed}` : "";
}