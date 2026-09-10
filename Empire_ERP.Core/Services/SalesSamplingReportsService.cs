using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class SalesSamplingReportsService : ISalesSamplingReportsService
    {
        public ISalesSamplingReportsRepository _salesSamplingReportsRepository { get; set; }
        public SalesSamplingReportsService(ISalesSamplingReportsRepository SalesSamplingReportsRepository)
        {
            _salesSamplingReportsRepository = SalesSamplingReportsRepository;
        }

        public MyHttpResponseMessage GetReportTypes(Common common)
        {
            return _salesSamplingReportsRepository.GetReportTypes(common.MenuID, common.RoleType, common.RoleID);
        }

        public MyHttpResponseMessage GetReportData(SalesSamplingReports report, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {

                var validReportIds = new HashSet<int?> { 140 };

                if (validReportIds.Contains(report.ReportID))
                {
                    response = _salesSamplingReportsRepository.GetReportData(report, common, menu);
                }
                else
                {
                    response.msgType = 2;
                    response.msg = "This report is not available yet but this will be available soon.";
                }
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                response.msg = _catchMessage;
                response.msgType = 2;
            }
            return response;
        }
    }
}