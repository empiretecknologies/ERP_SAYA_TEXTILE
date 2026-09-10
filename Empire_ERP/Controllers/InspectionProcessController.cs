using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json;
using System.Text;
using static Azure.Core.HttpHeader;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class InspectionProcessController : BaseController
    {
        public IInspectionProcessService _InspectionProcessService { get; set; }
        private readonly IWebHostEnvironment _hostingEnvironment;
        public InspectionProcessController(IInspectionProcessService InspectionProcessService, IMenuService menuService, IWebHostEnvironment hostingEnvironment,IBaseService baseService) : base(menuService,baseService)
        {
            _InspectionProcessService = InspectionProcessService;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            var common = CommonHelper.GetValues(HttpContext);

            var Menu = _menuService.GetMenu(common.MenuID);
            string? PERFIX = string.Empty;
            Menu menu = new Menu();
            if (Menu.data != null)
            {
                menu = (Menu)Menu.data;
                PERFIX = menu.PERFIX;
            }


            ViewBag.QualityCheck = DropdownService.GetQualityCheck();
            ViewBag.InspectionSizeChart = DropdownService.GetInspectionSizeChart();
            ViewBag.GetAQLChart = GetAQLChart();
            ViewBag.Username = common.Username;
            ViewBag.PERFIX = PERFIX;
            ViewBag.todayDate = DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.Items = DropdownService.RawItemsDropdown();
            ViewBag.FinishItems = DropdownService.FinishItemsDropdown();
            ViewBag.Units = DropdownService.UnitDropdown();
            ViewBag.Colors = DropdownService.ColorDropdown();
            ViewBag.Sizes = DropdownService.SizeDropdown();
            ViewBag.Supplier = GetParties();
            ViewBag.Ports = DropdownService.PortDropdown();
            ViewBag.SEntity = DropdownService.SEntityDropdown();
            ViewBag.DistributionChannel = DropdownService.DistributionChannelDropdown();
            ViewBag.Grade = DropdownService.GradeDropdown();
            ViewBag.Season = DropdownService.GetSeasonData();
            ViewBag.Items = DropdownService.ItemMasterDropdown(CommonHelper.GetValues(HttpContext).RoleID, CommonHelper.GetValues(HttpContext).RoleType);
            ViewBag.ClientPO = DropdownService.ClientPODropdown();
            ViewBag.Fabric = DropdownService.GetFabricData();
            ViewBag.GSMData = DropdownService.GetGSMData();
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
        public JsonResult GetFinishItems()
        {
            try
            {
                var data = DropdownService.FinishItemsDropdown();
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
                var data = DropdownService.CustomPartyTypeDropdownWithAccountCode(common.RoleID, common.RoleType);
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
                var data = DropdownService.InsJobsDropdown();
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
        public JsonResult GetProcesses()
        {
            try
            {
                var data = DropdownService.ProcessesDropdown();
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
        public JsonResult GetInspectionProcess()
        {
            try
            {
                var data = _InspectionProcessService.QuickSearch(CommonHelper.GetValues(HttpContext));
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
        public JsonResult GetAQLChart()
        {
            try
            {
                var data = _InspectionProcessService.GetAQLChart();
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
        public JsonResult Save(CustomInspectionProcess modelRecord)
        {
            try
            {
                var data = _InspectionProcessService.Save(modelRecord, CommonHelper.GetValues(HttpContext));
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
        public JsonResult SaveInspectionSheet(CustomInspectionSheet modelRecord)
        {
            try
            {
                var data = _InspectionProcessService.SaveInspectionSheet(modelRecord, CommonHelper.GetValues(HttpContext));
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
        public JsonResult GetLockGridData(int code)
        {
            try
            {
                var detailData = _InspectionProcessService.GetLockGridDataByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(new { Detail = detailData });
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
        public JsonResult GetInspectionProcessByCode(int code)
        {
            try
            {
                var data = _InspectionProcessService.GetInspectionProcessByCode(code, CommonHelper.GetValues(HttpContext));
                var detailData = _InspectionProcessService.GetInspectionProcessDetailByCode(code, CommonHelper.GetValues(HttpContext));
                var sheetData = _InspectionProcessService.GetInspectionSheetByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(new { Master = data, Detail = detailData, sheetData = sheetData });
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
        public JsonResult GetInspectionSheetByCode(int code)
        {
            try
            {
                var sheetData = _InspectionProcessService.GetInspectionSheetByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(new { sheetData = sheetData });
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
        public JsonResult GetInspectionProcessDetailByCode(int code)
        {
            try
            {
                var data = _InspectionProcessService.GetInspectionProcessDetailByCode(code, CommonHelper.GetValues(HttpContext));
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
        public JsonResult GetBatchDetailByProcess(string process)
        {
            try
            {
                var data = _InspectionProcessService.GetBatchDetailByProcess(process, CommonHelper.GetValues(HttpContext));
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
                var data = _InspectionProcessService.Delete(code, CommonHelper.GetValues(HttpContext));
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
        public JsonResult DeleteImage(DeleteImageRequest model)
        {
            try
            {
                var data = _InspectionProcessService.DeleteImage(model, CommonHelper.GetValues(HttpContext));
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
                var data = _InspectionProcessService.CopyRecord(record, CommonHelper.GetValues(HttpContext));
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
        public JsonResult DeleteInspectionProcessDetailByCode(int code)
        {
            try
            {
                var data = _InspectionProcessService.DeleteInspectionProcessDetailByCode(code, CommonHelper.GetValues(HttpContext));
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

        //[HttpPost]
        //public JsonResult SaveInspectionImages(IEnumerable<IFormFile> Files, string InspectionCode, string jobName, string jobNo, string ptranId)
        //{
        //    try
        //    {
        //        var common = CommonHelper.GetValues(HttpContext);
        //        List<string> savedPaths = new List<string>();

        //        // Folder Saving Logic
        //        string dbFolder = $@"Uploads/InspectionProcess/{jobName}/{common.MenuID}";
        //        string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dbFolder.Replace("/", "\\"));
        //        if (!Directory.Exists(physicalPath)) Directory.CreateDirectory(physicalPath);

        //        foreach (var file in Files)
        //        {
        //            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        //            string filePath = Path.Combine(physicalPath, fileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                file.CopyTo(stream);
        //            }
        //            savedPaths.Add("/" + dbFolder + "/" + fileName);
        //        }

        //        // Service call (Aapke style mein)
        //        var data = _InspectionProcessService.SaveInspectionImages(savedPaths, InspectionCode, jobNo, ptranId, common);
        //        return Json(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        string _catchMessage = ex.Message;
        //        if (ex.InnerException != null)
        //        {
        //            _catchMessage += "<br/>" + ex.InnerException.Message;
        //        }
        //        return Json(new { msg = _catchMessage, msgType = 2 });
        //    }
        //}

        //[HttpPost]
        //public JsonResult SaveInspectionImages(IEnumerable<IFormFile> Files, string modelJson)
        //{
        //    try
        //    {
        //        var common = CommonHelper.GetValues(HttpContext);
        //        var reqModel = JsonConvert.DeserializeObject<InspectionImageRequest>(modelJson);

        //        string dbFolder = $@"Client/InspectionProcess/{reqModel.JobName}/{common.MenuID}";
        //        string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dbFolder.Replace("/", "\\"));
        //        if (!Directory.Exists(physicalPath)) Directory.CreateDirectory(physicalPath);

        //        // 1. Files ko dictionary mein dalen taake naam se access ho sake
        //        var fileDict = Files?.ToDictionary(f => f.FileName, f => f);

        //        // 2. Sirf New images par loop chalayen aur path set karein
        //        foreach (var img in reqModel.Images.Where(x => x.IsNew))
        //        {
        //            if (fileDict != null && fileDict.ContainsKey(img.FileName))
        //            {
        //                var file = fileDict[img.FileName];
        //                string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        //                string filePath = Path.Combine(physicalPath, uniqueName);

        //                using (var stream = new FileStream(filePath, FileMode.Create))
        //                {
        //                    file.CopyTo(stream);
        //                }

        //                // Path ko direct model ke object mein save karein
        //                img.SavedPath = "/" + dbFolder + "/" + uniqueName;
        //            }
        //        }

        //        // 3. Service call (Ab humein alag se savedPaths list bhejne ki zaroorat nahi)
        //        var data = _InspectionProcessService.SaveInspectionImages(reqModel, common);
        //        return Json(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        string _msg = ex.InnerException != null ? ex.Message + "\n" + ex.InnerException.Message : ex.Message;
        //        return Json(new { msg = _msg, msgType = 2 });
        //    }
        //}

        [HttpPost]
        public JsonResult SaveInspectionImages(IEnumerable<IFormFile> Files, string modelJson)
        {
            try
            {
                var common = CommonHelper.GetValues(HttpContext);
                var reqModel = JsonConvert.DeserializeObject<InspectionImageRequest>(modelJson);

                // 1. Source check karke sub-folder define karen
                string subFolder = reqModel.IsSpecs ? "Specs" : "Findings";

                // 2. Path build karen: Client/InspectionProcess/JobName/MenuID/SubFolder
                string dbFolder = $@"Client/InspectionProcess/{reqModel.JobName}/{common.MenuID}/{subFolder}";
                string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dbFolder.Replace("/", "\\"));

                // Folder create karen agar nahi hai
                if (!Directory.Exists(physicalPath)) Directory.CreateDirectory(physicalPath);

                var fileDict = Files?.ToDictionary(f => f.FileName, f => f);

                foreach (var img in reqModel.Images.Where(x => x.IsNew))
                {
                    if (fileDict != null && fileDict.ContainsKey(img.FileName))
                    {
                        var file = fileDict[img.FileName];
                        string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(physicalPath, uniqueName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        // Database mein save hone wala relative path
                        img.SavedPath = "/" + dbFolder + "/" + uniqueName;
                    }
                }

                // Service call
                var data = _InspectionProcessService.SaveInspectionImages(reqModel, common);
                return Json(data);
            }
            catch (Exception ex)
            {
                string _msg = ex.InnerException != null ? ex.Message + "\n" + ex.InnerException.Message : ex.Message;
                return Json(new { msg = _msg, msgType = 2 });
            }
        }

        [HttpGet]
        public JsonResult GetInspectionImagesByCode(int ptranId, int jobNo, bool isSpecs)
        {
            try
            {
                var data = _InspectionProcessService.GetInspectionImagesByCode(ptranId, jobNo, isSpecs, CommonHelper.GetValues(HttpContext));
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
        public JsonResult GetPrintReport(InspectionProcessRDLCReport model)
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
        
        private string GenerateReport(InspectionProcessRDLCReport model)
        {
            var filePath = "";
            try
            {
                if (model != null && model.TRAN_ID > 0 && model.MD_ID > 0)
                {
                    Reports.Datasets.BarcodeReportDataset.MPODetailDataTable reportDetails = new Reports.Datasets.BarcodeReportDataset.MPODetailDataTable();
                    var responseMessage = _InspectionProcessService.GetDataForReport(model, reportDetails, CommonHelper.GetValues(HttpContext));
                    if (responseMessage.msgType != 1)
                    {
                        return "";
                    }
                    var reportData = (CustomInspectionProcessForPrintReport)responseMessage.data;

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

                                ReportParameter parameter = new ReportParameter("Header", reportData.Master?.HEADER_NAME);
                                ReportParameter parameter1 = new ReportParameter("CompanyName", reportData.Master?.COMPANY_NAME);
                                ReportParameter parameter2 = new ReportParameter("CompanyAddress", reportData.Master?.COMPANY_ADDRESS);
                                //ReportParameter parameter3 = new ReportParameter("CompanyPhone", reportData.Master?.COMPANY_PHONE);
                                ReportParameter parameter3 = new ReportParameter("User", reportData.Master?.USER);
                                ReportParameter parameter4 = new ReportParameter("Date", reportData.Master?.DATE);
                                ReportParameter parameter5 = new ReportParameter("Sig1", reportData.Master?.SIG1);
                                ReportParameter parameter6 = new ReportParameter("Sig2", reportData.Master?.SIG2);
                                ReportParameter parameter7 = new ReportParameter("Sig3", reportData.Master?.SIG3);
                                ReportParameter parameter8 = new ReportParameter("Sig4", reportData.Master?.SIG4);
                                ReportParameter parameter9 = new ReportParameter("ClientPo", reportData.Master?.CLIENT_PO);
                                ReportParameter parameter10 = new ReportParameter("Terms", reportData.Master?.TERMS);
                                ReportParameter parameter11 = new ReportParameter("Currency", reportData.Master?.CURRENCY);
                                ReportParameter parameter12 = new ReportParameter("OrderQty", reportData.Master?.ORDER_QTY);
                                //ReportParameter parameter3 = new ReportParameter("BranchAddress", reportData.Master?.BRANCH_ADDRESS);
                                //ReportParameter parameter4 = new ReportParameter("BranchPhone", reportData.Master?.BRANCH_PHONE);
                                //ReportParameter parameter12 = new ReportParameter("ShowCompanyLogo", Convert.ToString(showCompanyLogo));
                                ReportParameter parameter13 = new ReportParameter("CompanyLogo", new Uri(Path.Combine(_hostingEnvironment.WebRootPath, @$"Client\Company\{reportData.Master?.COMPANY_LOGO}")).AbsoluteUri);
                                ReportParameter parameter14 = new ReportParameter("SupName", reportData.Master?.SUP_NAME);
                                ReportParameter parameter15 = new ReportParameter("ClientName", reportData.Master?.CLIENT_NAME);
                                //ReportParameter parameter14 = new ReportParameter("Date", reportData.Master?.DATE);
                                //ReportParameter parameter17 = new ReportParameter("InvoiceNumber", reportData.Master?.INVOICE_NUMBER);


                                report.SetParameters(new ReportParameter[] { parameter, parameter1, parameter2, parameter3, parameter4, parameter5, parameter6, parameter7, parameter8, parameter9, parameter10, parameter11, parameter12, parameter13, parameter14, parameter15 });

                                report.Refresh();

                                report.DataSources.Add(new ReportDataSource() { Name = "MPODetail", Value = reportData.Detail });

                                byte[] file;
                                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, @"Client\InspectionProcess");
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
                                    filePath = $"/Client/InspectionProcess/{filePath}";
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
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Client", "uploads", "InspectionProcess");

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
                    response.data = $"/Client/uploads/InspectionProcess/{uniqueFileName}"; // Return file path
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