using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using System.Text;
using static Azure.Core.HttpHeader;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class SampleSubmissionController : BaseController
    {
        public ISampleSubmissionService _SampleSubmissionService { get; set; }
        private readonly IWebHostEnvironment _hostingEnvironment;
        public SampleSubmissionController(ISampleSubmissionService SampleSubmissionService, IMenuService menuService, IWebHostEnvironment hostingEnvironment,IBaseService baseService) : base(menuService,baseService)
        {
            _SampleSubmissionService = SampleSubmissionService;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            var common = CommonHelper.GetValues(HttpContext);
            ViewBag.Parties = DropdownService.CustomPartyTypeDropdownWithAccountCode(common.RoleID, common.RoleType);
            ViewBag.Yarn = DropdownService.GetYarnData();

            //ViewBag.Items = DropdownService.RawItemsDropdown();
            //ViewBag.FinishItems = DropdownService.FinishItemsDropdown();
            //ViewBag.Units = DropdownService.UnitDropdown();
            //ViewBag.Colors = DropdownService.ColorDropdown();
            //ViewBag.Sizes = DropdownService.SizeDropdown();
            //ViewBag.Supplier = GetParties();
            //ViewBag.Ports = DropdownService.PortDropdown();
            //ViewBag.SEntity = DropdownService.SEntityDropdown();
            //ViewBag.DistributionChannel = DropdownService.DistributionChannelDropdown();
            //ViewBag.Grade = DropdownService.GradeDropdown();
            //ViewBag.Season = DropdownService.GetSeasonData();
            //ViewBag.Items = DropdownService.ItemMasterDropdown(CommonHelper.GetValues(HttpContext).RoleID, CommonHelper.GetValues(HttpContext).RoleType);
            //ViewBag.ClientPO = DropdownService.ClientPODropdown();
            //ViewBag.Fabric = DropdownService.GetFabricData();
            //ViewBag.GSMData = DropdownService.GetGSMData();
            ViewBag.Permissions = CommonHelper.GetValues(HttpContext).RoleType == "A"
                ? "Admin"
                : CommonHelper.GetPermissionByMenueID(common.RoleID, common.MenuID);

            int compCond = 2;
            string compCondQuery = "SELECT CON_QTY FROM TBL_MENU_BUILDER WHERE DLT = 'T' AND ID = '" + common.MenuID + "'";
            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            {
                SqlCommand command = new SqlCommand(compCondQuery, connection);
                connection.Open();
                object result = command.ExecuteScalar();
                compCond = Convert.ToInt32(result);
            }

            return View();
        }

        [HttpGet]
        //public JsonResult GetFinishItems()
        //{
        //    try
        //    {
        //        var data = DropdownService.FinishItemsDropdown();
        //        return Json(new { data = data, msgType = 1 });
        //    }
        //    catch (Exception ex)
        //    {
        //        string _catchMessage = ex.Message;
        //        if (ex.InnerException != null)
        //        {
        //            _catchMessage += "<br/>" + ex.InnerException.Message;
        //        }
        //        return Json(new { data = _catchMessage, msgType = 2 });
        //    }
        //}

        //[HttpGet]
        //public JsonResult GetParties()
        //{
        //    try
        //    {
        //        var common = CommonHelper.GetValues(HttpContext);
        //        var data = DropdownService.CustomPartyTypeDropdownWithAccountCode(common.RoleID, common.RoleType);
        //        return Json(new { data = data, msgType = 1 });
        //    }
        //    catch (Exception ex)
        //    {
        //        string _catchMessage = ex.Message;
        //        if (ex.InnerException != null)
        //        {
        //            _catchMessage += "<br/>" + ex.InnerException.Message;
        //        }
        //        return Json(new { data = _catchMessage, msgType = 2 });
        //    }
        //}

        //[HttpGet]
        //public JsonResult POJobsDropdown()
        //{
        //    try
        //    {
        //        var data = DropdownService.POJobsDropdown();
        //        return Json(new { data = data, msgType = 1 });
        //    }
        //    catch (Exception ex)
        //    {
        //        string _catchMessage = ex.Message;
        //        if (ex.InnerException != null)
        //        {
        //            _catchMessage += "<br/>" + ex.InnerException.Message;
        //        }
        //        return Json(new { data = _catchMessage, msgType = 2 });
        //    }
        //}

        //[HttpGet]
        //public JsonResult GetProcesses()
        //{
        //    try
        //    {
        //        var data = DropdownService.ProcessesDropdown();
        //        return Json(new { data = data, msgType = 1 });
        //    }
        //    catch (Exception ex)
        //    {
        //        string _catchMessage = ex.Message;
        //        if (ex.InnerException != null)
        //        {
        //            _catchMessage += "<br/>" + ex.InnerException.Message;
        //        }
        //        return Json(new { data = _catchMessage, msgType = 2 });
        //    }
        //}
        [HttpGet]
        public JsonResult POJobsDropdown()
        {
            try
            {
                var data = DropdownService.SPJobsDropdown();
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
        public JsonResult GetSampleSubmissions()
        {
            try
            {
                var data = _SampleSubmissionService.QuickSearch(CommonHelper.GetValues(HttpContext));
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
        public JsonResult Save(CustomSampleSubmission modelRecord)
        {
            try
            {
                var data = _SampleSubmissionService.Save(modelRecord, CommonHelper.GetValues(HttpContext));
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

        [HttpGet]
        public JsonResult GetSampleSubmissionByCode(int code)
        {
            try
            {
                var data = _SampleSubmissionService.GetSampleSubmissionByCode(code, CommonHelper.GetValues(HttpContext));
                var detailData = _SampleSubmissionService.GetSampleSubmissionDetailByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(new { Master = data, Detail = detailData });
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

        [HttpGet]
        public JsonResult GetSampleSubmissionDetailByCode(int code)
        {
            try
            {
                var data = _SampleSubmissionService.GetSampleSubmissionDetailByCode(code, CommonHelper.GetValues(HttpContext));
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
        public JsonResult GetBatchDetailByProcess(SampleSubmission model)
        {
            try
            {
                var data = _SampleSubmissionService.GetBatchDetailByProcess(model,  CommonHelper.GetValues(HttpContext));
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
        public JsonResult Delete(int code)
        {
            try
            {
                var data = _SampleSubmissionService.Delete(code, CommonHelper.GetValues(HttpContext));
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
        public JsonResult CopyRecord(CopyRecord record)
        {
            try
            {
                var data = _SampleSubmissionService.CopyRecord(record, CommonHelper.GetValues(HttpContext));
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
        public JsonResult DeleteSampleSubmissionDetailByCode(int code)
        {
            try
            {
                var data = _SampleSubmissionService.DeleteSampleSubmissionDetailByCode(code, CommonHelper.GetValues(HttpContext));
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
        public JsonResult GetPrintReport(SampleSubmissionRDLCReport model)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var filePath = GenerateReport(model);
                if (!String.IsNullOrEmpty(filePath))
                {
                    response.data = filePath;
                    response.msg = "";
                    response.msgType = 1;
                }
                else
                {
                    response.msg = "Unable to generate report. Please try again later.";
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
        
        private string GenerateReport(SampleSubmissionRDLCReport model)
        {
            var filePath = "";
            try
            {
                if (model != null && model.TRAN_ID > 0 && model.MD_ID > 0)
                {
                    Reports.Datasets.BarcodeReportDataset.InspectionServiceChargesReportDataTable reportDetails = new Reports.Datasets.BarcodeReportDataset.InspectionServiceChargesReportDataTable();
                    var responseMessage = _SampleSubmissionService.GetDataForReport(model, reportDetails, CommonHelper.GetValues(HttpContext));
                    if (responseMessage.msgType != 1)
                    {
                        return "";
                    }
                    var reportData = (CustomSampleSubmissionForPrintReport)responseMessage.data;

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
                                ReportParameter parameter13 = new ReportParameter("Curr", reportData.Master?.CURRENCY);
                                ReportParameter parameter14 = new ReportParameter("Remarks", reportData.Master?.REMARKS);


                                report.SetParameters(new ReportParameter[] { parameter1, parameter2, parameter3, parameter4, parameter5, parameter6,
                                    parameter7, parameter8, parameter9, parameter10, parameter11, parameter12, parameter13, parameter14 });
                                report.Refresh();
                                report.DataSources.Add(new ReportDataSource() { Name = "ServiceChargesReportDataSet", Value = reportData.Detail });

                                byte[] file;
                                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, @"Client\SampleSubmission");
                                if (!Directory.Exists(uploadsFolder))
                                {
                                    Directory.CreateDirectory(uploadsFolder);
                                }

                                if (report.IsReadyForRendering)
                                {
                                    string input = reportData.Master?.CLIENT_PO;
                                    string[] parts = input.Split('/');
                                    string prefix = string.Empty;
                                    string voucherNumber = string.Empty;
                                    if (parts.Length >= 3)
                                    {
                                        prefix = parts[1];
                                        voucherNumber = parts[^1];
                                    }
                                    file = report.Render("PDF");
                                    filePath = $"{prefix} - {voucherNumber}" + ".pdf";

                                    stReader.Close();
                                    stReader.Dispose();
                                    stream.Flush();
                                    stream.Close();
                                    stream.Dispose();
                                    report.Dispose();
                                    string reportPath = Path.Combine(uploadsFolder, filePath);
                                    System.IO.File.WriteAllBytes(reportPath, file);
                                    filePath = $"/Client/SampleSubmission/{filePath}";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
            return filePath;
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
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Client", "uploads", "SampleSubmission");

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
                    response.data = $"/Client/uploads/SampleSubmission/{uniqueFileName}"; // Return file path
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
    }
}