using EtheriT.Coker.Web.MVC.Models.Dacshboard;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            DashboardModel model = new DashboardModel
            {
                Orders = new List<OrderItem> { 
                    new OrderItem{ 
                        Id="000000317",
                        Name="黃○瑜",
                        Time=DateTime.Now.AddHours(-12),
                        Price=540.0,
                        Statues="審核中"
                    },new OrderItem{
                        Id="000000318",
                        Name="張○君",
                        Time=DateTime.Now.AddHours(-6),
                        Price=900.0,
                        Statues="審核中"
                    },new OrderItem{
                        Id="000000319",
                        Name="顏○禎",
                        Time=DateTime.Now.AddHours(-4),
                        Price=900.0,
                        Statues="審核中"
                    },new OrderItem{
                        Id="000000320",
                        Name="張○偉",
                        Time=DateTime.Now.AddHours(-2),
                        Price=420.0,
                        Statues="審核中"
                    }
                }
            };
            return View(model);
        }
    }
}
