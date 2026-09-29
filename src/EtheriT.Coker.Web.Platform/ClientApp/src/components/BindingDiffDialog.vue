<script setup lang="ts">
    import { ref, watch } from "vue";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import type { BindingDiff, BindingDiffField } from "@/utils/binding-diff";

    const props = defineProps<{
        open: boolean;
        siteName: string;
        diffs: BindingDiff[];
    }>();

    const emit = defineEmits<{
        apply: [siteFields: BindingDiffField[]];
        cancel: [];
    }>();

    // 每個欄位選哪一邊；預設站台，因為線上網站正在用那個值
    const choices = ref<Record<string, "site" | "form">>({});

    watch(() => props.open, open => {
        if (!open) return;
        choices.value = Object.fromEntries(props.diffs.map(diff => [diff.Field, "site"]));
    });

    function apply(): void {
        emit("apply", props.diffs
            .filter(diff => choices.value[diff.Field] === "site")
            .map(diff => diff.Field));
    }
</script>

<template>
    <ConfirmDialog class="app-dialog-form"
                   :open="open"
                   icon="compare_arrows"
                   title="選擇要保留的資料"
                   :message="`本頁與站台「${siteName}」有 ${diffs.length} 個欄位不同，請逐一選擇要保留哪一邊。`"
                   confirm-text="套用"
                   cancel-text="取消綁定"
                   @confirm="apply"
                   @cancel="emit('cancel')">
        <ul class="binding-diff-list">
            <li v-for="diff in diffs" :key="diff.Field" class="binding-diff-item">
                <p :id="`binding-diff-${diff.Field}`" class="binding-diff-label">{{ diff.Label }}</p>
                <div class="binding-diff-options"
                     role="radiogroup"
                     :aria-labelledby="`binding-diff-${diff.Field}`">
                    <label class="choice">
                        <input v-model="choices[diff.Field]"
                               type="radio"
                               :name="`binding-${diff.Field}`"
                               value="form" />
                        <span class="choice-box" aria-hidden="true"></span>
                        <span>本頁：{{ diff.FormText }}</span>
                    </label>
                    <label class="choice">
                        <input v-model="choices[diff.Field]"
                               type="radio"
                               :name="`binding-${diff.Field}`"
                               value="site" />
                        <span class="choice-box" aria-hidden="true"></span>
                        <span>站台：{{ diff.SiteText }}</span>
                    </label>
                </div>
            </li>
        </ul>
    </ConfirmDialog>
</template>