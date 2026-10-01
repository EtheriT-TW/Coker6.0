<script setup lang="ts">
    import { computed, onBeforeUnmount, ref, watch } from "vue";
    import { useRoute } from "vue-router";
    import { FormFieldErrors } from "@/core/coker";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import QuickDomainDialog from "@/components/QuickDomainDialog.vue";
    import UrlField from "@/components/UrlField.vue";
    import { fetchDomainPassword, matchDomain } from "@/services/domain-api";
    import type { DomainSummary } from "@/types/domain";
    import { toDateInput } from "@/utils/date-input";

    // 網站編輯頁的一筆網址：輸入網址、指定主要網址、比對網域並顯示網域資料。
    // 這裡的比對結果只給使用者看；存檔時後端會逐筆重新比對，不相信前端。
    // 停用狀態由外層 <fieldset disabled> 控制。

    const props = defineProps<{
        url: string;
        /** 第幾筆（0 起算），用來組欄位 id 與標題 */
        index: number;
        isPrimary: boolean;
        /** 載入時後端比對出的網域；新增的列為 null */
        initialDomain: DomainSummary | null;
        errors: string[];
        canRemove: boolean;
        /** 新增網站時不顯示「編輯網域資料」：跳頁會丟掉還沒存的網站 */
        canEditDomain: boolean;
    }>();

    const emit = defineEmits<{
        "update:url": [value: string];
        setPrimary: [];
        remove: [];
        /** 本列的彈窗開關；外層據此擋住 Ctrl+S 偷偷送出網站表單 */
        dialogChange: [open: boolean];
    }>();

    const route = useRoute();
    const inputId = computed(() => `website-url-${props.index}`);

    // ── 網址 → 網域比對 ──
    const matchedDomain = ref<DomainSummary | null>(props.initialDomain);
    const matchedUrl = ref(props.url);     // 最近一次比對送出的網址，沒變就不重查
    const unmatchedHost = ref("");
    const suggestedDomainName = ref("");
    const matchingDomain = ref(false);
    const domainMatchError = ref("");
    const isDomainNotFoundOpen = ref(false);
    const isQuickDomainOpen = ref(false);
    let matchSequence = 0;                 // 比對請求序號，舊回應晚到時丟棄

    const isDomainMissing = computed(() =>
        unmatchedHost.value !== "" && matchedDomain.value === null && !matchingDomain.value);

    watch(() => isDomainNotFoundOpen.value || isQuickDomainOpen.value, open => emit("dialogChange", open));
    onBeforeUnmount(() => emit("dialogChange", false));

    // ── 網域密碼（唯讀；要改請到網域編輯頁） ──
    const PASSWORD_MASK = "********";
    const domainPassword = ref<string | null>(null);   // 按眼睛後讀回的明文；null＝遮罩中
    const revealingPassword = ref(false);
    const domainPasswordNotice = ref("");

    const domainPasswordDisplay = computed(() => {
        if (!matchedDomain.value?.HasPassword) return "";
        return domainPassword.value ?? PASSWORD_MASK;
    });

    // 比對到別的網域時清掉明文，避免顯示成上一個網域的密碼
    watch(() => matchedDomain.value?.Id, () => {
        domainPassword.value = null;
        domainPasswordNotice.value = "";
    });

    async function toggleDomainPassword(): Promise<void> {
        if (domainPassword.value !== null) {
            domainPassword.value = null;
            return;
        }

        const domainId = matchedDomain.value?.Id;
        if (!domainId) return;

        revealingPassword.value = true;
        domainPasswordNotice.value = "";
        try {
            const result = await fetchDomainPassword(domainId);
            // 等回應期間網址被改掉、換了網域，就丟掉這次結果
            if (matchedDomain.value?.Id !== domainId) return;

            if (result.State === "Ok") domainPassword.value = result.Password ?? "";
            else if (result.State === "Empty") domainPasswordNotice.value = "這筆網域尚未記錄密碼。";
            else domainPasswordNotice.value = "密碼已無法讀取，請到網域資料重新輸入。";
        }
        catch (error) {
            console.error(error);
            domainPasswordNotice.value = "讀取網域密碼失敗，請稍後再試。";
        }
        finally {
            revealingPassword.value = false;
        }
    }

    function resetDomainMatch(): void {
        matchSequence += 1;   // 讓還在路上的舊回應作廢
        matchedDomain.value = null;
        matchedUrl.value = "";
        unmatchedHost.value = "";
        domainMatchError.value = "";
        matchingDomain.value = false;
    }

    /**
     * 網址欄位 change／Enter 時比對網域（由長到短逐段比對在後端做）。
     * promptWhenMissing＝false：剛建完網域後的重查用，避免彈窗無限循環。
     */
    async function checkDomain(options: { force?: boolean; promptWhenMissing?: boolean } = {}): Promise<void> {
        const { force = false, promptWhenMissing = true } = options;
        const url = props.url.trim();
        if (!force && url === matchedUrl.value) return;

        resetDomainMatch();
        if (!url) return;

        const sequence = matchSequence;
        matchingDomain.value = true;
        try {
            const result = await matchDomain(url);
            if (sequence !== matchSequence) return;

            matchedUrl.value = url;
            if (!result.Host) {
                domainMatchError.value = "網址格式不正確，例：https://www.example.com.tw";
                return;
            }

            matchedDomain.value = result.Domain;
            if (result.Domain) return;

            unmatchedHost.value = result.Host;
            suggestedDomainName.value = result.SuggestedDomainName ?? result.Host;
            if (promptWhenMissing) isDomainNotFoundOpen.value = true;
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

    function openQuickDomain(): void {
        isDomainNotFoundOpen.value = false;
        isQuickDomainOpen.value = true;
    }

    async function onDomainCreated(): Promise<void> {
        isQuickDomainOpen.value = false;
        // 使用者可能在彈窗改過網域，重查一次確認真的對得上
        await checkDomain({ force: true, promptWhenMissing: false });
    }
</script>

<template>
    <article class="repeater-item">
        <header class="repeater-item-heading">
            <strong>網址 {{ props.index + 1 }}</strong>
            <label class="choice">
                <input type="radio"
                       name="website-primary-url"
                       :checked="props.isPrimary"
                       @change="emit('setPrimary')" />
                <span class="choice-box" aria-hidden="true"></span>
                <span>主要網址</span>
            </label>
        </header>

        <div class="repeater-item-body">
            <div class="repeater-row">
                <div class="form-grid">
                    <div class="form-field form-field-wide">
                        <label :for="inputId" class="form-label">網址 <i class="form-required">*</i></label>
                        <UrlField :model-value="props.url"
                                  :input-id="inputId"
                                  @update:model-value="value => emit('update:url', value)"
                                  @change="checkDomain()"
                                  @enter="checkDomain({ force: true })" />
                        <FormFieldErrors :errors="props.errors" />
                        <p v-if="matchingDomain" class="field-note" role="status">比對網域中…</p>
                        <p v-if="domainMatchError" class="field-note field-note-error" role="alert">{{ domainMatchError }}</p>
                        <p v-if="isDomainMissing" class="field-note field-note-error field-note-with-action" role="alert">
                            <span>網域管理中沒有「{{ unmatchedHost }}」對應的網域資料。</span>
                            <button class="field-note-action" type="button" @click="openQuickDomain">建立網域</button>
                        </p>
                    </div>
                </div>

                <!-- 空白標題佔位，讓按鈕與輸入框對齊在同一條線上 -->
                <div class="repeater-action">
                    <span aria-hidden="true">&nbsp;</span>
                    <button class="outline-icon-button outline-icon-button-danger"
                            type="button"
                            title="移除這筆"
                            :aria-label="`移除網址 ${props.index + 1}`"
                            :disabled="!props.canRemove"
                            @click="emit('remove')">
                        <span class="material-symbols-outlined">delete</span>
                    </button>
                </div>
            </div>

            <div class="form-grid form-grid-4">
                <div class="form-field">
                    <label>
                        <span>網域</span>
                        <input :value="matchedDomain?.DomainName ?? ''" type="text" readonly />
                    </label>
                    <p v-if="props.canEditDomain && matchedDomain" class="field-note field-note-with-action">
                        <RouterLink class="field-note-action"
                                    :to="{ name: 'domain-edit', params: { id: matchedDomain.Id }, query: { returnTo: route.fullPath } }">
                            編輯網域資料
                        </RouterLink>
                    </p>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域公司</span>
                        <input :value="matchedDomain?.Registrar ?? ''" type="text" readonly />
                    </label>
                </div>

                <div class="form-field">
                    <label :for="`${inputId}-password`" class="form-label">網域密碼</label>
                    <div class="input-with-action">
                        <input :id="`${inputId}-password`"
                               :value="domainPasswordDisplay"
                               type="text"
                               readonly
                               autocomplete="off"
                               spellcheck="false"
                               :placeholder="matchedDomain ? '尚未記錄' : ''" />
                        <button v-if="matchedDomain?.HasPassword"
                                class="outline-icon-button"
                                type="button"
                                :disabled="revealingPassword"
                                :title="domainPassword !== null ? '隱藏密碼' : '顯示密碼'"
                                :aria-label="domainPassword !== null ? '隱藏密碼' : '顯示密碼'"
                                :aria-pressed="domainPassword !== null"
                                @click="toggleDomainPassword">
                            <span class="material-symbols-outlined">
                                {{ domainPassword !== null ? "visibility_off" : "visibility" }}
                            </span>
                        </button>
                    </div>
                    <p v-if="domainPasswordNotice" class="field-note field-note-error">{{ domainPasswordNotice }}</p>
                </div>

                <div class="form-field">
                    <label>
                        <span>網域到期日期</span>
                        <input :value="toDateInput(matchedDomain?.EndDate)" type="date" readonly />
                    </label>
                </div>
            </div>
        </div>
    </article>

    <ConfirmDialog :open="isDomainNotFoundOpen"
                   icon="dns"
                   title="找不到網域"
                   :message="`網域管理中沒有「${unmatchedHost}」對應的網域資料，要現在建立嗎？`"
                   cancel-text="稍後再說"
                   confirm-text="填寫網域資料"
                   @cancel="isDomainNotFoundOpen = false"
                   @confirm="openQuickDomain" />

    <QuickDomainDialog :open="isQuickDomainOpen"
                       :initial-domain-name="suggestedDomainName"
                       @cancel="isQuickDomainOpen = false"
                       @created="onDomainCreated" />
</template>