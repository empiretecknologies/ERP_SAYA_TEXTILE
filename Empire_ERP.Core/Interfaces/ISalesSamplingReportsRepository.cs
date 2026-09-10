using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISalesSamplingReportsRepository
    {
        MyHttpResponseMessage GetReportTypes(int menuID, string roleType, int? roleId);
        MyHttpResponseMessage GetReportData(SalesSamplingReports report, Common common, Menu menu);
    }
}