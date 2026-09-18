<script setup lang="ts">
    import { computed } from "vue";
    import ConfirmDialog from "@/components/ConfirmDialog.vue";
    import { dismissAlert, pendingAlert } from "@/core/dialogs/alert-request";

    const request = computed(() => pendingAlert.value);
</script>

<template>
    <ConfirmDialog class="app-dialog-alert"
                   :open="request !== null"
                   hide-cancel
                   tone="danger"
                   :icon="request?.icon ?? 'error'"
                   :title="request?.title ?? ''"
                   :message="request?.message"
                   :confirm-text="request?.confirmText ?? '我知道了'"
                   @confirm="dismissAlert()"
                   @cancel="dismissAlert()">
        <ul v-if="request?.details?.length" class="app-dialog-details" role="alert">
            <li v-for="detail in request.details" :key="detail">{{ detail }}</li>
        </ul>
    </ConfirmDialog>
</template>
