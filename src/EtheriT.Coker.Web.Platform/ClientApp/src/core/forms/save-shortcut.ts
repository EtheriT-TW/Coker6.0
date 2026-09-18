import { hasOpenDialog } from "@/core/dialogs/dialog-stack";

type SaveShortcutHandler = () => void | Promise<void>;

const handlers: SaveShortcutHandler[] = [];
let listening = false;

function handleKeydown(event: KeyboardEvent): void {
  if (event.repeat ||
      !(event.ctrlKey || event.metaKey) ||
      event.key.toLowerCase() !== "s") return;

  const handler = handlers.at(-1);
  if (!handler) return;

  event.preventDefault();
  // 任何彈窗開著時（含錯誤彈窗）都不能偷偷送出底下的表單，
  // 否則錯誤彈窗會一直被自己重新蓋掉，或跳在快速建立彈窗上面。
  if (hasOpenDialog()) return;
  void handler();
}

export function registerSaveShortcut(handler: SaveShortcutHandler): () => void {
  handlers.push(handler);
  if (!listening) {
    window.addEventListener("keydown", handleKeydown);
    listening = true;
  }

  return () => {
    const index = handlers.lastIndexOf(handler);
    if (index >= 0) handlers.splice(index, 1);
    if (handlers.length === 0 && listening) {
      window.removeEventListener("keydown", handleKeydown);
      listening = false;
    }
  };
}
