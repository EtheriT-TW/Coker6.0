<script setup lang="ts">
    import { computed, onBeforeUnmount, ref, watch } from "vue";
    import { useRoute } from "vue-router";
    import { FormFieldErrors } from "@/core/coker";
    import IisBindingNote from "@/components/IisBindingNote.vue";
    import QuickDomainDialog from "@/components/QuickDomainDialog.vue";
    import UrlField from "@/components/UrlField.vue";
    import { matchDomain } from "@/services/domain-api";
    import type { DomainSummary } from "@/types/domain";
    import type { IisBindingHint } from "@/types/website";
    import { toDateInput } from "@/utils/date-input";
    import { toDomainName } from "@/utils/domain-name";

    // 網站編輯頁的一筆其他網址：輸入網址，並顯示比對到的網域與 IIS 綁定狀態。
    // 網域只是參考資訊：比對不到也能存檔；要補建或修改網域由使用者按右側按鈕自行處理，不自動登記。
    // 這裡的比對結果只給使用者看；存檔時後端會逐筆重新比對，不相信前端。
    // 停用狀態由外層 <fieldset disabled> 控制。

    const props = defineProps<{
        url: string;
        /** 第幾筆（0 起算），用來組欄位 id 與標題 */
        index: number;
        /** 載入時後端比對出的網域；新增的列為 null */
        initialDomain: DomainSummary | null;
        errors: string[];
        /** 新增網站時不顯示「編輯網域」：跳頁會丟掉還沒存的網站 */
        canEditDomain: boolean;
        /** IIS 綁定提示，由外層統一查詢後傳入 */
        iisHint: IisBindingHint | null;
    }>();

    const emit = defineEmits<{
        "update:url": [value: string];
        remove: [];
        /** 本列的彈窗開關；外層據此擋住 Ctrl+S 偷偷送出網站表單 */
        dialogChange: [open: boolean];
    }>();

    const route = useRoute();
    const inputId = computed(() => `website-url-${props.index}`);

    // ── 網址 → 網域比對 ──
    const matchedDomain = ref<DomainSummary | null>(props.initialDomain);
    const matchedUrl = ref(props.url);     // 最近一次比對成功送出的網址，沒變就不重查；"" 表示還沒比對
    const matchingDomain = ref(false);
    const domainMatchError = ref("");
    let matchSequence = 0;                 // 比對請求序號，舊回應晚到時丟棄

    // 有比對過、格式正確，卻沒有網域資料
    const isDomainMissing = computed(() =>
        matchedUrl.value !== "" &&
        matchedDomain.value === null &&
        !matchingDomain.value &&
        !domainMatchError.value);

    // ── 快速建立網域 ──
    const isQuickDomainOpen = ref(false);
    // 用網址推出建議網域（去 www.、子網域），彈窗裡使用者仍可修改
    const suggestedDomainName = computed(() => toDomainName(props.url.trim()));

    watch(isQuickDomainOpen, open => emit("dialogChange", open));
    onBeforeUnmount(() => emit("dialogChange", false));

    function resetDomainMatch(): void {
        matchSequence += 1;   // 讓還在路上的舊回應作廢
        matchedDomain.value = null;
        matchedUrl.value = "";
        domainMatchError.value = "";
        matchingDomain.value = false;
    }

    /** 網址欄位 change／Enter 時比對網域（由長到短逐段比對在後端做）。 */
    async function checkDomain(force = false): Promise<void> {
        const url = props.url.trim();
        if (!force && url === matchedUrl.value) return;

        resetDomainMatch();
        if (!url) return;

        const sequence = matchSequence;
        matchingDomain.value = true;
        try {
            const result = await matchDomain(url);
            if (sequence !== matchSequence) return;

            if (!result.Host) {
                domainMatchError.value = "網址格式不正確，例：https://www.example.com.tw";
                return;
            }

            matchedUrl.value = url;
            matchedDomain.value = result.Domain;
        }
        catch (error) {
            if (sequence !== matchSequence) return;
            console.error(error);
            domainMatchError.value = "比對網域失敗，請稍後再試。";
        }
        finally {
            if (sequence === matchSequence) matchingDomain.value = false;
        }
    }

    async function onDomainCreated(): Promise<void> {
        isQuickDomainOpen.value = false;
        // 使用者可能在彈窗改過網域，重查一次確認真的對得上
        await checkDomain(true);
    }
</script>

<template>
    <article class="repeater-item">
        <header class="repeater-item-heading">
            <strong>其他網址 {{ props.index + 1 }}</strong>
        </header>

        <div class="repeater-item-body">
            <div class="repeater-row repeater-row-url">
                <div class="form-grid">
                    <div class="form-field form-field-wide">
                        <label :for="inputId" class="form-label">網址 <i class="form-required">*</i></label>
                        <UrlField :model-value="props.url"
                                  :input-id="inputId"
                                  @update:model-value="value => emit('update:url', value)"
                                  @change="checkDomain()"
                                  @enter="checkDomain(true)" />
                        <FormFieldErrors :errors="props.errors" />
                        <p v-if="matchingDomain" class="field-note" role="status">比對網域中…</p>
                        <p v-else-if="domainMatchError" class="field-note field-note-error" role="alert">{{ domainMatchError }}</p>
                        <p v-else-if="matchedDomain" class="field-note">
                            對應網域：{{ matchedDomain.DomainName }}
                            <template v-if="matchedDomain.EndDate">
                                （到期 {{ toDateInput(matchedDomain.EndDate) }}）
                            </template>
                            <template v-if="matchedDomain.IsProxyPurchase">
                                （代購）
                            </template>
                        </p>
                        <p v-else-if="isDomainMissing" class="field-note" role="status">
                            網域管理中還沒有這個網址的網域資料，仍可儲存；可按右側按鈕建立網域。
                        </p>
                        <IisBindingNote :hint="props.iisHint" />
                    </div>
                </div>

                <!-- 空白標題佔位，讓按鈕與輸入框對齊在同一條線上 -->
                <div class="repeater-action">
                    <span class="repeater-action-spacer" aria-hidden="true">&nbsp;</span>
                    <div class="repeater-action-buttons">
                        <RouterLink v-if="matchedDomain && props.canEditDomain"
                                    class="outline-icon-button"
                                    title="編輯網域資料"
                                    :aria-label="`編輯其他網址 ${props.index + 1} 的網域資料`"
                                    :to="{ name: 'domain-edit', params: { id: matchedDomain.Id }, query: { returnTo: route.fullPath } }">
                            <span class="material-symbols-outlined" aria-hidden="true">edit</span>
                        </RouterLink>
                        <button v-else-if="isDomainMissing"
                                class="outline-icon-button"
                                type="button"
                                title="建立網域資料"
                                :aria-label="`建立其他網址 ${props.index + 1} 的網域資料`"
                                @click="isQuickDomainOpen = true">
                            <span class="material-symbols-outlined" aria-hidden="true">dns</span>
                        </button>
                        <button class="outline-icon-button outline-icon-button-danger"
                                type="button"
                                title="移除這筆"
                                :aria-label="`移除其他網址 ${props.index + 1}`"
                                @click="emit('remove')">
                            <span class="material-symbols-outlined" aria-hidden="true">delete</span>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </article>

    <QuickDomainDialog :open="isQuickDomainOpen"
                       :initial-domain-name="suggestedDomainName"
                       @cancel="isQuickDomainOpen = false"
                       @created="onDomainCreated" />
</template>