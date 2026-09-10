using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class SalesSamplingReportsRepository : ISalesSamplingReportsRepository
    {
        public MyHttpResponseMessage GetReportTypes(int menuID, string roleType, int? roleId)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = "TBL_REPORT_TYPES";
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "";
                    if (roleType == "A")
                    {
                        query = $"SELECT * FROM {table} " +
                                   "WHERE 1 = 1 " +
                                   $"AND M_ID = '{menuID}' " +
                                   "AND DLT = 'T' AND ASTATUS = 'Y' " +
                                   "ORDER BY SNO";
                    }
                    else
                    {
                        query = $"SELECT * FROM {table} " +
                                   "WHERE 1 = 1 " +
                                   $"AND M_ID = '{menuID}' " +
                                   $"AND DLT = 'T' AND ASTATUS = 'Y' AND R_ID IN (SELECT RMENU_ID from TBL_ROLE WHERE ROLE_ID = {roleId} AND MODULE_ID = 2) " +
                                   "ORDER BY SNO";
                    }
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            SNO = Convert.ToInt32(reader["SNO"]),
                            REPORT_NAME = Convert.ToString(reader["REPORT_NAME"]),
                            R_ID = Convert.ToInt32(reader["R_ID"]),
                        };
                        jsonDataResult.Add(row);
                    }
                    reader.Close();
                }

                response.data = jsonDataResult;
                response.msg = "";
                response.msgType = 1;
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

        public MyHttpResponseMessage GetReportData(SalesSamplingReports report, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<CustomSalesSamplingReports> jsonDataResult = new List<CustomSalesSamplingReports>();

                if (report.ReportID == 140)
                {
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {

                        string query = $@"EXEC SALES_SAMPLING '{report.ReportID}','{report.FromDate.Value.ToString("yyyy-MM-dd")}','{report.ToDate.Value.ToString("yyyy-MM-dd")}','{common.Branch}','{common.Period}','{report.PARTY_CODE}','{report.ACT_CODE}','{report.ITEM_CODE}','{report.SPARTY_CODE}','{report.SACT_CODE}'";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        int counter = 1;

                        while (reader.Read())
                        {
                            var row = new CustomSalesSamplingReports();
                            if (report.ReportID == 140)
                            {
                              

                                row = new CustomSalesSamplingReports
                                {
                                    ITEM_NAME = reader["ITEM_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM_NAME"]),
                                    DOC = reader["ITEM_IMAGE"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM_IMAGE"]),
                                    CLIENT_PO = reader["CLIENT_PO"] == DBNull.Value ? "" : Convert.ToString(reader["CLIENT_PO"]),
                                    REF_ON_DATE = reader["REF_ON_DATE"] == DBNull.Value || Convert.ToDateTime(reader["REF_ON_DATE"]) == new DateTime(1900, 1, 1) ? "" : Convert.ToDateTime(reader["REF_ON_DATE"]).ToString("dd/MM/yyyy"),
                                    QTY = reader["QTY"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["QTY"]),
                                    ITEM_DETAIL = reader["ITEM_DETAIL"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM_DETAIL"]),
                                    SIZE_NAME = reader["SIZE_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["SIZE_NAME"]),
                                    RATE = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["RATE"]),

                                };
                            }
                            
                            jsonDataResult.Add(row);
                        }
                        
                        reader.Close();
                    }
                    
                }
                
                response.data = jsonDataResult;
                response.msg = "";
                response.msgType = 1;
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