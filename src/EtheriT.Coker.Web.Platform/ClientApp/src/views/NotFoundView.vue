<script setup lang="ts">
import { RouterLink } from "vue-router";
</script>

<template>
  <section class="placeholder-card">
    <div>
      <span class="placeholder-icon"><span class="material-symbols-outlined">search_off</span></span>
      <h2>找不到這個頁面</h2>
      <p>網址可能已變更，請從 Platform 選單重新選擇功能。</p>
      <RouterLink class="primary-link" to="/">返回總覽</RouterLink>
    </div>
  </section>
</template>
