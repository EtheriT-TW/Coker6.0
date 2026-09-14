using EtheriT.Coker.Application.Configuration;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.BackgroundJob
{
    /// <summary>
    /// 由後台 Hangfire 控制補查時機，再呼叫各網站 Public 的綠界查單端點。
    /// Public 只負責使用該站金流設定查詢綠界與更新訂單狀態。
    /// </summary>
    public sealed class ECPayPaymentReconciliationWorking
    {
        private readonly CokerDbContext db;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ECPayPaymentReconciliationOptions options;
        private readonly IHostEnvironment environment;
        private readonly ILogger<ECPayPaymentReconciliationWorking> logger;

        public ECPayPaymentReconciliationWorking(
            CokerDbContext db,
            IHttpClientFactory httpClientFactory,
            IOptions<ECPayPaymentReconciliationOptions> options,
            IHostEnvironment environment,
            ILogger<ECPayPaymentReconciliationWorking> logger)
        {
            this.db = db;
            this.httpClientFactory = httpClientFactory;
            this.options = options.Value;
            this.environment = environment;
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(600)]
        public async Task ReconcilePendingApplePayOrders()
        {
            if (!options.Enabled)
                return;

            if (environment.IsEnvironment("Local") &&
                string.IsNullOrWhiteSpace(options.FrontBaseUrlOverride))
            {
                logger.LogInformation(
                    "ECPay payment reconciliation is skipped in Local because FrontBaseUrlOverride is not configured.");
                return;
            }

            var batchSize = Math.Clamp(options.BatchSize, 1, 200);
            var lookbackHours = Math.Clamp(options.LookbackHours, 1, 168);
            var minimumAgeMinutes = Math.Clamp(options.MinimumAgeMinutes, 1, 30);
            var requestTimeoutSeconds = Math.Clamp(options.RequestTimeoutSeconds, 5, 120);
            if (!string.IsNullOrWhiteSpace(options.FrontBaseUrlOverride) &&
                !options.TargetWebsiteId.HasValue)
            {
                logger.LogError(
                    "ECPay payment reconciliation requires TargetWebsiteId when FrontBaseUrlOverride is configured.");
                return;
            }

            var now = DateTime.Now;
            var createdAfter = now.AddHours(-lookbackHours);
            var createdBefore = now.AddMinutes(-minimumAgeMinutes);

            var pendingOrders = await (
                from order in db.Order_Headers.AsNoTracking()
                join payment in db.PaymentTypes.AsNoTracking()
                    on order.Payment equals payment.Id
                join website in db.Websites.AsNoTracking()
                    on order.FK_WebsiteId equals website.Id
                where !order.IsTemp &&
                      !order.IsDeleted &&
                      !website.IsDeleted &&
                      (!options.TargetWebsiteId.HasValue ||
                       order.FK_WebsiteId == options.TargetWebsiteId.Value) &&
                      payment.Code == "ECPayApplePay" &&
                      order.TransactionId != null &&
                      order.TransactionId != "" &&
                      (order.State == OrderStatusEnum.待確認 ||
                       order.State == OrderStatusEnum.待付款) &&
                      order.CreationTime >= createdAfter &&
                      order.CreationTime <= createdBefore
                orderby order.CreationTime
                select new
                {
                    OrderId = order.Id,
                    WebsiteId = website.Id,
                    website.DefaultUrl
                })
                .Take(batchSize)
                .ToListAsync();

            var client = httpClientFactory.CreateClient("ThirdPartyClient_Front");
            var paidCount = 0;
            var failedCount = 0;

            foreach (var order in pendingOrders)
            {
                var frontBaseUrl = string.IsNullOrWhiteSpace(options.FrontBaseUrlOverride)
                    ? order.DefaultUrl
                    : options.FrontBaseUrlOverride;

                if (!TryBuildQueryUrl(frontBaseUrl, order.OrderId, out var requestUrl))
                {
                    failedCount++;
                    logger.LogWarning(
                        "ECPay payment reconciliation skipped because the website URL is invalid. WebsiteId={WebsiteId}, OrderId={OrderId}, DefaultUrl={DefaultUrl}",
                        order.WebsiteId,
                        order.OrderId,
                        order.DefaultUrl);
                    continue;
                }

                try
                {
                    using var timeout = new CancellationTokenSource(
                        TimeSpan.FromSeconds(requestTimeoutSeconds));
                    using var httpResponse = await client.GetAsync(requestUrl, timeout.Token);
                    var content = await httpResponse.Content.ReadAsStringAsync(timeout.Token);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        failedCount++;
                        logger.LogWarning(
                            "ECPay payment reconciliation front API failed. WebsiteId={WebsiteId}, OrderId={OrderId}, StatusCode={StatusCode}",
                            order.WebsiteId,
                            order.OrderId,
                            (int)httpResponse.StatusCode);
                        continue;
                    }

                    var result = JsonConvert.DeserializeObject<ResponseMessageDto>(content);
                    if (result?.Success != true)
                    {
                        failedCount++;
                        logger.LogWarning(
                            "ECPay payment reconciliation query failed. WebsiteId={WebsiteId}, OrderId={OrderId}, Message={Message}",
                            order.WebsiteId,
                            order.OrderId,
                            result?.Message ?? result?.Error ?? "Empty response");
                        continue;
                    }

                    var stateText = (result.Message ?? "").Split(',').FirstOrDefault();
                    if (int.TryParse(stateText, out var state) &&
                        state == (int)OrderStatusEnum.已付款)
                    {
                        paidCount++;
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    logger.LogWarning(
                        ex,
                        "ECPay payment reconciliation request failed. WebsiteId={WebsiteId}, OrderId={OrderId}",
                        order.WebsiteId,
                        order.OrderId);
                }
            }

            logger.LogInformation(
                "ECPay Apple Pay reconciliation completed. Checked={Checked}, Paid={Paid}, Failed={Failed}",
                pendingOrders.Count,
                paidCount,
                failedCount);
        }

        private static bool TryBuildQueryUrl(
            string? defaultUrl,
            long orderId,
            out Uri? requestUrl)
        {
            requestUrl = null;
            if (!Uri.TryCreate(defaultUrl, UriKind.Absolute, out var websiteUri) ||
                (websiteUri.Scheme != Uri.UriSchemeHttp &&
                 websiteUri.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }

            requestUrl = new Uri(
                $"{websiteUri.ToString().TrimEnd('/')}/api/ThirdParty/ECPayOrderState?ohid={orderId}");
            return true;
        }
    }
}
