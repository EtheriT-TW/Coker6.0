using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar
{
    public class Sidebar : ViewComponent
    {
        public Sidebar() { 
        
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            Site site = new Site { 
                Title = "德瑞克",
                Jobs = new List<JobMenu> { 
                    new JobMenu{
                        PageName="Dashboard",
                        Title="儀表板",
                        Controller="Dashboard",
                        Action="Index",
                        Icon="public"
                    },
                    new JobMenu{
                        PageName="Theme",
                        Title="後台配色設定",
                        Controller="Theme",
                        Action="Index",
                        Icon="color_lens"
                    }
                }
            };
            return View(site);
        }
    }
}
