using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Empire_ERP.Controllers
{
    [ExtractMenuCode]
    [CheckSession]
    public class SalesSamplingReportsController : BaseController
    {
        public IPeriodService _periodService { get; set; }
        public ISalesSamplingReportsService _salesSamplingReportservice { get; set; }
        public ICompanyService _companyService { get; set; }
        public SalesSamplingReportsController(IMenuService menuService, IPeriodService periodService, ISalesSamplingReportsService SalesSamplingReportservice, ICompanyService companyService ,IBaseService baseService) : base(menuService,baseService)
        {
            _periodService = periodService;
            _salesSamplingReportservice = SalesSamplingReportservice;
            _companyService = companyService;
        }

        public IActionResult Index()
        {
            var common = CommonHelper.GetValues(HttpContext);
            var periodInfo = _periodService.GetPeriodById(Convert.ToInt32(common.Period));
            var currentCompanyResponse = _companyService.GetCompanyByCode(common.Company);
            var currentCompany = (Company)currentCompanyResponse.data;
            ViewBag.CompanyName = currentCompany.C_NAME;
            ViewBag.StartDate = ((Period)periodInfo.data).START_D.Value.ToString("yyyy-MM-dd");
            //ViewBag.EndDate = ((Period)periodInfo.data).CLOSING == 1
            //   ? ((Period)periodInfo.data).START_E.Value.ToString("yyyy-MM-dd")
            //   : DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.EndDate = ((Period)periodInfo.data).START_E.Value.ToString("yyyy-MM-dd");
            ViewBag.Supplier = DropdownService.CustomPartyTypeDropdownWithAccountCode(common.RoleID, common.RoleType);
            ViewBag.Items = DropdownService.ItemMasterDropdown(CommonHelper.GetValues(HttpContext).RoleID, CommonHelper.GetValues(HttpContext).RoleType);
            ViewBag.Season = DropdownService.GetSeasonData();
            return View();
        }

        [HttpGet]
        public JsonResult GetControls()
        {
            try
            {
                var common = CommonHelper.GetValues(HttpContext);
                var data = common.RoleType == "A"
                    ? DropdownService.GetAccountsForAccountingReport(true, 0, common.Branch, 0)
                    : DropdownService.GetAccountsForAccountingReport(true, common.RoleID, common.Branch, common.ShowSelected);
                //var data = DropdownService.GetAccountsForAccountingReport(true);
                return Json(new { data = data, msgType = 1 });
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(new { data = _catchMessage, msgType = 2 });
            }
        }

        [HttpGet]
        public JsonResult POJobsDropdown()
        {
            try
            {
                var data = DropdownService.POJobsDropdown();
                return Json(new { data = data, msgType = 1 });
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(new { data = _catchMessage, msgType = 2 });
            }
        }

        [HttpGet]
        public JsonResult GetSubsidiarities()
        {
            try
            {
                var common = CommonHelper.GetValues(HttpContext);
                var data = common.RoleType == "A"
                    ? DropdownService.GetAccountsForAccountingReport(false, 0, common.Branch, 0)
                    : DropdownService.GetAccountsForAccountingReport(false, common.RoleID, common.Branch, common.ShowSelected);
                //var data = DropdownService.GetAccountsForAccountingReport(false);
                return Json(new { data = data, msgType = 1 });
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(new { data = _catchMessage, msgType = 2 });
            }
        }

        [HttpGet]
        public JsonResult GetReportTypes()
        {
            try
            {
                var data = _salesSamplingReportservice.GetReportTypes(CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(new { data = _catchMessage, msgType = 2 });
            }
        }

        [HttpPost]
        public JsonResult GenerateReport(SalesSamplingReports report)
        {
            try
            {
                var common = CommonHelper.GetValues(HttpContext);
                var Menu = _menuService.GetMenu(common.MenuID);
                Menu menu = new Menu();
                if (Menu.data != null)
                {
                    menu = (Menu)Menu.data;
                }

                var data = _salesSamplingReportservice.GetReportData(report, CommonHelper.GetValues(HttpContext), menu);
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(new { data = _catchMessage, msgType = 2 });
            }
        }
    }
}
