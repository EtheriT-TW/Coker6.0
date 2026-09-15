<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";
import { RouterLink, RouterView, useRoute } from "vue-router";
import {
  sessionStateChangedEvent,
  type SessionState
} from "@/services/http-client";
import { getPlatformContext } from "@/services/platform-context";
import { recordPlatformLocation } from "@/services/navigation-preference";
import { startSessionLifecycle } from "@/services/session-lifecycle";
import {
  completeReauthentication,
  reauthenticate
} from "@/core/auth/reauthentication";
import { api } from "@/core/api/api-client";

const route = useRoute();
const sidebarOpen = ref(false);
const userName = ref("-");
const mvcUrl = ref("/");
const mvcLoginUrl = ref("/");
const sessionState = ref<SessionState | null>(null);
const checkingSession = ref(false);
const logoutConfirmationOpen = ref(false);
const loggingOut = ref(false);
const logoutError = ref("");
const reauthenticationPassword = ref("");
const reauthenticationError = ref("");
let stopSessionLifecycle: (() => void) | undefined;
let navigationPreferenceReady = false;

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

watch(() => route.fullPath, (path) => {
  sidebarOpen.value = false;
  if (navigationPreferenceReady) {
    void recordPlatformLocation(path);
  }
});

function handleSessionStateChanged(event: Event): void {
  const nextState = (event as CustomEvent<SessionState>).detail;
  if (sessionState.value !== nextState) {
    reauthenticationPassword.value = "";
    reauthenticationError.value = "";
  }
  sessionState.value = nextState;
}

function returnToMvc(): void {
  window.location.assign(mvcUrl.value);
}

async function logout(): Promise<void> {
  if (loggingOut.value) return;

  loggingOut.value = true;
  logoutError.value = "";
  try {
    await api.post<void>("/api/session/logout");
    window.location.replace(mvcLoginUrl.value);
  }
  catch (error) {
    console.error(error);
    logoutError.value = "登出失敗，請稍後再試。";
    loggingOut.value = false;
  }
}

async function submitReauthentication(): Promise<void> {
  if (!reauthenticationPassword.value || checkingSession.value) return;

  checkingSession.value = true;
  reauthenticationError.value = "";
  const result = await reauthenticate(reauthenticationPassword.value);
  if (result.success) {
    sessionState.value = null;
    reauthenticationPassword.value = "";
  }
  else {
    reauthenticationError.value = result.error ?? "重新登入失敗。";
  }
  checkingSession.value = false;
}

onMounted(async () => {
  window.addEventListener(sessionStateChangedEvent, handleSessionStateChanged);

  try {
    const context = await getPlatformContext();
    userName.value = context.UserName;
    mvcUrl.value = `${context.MvcUrl}/Welcome`;
    mvcLoginUrl.value = `${context.MvcUrl}/Account/Index`;
    navigationPreferenceReady = true;
    void recordPlatformLocation(route.fullPath);
    stopSessionLifecycle = startSessionLifecycle(
      context.SessionActivityIntervalSeconds
    );
  }
  catch (error)
  {
    console.error(error);
  }
});

onBeforeUnmount(() => {
  window.removeEventListener(sessionStateChangedEvent, handleSessionStateChanged);
  stopSessionLifecycle?.();
  completeReauthentication(false);
});
</script>

<template>
  <div class="platform-shell">
    <aside class="platform-sidebar" :class="{ open: sidebarOpen }">
      <RouterLink class="platform-brand" to="/">
        <span class="brand-mark">C</span>
        <span>
          <strong>Coker 6</strong>
          <small>客戶管理平台</small>
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
          <button
            class="icon-button"
            type="button"
            title="登出"
            aria-label="登出"
            @click="logoutConfirmationOpen = true"
          >
            <span class="material-symbols-outlined">logout</span>
          </button>
        </nav>
      </header>

      <main class="platform-content">
        <RouterView />
      </main>
    </div>

    <div
      v-if="logoutConfirmationOpen"
      class="session-shield"
      role="dialog"
      aria-modal="true"
      aria-labelledby="logout-dialog-title"
    >
      <section class="session-card">
        <span class="session-icon material-symbols-outlined">logout</span>
        <h2 id="logout-dialog-title">確定要登出嗎？</h2>
        <p>登出後，您需要重新登入才能繼續使用系統。</p>
        <p v-if="logoutError" class="logout-error">{{ logoutError }}</p>
        <div class="session-actions">
          <button
            class="session-button session-button-secondary"
            type="button"
            :disabled="loggingOut"
            @click="logoutConfirmationOpen = false"
          >
            取消
          </button>
          <button
            class="session-button session-button-danger"
            type="button"
            :disabled="loggingOut"
            @click="logout"
          >
            {{ loggingOut ? "登出中…" : "登出" }}
          </button>
        </div>
      </section>
    </div>

    <div v-if="sessionState" class="session-shield" role="dialog" aria-modal="true">
      <section class="session-card">
        <span class="session-icon material-symbols-outlined">
          {{ sessionState === "expired" ? "lock_clock" : "lock" }}
        </span>
        <h2>{{ sessionState === "expired" ? "登入狀態已過期" : "平台權限已變更" }}</h2>
        <p v-if="sessionState === 'expired'">
          目前頁面與尚未送出的內容會保留。請使用目前帳號重新驗證，成功後會接續原本的儲存。
        </p>
        <p v-else>
          目前帳號已無法使用客戶管理平台，請聯絡系統管理員確認權限。
        </p>
        <form
          v-if="sessionState === 'expired'"
          class="reauthentication-form"
          @submit.prevent="submitReauthentication"
        >
          <label>
            <span>帳號</span>
            <input :value="userName" type="text" autocomplete="username" readonly />
          </label>
          <label>
            <span>密碼</span>
            <input
              v-model="reauthenticationPassword"
              type="password"
              autocomplete="current-password"
              required
              autofocus
            />
          </label>
          <p v-if="reauthenticationError" class="reauthentication-error">
            {{ reauthenticationError }}
          </p>
          <div class="session-actions">
            <button
              class="session-button session-button-primary"
              type="submit"
              :disabled="checkingSession || !reauthenticationPassword"
            >
              {{ checkingSession ? "驗證中…" : "重新登入並繼續儲存" }}
            </button>
          </div>
        </form>
        <div v-else class="session-actions">
          <button
            class="session-button session-button-primary"
            type="button"
            @click="returnToMvc"
          >
            返回網站管理後台
          </button>
        </div>
      </section>
    </div>
  </div>
</template>
