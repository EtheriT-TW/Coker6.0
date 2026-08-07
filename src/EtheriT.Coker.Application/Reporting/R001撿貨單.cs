using System.ComponentModel;
using System.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using EtheriT.Coker.Application.Shared.Dto.ReportingModels;

namespace EtheriT.Coker.Application.Reporting
{
    public partial class R001撿貨單
    {
        public R001撿貨單()
        {
            InitializeComponent();
            tableRow4.BeforePrint += TableRow4_BeforePrint;
        }

        private void TableRow4_BeforePrint(object sender, CancelEventArgs e)
        {
            var item = GetCurrentRow() as R001撿貨單Model.訂單明細Item;
            if (item == null)
                return;

            // ===== 每列先還原預設 =====
            tableCell17.Padding = new PaddingInfo(2, 2, 0, 0, 100F);
            tableCell17.Font = new Font("Microsoft JhengHei", 10F, FontStyle.Regular);

            foreach (XRTableCell cell in tableRow4.Cells)
            {
                cell.BackColor = Color.Transparent;   // 不上底色
                cell.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
                cell.BorderWidth = 1;
                cell.Font = new Font("Microsoft JhengHei", 10F, FontStyle.Regular);
            }

            // 一般商品，不做額外處理
            if (!item.IsAdditional)
                return;

            // ===== 訂單層級優惠 =====
            if (item.IsOrderLevelAdditional)
            {
                // 不縮排
                tableCell17.Padding = new PaddingInfo(2, 2, 0, 0, 100F);

                // 用粗體 + 上框線強調
                tableCell17.Font = new Font("Microsoft JhengHei", 10F, FontStyle.Bold);

                foreach (XRTableCell cell in tableRow4.Cells)
                {
                    cell.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Top | BorderSide.Bottom;
                    cell.BorderWidth = 1;
                }

                return;
            }

            // ===== 商品型加價購 / 贈品 =====
            tableCell17.Padding = new PaddingInfo(18, 2, 0, 0, 100F);
        }
    }
}