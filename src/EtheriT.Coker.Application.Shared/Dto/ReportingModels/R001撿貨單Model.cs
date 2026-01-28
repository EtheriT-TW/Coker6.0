namespace EtheriT.Coker.Application.Shared.Dto.ReportingModels
{
    public class R001撿貨單Model
    {
        public string 網站名稱 { get; set; }
        public string 列印時間 { get; set; }
        public string 訂單編號 { get; set; }
        public string 客戶名稱 { get; set; }
        public string 支付方式 { get; set; }
        public string 訂單日期 { get; set; }
        public string 送貨方式 { get; set; }
        public string 收件人 { get; set; }
        public string 收件人電話 { get; set; }
        public string 收件人地址 { get; set; }
        public string 發票資訊 { get; set; }
        public string 用戶備註 { get; set; }
        public int 紅利折抵 { get; set; }
        public decimal 商品使用紅利 { get; set; }
        public decimal 訂單紅利折抵 { get; set; }
        public decimal 合計 { get; set; }
        public decimal 訂單折抵 { get; set; }
        public decimal 運費 { get; set; }
        public decimal 訂單總金額 { get; set; }
        public decimal 優惠券折抵 { get; set; }
        public List<訂單明細Item> 訂單明細 { get; set; }
        public class 訂單明細Item
        {
            public string 商品名稱 { get; set; }
            public string 商品規格 { get; set; }
            public string 商品單價 { get; set; }
            public decimal 商品金額 { get; set; }
            public decimal 商品紅利 { get; set; }
            public decimal 商品折扣 { get; set; }
            public int 商品數量 { get; set; }
            public string 商品小計 { get; set; }
        }
    }
}
