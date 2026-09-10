namespace Empire_ERP.Core.Entities
{
    public class PurchaseOrderReports
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ReportID { get; set; }
        public string? PARTY_CODE { get; set; }
        public string? ACT_CODE { get; set; }

        public string? SPARTY_CODE { get; set; }
        public string? SACT_CODE { get; set; }
        public string? JOB_NO { get; set; }
        public string? ITEM_CODE { get; set; }
        public string? SEASON { get; set; }
        public string? ControlCode { get; set; }
        public int? AccountCode { get; set; }
    }

    public class CustomPurchaseOrderReports
    {
        public string? suppName { get; set; }
        public string? suppActCode { get; set; }
        public string? suppCode { get; set; }
        public string? clientName { get; set; }
        public string? MODEL { get; set; }
        public string? orderNo { get; set; }
        public string? COLOR { get; set; }
        public string? SIZE { get; set; }
        public string? comment { get; set; }
        public string? CurrSign { get; set; }
        public double? QTY { get; set; }
        public double? RATE { get; set; }
        public double? AMT { get; set; }
        public string? lcPort { get; set; }
        public string? shipDate { get; set; }
        public string? bookingDate { get; set; }
        public int? SNO { get; set; }


        public string? chqDate { get; set; }
        public string? NatureName { get; set; }
        public decimal? AccountNature { get; set; }
        public string? bType { get; set; }
        public string? BankName { get; set; }
        public string? desc { get; set; }
        public string? VoucherNo { get; set; }
        public int? VoucherType { get; set; }
        public int? AccountCode { get; set; }
        public int? GroupOrder { get; set; }
        public string? chq { get; set; }
        public int? calcAmount { get; set; }
        public string? chqNo { get; set; }
        public int? AccountId { get; set; }
        public int? AccountGrCode { get; set; }
        public string? AccountName { get; set; }
        public string? AccountType { get; set; }
        public string? GRCode { get; set; }
        public string? ParentName { get; set; }
        public string? AccountDescription { get; set; }
        public string? LINK { get; set; }
        public int TRAN_ID { get; set; }
        public int VC_TYPE { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal? Balance { get; set; }
        public decimal? Balances { get; set; }
        public decimal? Balanced { get; set; }
        public string? BILL_NO { get; set; }
        public string? BILL_DATE { get; set; }


    }
}
