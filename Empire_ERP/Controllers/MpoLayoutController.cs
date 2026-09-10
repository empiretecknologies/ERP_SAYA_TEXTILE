using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Ghostscript.NET.Rasterizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.NETCore;
using PdfiumViewer;
using SkiaSharp;
using System.Data;
using System.Drawing.Imaging;
using System.Text;
using static Azure.Core.HttpHeader;
using Image = System.Drawing.Image;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class MpoLayoutController : BaseController
    {
        public IMpoLayoutService _mpoLayoutService { get; set; }
        private readonly IWebHostEnvironment _hostingEnvironment;
        private DataTable _subReportData;
        public MpoLayoutController(IMpoLayoutService mpoLayoutService, IMenuService menuService, IWebHostEnvironment hostingEnvironment,IBaseService baseService) : base(menuService,baseService)
        {
            _mpoLayoutService = mpoLayoutService;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            var common = CommonHelper.GetValues(HttpContext);

            ViewBag.Items = DropdownService.RawItemsDropdown();
            ViewBag.FinishItems = DropdownService.FinishItemsDropdown();
            ViewBag.Units = DropdownService.UnitDropdown();
            ViewBag.Colors = DropdownService.ColorDropdown();
            ViewBag.Sizes = DropdownService.SizeDropdown();
            ViewBag.Ports = DropdownService.PortDropdown();
            ViewBag.SEntity = DropdownService.SEntityDropdown();
            ViewBag.DistributionChannel = DropdownService.DistributionChannelDropdown();
            ViewBag.Grade = DropdownService.GradeDropdown();
            ViewBag.Season = DropdownService.GetSeasonData();
            ViewBag.Items = DropdownService.ItemMasterDropdown(CommonHelper.GetValues(HttpContext).RoleID, CommonHelper.GetValues(HttpContext).RoleType);
            ViewBag.ClientPO = DropdownService.ClientPODropdown();
            //ViewBag.Supplier = GetParties();
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
        public JsonResult QuickSearch()
        {
            try
            {
                var data = _mpoLayoutService.QuickSearch(CommonHelper.GetValues(HttpContext));
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
        public JsonResult Save(CustomMpoLayout modelRecord)
        {
            try
            {
                var data = _mpoLayoutService.Save(modelRecord, CommonHelper.GetValues(HttpContext));
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
        public JsonResult GetMpoLayoutByCode(int code)
        {
            try
            {
                var data = _mpoLayoutService.GetMpoLayoutByCode(code, CommonHelper.GetValues(HttpContext));
                var detailData = _mpoLayoutService.GetMpoLayoutDetailByCode(code, CommonHelper.GetValues(HttpContext));
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
        public JsonResult GetMpoLayoutDetailByCode(int code)
        {
            try
            {
                var data = _mpoLayoutService.GetMpoLayoutDetailByCode(code, CommonHelper.GetValues(HttpContext));
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
                var data = _mpoLayoutService.GetBatchDetailByProcess(process, CommonHelper.GetValues(HttpContext));
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
                var data = _mpoLayoutService.Delete(code, CommonHelper.GetValues(HttpContext));
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
                var data = _mpoLayoutService.CopyRecord(record, CommonHelper.GetValues(HttpContext));
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
        public JsonResult DeleteMpoLayoutDetailByCode(int code)
        {
            try
            {
                var data = _mpoLayoutService.DeleteMpoLayoutDetailByCode(code, CommonHelper.GetValues(HttpContext));
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
        public JsonResult GetPrintReport(MpoLayoutRDLCReport model)
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
        private byte[] ConvertPdfToImageBytes(string pdfPath, int pageNumber = 0, int dpi = 300)
        {
            using var document = PdfDocument.Load(pdfPath);
            using var image = document.Render(
                pageNumber, 
                dpi,        
                dpi,        
                PdfRenderFlags.Annotations
            );
            using var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
        //private string GenerateReport(MpoLayoutRDLCReport model)
        //{
        //    var filePath = "";
        //    try
        //    {
        //        if (model != null && model.TRAN_ID > 0 && model.MD_ID > 0)
        //        {
        //            Reports.Datasets.BarcodeReportDataset.MPOLayoutDataTable reportDetails = new Reports.Datasets.BarcodeReportDataset.MPOLayoutDataTable();
        //            var responseMessage = _mpoLayoutService.GetDataForReport(model, reportDetails, CommonHelper.GetValues(HttpContext));
        //            if (responseMessage.msgType != 1)
        //            {
        //                return "";
        //            }
        //            var reportData = (CustomMpoLayoutForPrintReport)responseMessage.data;

        //            string tempFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Client/TempImages");
        //            Directory.CreateDirectory(tempFolder);
        //            var detailData = reportData.Detail;
        //            foreach (DataRow row in detailData.Rows)
        //            {
        //                string[] docColumns = { "LAY_SUBD_DOC", "STOFF_SUBD_DOC", "CAD_FILE_DOC" };

        //                foreach (var col in docColumns)
        //                {
        //                    string docPath = row[col] == DBNull.Value ? string.Empty : row[col].ToString();
        //                    if (string.IsNullOrWhiteSpace(docPath)) continue;

        //                    docPath = docPath.TrimStart('~', '/', '\\');
        //                    string fullPath = Path.IsPathRooted(docPath) ? docPath : Path.Combine(_hostingEnvironment.WebRootPath, docPath);

        //                    if (!System.IO.File.Exists(fullPath)) continue;
        //                    string savePath = Path.Combine(tempFolder, Guid.NewGuid() + ".png");

        //                    if (Path.GetExtension(fullPath).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        using var doc = PdfiumViewer.PdfDocument.Load(fullPath);
        //                        using var img = doc.Render(0, 300, 300, PdfiumViewer.PdfRenderFlags.Annotations);
        //                        img.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
        //                    }
        //                    else
        //                    {
        //                        using var img = Image.FromFile(fullPath);
        //                        img.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
        //                    }
        //                    string uriPath = new Uri(savePath).AbsoluteUri;
        //                    row[col] = uriPath;
        //                }
        //            }



        //            using (LocalReport report = new LocalReport())
        //            {
        //                var path = Path.Combine(_hostingEnvironment.ContentRootPath, @$"Reports\{reportData.Master?.REPORT_NAME}.rdlc");
        //                using (var stReader = new StreamReader(path))
        //                {
        //                    string stringreader = stReader.ReadToEnd();
        //                    byte[] byteArray = Encoding.UTF8.GetBytes(stringreader);
        //                    using (var stream = new MemoryStream(byteArray))
        //                    {
        //                        report.EnableExternalImages = true;
        //                        report.LoadReportDefinition(stream);
        //                        report.DataSources.Clear();

        //                        var companyLogoPath = Path.Combine(_hostingEnvironment.WebRootPath, @$"Client\Company\{reportData.Master?.COMPANY_LOGO}");
        //                        bool? showCompanyLogo = true;
        //                        if (!System.IO.File.Exists(companyLogoPath))
        //                        {
        //                            showCompanyLogo = false;
        //                        }

        //                        ReportParameter parameter1 = new ReportParameter("Header", reportData.Master?.HEADER_NAME);
        //                        ReportParameter parameter2 = new ReportParameter("CompanyName", reportData.Master?.COMPANY_NAME);
        //                        ReportParameter parameter3 = new ReportParameter("BranchName", reportData.Master?.B_NAME);
        //                        ReportParameter parameter4 = new ReportParameter("BranchTerms", reportData.Master?.B_TERMS);
        //                        ReportParameter parameter5 = new ReportParameter("CompanyAddress", reportData.Master?.COMPANY_ADDRESS);
        //                        ReportParameter parameter6 = new ReportParameter("CompanyPhone", reportData.Master?.COMPANY_PHONE);
        //                        ReportParameter parameter7 = new ReportParameter("BranchWebsite", reportData.Master?.B_WEBSITE);
        //                        ReportParameter parameter8 = new ReportParameter("BranchEmail", reportData.Master?.EMAIL);
        //                        ReportParameter parameter9 = new ReportParameter("BranchGST", reportData.Master?.B_GST);
        //                        ReportParameter parameter10 = new ReportParameter("BranchNTN", reportData.Master?.B_NTN);
        //                        ReportParameter parameter11 = new ReportParameter("Sig1", reportData.Master?.SIG1);
        //                        ReportParameter parameter12 = new ReportParameter("Sig2", reportData.Master?.SIG2);
        //                        ReportParameter parameter13 = new ReportParameter("Sig3", reportData.Master?.SIG3);
        //                        ReportParameter parameter14 = new ReportParameter("Sig4", reportData.Master?.SIG4);
        //                        ReportParameter parameter15 = new ReportParameter("MenuTerms", reportData.Master?.MENU_TERMS);
        //                        ReportParameter parameter16 = new ReportParameter("ShowCompanyLogo", Convert.ToString(showCompanyLogo));
        //                        ReportParameter parameter17 = new ReportParameter("CompanyLogo", new Uri(Path.Combine(_hostingEnvironment.WebRootPath, @$"Client\Company\{reportData.Master?.COMPANY_LOGO}")).AbsoluteUri);
        //                        ReportParameter parameter18 = new ReportParameter("Date", reportData.Master?.DATE);
        //                        ReportParameter parameter19 = new ReportParameter("InvoiceNumber", reportData.Master?.INVOICE_NUMBER);
        //                        ReportParameter parameter20 = new ReportParameter("Status", reportData.Master?.STATUS);
        //                        ReportParameter parameter21 = new ReportParameter("SupName", reportData.Master?.SUP_NAME);
        //                        ReportParameter parameter22 = new ReportParameter("ClientName", reportData.Master?.CLIENT_NAME);
        //                        ReportParameter parameter23 = new ReportParameter("ClientPo", reportData.Master?.CLIENT_PO);
        //                        ReportParameter parameter24 = new ReportParameter("JobNo", reportData.Master?.JOB_NO);
        //                        ReportParameter parameter25 = new ReportParameter("DesignNo", reportData.Master?.DESIGN);
        //                        ReportParameter parameter26 = new ReportParameter("RootPath",new Uri(_hostingEnvironment.WebRootPath).AbsoluteUri);

        //                        report.SetParameters(new ReportParameter[] {  parameter1, parameter2, parameter3, parameter4, parameter5, parameter6, parameter7, parameter8, parameter9, 
        //                            parameter10, parameter11, parameter12, parameter13, parameter14, parameter15,parameter16,parameter17,parameter18,parameter19,parameter20,parameter21,parameter22,parameter23,parameter24, parameter25,parameter26});
        //                        report.Refresh();
        //                        report.DataSources.Add(new ReportDataSource() { Name = "MPOLayout", Value = reportData.Detail });
        //                        byte[] file;
        //                        string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, @"Client\MpoLayout");
        //                        if (!Directory.Exists(uploadsFolder))
        //                        {
        //                            Directory.CreateDirectory(uploadsFolder);
        //                        }

        //                        if (report.IsReadyForRendering)
        //                        {
        //                            string input = reportData.Master?.CLIENT_PO;
        //                            string[] parts = input.Split('/');
        //                            string prefix = string.Empty;
        //                            string voucherNumber = string.Empty;
        //                            if (parts.Length >= 3)
        //                            {
        //                                prefix = parts[1];
        //                                voucherNumber = parts[^1];
        //                            }
        //                            file = report.Render("PDF");
        //                            filePath = $"{prefix} - {voucherNumber}" + ".pdf";

        //                            stReader.Close();
        //                            stReader.Dispose();
        //                            stream.Flush();
        //                            stream.Close();
        //                            stream.Dispose();
        //                            report.Dispose();
        //                            string reportPath = Path.Combine(uploadsFolder, filePath);
        //                            System.IO.File.WriteAllBytes(reportPath, file);
        //                            filePath = $"/Client/MpoLayout/{filePath}";
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //    }
        //    return filePath;
        //}




        private string GenerateReport(MpoLayoutRDLCReport model)
        {
            var filePath = "";
            try
            {
                if (model != null && model.TRAN_ID > 0 && model.MD_ID > 0)
                {
                    Reports.Datasets.BarcodeReportDataset.MPOLayoutDataTable reportDetails = new Reports.Datasets.BarcodeReportDataset.MPOLayoutDataTable();
                    Reports.Datasets.BarcodeReportDataset.SubReportDataTable subReportDetails = new Reports.Datasets.BarcodeReportDataset.SubReportDataTable();

                    var responseMessage = _mpoLayoutService.GetDataForReport(model, reportDetails, subReportDetails, CommonHelper.GetValues(HttpContext));

                    if (responseMessage.msgType != 1) return "";

                    var reportData = (CustomMpoLayoutForPrintReport)responseMessage.data;
                    _subReportData = reportData.SubDetail;
                    this.ProcessDocumentPaths(reportData.Detail, reportData.SubDetail);

                    using (LocalReport report = new LocalReport())
                    {
                        var path = Path.Combine(_hostingEnvironment.ContentRootPath, @$"Reports\{reportData.Master?.REPORT_NAME}.rdlc");
                        var subReportPath = Path.Combine(_hostingEnvironment.ContentRootPath, @"Reports\MpoLayout_SUB.rdlc");

                        using (var stReader = new StreamReader(path))
                        {
                            string stringreader = stReader.ReadToEnd();
                            byte[] byteArray = Encoding.UTF8.GetBytes(stringreader);

                            using (var stream = new MemoryStream(byteArray))
                            {
                                report.EnableExternalImages = true;
                                report.LoadReportDefinition(stream);
                                using (var subReader = new StreamReader(subReportPath))
                                {
                                    report.LoadSubreportDefinition("MpoLayout_SUB", subReader);
                                }
                                report.SubreportProcessing += this.Report_SubreportProcessing;
                                report.DataSources.Clear();

                                var m = reportData.Master;
                                var companyLogoPath = Path.Combine(_hostingEnvironment.WebRootPath, @$"Client\Company\{m?.COMPANY_LOGO}");
                                bool showCompanyLogo = System.IO.File.Exists(companyLogoPath);

                                var parameters = new List<ReportParameter>();
                                parameters.Add(new ReportParameter("Header", m?.HEADER_NAME));
                                parameters.Add(new ReportParameter("CompanyName", m?.COMPANY_NAME));
                                parameters.Add(new ReportParameter("BranchName", m?.B_NAME));
                                parameters.Add(new ReportParameter("BranchTerms", m?.B_TERMS));
                                parameters.Add(new ReportParameter("CompanyAddress", m?.COMPANY_ADDRESS));
                                parameters.Add(new ReportParameter("CompanyPhone", m?.COMPANY_PHONE));
                                parameters.Add(new ReportParameter("BranchWebsite", m?.B_WEBSITE));
                                parameters.Add(new ReportParameter("BranchEmail", m?.EMAIL));
                                parameters.Add(new ReportParameter("BranchGST", m?.B_GST));
                                parameters.Add(new ReportParameter("BranchNTN", m?.B_NTN));
                                parameters.Add(new ReportParameter("Sig1", m?.SIG1));
                                parameters.Add(new ReportParameter("Sig2", m?.SIG2));
                                parameters.Add(new ReportParameter("Sig3", m?.SIG3));
                                parameters.Add(new ReportParameter("Sig4", m?.SIG4));
                                parameters.Add(new ReportParameter("MenuTerms", m?.MENU_TERMS));
                                parameters.Add(new ReportParameter("ShowCompanyLogo", showCompanyLogo.ToString()));
                                parameters.Add(new ReportParameter("CompanyLogo", new Uri(companyLogoPath).AbsoluteUri));
                                parameters.Add(new ReportParameter("Date", m?.DATE));
                                parameters.Add(new ReportParameter("InvoiceNumber", m?.INVOICE_NUMBER));
                                parameters.Add(new ReportParameter("Status", m?.STATUS));
                                parameters.Add(new ReportParameter("SupName", m?.SUP_NAME));
                                parameters.Add(new ReportParameter("ClientName", m?.CLIENT_NAME));
                                parameters.Add(new ReportParameter("ClientPo", m?.CLIENT_PO));
                                parameters.Add(new ReportParameter("JobNo", m?.JOB_NO));
                                parameters.Add(new ReportParameter("DesignNo", m?.DESIGN));
                                parameters.Add(new ReportParameter("RootPath", new Uri(_hostingEnvironment.WebRootPath).AbsoluteUri));

                                report.SetParameters(parameters);
                                report.DataSources.Add(new ReportDataSource("MPOLayout", reportData.Detail));

                                report.Refresh();

                                if (report.IsReadyForRendering)
                                {
                                    byte[] file = report.Render("PDF");

                                    string input = m?.CLIENT_PO ?? "Report";
                                    string[] parts = input.Split('/');
                                    string fileNamePart = parts.Length >= 3 ? $"{parts[1]} - {parts[^1]}" : "MPO";
                                    filePath = $"{fileNamePart}.pdf";

                                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, @"Client\MpoLayout");
                                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                                    string reportSavePath = Path.Combine(uploadsFolder, filePath);
                                    System.IO.File.WriteAllBytes(reportSavePath, file);

                                    filePath = $"/Client/MpoLayout/{filePath}";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return filePath;
        }

        private void Report_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            if (e.ReportPath == "MpoLayout_SUB")
            {
                if (this._subReportData != null)
                {
                    e.DataSources.Add(new ReportDataSource("SubReportDataSet", this._subReportData));
                }
            }
        }

        public void ProcessDocumentPaths(DataTable detailData, DataTable subDetailData)
        {
            string tempFolder = Path.Combine(this._hostingEnvironment.WebRootPath, "Client/TempImages");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            string[] docColumns = new string[] { "LAY_SUBD_DOC", "STOFF_SUBD_DOC", "CAD_FILE_DOC" };

            if (detailData != null)
            {
                foreach (DataRow row in detailData.Rows)
                {
                    foreach (string col in docColumns)
                    {
                        if (!detailData.Columns.Contains(col)) continue;

                        string docPath = (row[col] == DBNull.Value) ? string.Empty : row[col].ToString();
                        if (string.IsNullOrWhiteSpace(docPath)) continue;

                        docPath = docPath.TrimStart('~', '/', '\\');
                        string fullPath = Path.IsPathRooted(docPath) ? docPath : Path.Combine(this._hostingEnvironment.WebRootPath, docPath);

                        if (System.IO.File.Exists(fullPath))
                        {
                            if (Path.GetExtension(fullPath).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                            {
                                string savePath = Path.Combine(tempFolder, Guid.NewGuid().ToString() + ".png");
                                using (PdfiumViewer.PdfDocument doc = PdfiumViewer.PdfDocument.Load(fullPath))
                                {
                                    using (Image img = doc.Render(0, 300f, 300f, PdfiumViewer.PdfRenderFlags.Annotations))
                                    {
                                        img.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                                        row[col] = new Uri(savePath).AbsoluteUri;
                                    }
                                }
                            }
                            else
                            {
                                row[col] = new Uri(fullPath).AbsoluteUri;
                            }
                        }
                    }
                }
            }

            string[] subDocColumns = new string[] { "LayDoc" };

            if (subDetailData != null)
            {
                foreach (DataRow row2 in subDetailData.Rows)
                {
                    foreach (string col2 in subDocColumns)
                    {
                        if (!subDetailData.Columns.Contains(col2)) continue;

                        string docPath2 = (row2[col2] == DBNull.Value) ? string.Empty : row2[col2].ToString();
                        if (string.IsNullOrWhiteSpace(docPath2)) continue;

                        docPath2 = docPath2.TrimStart('~', '/', '\\');
                        string fullPath2 = Path.IsPathRooted(docPath2) ? docPath2 : Path.Combine(this._hostingEnvironment.WebRootPath, docPath2);

                        if (System.IO.File.Exists(fullPath2))
                        {
                            if (Path.GetExtension(fullPath2).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                            {
                                string savePath2 = Path.Combine(tempFolder, Guid.NewGuid().ToString() + ".png");
                                using (PdfiumViewer.PdfDocument doc2 = PdfiumViewer.PdfDocument.Load(fullPath2))
                                {
                                    using (Image img2 = doc2.Render(0, 300f, 300f, PdfiumViewer.PdfRenderFlags.Annotations))
                                    {
                                        img2.Save(savePath2, System.Drawing.Imaging.ImageFormat.Png);
                                        row2[col2] = new Uri(savePath2).AbsoluteUri;
                                    }
                                }
                            }
                            else
                            {
                                row2[col2] = new Uri(fullPath2).AbsoluteUri;
                            }
                        }
                    }
                }
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
    }
}