import { computed, ref } from "vue";

export interface ConfirmRequest {
  title: string;
  message?: string;
  icon?: string;
  confirmText?: string;
  cancelText?: string;
  /** 有值時多一顆按鈕，給三選一的情境用（例：儲存／不儲存／取消） */
  altText?: string;
  tone?: "primary" | "danger";
}

export type ConfirmChoice = "confirm" | "alt" | "cancel";

interface PendingConfirm extends ConfirmRequest {
  resolve: (choice: ConfirmChoice) => void;
}

const pending = ref<PendingConfirm | null>(null);

/** 三選一版本；按 Escape 視同 cancel。 */
export function requestChoice(request: ConfirmRequest): Promise<ConfirmChoice> {
  // 前一個還沒回答又來新的：視同取消，避免舊的 Promise 永遠不 resolve
  pending.value?.resolve("cancel");
  return new Promise<ConfirmChoice>(resolve => {
    pending.value = { ...request, resolve };
  });
}

/** 取代 window.confirm：回傳 Promise，由 ConfirmHost 顯示彈窗並等使用者回答。 */
export async function requestConfirm(request: ConfirmRequest): Promise<boolean> {
  return await requestChoice(request) === "confirm";
}

export function answerConfirm(choice: ConfirmChoice): void {
  const current = pending.value;
  pending.value = null;
  current?.resolve(choice);
}

export const pendingConfirm = computed(() => pending.value);