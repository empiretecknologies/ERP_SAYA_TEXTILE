using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZXing.QrCode.Internal;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class QCJobAssignmentRepository : IQCJobAssignmentRepository
    {
        public IMenuRepository _menuRepository { get; set; }
        public IBranchRepository _branchRepository { get; set; }

        public QCJobAssignmentRepository(IMenuRepository menuRepository, IBranchRepository branchRepository)
        {
            _menuRepository = menuRepository;
            _branchRepository = branchRepository;
        }

        public MyHttpResponseMessage GetQCJobAssignments(int Branch, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                var menu = (Menu)Menu.data;

                List<object> jsonDataResult = new List<object>();
                var UnApprovedData = new List<object>();
                var ApprovedData = new List<object>();

                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    connection.Open();

                    string UnApprovedQuery = $@"EXEC INSPECTION_PROC 'UnApproved','{common.Branch}','{common.Period}'";
                    SqlCommand cmd1 = new SqlCommand(UnApprovedQuery, connection);
                    SqlDataReader reader = cmd1.ExecuteReader();

                    while (reader.Read())
                    {
                        UnApprovedData.Add(new
                        {
                            VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? "" : Convert.ToString(reader["VOUCHER_NO"]),
                            CLIENT_PO = reader["CLIENT_PO"] == DBNull.Value ? "" : Convert.ToString(reader["CLIENT_PO"]),
                            PARTY_NAME = reader["PARTY_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["PARTY_NAME"]),
                            SUPPLIER = reader["SUPPLIER"] == DBNull.Value ? "" : Convert.ToString(reader["SUPPLIER"]),
                            JOB_NAME = reader["JOB_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["JOB_NAME"]),
                            ORDER_NO = reader["ORDER_NO"] == DBNull.Value ? "" : Convert.ToString(reader["ORDER_NO"]),
                            SIZE = reader["SIZE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE"]),
                            PICK_ID = reader["DT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DT_CODE"]),
                            COLOR = reader["COLOR"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COLOR"]),
                            QTY = reader["QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QTY"]),
                            UNIT = reader["UNIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["UNIT"]),
                            RATE = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RATE"]),
                            AMT = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AMT"]),

                            SHIP_DATE = reader["SHIP_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["SHIP_DATE"]).ToString("yyyy-MM-dd"),
                            HANDOVER_DATE = reader["HANDOVER_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["HANDOVER_DATE"]).ToString("yyyy-MM-dd"),

                            ITEM_CODE = reader["ITEM_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ITEM_CODE"]),
                            PORT = reader["LC_PORT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["LC_PORT"]),
                            DT_DESC = reader["DT_DESC"] == DBNull.Value ? "" : Convert.ToString(reader["DT_DESC"]),
                            INTAKE = reader["INTAKE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["INTAKE"]),
                            DCHANNEL_CODE = reader["DCHANNEL_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DCHANNEL_CODE"]),
                            SENTITY_CODE = reader["SENTITY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SENTITY_CODE"]),
                            GRADE_CODE = reader["GRADE_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["GRADE_CODE"]),
                            SEASON_CODE = reader["SEASON_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SEASON_CODE"]),
                            CARTON = reader["CARTON"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CARTON"]),
                            TOTAL_CARTON = reader["TOTAL_CARTON"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TOTAL_CARTON"])
                        });
                    }
                    reader.Close();

                    // --- 2. DOSRA PROC CALL (Approved) ---
                    string ApprovedQuery = $@"EXEC INSPECTION_PROC 'Approved','{common.Branch}','{common.Period}'";
                    SqlCommand cmd2 = new SqlCommand(ApprovedQuery, connection);
                    reader = cmd2.ExecuteReader();

                    while (reader.Read())
                    {
                        ApprovedData.Add(new
                        {
                            AQL = reader["AQL"] == DBNull.Value ? "" : Convert.ToString(reader["AQL"]),
                            ORDER_NO = reader["ORDER_NO"] == DBNull.Value ? "" : Convert.ToString(reader["ORDER_NO"]),
                            COLOR = reader["COLOR"] == DBNull.Value ? "" : Convert.ToString(reader["COLOR"]),
                            SIZE = reader["SIZE"] == DBNull.Value ? "" : Convert.ToString(reader["SIZE"]),
                            QTY = reader["QTY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["QTY"]),
                            UNIT = reader["UNIT"] == DBNull.Value ? "" : Convert.ToString(reader["UNIT"]),
                            RATE = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RATE"]),
                            AMT = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AMT"]),
                            SHIP_DATE = reader["SHIP_DATE"] == DBNull.Value ? "" : (Convert.ToDateTime(reader["SHIP_DATE"]) == new DateTime(1900, 1, 1) ? "" : Convert.ToDateTime(reader["SHIP_DATE"]).ToString("dd/MM/yyyy")),
                            HANDOVER_DATE = reader["HANDOVER_DATE"] == DBNull.Value ? "" : (Convert.ToDateTime(reader["HANDOVER_DATE"]) == new DateTime(1900, 1, 1) ? "" : Convert.ToDateTime(reader["HANDOVER_DATE"]).ToString("dd/MM/yyyy")),
                            BOOKING_DATE = reader["BOOKING_DATE"] == DBNull.Value ? "" : (Convert.ToDateTime(reader["BOOKING_DATE"]) == new DateTime(1900, 1, 1) ? "" : Convert.ToDateTime(reader["BOOKING_DATE"]).ToString("dd/MM/yyyy")),
                            PORT = reader["PORT"] == DBNull.Value ? "" : Convert.ToString(reader["PORT"]),
                            ITEM = reader["ITEM"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM"]),
                            DT_DESC = reader["DT_DESC"] == DBNull.Value ? "" : Convert.ToString(reader["DT_DESC"]),
                            ENTITY = reader["ENTITY"] == DBNull.Value ? "" : Convert.ToString(reader["ENTITY"]),
                            GRADE = reader["GRADE"] == DBNull.Value ? "" : Convert.ToString(reader["GRADE"]),
                            SEASON = reader["SEASON"] == DBNull.Value ? "" : Convert.ToString(reader["SEASON"]),
                        });
                    }
                    reader.Close();
                }

                jsonDataResult.Add(new
                {
                    UnApprovedData = UnApprovedData,
                    ApprovedData = ApprovedData
                });

                response.data = jsonDataResult;
                response.msg = "";
                response.msgType = 1;

            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null) _catchMessage += "<br/>" + ex.InnerException.Message;
                response.msg = _catchMessage;
                response.msgType = 2;
            }
            return response;
        }


        public MyHttpResponseMessage Save(CustomQCJobAssignment modelRecords, Common common)
        {



            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var MenuResult = _menuRepository.GetMenu(common.MenuID);
                if (MenuResult.data == null)
                {
                    response.msg = "Menu not found";
                    response.msgType = 2;
                    return response;
                }

                var menu = (Menu)MenuResult.data;
                string table = menu.TABLE1;
                string connectionString = new SQLService().getconnstring();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;

                    try
                    {
                        // TRAN_ID loop se bahar ek bar generate hogi
                        var code = GenerateNextId(common);
                        var voucherNo = GenerateVoucherNo(common, Convert.ToInt32(code), CommonService.GetDateTime("Pakistan Standard Time"));

                        foreach (var item in modelRecords.Master)
                        {
                            // Har loop par method call hoga database se fresh DT_CODE lene ke liye
                            int dtCode = GenerateNextDetailId(command, menu);

                            if (dtCode > 0)
                            {
                                string query = @$"INSERT INTO {table} (
                                            [TRAN_ID], [DT_CODE], [INS_SIZES_ID], [BCODE], [PERIOD_ID],
                                            [ADD_USER_ID], [ADD_DATE], [ADD_COMPUTER_NAME], [ADD_IP_ADDRESS], [EDIT_USER_ID],
                                            [EDIT_DATE], [EDIT_COMPUTER_NAME], [EDIT_IP_ADDRESS], [ADD_POSTALCODE],
                                            [EDIT_POSTALCODE], [MENU_ID], [DLT], [PICK_ID]
                                        )
                                        VALUES (
                                            '{code}',
                                            '{dtCode}',
                                            '{item.T_SIZES}',
                                            '{common.Branch}',
                                            '{common.Period}',
                                            '{common.Username}',
                                            '{CommonService.GetDateTime("Pakistan Standard Time")}',
                                            '{common.ComputerName}',
                                            '{common.IPAddress}',
                                            '{common.Username}',
                                            '{CommonService.GetDateTime("Pakistan Standard Time")}',
                                            '{common.ComputerName}',
                                            '{common.IPAddress}',
                                            '{common.PostalCode}',
                                            '{common.PostalCode}',
                                            '{common.MenuID}',
                                            'T',
                                            '{item.PICK_ID}');";

                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        response.msgType = 1;
                        response.msg = "Record Added Successfully";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.msg = ex.Message;
                        response.msgType = 2;
                    }
                }
            }
            catch (Exception ex)
            {
                response.msg = ex.Message;
                response.msgType = 2;
            }
            return response;
        }

        private int GenerateNextDetailId(SqlCommand command, Menu menu)
        {
            try
            {
                string? table = menu.TABLE1;
                string maxIdQuery = $"SELECT ISNULL(MAX(DT_CODE), 0) + 1 FROM {table}";
                command.CommandText = maxIdQuery;
                object result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        private string GenerateVoucherNo(Common common, int code, string vDate)
        {
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? prefix = string.Empty, shortName = string.Empty;
                int voucherLength = 0;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    voucherLength = Convert.ToInt32(menu.VOUCHER_LEN);
                    prefix = menu.PERFIX;
                }

                var branchData = _branchRepository.GetBranchByCode(common.Branch);
                if (branchData.data != null)
                {
                    var branch = (Branch)branchData.data;
                    shortName = branch.B_SHORT_NAME;
                }

                if (!String.IsNullOrWhiteSpace(shortName) && !String.IsNullOrWhiteSpace(prefix) && voucherLength > 0 && code > 0)
                {
                    //string paddedVoucherValue = "0".ToString().PadLeft(voucherLength - 1, '0') + code;
                    string paddedVoucherValue = code.ToString().PadLeft(voucherLength, '0');
                    return $"{shortName}/{prefix}/{Convert.ToDateTime(vDate).ToString("yy-MM")}/{paddedVoucherValue}";
                }
            }
            catch (Exception ex)
            {

            }
            return string.Empty;
        }

        public string GenerateNextId(Common common)
        {
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    string maxIdQuery = "SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM " + table;
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        SqlCommand command = new SqlCommand(maxIdQuery, connection);
                        connection.Open();
                        object result = command.ExecuteScalar();
                        int nextId = Convert.ToInt32(result);
                        return Convert.ToString(nextId);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static dynamic GetPermissionByMenueID(int? roleId, int? menuId)
        {
            object json = null;
            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            {
                string query = $@"SELECT * FROM TBL_ROLE WHERE ROLE_ID = {roleId} AND RMENU_ID = {menuId} AND MODULE_ID = 1";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var jsonDataResult = new
                    {
                        ROLE_ID = reader["ROLE_ID"],
                        ROLE_NAME = reader["ROLE_NAME"],
                        ROLE_TYPE = reader["ROLE_TYPE"],
                        MODULE_ID = reader["MODULE_ID"],
                        DT_CODE = reader["DT_CODE"],
                        R_ADD = Convert.ToBoolean(reader["R_ADD"]),
                        R_EDIT = Convert.ToBoolean(reader["R_EDIT"]),
                        R_DLT = Convert.ToBoolean(reader["R_DLT"]),
                        R_VIEW = Convert.ToBoolean(reader["R_VIEW"]),
                        R_PRINT = Convert.ToBoolean(reader["R_PRINT"]),
                        R_COPY = Convert.ToBoolean(reader["R_COPY"]),
                        R_BCODE = reader["R_BCODE"],
                        RMENU_ID = reader["RMENU_ID"]
                    };
                    json = jsonDataResult;
                }
                reader.Close();
            }
            return json;
        }

        public MyHttpResponseMessage GetDataForReport(int amount, int branch, CustomMenuDetail menuDetails, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            POSTransactionReport masterData = new POSTransactionReport();
            CustomPOSTransactionForPrintReport reportData = new CustomPOSTransactionForPrintReport();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                var menu = (Menu)Menu.data;
                string connectionString = new SQLService().getconnstring();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                }

                var branchData = DropdownService.BranchDataDropdown();
                Branch FromBranch = branchData.FirstOrDefault(b => b.BCODE == Convert.ToInt32(common.Branch));
                Branch ToBranch = branchData.FirstOrDefault(b => b.BCODE == branch);

                var viewModel = new QCJobAssignmentViewModel
                {
                    Amount = amount.ToString("N0"),
                    ToBranch = ToBranch.B_NAME,
                    ToBranchAddress = ToBranch.B_ADDRESS,
                    FromBranch = FromBranch.B_NAME,
                    FromBranchAddress = FromBranch.B_ADDRESS,
                    ToBranchPhone = ToBranch.B_TEL,
                    FromBranchPhone = FromBranch.B_TEL
                };

                response.viewModel = viewModel;
                response.data = menuDetails;
                //response.voucherNo = voucherNo;
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

        public MyHttpResponseMessage GetTJVRecord(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    List<object> jsonDataResult = new List<object>();
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = $"SELECT * FROM {table} WHERE DLT = 'T' AND TRAN_ID = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                DEBIT_AC = reader["DEBIT_AC"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT_AC"]),
                                CREDIT_AC = reader["CREDIT_AC"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CREDIT_AC"]),
                                DDESC = Convert.ToString(reader["DDESC"]),
                            };
                            jsonDataResult.Add(row);
                        }
                        reader.Close();
                    }

                    response.data = jsonDataResult;
                    response.msg = "";
                    response.msgType = 1;
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
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
            return response;
        }
    }
}