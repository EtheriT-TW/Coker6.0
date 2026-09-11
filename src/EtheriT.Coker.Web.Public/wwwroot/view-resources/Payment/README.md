# 新增金流

先判斷金流類型：

- **跳轉式**：取得付款網址後跳離網站，例如 LinePay、PCHomePay。
- **嵌入式**：在網站內顯示付款 UI 或使用原廠 SDK，例如 ECPay。

以下範例使用 ProviderCode `ExamplePay`、第三方 ID `5`。

## 需要異動的檔案

一般跳轉式金流：

```text
新增  src/EtheriT.Coker.Application.Shared/ThirdParty/IExamplePayAppService.cs
新增  src/EtheriT.Coker.Application/ThirdParty/ExamplePayAppService.cs
修改  src/EtheriT.Coker.Web.Public/wwwroot/view-resources/Payment/payment.providers.config.js
修改  src/EtheriT.Coker.Web.Public/Program.cs
修改  src/EtheriT.Coker.Web.Public/Controllers/api/ThirdPartyController.cs
修改  src/EtheriT.Coker.Application.Shared/Payment/PaymentProviderRegistry.cs
修改  src/EtheriT.Coker.Application/Order/OrderAppService.cs
```

嵌入式金流除了後端串接外，前端再新增：

```text
新增  src/EtheriT.Coker.Web.Public/wwwroot/view-resources/Payment/embedded/example-pay/provider.js
新增  src/EtheriT.Coker.Web.Public/wwwroot/view-resources/Payment/embedded/example-pay/shopping-cart.js（需要購物車時）
新增  src/EtheriT.Coker.Web.Public/wwwroot/view-resources/Payment/embedded/example-pay/member.js（需要會員重新付款時）
修改  src/EtheriT.Coker.Web.Public/bundles.json
```

## 跳轉式金流

### 1. 增加前端 Config

修改以下檔案：

```text
src/EtheriT.Coker.Web.Public/wwwroot/view-resources/Payment/payment.providers.config.js
```

在 `definitions` 加一筆：

```js
{
    code: "ExamplePay",
    mode: "redirect",
    aliases: [5]
}
```

Redirect 共用流程會自動完成註冊，不需要建立 Provider JS。

### 2. 建立後端付款服務

新增以下兩個檔案：

```text
src/EtheriT.Coker.Application.Shared/ThirdParty/IExamplePayAppService.cs
src/EtheriT.Coker.Application/ThirdParty/ExamplePayAppService.cs
```

`ExamplePayAppService` 負責呼叫金流商 API，並讓付款請求回傳統一格式：

```text
Success = true
Message = 付款網址
```

接著在 `src/EtheriT.Coker.Web.Public/Program.cs` 註冊服務：

```csharp
builder.Services.AddScoped<IExamplePayAppService, ExamplePayAppService>();
```

### 3. 接入共用 PayRequest API

前端固定呼叫：

```text
GET /api/ThirdParty/PayRequest?ohid={訂單ID}&paytype=ExamplePay
```

這是網站自己的共用 API，不是金流商網址。金流商網址與金鑰必須留在後端 `ExamplePayAppService`，不可放進前端 Config。

不需要另外新增一支前端付款 API。請修改：

```text
src/EtheriT.Coker.Web.Public/Controllers/api/ThirdPartyController.cs
```

在建構子注入 `IExamplePayAppService`，並在 `PayRequest()` 增加：

```csharp
case "5":
case "ExamplePay":
    return await examplePayAppService.ExamplePayRequest(ohid);
```

如果金流商需要 Return URL 或 Notify URL，再於同一個 Controller 增加對應 Action；這些 callback 路徑不放在前端 Config。

### 4. 增加後端 Registry

修改以下檔案：

```text
src/EtheriT.Coker.Application.Shared/Payment/PaymentProviderRegistry.cs
```

```csharp
[5] = new PaymentProviderDescriptor
{
    ProviderCode = "ExamplePay",
    RenderMode = "Standard"
}
```

### 5. 增加訂單完成後的 ProviderCode

請修改：

```text
src/EtheriT.Coker.Application/Order/OrderAppService.cs
```

在 `FillPaymentMessageAndSendMailAsync()` 的付款方式分派加入 `ExamplePay`，讓訂單建立結果以以下格式回傳：

```text
ExamplePay,{OrderId},{CreationTime}
```

