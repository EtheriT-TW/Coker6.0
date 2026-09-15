import { initializeSidebar } from "./shell/sidebar.js";
import { initializeDevExtreme } from "./vendor/devextreme.js";

function initializePlatform() {
  initializeDevExtreme("zh-tw");
  initializeSidebar();
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initializePlatform, { once: true });
} else {
  initializePlatform();
}
