using System.Data;

namespace Empire_ERP.Core.Entities
{
    public class PriceQuoted
    {
        public int? TRAN_ID { get; set; }
        public DateTime? V_DATE { get; set; }
        public string? VOUCHER_NO { get; set; }
        public string? REF { get; set; }
        public string? CLIENT_PO { get; set; }
        public int? PARTY_CODE { get; set; }
        public int? ACT_CODE { get; set; }
        public int? JOBNO { get; set; }
        


        public string? REMARKS { get; set; }
        
        public string? ASTATUS { get; set; }
        public int? MENU_ID { get; set; }
        
        public string? DLT { get; set; }
        
    }

    public class PriceQuotedDetail
    {
        public int? TRAN_ID { get; set; }
        public int? DT_CODE { get; set; }
        public int? SPARTY_CODE { get; set; }
        public int? SACT_CODE { get; set; }
        public int? YARN_CODE { get; set; }
        public double? COMM_RATE { get; set; }
        public DateTime? PQ_DATE { get; set; }
        public double? RATE { get; set; }
        public int? PICK_ID { get; set; }
        public string? REV_STATUS { get; set; }
        public int? REV_REF { get; set; }
       
        
        public bool? REV_TOGGLE { get; set; }

    }

    public class CustomPriceQuoted
    {
        public PriceQuoted? Master { get; set; }
        public List<PriceQuotedDetail>? Detail { get; set; }
    }

    public class PriceQuotedRDLCReport
    {
        public int? TRAN_ID { get; set; }
        public int? MD_ID { get; set; }
        public string? REPORT_NAME { get; set; }
        public string? MD_NAME { get; set; }
        public string? INVOICE_NUMBER { get; set; }
        public string? ASTATUS { get; set; }
        public string? VOUCHER_NO { get; set; }

        public string? DATE { get; set; }
        public string? MITEM_NAME { get; set; }
        public string? COMPANY_NAME { get; set; }
        public string? COMPANY_ADDRESS { get; set; }
        public string? COMPANY_PHONE { get; set; }
        public string? BRANCH_ADDRESS { get; set; }
        public string? BRANCH_PHONE { get; set; }
        public string? COMPANY_LOGO { get; set; }
        public string? COMPANY_WATER { get; set; }
        public string? HEADER_NAME { get; set; }
        public string? REF { get; set; }
        public string? PROCESS { get; set; }
        public string? REMARKS { get; set; }
        public string? ORDER_QTY { get; set; }
        public string? BQTY { get; set; }
        public string? COST { get; set; }
        public string? LOSS { get; set; }
        public string? SIG1 { get; set; }
        public string? SIG2 { get; set; }
        public string? SIG3 { get; set; }
        public string? SIG4 { get; set; }
        public string? REFERENCENO { get; set; }
        public string? TERM { get; set; }
        public string? EDIT_USER_ID { get; set; }
        public string? MENU_TERMS { get; set; }
        public string? TERMS { get; set; }
        public string? SUP_NAME { get; set; }
        public string? CLIENT_NAME { get; set; }
        public string? CLIENT_PO { get; set; }
        public string? PARTY_NAME { get; set; }
        public string? CURRENCY { get; set; }
        public string? CURR{ get; set; }
        public string? USER { get; set; }
    }

    public class CustomPriceQuotedForPrintReport
    {
        public PriceQuotedRDLCReport? Master { get; set; }
        public DataTable? Detail { get; set; }
    }
}