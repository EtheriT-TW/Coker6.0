<script setup lang="ts">
defineProps<{
  errors?: string[];
}>();
</script>

<template>
  <ul v-if="errors?.length" class="form-field-errors" role="alert">
    <li v-for="message in errors" :key="message">{{ message }}</li>
  </ul>
</template>
