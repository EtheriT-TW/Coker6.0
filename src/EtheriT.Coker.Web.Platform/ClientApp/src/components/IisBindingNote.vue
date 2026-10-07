<script setup lang="ts">
    import type { IisBindingHint } from "@/types/website";

    // IIS 綁定提示：主要網址與其他網址共用同一種顯示；只是參考資訊，不擋存檔
    const props = defineProps<{
        hint: IisBindingHint | null;
    }>();
</script>

<template>
    <p v-if="props.hint"
       class="field-note iis-binding-note"
       :class="`iis-binding-note-${props.hint.Tone}`"
       role="status">{{ props.hint.Text }}</p>
</template>