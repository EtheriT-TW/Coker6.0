import { getPageRoot } from "../shared/page.js";

const page = getPageRoot("companies");

if (page) {
  // 客戶 DataGrid 與頁面事件由這個 entry point 初始化。
  page.dataset.moduleReady = "true";
}
