<script setup lang="ts">
    import { computed } from "vue";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import { answerConfirm, pendingConfirm } from "@/core/dialogs/confirm-request";

    const request = computed(() => pendingConfirm.value);
</script>

<template>
    <ConfirmDialog :open="request !== null"
                   :title="request?.title ?? ''"
                   :message="request?.message"
                   :icon="request?.icon"
                   :confirm-text="request?.confirmText"
                   :cancel-text="request?.cancelText"
                   :tone="request?.tone"
                   @confirm="answerConfirm(true)"
                   @cancel="answerConfirm(false)" />
</template>