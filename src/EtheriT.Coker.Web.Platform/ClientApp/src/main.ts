import { createApp } from "vue";
import zhTwMessages from "devextreme/localization/messages/zh-tw.json";
import { loadMessages, locale } from "devextreme/localization";
import config from "devextreme/core/config";
import "devextreme/dist/css/dx.fluent.blue.light.css";
import "./styles/main.css";
import App from "./App.vue";
import { router } from "./router";
import { licenseKey } from "./devextreme-license";

config({ licenseKey });
loadMessages(zhTwMessages);
locale("zh-TW");

createApp(App)
  .use(router)
  .mount("#app");
