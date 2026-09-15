<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { RouterLink, RouterView, useRoute } from "vue-router";
import { getPlatformContext } from "@/services/platform-context";

const route = useRoute();
const sidebarOpen = ref(false);
const userName = ref("-");
const mvcUrl = ref("/");

const pageTitle = computed(() =>
  typeof route.meta.title === "string" ? route.meta.title : "Platform"
);

const platformLinks = [
  { to: "/", icon: "space_dashboard", label: "總覽" },
  { to: "/companies", icon: "domain", label: "客戶資料" },
  { to: "/websites", icon: "language", label: "網站與站台" },
  { to: "/licenses", icon: "verified_user", label: "版本與授權" }
];

const operationLinks = [
  { to: "/analytics", icon: "monitoring", label: "營運分析" },
  { to: "/settings", icon: "settings", label: "系統設定" }
];

watch(() => route.fullPath, () => {
  sidebarOpen.value = false;
});

onMounted(async () => {
  try {
    const context = await getPlatformContext();
    userName.value = context.UserName;
    mvcUrl.value = `${context.MvcUrl}/Welcome`;
  }
  catch (error)
  {
    console.error(error);
  }
});
</script>

<template>
  <div class="platform-shell">
    <aside class="platform-sidebar" :class="{ open: sidebarOpen }">
      <RouterLink class="platform-brand" to="/">
        <span class="brand-mark">C</span>
        <span>
          <strong>Coker</strong>
          <small>Customer Platform</small>
        </span>
      </RouterLink>

      <nav class="platform-nav" aria-label="主要選單">
        <span class="nav-caption">平台管理</span>
        <RouterLink
          v-for="item in platformLinks"
          :key="item.to"
          class="platform-nav-link"
          :to="item.to"
        >
          <span class="material-symbols-outlined">{{ item.icon }}</span>
          <span>{{ item.label }}</span>
        </RouterLink>

        <span class="nav-caption nav-caption-spaced">營運</span>
        <RouterLink
          v-for="item in operationLinks"
          :key="item.to"
          class="platform-nav-link"
          :to="item.to"
        >
          <span class="material-symbols-outlined">{{ item.icon }}</span>
          <span>{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div class="sidebar-footer">
        <span class="environment-dot"></span>
        <span>Platform 管理系統</span>
      </div>
    </aside>

    <button
      class="sidebar-backdrop"
      :class="{ show: sidebarOpen }"
      type="button"
      aria-label="關閉選單"
      @click="sidebarOpen = false"
    ></button>

    <div class="platform-main">
      <header class="platform-header">
        <button
          class="icon-button sidebar-toggle"
          type="button"
          aria-label="開啟選單"
          @click="sidebarOpen = true"
        >
          <span class="material-symbols-outlined">menu</span>
        </button>

        <div class="header-context">
          <span class="header-eyebrow">客戶管理平台</span>
          <strong>{{ pageTitle }}</strong>
        </div>

        <nav class="header-actions" aria-label="系統捷徑">
          <a class="icon-button" :href="mvcUrl" title="返回網站管理後台" aria-label="返回網站管理後台">
            <span class="material-symbols-outlined">apps</span>
          </a>
          <RouterLink class="icon-button" to="/" title="Platform 首頁" aria-label="Platform 首頁">
            <span class="material-symbols-outlined">home</span>
          </RouterLink>
          <span class="header-divider"></span>
          <div class="account-chip">
            <span class="account-avatar material-symbols-outlined">person</span>
            <span class="account-copy">
              <small>系統管理員</small>
              <strong>{{ userName }}</strong>
            </span>
          </div>
        </nav>
      </header>

      <main class="platform-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>
