using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models.OldDB
{
    [Table("menu_sub")]
    public class MenuSub
    {
        public int id { get; set; }
        public int hid { get; set; }
        public int authors_id { get; set; }
        public string title { get; set; }
        public string banner_img { get; set; }
        public string? title2 { get; set; }
        public string? cont { get; set; }
        public string? img { get; set; }
        public string ser_no { get; set; }
        public string disp_opt { get; set; }
        public DateTime? cdate { get; set; }
        public DateTime? edate { get; set; }
        public int? popular { get; set; }
        public string? popular_disp { get; set; }
        public string? use_module { get; set; }
        public int? page_title_num { get; set; }
        public string? part_img { get; set; }
        public string? part_img2 { get; set; }
        public string? over_img { get; set; }
        public string? out_img { get; set; }
        public string? link_url { get; set; }
        public string? link_target { get; set; }
        public string? examine { get; set; }
        public string? member_read { get; set; }
        public string? change { get; set; }
        public string? logo_img { get; set; }
        public int? logo_width { get; set; }
        public int? logo_height { get; set; }
        public int? margin_top { get; set; }
        public int? margin_left { get; set; }
        public string? background_img { get; set; }
        public int? background_hight { get; set; }
        public int? style_no { get; set; }
        public string? lanBar { get; set; }
        public string? logo_img2 { get; set; }
        public string? logo_img3 { get; set; }
        public string? logo_img4 { get; set; }
        public string? logo_img5 { get; set; }
        public string? weblink1 { get; set; }
        public string? weblink2 { get; set; }
        public string? weblink3 { get; set; }
        public string? weblink4 { get; set; }
        public string? weblink5 { get; set; }
        public string? body_background_img { get; set; }
        public int? menu_level { get; set; }
        public string? menuBotton { get; set; }
        public string? prod_au_id { get; set; }
        public string? fixLeft { get; set; }
        public string? displayList { get; set; }
        public string? footmap_display { get; set; }
        public string? views { get; set; }
        public string? views_switch { get; set; }
        public string? prod_au_views { get; set; }
        public string? prod_au_views_switch { get; set; }
        public string? displayList_img { get; set; }
        public string? app_tag { get; set; }
        public int showTag { get; set; }
        public string? meta_keyword { get; set; }
        public string? meta_description { get; set; }
        public string isUploadTTB { get; set; }
        public string showList { get; set; }
        public string? prodRelationType { get; set; }
        public string inDetail { get; set; }
        public string? weblinkTitle1 { get; set; }
        public string? weblinkTitle2 { get; set; }
        public string? weblinkTitle3 { get; set; }
        public string? weblinkTitle4 { get; set; }
        public string? weblinkTitle5 { get; set; }
        public double? PositionX { get; set; }
        public double? PositionY { get; set; }
        public string? StoreName { get; set; }
    }
}
