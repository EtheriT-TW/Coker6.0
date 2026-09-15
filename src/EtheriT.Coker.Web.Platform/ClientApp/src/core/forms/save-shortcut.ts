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
