using System.Collections.Generic;

namespace EtheriT.Coker.Web.MVC.Models.FileManagement
{
    public class IndexModel
    {
        public List<string> AllowContentType { get; set; } = new List<string>();
        public List<string> AllowFileExtension { get; set; } = new List<string>();
        public string UploadFilePathBase { get; set; } = string.Empty;
    }
}
