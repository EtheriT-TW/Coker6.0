using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models.OldDB
{
    [Table("shopInfo")]
    public class ShopInfo
    {
        public int id { get; set; }
        public int menuID { get; set; }
        public string? ListName { get; set; }
        public string? orgname { get; set; }
        public string? co_organiser { get; set; }
        public DateTime updatetime { get; set; }
        public string? GovID { get; set; }
        public string? name { get; set; }
        public string? name_en { get; set; }
        public string? Zone { get; set; }
        public string? Toldescribe { get; set; }
        public string? Description { get; set; }
        public string? Tel { get; set; }
        public string? org { get; set; }
        public string? Add { get; set; }
        public string? location { get; set; }
        public string? Zipcode { get; set; }
        public string? Travellinginfo { get; set; }
        public string? Opentime { get; set; }
        public string? Website { get; set; }
        public string? Picture1 { get; set; }
        public string? Picture2 { get; set; }
        public string? Picture3 { get; set; }
        public string? Picdescribe1 { get; set; }
        public string? Picdescribe2 { get; set; }
        public string? Picdescribe3 { get; set; }
        public string? Map { get; set; }
        public string? Gov { get; set; }
        public float? Px { get; set; }
        public float? Py { get; set; }
        public string? orgClass { get; set; }
        public string? Class1 { get; set; }
        public string? Class2 { get; set; }
        public string? Class3 { get; set; }
        public string? Level { get; set; }
        public string? Parkinginfo { get; set; }
        public float? Parkinginfo_px { get; set; }
        public float? Parkinginfo_py { get; set; }
        public string? Ticketinfo { get; set; }
        public string? Remarks { get; set; }
        public string? Keyword { get; set; }
        public DateTime Changetime { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public string? cycle { get; set; }
        public string? noncycle { get; set; }
        public string? Particpation { get; set; }
        public string? Grade { get; set; }
        public string? Fax { get; set; }
        public string? Spec { get; set; }
        public string? Serviceinfo { get; set; }
        public string? source { get; set; }
        public int? TripAdvisorCommentNum { get; set; }
        public string? TripAdvisorUrl { get; set; }
        public int? TripAdvisorRank { get; set; }
        public float? TripAdvisorComment { get; set; }
        public int? googleCommentNum { get; set; }
        public string? googleUrl { get; set; }
        public float? googleComment { get; set; }
        public int? duration { get; set; }
        public string? busInfo { get; set; }
        public string? BeaconMajor { get; set; }
        public string? BeaconMinor { get; set; }
        public string? QRCode { get; set; }
        public string Charge { get; set; }
        public string? Offer { get; set; }
        public string? Add_en { get; set; }
        public string? Toldescribe_en { get; set; }
        public string? Opentime_en { get; set; }
        public string? Tel2 { get; set; }
    }
}
