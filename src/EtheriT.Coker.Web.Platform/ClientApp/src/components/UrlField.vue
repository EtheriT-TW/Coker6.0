<script setup lang="ts">
    import { computed, ref, watch } from "vue";
    import { URL_SCHEMES, joinUrl, splitUrl, type UrlScheme } from "@/utils/url-parts";

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

    // 網址清空後沒地方記協定，另外存一份，重打時保留上次的選擇
    const scheme = ref<UrlScheme>(splitUrl(props.modelValue).scheme ?? "https");
    const rest = computed(() => splitUrl(props.modelValue).rest);

    // 外部改網址（載入資料、站台帶入預設網址）時，同步下拉選項
    watch(() => props.modelValue, value => {
        const parsed = splitUrl(value).scheme;
        if (parsed) scheme.value = parsed;
    });

    function onSchemeChange(event: Event): void {
        scheme.value = (event.target as HTMLSelectElement).value as UrlScheme;
        emit("update:modelValue", joinUrl(scheme.value, rest.value));
        emit("change");
    }

    function onInput(event: Event): void {
        const input = event.target as HTMLInputElement;
        const parsed = splitUrl(input.value);
        if (parsed.scheme) {
            scheme.value = parsed.scheme;
            // 畫面上的值沒變時 Vue 不會重畫，要手動把 https:// 拿掉
            input.value = parsed.rest;
        }
        emit("update:modelValue", joinUrl(scheme.value, parsed.rest));
    }
</script>

<template>
    <div class="url-field">
        <select class="url-field-scheme"
                aria-label="通訊協定"
                :value="scheme"
                :disabled="props.disabled"
                @change="onSchemeChange">
            <option v-for="option in URL_SCHEMES" :key="option" :value="option">{{ option }}://</option>
        </select>
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