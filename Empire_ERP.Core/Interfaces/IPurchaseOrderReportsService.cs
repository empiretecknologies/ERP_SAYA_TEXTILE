using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface IPurchaseOrderReportsService
    {
        MyHttpResponseMessage GetReportTypes(Common common);
        MyHttpResponseMessage GetReportData(PurchaseOrderReports report, Common common, Menu menu);
    }
}