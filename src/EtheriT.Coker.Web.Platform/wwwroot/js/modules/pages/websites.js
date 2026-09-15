import { getPageRoot } from "../shared/page.js";

const page = getPageRoot("websites");

if (page) {
  // 網站 DataGrid 與頁面事件由這個 entry point 初始化。
  page.dataset.moduleReady = "true";
}
