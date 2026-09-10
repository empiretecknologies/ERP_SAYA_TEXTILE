using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class PurchaseOrderReportsRepository : IPurchaseOrderReportsRepository
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

        public MyHttpResponseMessage GetReportData(PurchaseOrderReports report, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<CustomPurchaseOrderReports> jsonDataResult = new List<CustomPurchaseOrderReports>();

                if (report.ReportID == 138 || report.ReportID == 139)
                {
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = $@"EXEC PURCHASE_PLANNING '{report.ReportID}','{report.FromDate.Value.ToString("yyyy-MM-dd")}','{report.ToDate.Value.ToString("yyyy-MM-dd")}','{common.Branch}',
                                            '{common.Period}','{report.PARTY_CODE}','{report.ACT_CODE}','{report.SPARTY_CODE}','{report.SACT_CODE}','{report.JOB_NO}','{report.ITEM_CODE}','{report.SEASON}'";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        int counter = 1;

                        while (reader.Read())
                        {
                            var row = new CustomPurchaseOrderReports();
                            if (report.ReportID == 138)
                            {
                              

                                row = new CustomPurchaseOrderReports
                                {
                                    suppName = reader["SUPP_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["SUPP_NAME"]),
                                    clientName = reader["CLIENT_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["CLIENT_NAME"]),
                                    MODEL = reader["MODEL"] == DBNull.Value ? "" : Convert.ToString(reader["MODEL"]),
                                    orderNo = reader["ORDER_NO"] == DBNull.Value ? "" : Convert.ToString(reader["ORDER_NO"]),
                                    COLOR = reader["COLOR"] == DBNull.Value ? "" : Convert.ToString(reader["COLOR"]),
                                    SIZE = reader["SIZE"] == DBNull.Value ? "" : Convert.ToString(reader["SIZE"]),
                                    QTY = reader["QTY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["QTY"]),
                                    RATE = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RATE"]),
                                    AMT = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AMT"]),
                                    lcPort = reader["LC_PORT"] == DBNull.Value ? "" : Convert.ToString(reader["LC_PORT"]),
                                    shipDate = reader["SHIP_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["SHIP_DATE"]).ToString("dd/MM/yyyy"),
                                    bookingDate = reader["BOOKING_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["BOOKING_DATE"]).ToString("dd/MM/yyyy"),
                                    comment = reader["COMMENT"] == DBNull.Value ? "" : Convert.ToString(reader["COMMENT"]),
                                    CurrSign = "$",
                                };
                            }
                            else if (report.ReportID == 139)
                            {
                                row = new CustomPurchaseOrderReports
                                {
                                    SNO = counter, // <-- yahi SNO assign ho raha hai
                                    suppName = reader["SUPP_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["SUPP_NAME"]),
                                    suppCode = reader["SUPP_CODE"] == DBNull.Value ? "" : Convert.ToString(reader["SUPP_CODE"]),
                                    suppActCode = reader["SUPP_ACT_CODE"] == DBNull.Value ? "" : Convert.ToString(reader["SUPP_ACT_CODE"]),
                                    SIZE = reader["SIZE"] == DBNull.Value ? "" : Convert.ToString(reader["SIZE"]),
                                    QTY = reader["QTY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["QTY"]),
                                    //RATE = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RATE"]),
                                    RATE = reader["RATE"] == DBNull.Value ? 0 : Math.Round(Convert.ToDouble(reader["RATE"]), 4),
                                    AMT = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AMT"]),
                                };

                                counter++; 
                            }
                            //else if (report.ReportID == 6) 
                            //{
                            //    row = new CustomPurchaseOrderReports
                            //    {
                            //        AccountCode = reader["ACCOUNT_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACCOUNT_ID"]),
                            //        AccountType = Convert.ToString(reader["ACCOUNT_TYPE"]),
                            //        GRCode = Convert.ToString(reader["ACT_GR_CODE"]),
                            //        ParentName = Convert.ToString(reader["PARENT_NAME"]),
                            //        AccountName = Convert.ToString(reader["ACT_NAME"]),
                            //        Balances = reader["BALANCE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BALANCE"])
                            //    };
                            //}
                            //else if (report.ReportID == 7) 
                            //{
                            //    row = new CustomPurchaseOrderReports
                            //    {
                            //        AccountCode = reader["ACCOUNT_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACCOUNT_ID"]),
                            //        AccountType = Convert.ToString(reader["ACCOUNT_TYPE"]),
                            //        GRCode = Convert.ToString(reader["ACT_GR_CODE"]),
                            //        ParentName = Convert.ToString(reader["PARENT_NAME"]),
                            //        AccountName = Convert.ToString(reader["ACT_NAME"]),
                            //        Balance = reader["BALANCE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BALANCE"])

                            //    };
                            //}
                            //else if (report.ReportID == 121) 
                            //{
                            //    row = new CustomPurchaseOrderReports    
                            //    {     
                            //        ParentName = Convert.ToString(reader["PARENT_NAME"]),
                            //        Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["DEBIT"]),
                            //        AccountName = Convert.ToString(reader["ACT_NAME"]),
                            //        VC_TYPE = reader["VC_TYPE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["VC_TYPE"])
                            //    };
                            //}
                            //else if (report.ReportID == 101)
                            //{
                            //    row = new CustomPurchaseOrderReports
                            //    {
                            //        chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                            //        LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                            //        TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                            //        VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                            //        chq = reader["CHQ"] == DBNull.Value ? "" : Convert.ToString(reader["CHQ"]),
                            //        Amt = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AMT"]),
                            //        bType = Convert.ToString(reader["B_TYPE"]),
                            //        BankName = Convert.ToString(reader["BANK_NAME"]),
                            //        desc = Convert.ToString(reader["DDESC"]),
                            //    };
                            //}
                            //else if (report.ReportID == 103)
                            //{
                            //    row = new CustomPurchaseOrderReports
                            //    {
                            //        AccountCode = Convert.ToInt32(reader["ACT_CODE"]),
                            //        AccountName = Convert.ToString(reader["ACT_NAME"]),
                            //        Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["DEBIT"]),
                            //        Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CREDIT"]),
                            //        ParentName = Convert.ToString(reader["PARENT_NAME"]),
                            //        AccountNature = Convert.ToInt32(reader["ACT_NATURE"]),
                            //        NatureName = Convert.ToString(reader["NATURE_NAME"]),
                            //        //GroupOrder = groupOrderMap.ContainsKey(Convert.ToString(reader["NATURE_NAME"]))
                            //        //? groupOrderMap[Convert.ToString(reader["NATURE_NAME"])]
                            //        //: 99  // agar map me na mile toh last me show kare
                            //    };
                            //}

                            jsonDataResult.Add(row);
                        }
                        //if (report.ReportID == 103)
                        //{
                        //    jsonDataResult.Add(new CustomPurchaseOrderReports
                        //    {
                        //        AccountNature = Convert.ToDecimal(7.2), 
                        //    });
                        //}
                        reader.Close();
                    }
                    //if(report.ReportID != 6 && report.ReportID != 7)
                    //{
                    //    #region Calculate Balance Amount
                    //    var distinctActCodes = jsonDataResult.Select(v => v.AccountCode).Distinct();

                    //    foreach (var accountCode in distinctActCodes)
                    //    {
                    //        var individualAccountVouchers = jsonDataResult.Where(v => v.AccountCode == accountCode).ToList();
                    //        decimal runningBalance = 0;

                    //        foreach (var voucher in individualAccountVouchers)
                    //        {
                    //            runningBalance += Convert.ToDecimal(voucher.Debit) - Convert.ToDecimal(voucher.Credit);
                    //            voucher.Balance = runningBalance;
                    //        }
                    //    }
                    //    #endregion
                    //}
                }
                //else if(report.ReportID == 37)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query;
                //        query = $@"SELECT JV.V_DATE,JV.VOUCHER_NO,
                //                CASE WHEN JV.PARTY_CODE = 0 THEN CH.ACT_NAME ELSE PT.PARTY_NAME END AS ACT_NAME,
                //                JV.DEBIT,JV.CREDIT,JV.DT_DESC
                //                from TBL_JV JV
                //                LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE
                //                LEFT OUTER JOIN TBL_CHART CH
                //                ON CH.ACT_CODE = JV.ACT_CODE
                //                WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T' AND JV.BCODE = 1 AND JV.PERIOD_ID = 1
                //                AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                ORDER BY V_DATE , CREDIT";

                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["V_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["V_DATE"]).ToString("dd/MM/yyyy"),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                Debit = Convert.ToDecimal(reader["DEBIT"]),
                //                Credit = Convert.ToDecimal(reader["CREDIT"])
                //            };
                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 92)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = $"EXEC APROC '{report.ReportID}','{report.FromDate.Value.ToString("yyyy-MM-dd")}','{report.ToDate.Value.ToString("yyyy-MM-dd")}','{report.ControlCode}','{report.AccountCode}','{common.Branch}','{common.Period}'";
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                AccountCode = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                ParentName = Convert.ToString(reader["PARENT_NAME"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["DEBIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 106)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        CASE WHEN JV.PARTY_CODE = 0 THEN CH.ACT_NAME ELSE PT.PARTY_NAME END AS ACT_NAME,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_NO,JV.CHQ_DATE
                //                        from TBL_CBRECEIPT JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART CH
                //                        ON CH.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 107)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE  AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        CASE WHEN JV.PARTY_CODE = 0 THEN CH.ACT_NAME ELSE PT.PARTY_NAME END AS ACT_NAME,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_DATE,JV.CHQ_NO
                //                        from TBL_CBPAYMENT JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART CH
                //                        ON CH.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                         AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 108)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        CASE WHEN JV.PARTY_CODE = 0 THEN CH.ACT_NAME ELSE PT.PARTY_NAME END AS ACT_NAME,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_DATE,JV.CHQ_NO
                //                        from TBL_BBRECEIPT JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART CH
                //                        ON CH.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 109)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        CASE WHEN JV.PARTY_CODE = 0 THEN CH.ACT_NAME ELSE PT.PARTY_NAME END AS ACT_NAME,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_DATE,JV.CHQ_NO
                //                        from TBL_BBPAYMENT JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART CH
                //                        ON CH.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 110)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE ,JV.VOUCHER_NO,
                //                        PT.PARTY_NAME AS ACT_NAME,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_DATE,JV.CHQ_NO
                //                        from TBL_PRECEIPT JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                                        
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 111)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        PT.PARTY_NAME AS ACT_NAME,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_DATE,JV.CHQ_NO
                //                        from TBL_PPAYMENT JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                                        
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 112)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        PT.PARTY_NAME AS ACT_NAME,
                //                        JV.QTY,JV.RATE,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_DATE,JV.CHQ_NO
                //                        from TBL_PB_VOUCHER JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                                        
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                Qty = reader["QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QTY"]),
                //                Rate = reader["RATE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RATE"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 113)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        PT.PARTY_NAME AS ACT_NAME,
                //                        JV.QTY,JV.RATE,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_DATE,JV.CHQ_NO
                //                        from TBL_DN_VOUCHER JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE

                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                                        
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                Qty = reader["QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QTY"]),
                //                Rate = reader["RATE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RATE"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 114)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        PT.PARTY_NAME AS ACT_NAME,
                //                        JV.QTY,JV.RATE,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_DATE,JV.CHQ_NO
                //                        from TBL_SB_VOUCHER JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE

                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                                        
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                Qty = reader["QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QTY"]),
                //                Rate = reader["RATE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RATE"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 115)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        PT.PARTY_NAME AS ACT_NAME,
                //                        JV.QTY,JV.RATE,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,BK.ACT_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_DATE,JV.CHQ_NO
                //                        from TBL_CN_VOUCHER JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE

                //                        LEFT OUTER JOIN TBL_CHART BK
                //                        ON BK.ACT_CODE = JV.BOOK_TYPE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                                        
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                Qty = reader["QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QTY"]),
                //                Rate = reader["RATE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["RATE"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 116)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = @$"select JV.V_DATE AS VOUCHER_DATE,JV.VOUCHER_NO,
                //                        PT.PARTY_NAME AS ACT_NAME,
                //                        CASE WHEN DC_TYPE = 'C' THEN JV.AMT WHEN DC_TYPE = 'D' THEN 0 END AS DEBIT,
                //                        CASE WHEN DC_TYPE = 'D' THEN JV.AMT WHEN DC_TYPE = 'C' THEN 0 END AS CREDIT,
                //                        JV.DT_DESC,PT.PARTY_NAME AS BOOK_tYPE,
                //                        JV.MENU_ID,JV.TRAN_ID,MB.MENU_PARENT_CODE,MB.MENU_PAGE,JV.CHQ_NO,JV.CHQ_DATE
                //                        from TBL_PPPV JV
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
                //                        ON PT.PARTY_CODE = JV.PARTY_CODE AND PT.ACT_CODE = JV.ACT_CODE
                //                        LEFT OUTER JOIN TBL_PARTY_TYPES PTT
                //                        ON PT.PARTY_CODE = JV.TPARTY_CODE AND PT.ACT_CODE = JV.TACT_CODE
                //                        LEFT OUTER JOIN TBL_MENU_BUILDER MB
                //                        ON MB.ID = JV.MENU_ID
                //                        WHERE JV.ASTATUS = 'Y' AND JV.DLT = 'T'
                //                        AND JV.BCODE = {common.Branch} AND JV.PERIOD_ID = {common.Period}
                //                        AND JV.V_DATE BETWEEN '{report.FromDate.Value.ToString("yyyy-MM-dd")}' AND '{report.ToDate.Value.ToString("yyyy-MM-dd")}'
                //                        ORDER BY V_DATE , CREDIT";
                                        
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                VoucherDate = reader["VOUCHER_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["VOUCHER_DATE"]).ToString("dd/MM/yyyy"),
                //                chqDate = reader["CHQ_DATE"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("dd/MM/yyyy"),
                //                chqNo = Convert.ToString(reader["CHQ_NO"]),
                //                LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),
                //                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                //                VoucherNo = Convert.ToString(reader["VOUCHER_NO"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                AccountDescription = Convert.ToString(reader["DT_DESC"]),
                //                bType = Convert.ToString(reader["BOOK_tYPE"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]),
                //                Credit = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                //else if (report.ReportID == 121)
                //{
                //    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                //    {
                //        string query = $"EXEC APROC '{report.ReportID}','{report.FromDate.Value.ToString("yyyy-MM-dd")}','{report.ToDate.Value.ToString("yyyy-MM-dd")}','{report.ControlCode}','{report.AccountCode}','{common.Branch}','{common.Period}'";
                //        SqlCommand command = new SqlCommand(query, connection);
                //        connection.Open();
                //        SqlDataReader reader = command.ExecuteReader();
                //        while (reader.Read())
                //        {
                //            var row = new CustomPurchaseOrderReports();
                //            row = new CustomPurchaseOrderReports
                //            {
                //                ParentName = Convert.ToString(reader["PARENT_NAME"]),
                //                AccountName = Convert.ToString(reader["ACT_NAME"]),
                //                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["DEBIT"]),
                //                VoucherType = reader["VC_TYPE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["VC_TYPE"]),
                //            };

                //            jsonDataResult.Add(row);
                //        }
                //        reader.Close();
                //    }
                //}
                else if (report.ReportID == 121)
                {
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = $"EXEC APROC '{report.ReportID}','{report.FromDate.Value:yyyy-MM-dd}','{report.ToDate.Value:yyyy-MM-dd}','{report.ControlCode}','{report.AccountCode}','{common.Branch}','{common.Period}'";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            var row = new CustomPurchaseOrderReports
                            {
                                ParentName = Convert.ToString(reader["PARENT_NAME"]),
                                AccountName = Convert.ToString(reader["ACT_NAME"]),
                                Debit = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["DEBIT"]),
                                VoucherType = reader["VC_TYPE"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["VC_TYPE"]),
                            };

                            jsonDataResult.Add(row);
                        }
                        reader.Close();
                    }

                    var groupedSums = jsonDataResult
                        .Where(x => x.VoucherType.HasValue)
                        .GroupBy(x => x.VoucherType.Value)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.Debit));

                    decimal sum1 = groupedSums.ContainsKey(1) ? groupedSums[1] ?? 0 : 0;
                    decimal sum2 = groupedSums.ContainsKey(2) ? groupedSums[2] ?? 0 : 0;
                    decimal sum3 = groupedSums.ContainsKey(3) ? groupedSums[3] ?? 0 : 0;
                    decimal sum4 = groupedSums.ContainsKey(4) ? groupedSums[4] ?? 0 : 0;

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