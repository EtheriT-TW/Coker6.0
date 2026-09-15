<script setup lang="ts">
defineProps<{
  icon: string;
  title: string;
  description: string;
}>();
</script>

<template>
  <section class="placeholder-card">
    <div>
      <span class="placeholder-icon">
        <span class="material-symbols-outlined">{{ icon }}</span>
      </span>
      <h2>{{ title }}</h2>
      <p>{{ description }}</p>
      <span class="phase-badge">尚未接入資料</span>
    </div>
  </section>
</template>
