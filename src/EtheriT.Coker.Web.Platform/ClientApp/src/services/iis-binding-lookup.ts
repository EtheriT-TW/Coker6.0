import { onBeforeUnmount, ref, watch } from "vue";
import { fetchIisBindings } from "@/services/website-api";
import type { IisBindingHint, IisBindingResult } from "@/types/website";
import { toBindingHost } from "@/utils/domain-name";

// 等使用者停手再查，避免每打一個字就打一次 API
const LOOKUP_DELAY_MS = 400;

/** 後端給 UTC；沒帶時區的字串 Date 會當成本地時間，要補 Z。 */
function formatLastSeen(value: string | null): string {
    if (!value) return "";
    const date = new Date(/(z|[+-]\d{2}:\d{2})$/i.test(value) ? value : `${value}Z`);
    return Number.isNaN(date.getTime())
        ? ""
        : `（最後回報 ${date.toLocaleString("zh-TW", { hour12: false })}）`;
}

/**
 * 查詢主機上的 IIS 綁定，只是參考資訊，不擋存檔。
 * 主機或網址一變就作廢舊請求（sequence），舊回應晚到時丟棄，寫法同 WebsiteUrlItem 的 matchSequence。
 */
export function useIisBindingLookup(source: () => { hostLocation: string; urls: string[] }) {
    const result = ref<IisBindingResult | null>(null);
    const pending = ref(false);
    const failed = ref(false);
    let sequence = 0;
    let timer = 0;

    // 主機＋排序後的 host 組成查詢 key；key 沒變就不重查，"" 表示不用查（IP、格式不合的網址不送，後端也會略過）
    function queryKey(): string {
        const { hostLocation, urls } = source();
        const hosts = [...new Set(urls.map(toBindingHost).filter(host => host !== ""))].sort();
        return hostLocation && hosts.length > 0 ? JSON.stringify([hostLocation, hosts]) : "";
    }

    async function lookup(key: string, current: number): Promise<void> {
        const [hostLocation, hosts] = JSON.parse(key) as [string, string[]];
        try {
            const response = await fetchIisBindings(hostLocation, hosts);
            if (current !== sequence) return;
            result.value = response;
        }
        catch (error) {
            if (current !== sequence) return;
            console.error(error);
            result.value = null;
            failed.value = true;
        }
        finally {
            if (current === sequence) pending.value = false;
        }
    }

    watch(queryKey, key => {
        sequence += 1;   // 讓還在路上的舊回應作廢
        window.clearTimeout(timer);
        failed.value = false;
        if (!key) {
            result.value = null;
            pending.value = false;
            return;
        }
        // 等待期間顯示「查詢中」，不顯示上一次（可能是別台主機）的結果
        pending.value = true;
        const current = sequence;
        timer = window.setTimeout(() => { void lookup(key, current); }, LOOKUP_DELAY_MS);
    }, { immediate: true });

    onBeforeUnmount(() => {
        sequence += 1;
        window.clearTimeout(timer);
    });

    /** 某一筆網址要顯示的提示；不用顯示時回傳 null。 */
    function hintFor(url: string): IisBindingHint | null {
        const host = toBindingHost(url);
        if (!host || !source().hostLocation) return null;
        if (pending.value) return { Tone: "muted", Text: "查詢網址狀態中…" };
        if (failed.value) return { Tone: "muted", Text: "目前無法取得網址狀態" };

        const response = result.value;
        if (!response) return null;
        // Status 不是 Ok 時 Items 一律是空陣列，不能解讀成「沒綁定」
        if (response.Status === "Offline") {
            return { Tone: "muted", Text: `資料已過期${formatLastSeen(response.LastSeenAtUtc)}` };
        }
        if (response.Status !== "Ok") return { Tone: "muted", Text: "目前無法取得網址狀態" };

        const item = response.Items.find(binding => binding.Host === host);
        if (!item) return null;
        return item.SiteName
            ? { Tone: "ok", Text: "主機已設定此網址" }
            : { Tone: "warning", Text: "此網址尚未在主機啟用" };
    }

    return { hintFor };
}
