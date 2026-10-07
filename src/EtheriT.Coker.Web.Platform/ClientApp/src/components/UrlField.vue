<script setup lang="ts">
    import { computed, ref } from "vue";
    import { joinUrl, splitUrl, type UrlScheme } from "@/utils/url-parts";

    // 新網址一律 https://；載入的舊網址是 http:// 時協定唯讀顯示、原樣保留，不替使用者改掉
    const props = defineProps<{
        modelValue: string;
        inputId: string;
        disabled?: boolean;
    }>();

    const emit = defineEmits<{
        "update:modelValue": [value: string];
        change: [];
        enter: [];
    }>();

    // 列元件用 Key 掛載，載入的網址在建立時就確定，之後不必再同步外部值
    const scheme = ref<UrlScheme>(splitUrl(props.modelValue).scheme === "http" ? "http" : "https");
    const rest = computed(() => splitUrl(props.modelValue).rest);
    const isLegacyScheme = computed(() => scheme.value === "http");

    function onInput(event: Event): void {
        const input = event.target as HTMLInputElement;
        const parsed = splitUrl(input.value);
        if (parsed.scheme) {
            // 貼上 https:// 可把舊的 http 網址升級；貼上 http:// 不會把新網址降級
            if (parsed.scheme === "https") scheme.value = "https";
            // 畫面上的值沒變時 Vue 不會重畫，要手動把協定拿掉
            input.value = parsed.rest;
        }
        emit("update:modelValue", joinUrl(scheme.value, parsed.rest));
    }
</script>

<template>
    <div class="url-field">
        <span class="url-field-scheme"
              :class="{ 'url-field-scheme-legacy': isLegacyScheme }"
              :title="isLegacyScheme ? '舊網址使用 http://，原樣保留' : undefined">{{ scheme }}://</span>
        <input :id="props.inputId"
               class="url-field-input"
               :value="rest"
               type="text"
               inputmode="url"
               maxlength="500"
               placeholder="例：www.example.com.tw"
               :disabled="props.disabled"
               @input="onInput"
               @change="emit('change')"
               @keydown.enter.prevent="emit('enter')" />
    </div>
</template>