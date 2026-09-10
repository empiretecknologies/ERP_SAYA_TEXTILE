using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface IPurchaseOrderReportsRepository
    {
        MyHttpResponseMessage GetReportTypes(int menuID, string roleType, int? roleId);
        MyHttpResponseMessage GetReportData(PurchaseOrderReports report, Common common, Menu menu);
    }
}