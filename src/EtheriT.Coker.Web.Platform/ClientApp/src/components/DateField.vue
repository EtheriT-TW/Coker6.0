<script setup lang="ts">
    import DxDateBox from "devextreme-vue/date-box";

    const DATE_MIN = "1900-01-01";
    const DATE_MAX = "9999-12-31";

    const props = defineProps<{
        modelValue: string;
        disabled?: boolean;
    }>();

    const emit = defineEmits<{
        "update:modelValue": [value: string];
    }>();

    // 表單模型用 "" 代表沒填；DxDateBox 按清除會回傳 null，要轉回 ""
    function onValueChanged(value: string | null): void {
        emit("update:modelValue", value ?? "");
    }
</script>

<template>
    <DxDateBox class="date-field"
               :value="props.modelValue || null"
               type="date"
               picker-type="calendar"
               display-format="yyyy/MM/dd"
               date-serialization-format="yyyy-MM-dd"
               :use-mask-behavior="true"
               :show-clear-button="true"
               :min="DATE_MIN"
               :max="DATE_MAX"
               :disabled="props.disabled"
               @update:value="onValueChanged" />
</template>