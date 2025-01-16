using DevExpress.Data;
using Serilog;

namespace EtheriT.Coker.Web.Public.Middlewares
{
	public class FlowSizeLogMiddleware
	{
		private readonly RequestDelegate _next;
		private static long total = 0;
		private static long totalRequest = 0;
		private static long totalResponse = 0;

		public FlowSizeLogMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var request = context.Request;
			var clientIp = context.Connection.RemoteIpAddress?.ToString();
			var userAgent = request.Headers["User-Agent"].ToString();
			var url = $"{request.Scheme}://{request.Host}{request.Path}";

			// 計算請求大小
			var requestSize = request.ContentLength ?? 0;
			if (requestSize == 0 && request.Body.CanSeek)
			{
				request.Body.Position = 0;
				requestSize = request.Body.Length;
			}

			// 使用 MemoryStream 來捕捉回應
			var originalResponseBody = context.Response.Body;
			using (var responseBody = new MemoryStream())
			{
				context.Response.Body = responseBody;

				// 呼叫下一個中間件
				await _next(context);

				// 重置流位置以便讀取捕捉的回應數據
				responseBody.Position = 0;

				// 將捕捉的回應數據複製回原始的 Response.Body
				await responseBody.CopyToAsync(originalResponseBody);

				// 確保捕獲了完整的回應體
				var responseSize = responseBody.Length; // 捕捉實際的回應體大小
				var statusCode = context.Response.StatusCode; // 獲取狀態碼

				total += responseSize + requestSize;
				totalRequest += requestSize;
				totalResponse += responseSize;
				Console.WriteLine("total=" + total + "byte");
				Console.WriteLine("totalRequest=" + totalRequest + "byte");
				Console.WriteLine("totalResponse=" + totalResponse + "byte");
				// 使用 Serilog 記錄流量數據
				Log.Information("IP: {Ip}, URL: {Url}, User-Agent: {UserAgent}, Request Size: {RequestSize} bytes, Response Size: {ResponseSize} bytes, Status Code: {StatusCode}, Total Request Size: {totalRequest} bytes, Total Response Size: {totalResponse} bytes, Total: {total} bytes",
					clientIp, url, userAgent, requestSize, responseSize, statusCode, totalRequest, totalResponse, total);
			}
		}
	}
}
