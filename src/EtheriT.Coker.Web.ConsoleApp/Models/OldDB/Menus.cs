using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models
{
    [Table("menu")]
    public class Menus
    {
        public int id { get; set; }
        public int sub_id { get; set; }
        public string title { get; set; }
        public string disp_opt { get; set; }
        public string ser_no { get; set; }
        public string? img1 { get; set; }
        public string? cont { get; set; }
        public string? cdate { get; set; }
        public string? edate { get; set; }
        public string? img2 { get; set; }
        public string? img3 { get; set; }
        public int? popular { get; set; }
        public string? popular_disp { get; set; }
        public string? use_module { get; set; }
        public string? media_link { get; set; }
        public int? img_weight { get; set; }
        public int? img_height { get; set; }
        public string? arrange { get; set; }
        public string? url_link { get; set; }
        public string? link_target { get; set; }
        public string? note_date { get; set; }
        public int? examine { get; set; }
        public string? start_date { get; set; }
        public string? end_date { get; set; }
        public string? QRCodeText { get; set; }
        public string? download_select { get; set; }
        public string? download_link { get; set; }
        public string? sid { get; set; }
        public int contDispType { get; set; }
        public string? meta_keyword { get; set; }
        public string? meta_description { get; set; }
        public string? fileExt { get; set; }
        public DateTime? lastUploadTime { get; set; }
        public string? sonOrgName { get; set; }
        public int? sonID { get; set; }
        public string? Del { get; set; }
        public int? fid { get; set; }
        public string? orgTitle { get; set; }
        public string? ARProjectId { get; set; }
        public int? ARProjectTime { get; set; }
        public string? AR_DatabaseName { get; set; }
        public string? AR_TargeImageName { get; set; }
        public int? AR_TaskType { get; set; }
    }
}
