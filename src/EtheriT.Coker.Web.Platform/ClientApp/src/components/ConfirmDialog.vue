<script setup lang="ts">
    import { nextTick, onBeforeUnmount, ref, watch } from "vue"
    const props = withDefaults(defineProps<{
        open: boolean;
        title: string;
        message?: string;
        icon?: string;
        confirmText?: string;
        cancelText?: string;
        tone?: "primary" | "danger";
        busy?: boolean;
    }>(), {
        icon: "help",
        confirmText: "確認",
        cancelText: "取消",
        tone: "primary",
        busy: false
    });

    const emit = defineEmits<{ confirm: []; cancel: [] }>();

    const confirmButton = ref<HTMLButtonElement | null>(null);
    let lastFocused: HTMLElement | null = null;

    function handleKeydown(event: KeyboardEvent): void {
        if (event.key === "Escape" && !props.busy) {
            event.preventDefault();
            emit("cancel");
        }
    }

    function release(): void {
        document.body.classList.remove("dialog-open");
        window.removeEventListener("keydown", handleKeydown);
    }

    watch(() => props.open, async (open) => {
        if (open) {
            lastFocused = document.activeElement as HTMLElement | null;
            document.body.classList.add("dialog-open");
            window.addEventListener("keydown", handleKeydown);
            await nextTick();
            confirmButton.value?.focus();
        }
        else {
            release();
            lastFocused?.focus();
            lastFocused = null;
        }
    });

    onBeforeUnmount(release);
</script>

<template>
    <div v-if="open"
         class="app-dialog"
         role="dialog"
         aria-modal="true"
         :aria-label="title">
        <section class="app-dialog-card">
            <span class="app-dialog-icon material-symbols-outlined" :class="`tone-${tone}`">
                {{ icon }}
            </span>
            <h2>{{ title }}</h2>
            <p v-if="message">{{ message }}</p>
            <slot />
            <div class="app-dialog-actions">
                <button class="ui-button ui-button-secondary"
                        type="button"
                        :disabled="busy"
                        @click="emit('cancel')">
                    {{ cancelText }}
                </button>
                <button ref="confirmButton"
                        class="ui-button"
                        :class="tone === 'danger' ? 'ui-button-danger' : 'ui-button-primary'"
                        type="button"
                        :disabled="busy"
                        @click="emit('confirm')">
                    {{ confirmText }}
                </button>
            </div>
        </section>
    </div>
</template>