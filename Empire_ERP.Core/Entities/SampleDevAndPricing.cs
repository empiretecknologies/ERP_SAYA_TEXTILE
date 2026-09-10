using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire_ERP.Core.Entities
{
    public class SampleDevAndPricing
    {
        public int? TRAN_ID { get; set; }
        public DateTime? V_DATE { get; set; }
        public DateTime? REC_ON_DATE { get; set; }
        public string? VOUCHER_NO { get; set; }
        public int? PARTY_CODE { get; set; }
        public int? ACT_CODE { get; set; }
        public int? SCODE { get; set; }
        public int? SACODE { get; set; }
        public double? COMM { get; set; }
        public string? COMM_AMT { get; set; }
        public double? COMM_VAL { get; set; }
        public double? DISC { get; set; }
        public string? REF { get; set; }
        public string? REMARKS { get; set; }
        public string? SUP_INVNO { get; set; }
        public string? BTYPE { get; set; }
        public string? CLIENT_PO { get; set; }
        public int? MD_ID { get; set; }
        public int? BCODE { get; set; }
        public int? PERIOD_ID { get; set; }
        public string? ADD_USER_ID { get; set; }
        public DateTime? ADD_DATE { get; set; }
        public string? ADD_COMPUTER_NAME { get; set; }
        public string? ADD_IP_ADDRESS { get; set; }
        public string? EDIT_USER_ID { get; set; }
        public DateTime? EDIT_DATE { get; set; }
        public string? EDIT_COMPUTER_NAME { get; set; }
        public string? EDIT_IP_ADDRESS { get; set; }
        public string? ADD_POSTALCODE { get; set; }
        public string? EDIT_POSTALCODE { get; set; }
        public string? ASTATUS { get; set; }
        public int? MENU_ID { get; set; }
        public string? DLT { get; set; }
        public string? HS_CODE { get; set; }
        public string? DOC { get; set; }
        public string? PFR_DOC { get; set; }
        public string? ARTICLE_DOC { get; set; }
        public int? CURR_CODE { get; set; }
        public double? CRATE { get; set; }
        public int? BPARTY_CODE { get; set; }
        public int? BACT_CODE { get; set; }
        public int? DEPARTMENT { get; set; }
        public int? CURRENCY { get; set; }
        public int? SEASON { get; set; }
        public string? INTAKE { get; set; }
        public string? JOB_NO { get; set; }
    }

    public class CustomSampleDevAndPricing
    {
        public SampleDevAndPricing? Master { get; set; }
        public List<SampleDevAndPricingDetail>? Detail { get; set; }
    }


    public class SampleDevAndPricingRDLCReport
    {
        public int? TRAN_ID { get; set; }
        public int? MD_ID { get; set; }
        public string? REPORT_NAME { get; set; }
        public string? MD_NAME { get; set; }
        public string? BUYER_NAME { get; set; }
        public string? INVOICE_NUMBER { get; set; }
        public string? JOB_NO { get; set; }
        public string? DATE { get; set; }
        public string? REC_ON_DATE { get; set; }
        public string? PARTY_NAME { get; set; }
        public string? DEP { get; set; }
        public string? SEASON { get; set; }
        public int? INTAKE { get; set; }
        public string? COMPANY_NAME { get; set; }
        public string? COMPANY_ADDRESS { get; set; }
        public string? COMPANY_PHONE { get; set; }
        public string? BRANCH_ADDRESS { get; set; }
        public string? BRANCH_PHONE { get; set; }
        public string? COMPANY_LOGO { get; set; }
        public string? COMPANY_WATER { get; set; }
        public string? HEADER_NAME { get; set; }
        public string? ACT_GRCODE { get; set; }
        public string? SIG1 { get; set; }
        public string? SIG2 { get; set; }
        public string? SIG3 { get; set; }
        public string? SIG4 { get; set; }
        public string? REFERENCENO { get; set; }
        public string? REF { get; set; }
        public string? REMARKS { get; set; }
        public string? SUP_INVNO { get; set; }
        public string? CLIENT_PO { get; set; }
        public string? TERM { get; set; }
        public string? CURR { get; set; }
        public string? CURR_SIG { get; set; }
        public string? EDIT_USER_ID { get; set; }
        public string? MENU_TERMS { get; set; }
        public decimal DISC { get; set; }
    }

    public class CustomSampleDevAndPricingForPrintReport
    {
        public SampleDevAndPricingRDLCReport? Master { get; set; }
        public DataTable? Detail { get; set; }
    }

    public class SampleDevAndPricingDetail
    {
        public int? TRAN_ID { get; set; }
        public int? DT_CODE { get; set; }
        public int? ITEM_CODE { get; set; }
        public int? FABRIC { get; set; }
        public string? STYLE { get; set; }
        public double? QTY { get; set; }
        public int? UNIT { get; set; }
        public double? QTY2 { get; set; }
        public double? BAL_QTY { get; set; }
        public double? RATE { get; set; }
        public double? AMT { get; set; }
        public string? COMM_TYPE { get; set; }
        public double? COMM_RATE { get; set; }
        public double? COMM_AMT { get; set; }
        public double? DISC { get; set; }
        public double? DISC_AMT { get; set; }
        public double? TAX { get; set; }
        public double? TAX_AMT { get; set; }
        public double? ADV { get; set; }
        public double? ADV_AMT { get; set; }
        public double? NET_AMT { get; set; }
        public string? IMAGE { get; set; }
        public string? DT_DESC { get; set; }
        public string? QUALITY { get; set; }
        public string? RATIO { get; set; }
        public string? CLIENT_PO { get; set; }
        public int? COLOR { get; set; }
        public int? SIZE { get; set; }
        public int? GRADE { get; set; }
        public int? GSM { get; set; }
        public decimal? PROJ_QTY { get; set; }
        public decimal? TARGET_PRICE { get; set; }
        public int? WAREHOUSE { get; set; }
        public DateTime? DEL_DATE { get; set; }
        public DateTime? DUE_DATE { get; set; }
        public int? DUE_DAYS { get; set; }
        public string? VEH { get; set; }
        public int? BCODE { get; set; }
        public int? PERIOD_ID { get; set; }
        public string? ADD_USER_ID { get; set; }
        public DateTime? ADD_DATE { get; set; }
        public string? ADD_COMPUTER_NAME { get; set; }
        public string? ADD_IP_ADDRESS { get; set; }
        public string? EDIT_USER_ID { get; set; }
        public DateTime? EDIT_DATE { get; set; }
        public string? EDIT_COMPUTER_NAME { get; set; }
        public string? EDIT_IP_ADDRESS { get; set; }
        public string? ADD_POSTALCODE { get; set; }
        public string? EDIT_POSTALCODE { get; set; }
        public int? MENU_ID { get; set; }
        public string? DLT { get; set; }
        public int? CHK { get; set; }
        public int? PICK_ID { get; set; }
        public int? PICK_ID_D { get; set; }
        public int? PARTY_CODE { get; set; }
        public int? ACT_CODE { get; set; }
        public int? CURR_CODE { get; set; }
        public decimal? CRATE { get; set; }
    }
}
