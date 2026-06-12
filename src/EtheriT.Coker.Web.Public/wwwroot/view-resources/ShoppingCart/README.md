# ShoppingCart object module split (with ECPay logistics)

這版移除 `with (state)`、`cart._fn` 與大量 wrapper function。

每個檔案都是完整 IIFE 物件模組，模組之間直接透過 `cart.<Module>.<Function>()` 呼叫。

## 建議放置路徑

`wwwroot/view-resources/ShoppingCart/`

## Bundle 順序

```json
[
  "wwwroot/view-resources/ShoppingCart/shopping-cart.core.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.utils.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.items.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.shipping.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.logistics.ecpay.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.pricing.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.forms.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.payment.core.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.payment.redirect.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.payment.ecpay.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.payment.linepay.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.payment.pchomepay.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.order.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.checkout-result.js",
  "wwwroot/view-resources/ShoppingCart/shopping-cart.init.js"
]
```

## 綠界物流

`shopping-cart.logistics.ecpay.js` 負責綠界物流 / 超商電子地圖流程：

- `.btn_getmap` click 綁定
- 選店前暫存 `orderForm`
- 填入並送出 `form#ecpayLogisticsForm`
- 選店返回後還原表單、已選商品與收件 / 發票狀態

注意：客戶自己的綠界物流帳號或平台綠界物流帳號的判斷不寫死在前端 JS。這支 JS 沿用既有 `ecpayLogisticsForm`，由後端輸出 form action / hidden input 時決定實際走哪個帳號流程。

## 金流擴充

- `shopping-cart.payment.ecpay.js`：只處理綠界金流 / 嵌入式付款。
- `shopping-cart.logistics.ecpay.js`：只處理綠界物流 / 電子地圖。
- Redirect 型金流請新增 `shopping-cart.payment.xxx.js`，並呼叫 `cart.Payment.Core.register(...)`。
- 訂單建立後由 `cart.Payment.Core.afterOrderCreated(...)` 分派，不需要再修改 `shopping-cart.order.js`。

## 保留的全域相容入口

- `window.PageReady`
- `window.CardDataGet`
- `window.RecipientsList_ContentReady`
- `window.RecipientsList_SelectChange`
- `window.RecipientsList_DeleteButtonClicked`

其他功能請優先透過 `ShoppingCart.<Module>.<Function>()` 呼叫。
