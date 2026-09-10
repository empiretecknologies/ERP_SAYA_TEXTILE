using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using System.Data;
using System.Text;
using static Azure.Core.HttpHeader;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class SampleDevAndPricingController : BaseController
    {
        public ISampleDevAndPricingService _SampleDevAndPricingService { get; set; }
        private readonly IWebHostEnvironment _hostingEnvironment;
        public SampleDevAndPricingController(ISampleDevAndPricingService SampleDevAndPricingService, IMenuService menuService, IWebHostEnvironment hostingEnvironment,IBaseService baseService) : base(menuService,baseService)
        {
            _SampleDevAndPricingService = SampleDevAndPricingService;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            var common = CommonHelper.GetValues(HttpContext);
            ViewBag.DateTime = CommonService.GetDateTime("Pakistan Standard Time");
            ViewBag.Departments = DropdownService.DepartmentDropdownWithControlName();
            ViewBag.Seasons = DropdownService.GetSeasonData();
            var BranchID = HttpContext.Session.GetString("Branch");
            var CompanyID = HttpContext.Session.GetString("Company");
            string nextId = "";
            string formType = "";
            int compCond = 2;

            string maxIdQuery = "SELECT PICK_TYPE, B_I FROM TBL_MENU_BUILDER WHERE DLT = 'T' AND ID = '" + common.MenuID + "'";
            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            {
                SqlCommand command = new SqlCommand(maxIdQuery, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Indexes: 0 = MENU_NAME, 1 = B_I
                        //formType = reader.GetString(0);
                        formType = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        nextId = reader.GetValue(1).ToString(); // Use GetValue to be safe for non-string types
                    }
                }
            }

            string compCondQuery = "SELECT CON_QTY FROM TBL_MENU_BUILDER WHERE DLT = 'T' AND ID = '" + common.MenuID + "'";
            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            {
                SqlCommand command = new SqlCommand(compCondQuery, connection);
                connection.Open();
                object result = command.ExecuteScalar();
                compCond = Convert.ToInt32(result);
            }

            ViewBag.Type = nextId;
            ViewBag.FormType = formType;
            ViewBag.CompCond = compCond;
            ViewBag.Items = DropdownService.ItemMasterDropdown(CommonHelper.GetValues(HttpContext).RoleID, CommonHelper.GetValues(HttpContext).RoleType);
            ViewBag.Sizes = DropdownService.SizeDropdown();
            ViewBag.Blend = DropdownService.GetBlendData();
            ViewBag.GSMData = DropdownService.GetGSMData();
            ViewBag.Fabric = DropdownService.GetFabricData();
            ViewBag.Units = DropdownService.UnitDropdown();
            ViewBag.Colors = DropdownService.ColorDropdown();
            ViewBag.Grades = DropdownService.GradeDropdown();
            ViewBag.ClientPO = DropdownService.ClientPODropdown();
            ViewBag.Currencies = DropdownService.CurrencyDropdownWithControlName();
            ViewBag.ReportTypes = GetReportTypes();
            ViewBag.PartyType = common.RoleType == "A"
                ? DropdownService.PartyTypeDropdownForInvoice(0, common.Branch, 0)
                : DropdownService.PartyTypeDropdownForInvoice(common.RoleID, common.Branch, common.ShowSelected);
            ViewBag.Salesman = common.RoleType == "A"
                ? DropdownService.SalesmanDropdown(0, common.Branch, 0)
                : DropdownService.SalesmanDropdown(common.RoleID, common.Branch, common.ShowSelected);
            ViewBag.Permissions = common.RoleType == "A"
                ? "Admin"
                : CommonHelper.GetPermissionByMenueID(common.RoleID, common.MenuID);
            ViewBag.Limit = CommonHelper.GetLimitByMenueID(common.MenuID);
            ViewBag.Parties = DropdownService.CustomPartyTypeDropdownWithAccountCode(common.RoleID, common.RoleType);
            var response = _menuService.GetMenu(common.MenuID);
            if (response.msgType == 1)
            {
                ViewBag.DATA_CLEAR = ((Menu)response.data).DATA_CLEAR;
            }
            return View();
        }
        [HttpGet]
        public JsonResult GetCurrencies()
        {
            try
            {
                var data = DropdownService.CurrencyDropdownWithControlName();
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
        public JsonResult GetParties()
        {
            try
            {
                var common = CommonHelper.GetValues(HttpContext);
                var data = common.RoleType == "A"
                    ? DropdownService.PartyTypeDropdownForInvoice(0, common.Branch, 0)
                    : DropdownService.PartyTypeDropdownForInvoice(common.RoleID, common.Branch, common.ShowSelected);
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
        public JsonResult GetItems()
        {
            try
            {
                var common = CommonHelper.GetValues(HttpContext);
                var data = DropdownService.ItemMasterDropdown(common.RoleID, common.RoleType);
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetUnits()
        {
            try
            {
                var data = DropdownService.UnitDropdown();
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetColors()
        {
            try
            {
                var data = DropdownService.ColorDropdown();
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetSizes()
        {
            try
            {
                var data = DropdownService.SizeDropdown();
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetGrades()
        {
            try
            {
                var data = DropdownService.GradeDropdown();
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetWarehouses()
        {
            try
            {
                var data = DropdownService.WareHouseDropdown();
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetDepartments()
        {
            try
            {
                var data = DropdownService.DepartmentDropdownWithControlName();
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult QuickSearch()
        {
            try
            {
                var data = _SampleDevAndPricingService.QuickSearch(CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        //[HttpPost]
        //public JsonResult GetSampleDevAndPricing(int skip = 0, int take = 12, string sort = null, string filter = null, string group = null)
        //{
        //    try
        //    {
        //        var data = _SampleDevAndPricingService.QuickSearch(CommonHelper.GetValues(HttpContext), skip, take, filter, group, sort);
        //        return Json(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        string _catchMessage = ex.Message;
        //        if (ex.InnerException != null)
        //        {
        //            _catchMessage += "<br/>" + ex.InnerException.Message;
        //        }
        //        return Json(_catchMessage);
        //    }
        //}

        [HttpGet]
        public JsonResult GetSampleDevAndPricingByCode(int code)
        {
            try
            {
                var data = _SampleDevAndPricingService.GetSampleDevAndPricingByCode(code, CommonHelper.GetValues(HttpContext));
                var detailData = _SampleDevAndPricingService.GetSampleDevAndPricingDetailByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(new { Master = data, Detail = detailData });
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetSampleDevAndPricingDetailByCode(int code)
        {
            try
            {
                var data = _SampleDevAndPricingService.GetSampleDevAndPricingDetailByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetSampleDevAndPricingPickDetailByCode(int code)
        {
            try
            {
                var data = _SampleDevAndPricingService.GetSampleDevAndPricingPickDetailByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetSampleDevAndPricingDetailByItem(int code, int qty)
        {
            try
            {
                var data = _SampleDevAndPricingService.GetSampleDevAndPricingDetailByItem(code, qty, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpPost]
        public JsonResult Save(CustomSampleDevAndPricing modelRecord)
        {
            try
            {
                var data = _SampleDevAndPricingService.Save(modelRecord, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpPost]
        public JsonResult Delete(int code)
        {
            try
            {
                var data = _SampleDevAndPricingService.Delete(code, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveImage()
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            var uniqueFileName = "";
            var filePath = "";

            try
            {
                IFormFile uploadedFile = Request.Form.Files[0];
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Client", "uploads", "MpoLayout");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Get the file extension
                    string fileExtension = Path.GetExtension(uploadedFile.FileName).ToLower();


                    // Generate a unique filename for the PDF
                    uniqueFileName = Guid.NewGuid().ToString().Substring(0, 25) + fileExtension;
                    filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the PDF file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    }

                    response.msg = "PDF uploaded successfully.";
                    response.msgType = 1;
                    response.data = $"/Client/uploads/MpoLayout/{uniqueFileName}"; // Return file path
                }
                else
                {
                    response.msg = "No file uploaded.";
                    response.msgType = 2;
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

            return Json(response);
        }

        [HttpPost]
        public JsonResult CopyRecord(CopyRecord record)
        {
            try
            {
                var data = _SampleDevAndPricingService.CopyRecord(record, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(new { msg = _catchMessage, msgType = 2 });
            }
        }

        [HttpPost]
        public JsonResult DeleteSampleDevAndPricingDetailByCode(int code)
        {
            try
            {
                var data = _SampleDevAndPricingService.DeleteSampleDevAndPricingDetailByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public MyHttpResponseMessage GetSalesmanByParty(int Id)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            string nextId = "";
            string maxIdQuery = "SELECT SACT_CODE FROM TBL_PARTY_TYPES WHERE  PARTY_CODE = '" + Id + "'";
            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            {
                SqlCommand command = new SqlCommand(maxIdQuery, connection);
                connection.Open();
                object result = command.ExecuteScalar();
                nextId = Convert.ToString(result);
            }
            var companies = FetchSalesman(nextId);
            List<PartyTypes> companyList = new List<PartyTypes>();
            if (companies.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow Row in companies.Tables[0].Rows)
                {
                    PartyTypes company = new PartyTypes();
                    company.PARTY_CODE = Convert.ToInt32(Row["PARTY_CODE"]);
                    company.PARTY_NAME = Convert.ToString(Row["PARTY_NAME"]);
                    companyList.Add(company);
                }
            }
            response.data = companies;
            response.msg = "";
            response.msgType = 1;
            return response;
        }

        public DataSet FetchSalesman(string Id)
        {
            string query = string.Empty;
            if (Id == "")
            {
                query = "SELECT PARTY_CODE,PARTY_NAME FROM TBL_PARTY_TYPES WHERE PARTY_TYPE_CODE = 9 AND DLT = 'T' AND ASTATUS = 'Y'";
            }
            else
            {
                query = "SELECT PARTY_CODE,PARTY_NAME FROM TBL_PARTY_TYPES WHERE PARTY_TYPE_CODE = 9 AND DLT = 'T' AND ASTATUS = 'Y' AND ACT_CODE = '" + Id + "'";
            }
            DataSet data = SqlHelper.ExecuteDataset(new SQLService().getconnstring(), CommandType.Text, query);
            return data;
        }

        [HttpGet]
        public JsonResult GetReportTypes()
        {
            try
            {
                var data = _menuService.GetMenuDetails(CommonHelper.GetValues(HttpContext).MenuID);
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
        public JsonResult GetPrintReport(SampleDevAndPricingRDLCReport model)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var responseReport = GenerateReport(model);

                if (responseReport.msgType == 1)
                {
                    response.data = responseReport.data;
                    response.msg = "";
                    response.msgType = 1;
                }
                else
                {
                    response.msg = responseReport.msg;
                    response.msgType = 2;
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
            return Json(response);
        }


        private MyHttpResponseMessage GenerateReport(SampleDevAndPricingRDLCReport model)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();

            var filePath = "";
            try
            {
                if (model != null && model.TRAN_ID > 0)
                {
                    Reports.Datasets.BarcodeReportDataset.InspectionServiceChargesReportDataTable inspectionServiceChargesDetails = new Reports.Datasets.BarcodeReportDataset.InspectionServiceChargesReportDataTable();
                    var responseMessage = _SampleDevAndPricingService.GetDataForReport(model, inspectionServiceChargesDetails, CommonHelper.GetValues(HttpContext));
                    if (responseMessage.msgType != 1)
                    {
                        return responseMessage;
                    }
                    var reportData = (CustomSampleDevAndPricingForPrintReport)responseMessage.data;

                    using (LocalReport report = new LocalReport())
                    {
                        var path = Path.Combine(_hostingEnvironment.ContentRootPath, @$"Reports\{reportData.Master?.REPORT_NAME}.rdlc");
                        using (var stReader = new StreamReader(path))
                        {
                            string stringreader = stReader.ReadToEnd();
                            byte[] byteArray = Encoding.UTF8.GetBytes(stringreader);
                            using (var stream = new MemoryStream(byteArray))
                            {
                                report.EnableExternalImages = true;
                                report.LoadReportDefinition(stream);
                                report.DataSources.Clear();

                                var companyLogoPath = Path.Combine(_hostingEnvironment.WebRootPath, @$"Client\Company\{reportData.Master?.COMPANY_LOGO}");
                                bool? showCompanyLogo = true;
                                if (!System.IO.File.Exists(companyLogoPath))
                                {
                                    showCompanyLogo = false;
                                }

                                ReportParameter parameter1 = new ReportParameter("Header", reportData.Master?.HEADER_NAME);
                                ReportParameter parameter2 = new ReportParameter("CompanyName", reportData.Master?.COMPANY_NAME);
                                ReportParameter parameter3 = new ReportParameter("CompanyAddress", reportData.Master?.COMPANY_ADDRESS);
                                ReportParameter parameter4 = new ReportParameter("CompanyLogo", new Uri(Path.Combine(_hostingEnvironment.WebRootPath, @$"Client\Company\{reportData.Master?.COMPANY_LOGO}")).AbsoluteUri);
                                ReportParameter parameter5 = new ReportParameter("Date", reportData.Master?.DATE);
                                ReportParameter parameter6 = new ReportParameter("Sig1", reportData.Master?.SIG1);
                                ReportParameter parameter7 = new ReportParameter("Sig2", reportData.Master?.SIG2);
                                ReportParameter parameter8 = new ReportParameter("Sig3", reportData.Master?.SIG3);
                                ReportParameter parameter9 = new ReportParameter("Sig4", reportData.Master?.SIG4);
                                ReportParameter parameter10 = new ReportParameter("User", reportData.Master?.SIG4);
                                ReportParameter parameter11 = new ReportParameter("ClientPO", reportData.Master?.CLIENT_PO);
                                ReportParameter parameter12 = new ReportParameter("Party", reportData.Master?.PARTY_NAME);
                                ReportParameter parameter13 = new ReportParameter("Curr", reportData.Master?.CURR);
                                ReportParameter parameter14 = new ReportParameter("RecOn", reportData.Master?.REC_ON_DATE);



                                report.SetParameters(new ReportParameter[] { parameter1, parameter2, parameter3, parameter4, parameter5, parameter6, 
                                    parameter7, parameter8, parameter9, parameter10, parameter11, parameter12, parameter13, parameter14 });
                                report.Refresh();
                                report.DataSources.Add(new ReportDataSource() { Name = "ServiceChargesReportDataSet", Value = reportData.Detail });


                                byte[] file;
                                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, @"Client\SampleDevAndPricingReport");
                                if (!Directory.Exists(uploadsFolder))
                                {
                                    Directory.CreateDirectory(uploadsFolder);
                                }

                                if (report.IsReadyForRendering)
                                {
                                    string input = reportData.Master?.INVOICE_NUMBER;
                                    string[] parts = input.Split('/');
                                    string prefix = string.Empty;
                                    string voucherNumber = string.Empty;
                                    if (parts.Length >= 3)
                                    {
                                        prefix = parts[1];
                                        voucherNumber = parts[^1];
                                    }
                                    file = report.Render("PDF");
                                    filePath = $"{prefix} - {(reportData.Master?.PARTY_NAME).Replace(" / ", " - ")} - {voucherNumber}" + ".pdf";

                                    stReader.Close();
                                    stReader.Dispose();
                                    stream.Flush();
                                    stream.Close();
                                    stream.Dispose();
                                    report.Dispose();
                                    string reportPath = Path.Combine(uploadsFolder, filePath);
                                    System.IO.File.WriteAllBytes(reportPath, file);
                                    filePath = $"/Client/SampleDevAndPricingReport/{filePath}";
                                    response.data = filePath;
                                    response.msgType = 1;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.msg = "Unable to generate report. Please try again later.";
                response.msgType = 2;
            }
            return response;
        }

        [HttpGet]
        public JsonResult GetPickDataBySupplier(int pCode, int actCode)
        {
            try
            {
                var data = _SampleDevAndPricingService.GetPickDataBySupplier(pCode, actCode, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(new { msg = _catchMessage, msgType = 2 });
            }
        }

        
    }
}
