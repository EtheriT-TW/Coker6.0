using EtheriT.Coker.Application.Cdn;

namespace EtheriT.Coker.Web.Public.Middlewares
{
    public sealed class CdnTrustedProxyMiddleware
    {
        public const string OriginalRemoteIpAddressItemKey =
            "CdnTrustedProxy.OriginalRemoteIpAddress";
        public const string ProviderItemKey = "CdnTrustedProxy.Provider";

        private readonly RequestDelegate next;

        public CdnTrustedProxyMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            CdnClientIpResolver resolver)
        {
            var resolution = await resolver.ResolveAsync(
                context,
                context.RequestAborted);

            if (resolution != null)
            {
                context.Items[OriginalRemoteIpAddressItemKey] =
                    resolution.TrustedProxyIpAddress;
                context.Items[ProviderItemKey] = resolution.Provider;
                context.Connection.RemoteIpAddress = resolution.ClientIpAddress;
            }

            await next(context);
        }
    }
}
