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

    // 網站編輯頁的一筆網址（一行）：勾選主要網址、輸入網址，並顯示比對到的網域與 IIS 綁定狀態。
    // 網域只是參考資訊：比對不到也能存檔；要補建或修改網域由使用者按右側按鈕自行處理，不自動登記。
    // 這裡的比對結果只給使用者看；存檔時後端會逐筆重新比對，不相信前端。
    // 停用狀態由外層 <fieldset disabled> 控制。

    const props = defineProps<{
        url: string;
        /** 第幾筆（0 起算），用來組欄位 id 與標題 */
        index: number;
        /** 這筆是不是主要網址；同一頁的勾選共用 name，只會有一筆被選 */
        isPrimary: boolean;
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
        /** 使用者勾選這筆當主要網址；取消其他列由外層處理 */
        setPrimary: [];
        remove: [];
        dialogChange: [open: boolean];
        /** 本列是否比對不到網域；外層存檔確認時據此提醒 */
        domainMissingChange: [missing: boolean];
    }>();

    const route = useRoute();
    const inputId = computed(() => `website-url-${props.index}`);
    const rowLabel = computed(() => `網址 ${props.index + 1}`);

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
    // immediate：載入時就沒有網域的列也要通知外層
    watch(isDomainMissing, missing => emit("domainMissingChange", missing), { immediate: true });
    onBeforeUnmount(() => {
        emit("dialogChange", false);
        // 刪掉這列時，外層的記錄也要清掉
        emit("domainMissingChange", false);
    });

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
    <article class="url-row" :class="{ 'url-row-primary': props.isPrimary }">
        <label class="choice url-row-choice" title="設為主要網址">
            <input type="radio"
                   name="website-primary-url"
                   :checked="props.isPrimary"
                   @change="emit('setPrimary')" />
            <span class="choice-box" aria-hidden="true"></span>
            <span>主要</span>
        </label>

        <div class="url-row-field">
            <label :for="inputId" class="visually-hidden">{{ rowLabel }}</label>
            <UrlField :model-value="props.url"
                      :input-id="inputId"
                      @update:model-value="value => emit('update:url', value)"
                      @change="checkDomain()"
                      @enter="checkDomain(true)" />
        </div>

        <!-- 網域與主機狀態放網址右側；平板以下移到下一行 -->
        <div class="url-row-meta">
            <p v-if="matchingDomain" class="field-note" role="status">比對網域中…</p>
            <p v-else-if="matchedDomain" class="field-note">
                網域 {{ matchedDomain.DomainName }}
                <template v-if="matchedDomain.EndDate">（到期 {{ toDateInput(matchedDomain.EndDate) }}）</template>
                <template v-if="matchedDomain.IsProxyPurchase">（代購）</template>
            </p>
            <p v-else-if="isDomainMissing" class="field-note domain-missing-note" role="status">
                <span class="material-symbols-outlined" aria-hidden="true">warning</span>
                尚無網域資料，請按右側按鈕建立
            </p>
            <IisBindingNote :hint="props.iisHint" />
        </div>

        <div class="repeater-action-buttons url-row-actions">
            <RouterLink v-if="matchedDomain && props.canEditDomain"
                        class="outline-icon-button"
                        title="編輯網域資料"
                        :aria-label="`編輯${rowLabel}的網域資料`"
                        :to="{ name: 'domain-edit', params: { id: matchedDomain.Id }, query: { returnTo: route.fullPath } }">
                <span class="material-symbols-outlined" aria-hidden="true">edit</span>
            </RouterLink>
            <button v-else-if="isDomainMissing"
                    class="outline-icon-button outline-icon-button-attention"
                    type="button"
                    title="建立網域資料"
                    :aria-label="`建立${rowLabel}的網域資料`"
                    @click="isQuickDomainOpen = true">
                <span class="material-symbols-outlined" aria-hidden="true">dns</span>
            </button>
            <button class="outline-icon-button outline-icon-button-danger"
                    type="button"
                    title="移除這筆"
                    :aria-label="`移除${rowLabel}`"
                    @click="emit('remove')">
                <span class="material-symbols-outlined" aria-hidden="true">delete</span>
            </button>
        </div>

        <!-- 只有出錯時才多一行 -->
        <div v-if="props.errors.length || domainMatchError" class="url-row-errors">
            <FormFieldErrors :errors="props.errors" />
            <p v-if="domainMatchError" class="field-note field-note-error" role="alert">{{ domainMatchError }}</p>
        </div>
    </article>

    <QuickDomainDialog :open="isQuickDomainOpen"
                       :initial-domain-name="suggestedDomainName"
                       @cancel="isQuickDomainOpen = false"
                       @created="onDomainCreated" />
</template>