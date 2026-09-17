import { computed, ref } from "vue";

export interface ConfirmRequest {
  title: string;
  message?: string;
  icon?: string;
  confirmText?: string;
  cancelText?: string;
  tone?: "primary" | "danger";
}

interface PendingConfirm extends ConfirmRequest {
  resolve: (confirmed: boolean) => void;
}

const pending = ref<PendingConfirm | null>(null);

/** 取代 window.confirm：回傳 Promise，由 ConfirmHost 顯示彈窗並等使用者回答。 */
export function requestConfirm(request: ConfirmRequest): Promise<boolean> {
  // 前一個還沒回答又來新的：視同取消，避免舊的 Promise 永遠不 resolve
  pending.value?.resolve(false);
  return new Promise<boolean>(resolve => {
    pending.value = { ...request, resolve };
  });
}

export function answerConfirm(confirmed: boolean): void {
  const current = pending.value;
  pending.value = null;
  current?.resolve(confirmed);
}

export const pendingConfirm = computed(() => pending.value);