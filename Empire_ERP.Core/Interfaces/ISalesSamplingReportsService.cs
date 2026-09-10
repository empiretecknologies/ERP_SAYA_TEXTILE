using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISalesSamplingReportsService
    {
        MyHttpResponseMessage GetReportTypes(Common common);
        MyHttpResponseMessage GetReportData(SalesSamplingReports report, Common common, Menu menu);
    }
}