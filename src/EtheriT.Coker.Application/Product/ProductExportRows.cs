namespace EtheriT.Coker.Application.Product
{
    internal sealed class ProductExportRow
    {
        public long ProductId { get; set; }
        public string ItemNo { get; set; } = "";
        public string SubItemNo { get; set; } = "";
        public string ProdName { get; set; } = "";
        public string Status { get; set; } = "";
        public string Introduction { get; set; } = "";
        public string Description { get; set; } = "";
        public string SaveHtml { get; set; } = "";
        public string SaveCss { get; set; } = "";
        public string Image1 { get; set; } = "";
        public string Image2 { get; set; } = "";
        public string Image3 { get; set; } = "";
        public string Image4 { get; set; } = "";
        public string Image5 { get; set; } = "";
        public string Image6 { get; set; } = "";
        public string Image7 { get; set; } = "";
        public string FileName1 { get; set; } = "";
        public string File1 { get; set; } = "";
        public string FileName2 { get; set; } = "";
        public string File2 { get; set; } = "";
        public string FileName3 { get; set; } = "";
        public string File3 { get; set; } = "";
        public string FileName4 { get; set; } = "";
        public string File4 { get; set; } = "";
        public string FileName5 { get; set; } = "";
        public string File5 { get; set; } = "";
        public string FileName6 { get; set; } = "";
        public string File6 { get; set; } = "";
        public string FileName7 { get; set; } = "";
        public string File7 { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Visible { get; set; } = "";
        public string OnShelf { get; set; } = "";
        public string Spec1Name { get; set; } = "";
        public string Spec1 { get; set; } = "";
        public string Spec2Name { get; set; } = "";
        public string Spec2 { get; set; } = "";
        public string SpecImage { get; set; } = "";
        public string SpecDescription { get; set; } = "";
        public string SpecVisible { get; set; } = "";
        public int Stock { get; set; }
        public int Min_Qty { get; set; }
        public int Alert_Qty { get; set; }
        public decimal SuggestPrice { get; set; }
        public string RoleName { get; set; } = "";
        public string Price { get; set; } = "";
        public int Bonus { get; set; }
        public string Tag1 { get; set; } = "";
        public string Tag2 { get; set; } = "";
        public string Tag3 { get; set; } = "";
        public string Tag4 { get; set; } = "";
        public string Tag5 { get; set; } = "";
        public string Tag6 { get; set; } = "";
    }

    internal sealed class DirectoryExportRow
    {
        public string Level1 { get; set; } = "";
        public string Level1RouterName { get; set; } = "";
        public string Level2 { get; set; } = "";
        public string Level2RouterName { get; set; } = "";
        public string Level3 { get; set; } = "";
        public string Level3RouterName { get; set; } = "";
        public string Tag1 { get; set; } = "";
        public string Tag2 { get; set; } = "";
        public string Tag3 { get; set; } = "";
    }

    internal sealed class TechCertExportRow
    {
        public string ItemNo { get; set; } = "";
        public string ProdName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Image1 { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
