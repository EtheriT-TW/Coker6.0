using EtheriT.Coker.Web.MVC.Common;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar
{
    public class Sidebar : ViewComponent
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public Sidebar(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public IViewComponentResult Invoke()
        {
            var context = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException(
                    "Sidebar requires an active HttpContext.");

            if (!context.Items.TryGetValue(
                    CokerContextKeys.NavigationSite,
                    out var value) ||
                value is not Site site)
            {
                throw new InvalidOperationException(
                    "NavigationSite has not been initialized.");
            }

            return View(site);
        }
    }
}