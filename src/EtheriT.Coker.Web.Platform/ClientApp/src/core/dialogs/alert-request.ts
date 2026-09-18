import { computed, ref } from "vue";

export interface AlertRequest {
  title: string;
  message?: string;
  details?: string[];
  icon?: string;
  confirmText?: string;
}

interface PendingAlert extends AlertRequest {
  resolve: () => void;
}

const pending = ref<PendingAlert | null>(null);

/** 取代 window.alert：回傳 Promise，由 AlertHost 顯示彈窗並等使用者關閉。 */
export function requestAlert(request: AlertRequest): Promise<void> {
  // 前一個還沒關又來新的：直接放行舊的，避免它的 Promise 永遠不 resolve
  pending.value?.resolve();
  return new Promise<void>(resolve => {
    pending.value = { ...request, resolve };
  });
}

export function dismissAlert(): void {
  const current = pending.value;
  pending.value = null;
  current?.resolve();
}

export const pendingAlert = computed(() => pending.value);
