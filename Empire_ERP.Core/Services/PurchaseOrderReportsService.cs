using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class PurchaseOrderReportsService : IPurchaseOrderReportsService
    {
        public IPurchaseOrderReportsRepository _PurchaseOrderReportsRepository { get; set; }
        public PurchaseOrderReportsService(IPurchaseOrderReportsRepository PurchaseOrderReportsRepository)
        {
            _PurchaseOrderReportsRepository = PurchaseOrderReportsRepository;
        }

        public MyHttpResponseMessage GetReportTypes(Common common)
        {
            return _PurchaseOrderReportsRepository.GetReportTypes(common.MenuID, common.RoleType, common.RoleID);
        }

        public MyHttpResponseMessage GetReportData(PurchaseOrderReports report, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {

                if (report.ReportID == 138 || report.ReportID == 139)
                {
                    response = _PurchaseOrderReportsRepository.GetReportData(report, common, menu);
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