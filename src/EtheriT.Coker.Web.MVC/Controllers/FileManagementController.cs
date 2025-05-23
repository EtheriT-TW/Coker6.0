using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class FileManagementController : Controller
    {
        private readonly IConfiguration _configuration;

        public FileManagementController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            Models.FileManagement.IndexModel model = new Models.FileManagement.IndexModel();


            model.AllowContentType = _configuration.GetSection("VirtualDirectory:FileAllow:Ext").Get<List<string>>() ?? new List<string>();            // 使用 FileExtensionContentTypeProvider 從 MIME 值反推取得副檔名
            var provider = new FileExtensionContentTypeProvider();

            // 從 MIME 反向尋找副檔名並只保留允許的副檔名
            model.AllowFileExtension = provider.Mappings.Where(x => model.AllowContentType.Contains(x.Value))
                                                        .Select(x => x.Key)
                                                        .OrderBy(x => x)
                                                        .Distinct()
                                                        .ToList();

            return View(model);
        }
    }
}
