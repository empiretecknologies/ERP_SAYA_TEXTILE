namespace Empire_ERP.Core.Entities
{
    public class SalesSamplingReports
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

    public class CustomSalesSamplingReports
    {
        public string? ITEM_NAME { get; set; }
        public string? DOC { get; set; }
        public string? SIZE_NAME { get; set; }
        public string? CLIENT_PO { get; set; }
        public string? ITEM_DETAIL { get; set; }
        public decimal? QTY { get; set; }
        public decimal? RATE { get; set; }
        public string? REF_ON_DATE { get; set; }
        


    }
}
