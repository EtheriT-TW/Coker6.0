using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models.OldDB
{
    [Table("menu_cont")]
    public class Menu_cont
    {
        public int id { get; set; }
        public int menu_id { get; set; }
        public string? img { get; set; }
        public string? img_align { get; set; }
        public string? cont { get; set; }
        public string? ser_no { get; set; }
        public string? cdate { get; set; }
        public string? edate { get; set; }
        public string? disp_opt { get; set; }
        public string type { get; set; }
        public string? title { get; set; }
        public string? col10 { get; set; }
        public string? col9 { get; set; }
        public string? col8 { get; set; }
        public string? col7 { get; set; }
        public string? col6 { get; set; }
        public string? col5 { get; set; }
        public string? col4 { get; set; }
        public string? col3 { get; set; }
        public string? col2 { get; set; }
        public string? col1 { get; set; }
        public string? start_date { get; set; }
        public string? end_date { get; set; }
        public double? col11 { get; set; }
        public int objectType { get; set; }
        public int? bid { get; set; }
    }
}
