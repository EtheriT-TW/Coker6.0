namespace EtheriT.Coker.Web.MVC.Models.BonusManagement
{
    public class TransactionViewModel
    {
        public readonly List<TransactionOperationSelectBoxData> TransactionOperationSelectBoxDataSource =
            new List<TransactionOperationSelectBoxData>()
            {
                new TransactionOperationSelectBoxData() { DisplayName = "新增", OperationValue = "+" },
                new TransactionOperationSelectBoxData() { DisplayName = "扣除", OperationValue = "-" },
            };
        public int ConstTransactionReasonMaxLength { get; set; } = 25;
        public int ConstTransactionPointMax { get; set; } = 9999;
        public string? ConstRewardPointsExpireDays { get; set; }
        public DateTime ConstRewardPointsExpireDateTime { get; set; }
    }

    public class  TransactionOperationSelectBoxData
    {
        public string? DisplayName { get; set; }
        public string? OperationValue { get; set; }
    }
}