完成後，購物車結帳與會員重新付款會自動走共用 Redirect Flow。

### 特殊 Redirect 流程

只有當新金流無法使用共用 `PayRequest` 回傳格式時，才建立獨立前端 Provider bundle，並在 Config 增加：

```js
moduleUrl: "/js/Payment/providers/ExamplePay.min.js"
```

Redirect 自動註冊器會略過這筆設定，改由 Loader 載入自訂 Provider。特殊行為應寫在 Provider 模組，不要把程式邏輯塞進 Config。

## 嵌入式金流

### 1. 建立 Provider 目錄

```text
src/EtheriT.Coker.Web.Public/wwwroot/view-resources/Payment/
└─ embedded/example-pay/
   ├─ provider.js
   ├─ shopping-cart.js
   └─ member.js
```

- `provider.js`：SDK、付款操作與 callback。
- `shopping-cart.js`：購物車 Adapter。
- `member.js`：會員重新付款 Adapter。

可參考現有的 `embedded/ecpay/`。不支援的頁面不需要建立 Adapter。

### 2. 增加前端 Config

修改：

```text
src/EtheriT.Coker.Web.Public/wwwroot/view-resources/Payment/payment.providers.config.js
```

```js
{
    code: "ExamplePay",
    mode: "embedded",
    aliases: [5],
    moduleUrl: "/js/Payment/providers/ExamplePay.min.js",
    hosts: ["Member", "ShoppingCart"]
}
```

### 3. 註冊 Provider 與 Adapter

`provider.js`：

```js
C.Payment.Core.register({
    code: "ExamplePay",
    mode: "embedded",
    aliases: C.Payment.Catalog.get("ExamplePay").aliases,
    create: createExamplePayProvider
});
```

`shopping-cart.js`：

```js
C.Payment.Embedded.registerAdapter(
    "ExamplePay",
    "ShoppingCart",
    createShoppingCartAdapter
);
```

`member.js`：

```js
C.Payment.Embedded.registerAdapter(
    "ExamplePay",
    "Member",
    createMemberAdapter
);
```

### 4. 增加獨立 Bundle

修改：

```text
src/EtheriT.Coker.Web.Public/bundles.json
```

加入：

```json
{
  "output": "js/Payment/providers/ExamplePay.min.js",
  "input": [
    "wwwroot/view-resources/Payment/embedded/example-pay/provider.js",
    "wwwroot/view-resources/Payment/embedded/example-pay/member.js",
    "wwwroot/view-resources/Payment/embedded/example-pay/shopping-cart.js"
  ]
}
```

### 5. 增加後端 Registry

修改：

```text
src/EtheriT.Coker.Application.Shared/Payment/PaymentProviderRegistry.cs
```

```csharp
[5] = new PaymentProviderDescriptor
{
    ProviderCode = "ExamplePay",
    RenderMode = "Embedded",
    EntryTitle = "ExamplePay"
}
```

## 完成前檢查

- 前後端 `ProviderCode` 相同。
- 第三方 ID 只出現在 Config／Registry。
- ShoppingCart、Member 共用流程沒有新增金流名稱判斷。
- Provider bundle 只在後台啟用或訂單需要時載入。
- 原廠 SDK 只在 Provider 初始化時載入。
- 已確認購物車結帳與會員重新付款。

# 結構說明

```text
Payment/
├─ payment.core.js               Provider 註冊與建立
├─ payment.providers.config.js   Provider 清單
├─ payment.loader.js             按需載入 Provider
├─ payment.flow.js               結帳與重新付款流程
├─ redirect/provider.js          跳轉式共用實作
├─ embedded/interface.js         Adapter 介面
├─ embedded/{provider}/          特定金流實作
└─ hosts/                        頁面共用介面
```

載入流程：

```text
Payment Core
  → 讀取啟用的 ProviderCode
  → 載入需要的 Provider bundle
  → 建立頁面 Adapter
  → 需要時載入原廠 SDK
```

Config 只放代碼、模式、alias、bundle URL 與支援頁面。SDK、API、callback 和金流私有狀態都放在各 Provider 目錄。

## ECPay Apple Pay

- 只能在綠界正式環境完成驗證。
- 桌機由 ECPay SDK 顯示 QR Code；手機才檢查 Wallet 並顯示確認。
- Apple Pay 結果由 `getApplePayResultData` 接收，不使用 PayToken。
