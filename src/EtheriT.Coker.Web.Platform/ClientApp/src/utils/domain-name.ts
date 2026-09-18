/**
 * 前端即時顯示用：把貼上的網址整理成網域（去協定、路徑、通訊埠、www.）。
 * 規則對齊後端 Services/PlatformDomainName.cs，但這裡只負責「讓使用者看到乾淨的值」，
 * 不做驗證 —— 整理不出結果就原樣保留，格式錯誤的訊息一律由後端回。
 */

/** 與後端 HostPattern() 同一份規則 */
const HOST_PATTERN = /^(?=.{1,253}$)([a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z0-9-]{2,63}$/;

/**
 * 常見的「兩段式公共後綴」。列在這裡的，註冊網域要保留三段（example.com.tw），
 * 其餘一律保留兩段（example.com）。清單外的冷門後綴會判斷錯，需要時再補。
 */
const TWO_LABEL_SUFFIXES = new Set([
    "com.tw", "net.tw", "org.tw", "gov.tw", "edu.tw", "idv.tw", "game.tw", "ebiz.tw", "club.tw",
    "com.cn", "net.cn", "org.cn",
    "com.hk", "com.sg", "com.my", "com.au", "com.br",
    "co.jp", "ne.jp", "or.jp",
    "co.kr", "co.uk", "org.uk", "co.nz", "co.th", "co.id", "co.in"
]);

/** shop.example.com.tw → example.com.tw；blog.example.com → example.com */
function stripSubdomain(host: string): string {
    const labels = host.split(".");
    if (labels.length <= 2) return host;

    const keep = TWO_LABEL_SUFFIXES.has(labels.slice(-2).join(".")) ? 3 : 2;
    return labels.length <= keep ? host : labels.slice(-keep).join(".");
}

export function toDomainName(input: string): string {
    const text = input.trim();
    if (text === "") return "";

    // 沒寫協定就補一個，URL 才解得開
    const withScheme = text.includes("://") ? text : `http://${text}`;

    let host: string;
    try {
        // 瀏覽器的 URL 會順便把中文網域轉成 xn-- 形式，與後端 IdnHost 一致
        host = new URL(withScheme).hostname.replace(/\.$/, "").toLowerCase();
    }
    catch {
        return text;
    }

    // 不是合法主機名稱（IP、localhost、打字打一半）就原樣保留
    if (!HOST_PATTERN.test(host)) return text;

    // 去掉子網域後必須還有兩段以上，否則 www.tw 會變成無效的 tw
    const stripped = stripSubdomain(host.startsWith("www.") ? host.slice(4) : host);
    return stripped.includes(".") ? stripped : host;
}