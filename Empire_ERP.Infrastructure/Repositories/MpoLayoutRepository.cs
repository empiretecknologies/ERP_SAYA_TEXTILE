using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class MpoLayoutRepository : IMpoLayoutRepository
    {
        public IBranchRepository _branchRepository { get; set; }
        public IMenuService _menuRepository { get; set; }



        public MpoLayoutRepository(IBranchRepository branchRepository, IMenuService menuRepository)
        {
            _branchRepository = branchRepository;
            _menuRepository = menuRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                string? detailTable = menu.TABLE2;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = $@"SELECT A.TRAN_ID, A.V_DATE, A.VOUCHER_NO,
                                        CASE WHEN A.ASTATUS = 'Y' THEN 'Active' WHEN A.ASTATUS = 'N' THEN 'In-Active' ELSE '' END ASTATUS,
                                        A.REMARKS, MPO.CLIENT_PO, MPO.JOB_NO
                                        FROM {table} A 
                                        LEFT OUTER JOIN TBL_MPO_MASTER MPO ON MPO.TRAN_ID = A.JOB_NO
                                        WHERE A.DLT = 'T' AND A.ASTATUS = 'Y' AND A.BCODE = '{common.Branch}' AND A.PERIOD_ID = '{common.Period}' ORDER BY A.TRAN_ID DESC";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = reader["TRAN_ID"] == DBNull.Value ? null : Convert.ToString(reader["TRAN_ID"]),
                            ASTATUS = reader["ASTATUS"] == DBNull.Value ? null : Convert.ToString(reader["ASTATUS"]),
                            V_DATE = GetDate(reader["V_DATE"], "dd"),
                            //CARD_FILE_DATE = GetDate(reader["CARD_FILE_DATE"], "dd"),
                            //LAY_SUBM_DATE = GetDate(reader["LAY_SUBM_DATE"], "dd"),
                            //LAY_SUBD_DATE = GetDate(reader["LAY_SUBD_DATE"], "dd"),
                            //LAY_APPR_DATE = GetDate(reader["LAY_APPR_DATE"], "dd"),
                            //STOFF_SUBM_DATE = GetDate(reader["STOFF_SUBM_DATE"], "dd"),
                            //STOFF_SUBD_DATE = GetDate(reader["STOFF_SUBD_DATE"], "dd"),
                            //STOFF_APPR_DATE = GetDate(reader["STOFF_APPR_DATE"], "dd"),
                            //REV_STATUS = reader["REV_STATUS"] == DBNull.Value ? null : Convert.ToString(reader["REV_STATUS"]),
                            VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? null : Convert.ToString(reader["VOUCHER_NO"]),
                            REMARKS = reader["REMARKS"] == DBNull.Value ? null : Convert.ToString(reader["REMARKS"]),
                            CLIENT_PO = Convert.ToString(reader["CLIENT_PO"]),
                            JOB_NO = Convert.ToString(reader["JOB_NO"]),
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

        string GetDate(object dbValue, string format)
        {
            if (dbValue == DBNull.Value)
                return "";

            DateTime dt = Convert.ToDateTime(dbValue);

            // Ignore SQL default dummy date
            if (dt == new DateTime(1900, 1, 1))
                return "";
            if (format == "dd")
            {
                return dt.ToString("dd-MM-yyyy");
            }
            else
            {
                return dt.ToString("MM-dd-yyyy");
            }
        }


        private int GenerateNextId(Common common, SqlCommand command, Menu menu)
        {
            try
            {
                string? table = menu.TABLE1;
                string maxIdQuery = $"SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM {table} WHERE BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                command.CommandText = maxIdQuery;
                object result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        private string GenerateVoucherNo(Common common, int code, string vDate, Menu menu)
        {
            try
            {
                string? prefix = menu.PERFIX, shortName = string.Empty;
                int voucherLength = Convert.ToInt32(menu.VOUCHER_LEN);
                var branchData = _branchRepository.GetBranchByCode(common.Branch);
                if (branchData.data != null)
                {
                    var branch = (Branch)branchData.data;
                    shortName = branch.B_SHORT_NAME;
                }

                if (!String.IsNullOrWhiteSpace(shortName) && !String.IsNullOrWhiteSpace(prefix) && voucherLength > 0 && code > 0)
                {
                    string paddedVoucherValue = code.ToString().PadLeft(voucherLength, '0');
                    return $"{shortName}/{prefix}/{Convert.ToDateTime(vDate).ToString("yy-MM")}/{paddedVoucherValue}";
                }
            }
            catch (Exception ex)
            {

            }
            return string.Empty;
        }

        private int GenerateNextDetailId(SqlCommand command, Menu menu)
        {
            try
            {
                string? table = menu.TABLE2;
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

        public MyHttpResponseMessage Save(CustomMpoLayout modelRecord, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1, detailTable = menu.TABLE2;
                var ip = common.IPAddress;
                var computerName = common.ComputerName;
                var postalCode = common.PostalCode;
                var username = common.Username;
                var branch = common.Branch;
                var periodID = common.Period;
                var menuID = common.MenuID;
                string connectionString = new SQLService().getconnstring();
                List<CustomPartyType> partiesData = DropdownService.CustomPartyTypeDropdownWithAccountCode(common.RoleID, common.RoleType);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    try
                    {
                        string query = "", detailQuery = "", voucherNo = string.Empty;
                        bool IsMasterAdded = true, IsNew = false;
                        int code = 0;

                        if (modelRecord.Master.TRAN_ID == null || modelRecord.Master.TRAN_ID == 0)
                        {
                            IsNew = true;
                            code = GenerateNextId(common, command, menu);

                            if (code > 0)
                            {
                                modelRecord.Master.TRAN_ID = code;
                                voucherNo = GenerateVoucherNo(common, code, CommonService.GetDateTime("Pakistan Standard Time"), menu);
                                if (String.IsNullOrWhiteSpace(voucherNo))
                                {
                                    IsMasterAdded = false;
                                }
                            }
                            else
                            {
                                IsMasterAdded = false;
                            }

                            query = $"INSERT INTO {table} " +
                                    "(TRAN_ID, V_DATE, VOUCHER_NO, REMARKS, BCODE, " +
                                    "PERIOD_ID, JOB_NO, ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME, " +
                                    "ADD_IP_ADDRESS, EDIT_USER_ID, EDIT_DATE, EDIT_COMPUTER_NAME, " +
                                    "EDIT_IP_ADDRESS, ADD_POSTALCODE, EDIT_POSTALCODE, ASTATUS, " +
                                    "MENU_ID, DLT) " +
                                    $"VALUES " +
                                    $"('{code}', '{modelRecord.Master.V_DATE}', '{voucherNo}'," +
                                    $"'{modelRecord.Master.REMARKS}' , '{branch}', '{periodID}', '{modelRecord.Master.JOB_NO}'," +
                                    $"'{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}'," +
                                    $"'{ip}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}'," +
                                    $"'{computerName}', '{ip}', " +
                                    $"'{postalCode}', '{postalCode}', " +
                                    $"'{modelRecord.Master.ASTATUS}', '{menuID}', 'T')";

                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            query = $"UPDATE {table} " +
                                    $"SET V_DATE = '{modelRecord.Master.V_DATE}', " +
                                    $"REMARKS = '{modelRecord.Master.REMARKS}', " +
                                    $"EDIT_USER_ID = '{modelRecord.Master.EDIT_USER_ID}', " +
                                    $"EDIT_DATE = '{modelRecord.Master.EDIT_DATE}', " +
                                    $"EDIT_COMPUTER_NAME = '{modelRecord.Master.EDIT_COMPUTER_NAME}', " +
                                    $"EDIT_IP_ADDRESS = '{modelRecord.Master.EDIT_IP_ADDRESS}', " +
                                    $"EDIT_POSTALCODE = '{modelRecord.Master.EDIT_POSTALCODE}', " +
                                    $"ASTATUS = '{modelRecord.Master.ASTATUS}' " +
                                    $"WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND BCODE = '{branch}' AND PERIOD_ID = '{periodID}'";

                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }

                        var isDetailAdded = true;

                        foreach (var item in modelRecord.Detail.ToList())
                        {
                            try
                            {

                                if (item.DT_CODE == null || item.DT_CODE == 0)
                                {
                                    int detailCode = GenerateNextDetailId(command, menu);
                                    if (detailCode > 0)
                                    {
                                        detailQuery = $@"INSERT INTO {detailTable}
                                                       (TRAN_ID, DT_CODE, CARD_FILE_DATE, CAD_FILE_DOC, LAY_SUBM_DATE, LAY_SUBD_DATE, LAY_SUBD_DOC, LAY_APPR_DATE, STOFF_SUBM_DATE, STOFF_SUBD_DATE, STOFF_SUBD_DOC, STOFF_APPR_DATE, AWB,
                                                       REV_STATUS,COMMENT,REMARKS,REV_REF,BCODE, PERIOD_ID, ADD_USER_ID, ADD_DATE,ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID,
                                                       EDIT_DATE, EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS, ADD_POSTALCODE, EDIT_POSTALCODE, MENU_ID, DLT)
                                                       VALUES
                                                       ('{modelRecord.Master.TRAN_ID}', '{detailCode}', '{item.CARD_FILE_DATE}', '{item.CAD_FILE_DOC}', '{item.LAY_SUBM_DATE}', '{item.LAY_SUBD_DATE}', '{item.LAY_SUBD_DOC}',
                                                       '{item.LAY_APPR_DATE}','{item.STOFF_SUBM_DATE}','{item.STOFF_SUBD_DATE}', '{item.STOFF_SUBD_DOC}', '{item.STOFF_APPR_DATE}', '{item.AWB}',
                                                       '{item.REV_STATUS}','{item.COMMENT}','{item.REMARKS}', {detailCode}, '{branch}', '{periodID}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}',
                                                       '{ip}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}', '{ip}', '{postalCode}', '{postalCode}', '{menuID}', 'T')";

                                        command.CommandText = detailQuery;
                                        command.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        isDetailAdded = false;
                                    }
                                }
                                else if ((item.DT_CODE != null || item.DT_CODE != 0) && item.REV_TOGGLE == true)
                                {
                                    int detailCode = GenerateNextDetailId(command, menu);
                                    if (detailCode > 0)
                                    {
                                        detailQuery = $@"INSERT INTO {detailTable}
                                                       (TRAN_ID, DT_CODE, CARD_FILE_DATE, CAD_FILE_DOC, LAY_SUBM_DATE, LAY_SUBD_DATE, LAY_SUBD_DOC, LAY_APPR_DATE, STOFF_SUBM_DATE, STOFF_SUBD_DATE, STOFF_SUBD_DOC, STOFF_APPR_DATE, AWB,
                                                       REV_STATUS,COMMENT,REMARKS, REV_REF,BCODE, PERIOD_ID, ADD_USER_ID, ADD_DATE,ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID,
                                                       EDIT_DATE, EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS, ADD_POSTALCODE, EDIT_POSTALCODE, MENU_ID, DLT)
                                                       VALUES
                                                       ('{modelRecord.Master.TRAN_ID}', '{detailCode}', '{item.CARD_FILE_DATE}', '{item.CAD_FILE_DOC}', '{item.LAY_SUBM_DATE}', '{item.LAY_SUBD_DATE}', '{item.LAY_SUBD_DOC}',
                                                       '{item.LAY_APPR_DATE}','{item.STOFF_SUBM_DATE}','{item.STOFF_SUBD_DATE}', '{item.STOFF_SUBD_DOC}', '{item.STOFF_APPR_DATE}', '{item.AWB}',
                                                       '{item.REV_STATUS}','{item.COMMENT}','{item.REMARKS}', {item.REV_REF}, '{branch}', '{periodID}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}',
                                                       '{ip}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}', '{ip}', '{postalCode}', '{postalCode}', '{menuID}', 'T')";

                                        command.CommandText = detailQuery;
                                        command.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        isDetailAdded = false;
                                    }
                                }
                                else if ((item.DT_CODE != null || item.DT_CODE != 0) && item.REV_TOGGLE != true)
                                {
                                    string revStatusCheckQuery = $@"SELECT REV_STATUS FROM {detailTable} WHERE DT_CODE = '{item.DT_CODE}'";
                                    command.CommandText = revStatusCheckQuery;
                                    object revStatus = command.ExecuteScalar();
                                    string revStatusStr = revStatus?.ToString().Trim();


                                    //if (revStatusStr != "N")
                                    if (revStatusStr != item.REV_STATUS)
                                    {
                                        response.msgType = 2;
                                        response.msg = $"You Cannot change Revised entry to New entry.";
                                        return response;
                                    }

                                    detailQuery = $"UPDATE {detailTable} " +
                                                  $"SET CARD_FILE_DATE = '{item.CARD_FILE_DATE}', " +
                                                  $"CAD_FILE_DOC = '{item.CAD_FILE_DOC}', " +
                                                  $"LAY_SUBM_DATE = '{item.LAY_SUBM_DATE}', " +
                                                  $"LAY_SUBD_DATE = '{item.LAY_SUBD_DATE}', " +
                                                  $"LAY_SUBD_DOC = '{item.LAY_SUBD_DOC}', " +
                                                  $"LAY_APPR_DATE = '{item.LAY_APPR_DATE}', " +
                                                  $"STOFF_SUBM_DATE = '{item.STOFF_SUBM_DATE}', " +
                                                  $"STOFF_SUBD_DATE = '{item.STOFF_SUBD_DATE}', " +
                                                  $"STOFF_SUBD_DOC = '{item.STOFF_SUBD_DOC}', " +
                                                  $"STOFF_APPR_DATE = '{item.STOFF_APPR_DATE}', " +
                                                  $"REV_STATUS = '{item.REV_STATUS}', " +
                                                  $"AWB = '{item.AWB}', " +
                                                  $"COMMENT = '{item.COMMENT}', " +
                                                  $"REMARKS = '{item.REMARKS}', " +
                                                  $"EDIT_USER_ID = '{username}', " +
                                                  $"EDIT_DATE = '{CommonService.GetDateTime("Pakistan Standard Time")}', " +
                                                  $"EDIT_COMPUTER_NAME = '{computerName}', " +
                                                  $"EDIT_IP_ADDRESS = '{ip}', " +
                                                  $"EDIT_POSTALCODE = '{postalCode}', " +
                                                  $"DLT = 'T' " +
                                                  $"WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND DT_CODE = '{item.DT_CODE}' AND BCODE = '{branch}' AND PERIOD_ID = '{periodID}'";

                                    command.CommandText = detailQuery;
                                    command.ExecuteNonQuery();
                                }
                            }
                            catch (Exception)
                            {
                                isDetailAdded = false;
                            }
                        }

                        if (IsMasterAdded && isDetailAdded)
                        {
                            transaction.Commit();
                            response.data = new
                            {
                                code = IsNew ? code : modelRecord.Master.TRAN_ID,
                                voucherNo = IsNew ? voucherNo : modelRecord.Master.VOUCHER_NO,
                            };
                            response.msgType = 1;
                            response.msg = IsNew ? "Record Added Successfully" : "Record Updated Successfully";
                        }
                        else
                        {
                            transaction.Rollback();
                            response.data = "";
                            response.msg = "Something went wrong! please try again later.";
                            response.msgType = 2;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        string _catchMessage = ex.Message;
                        if (ex.InnerException != null)
                        {
                            _catchMessage += "<br/>" + ex.InnerException.Message;
                        }
                        response.msg = _catchMessage;
                        response.msgType = 2;
                    }
                }
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                response.msgType = 2;
                response.msg = _catchMessage;
            }
            return response;
        }

        public MyHttpResponseMessage GetMpoLayoutByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {

                    string query = $@"SELECT M.TRAN_ID, M.V_DATE, M.JOB_NO , M.VOUCHER_NO, M.REMARKS, M.ASTATUS 
                                    FROM {table} M
                                    WHERE M.DLT = 'T' AND M.TRAN_ID = '{code}' AND M.BCODE = '{common.Branch}' AND M.PERIOD_ID = '{common.Period}'";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = Convert.ToString(reader["TRAN_ID"]),
                            V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                            JOB_NO = Convert.ToString(reader["JOB_NO"]),
                            VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
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

        public MyHttpResponseMessage GetMpoLayoutDetailByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE2;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {

                    string query = $@"WITH GroupStatus AS
                                        (
                                            SELECT 
                                                REV_REF,
                                                HasC = MAX(CASE WHEN REV_STATUS = 'C' THEN 1 ELSE 0 END),
                                                HasR = MAX(CASE WHEN REV_STATUS = 'R' THEN 1 ELSE 0 END)
                                            FROM {table}
                                            GROUP BY REV_REF
                                        ),
                                        Final AS
                                        (
                                            SELECT t.*
                                            FROM {table} t
                                            JOIN GroupStatus g ON t.REV_REF = g.REV_REF
                                            WHERE g.HasC = 0
                                                AND ((g.HasR = 1 AND t.REV_STATUS = 'R' AND t.DT_CODE = (
                                                            SELECT MAX(DT_CODE) 
                                                            FROM {table} 
                                                            WHERE REV_REF = t.REV_REF AND REV_STATUS = 'R')) OR (g.HasR = 0 AND t.REV_STATUS = 'N')))
                                        SELECT * FROM Final WHERE DLT = 'T' AND TRAN_ID = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'
                                        ORDER BY REV_REF, DT_CODE;";


                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            //LAY_SUBD_DATE = Convert.ToString(reader["LAY_SUBD_DATE"]),
                            DT_CODE = Convert.ToString(reader["DT_CODE"]),
                            CARD_FILE_DATE = GetDate(reader["CARD_FILE_DATE"], "MM"),
                            LAY_SUBM_DATE = GetDate(reader["LAY_SUBM_DATE"], "MM"),
                            LAY_SUBD_DATE = GetDate(reader["LAY_SUBD_DATE"], "MM"),
                            LAY_APPR_DATE = GetDate(reader["LAY_APPR_DATE"], "MM"),
                            STOFF_SUBM_DATE = GetDate(reader["STOFF_SUBM_DATE"], "MM"),
                            STOFF_SUBD_DATE = GetDate(reader["STOFF_SUBD_DATE"], "MM"),
                            STOFF_APPR_DATE = GetDate(reader["STOFF_APPR_DATE"], "MM"),
                            LAY_SUBD_DOC = Convert.ToString(reader["LAY_SUBD_DOC"]),
                            STOFF_SUBD_DOC = Convert.ToString(reader["STOFF_SUBD_DOC"]),
                            CAD_FILE_DOC = Convert.ToString(reader["CAD_FILE_DOC"]),
                            AWB = Convert.ToString(reader["AWB"]),
                            COMMENT = Convert.ToString(reader["COMMENT"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            REV_STATUS = Convert.ToString(reader["REV_STATUS"]),
                            REV_REF = reader["REV_REF"] != DBNull.Value ? Convert.ToInt32(reader["REV_REF"]) : 0,
                            REV_COLOR = "Blue",
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

        public MyHttpResponseMessage GetBatchDetailByProcess(string process, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty, detailTable = string.Empty, pickMasterTable = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                    pickMasterTable = menu.PICK_TABLE_MASTER;
                    //pickDetailTable = menu.PICK_TABLE_DETAIL;
                }

                if (!String.IsNullOrWhiteSpace(table) && !String.IsNullOrWhiteSpace(detailTable) && !String.IsNullOrWhiteSpace(pickMasterTable))
                {
                    List<object> jsonDataResult = new List<object>();
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = $@"SELECT MPO.TRAN_ID,MPO.V_DATE,MPO.VOUCHER_NO, MPO.ITEM_CODE, MPO.GRADE, MPO.PARTY_CODE, MPO.ACT_CODE, MPO.REF, MPO.JOB_NO, MPO.EMP_ID, MPO.DEP_ID,
                                        MPO.SPARTY_CODE, MPO.SACT_CODE, MPO.QTY, MPO.UNIT, MPO.RATE, MPO.AMT, MPO.REMARKS ,CASE WHEN MPO.ASTATUS = 'Y' THEN 'Active' WHEN MPO.ASTATUS = 'N' THEN 'In-Active' ELSE '' END AASTATUS,
                                        PTC.PARTY_NAME AS CLIENT_NAME, PTS.PARTY_NAME AS SUPPLIER_NAME,IT.ITEM_NAME,EMP.ENAME,PT.GROUP_NAME AS TERMS_NAME, U.GROUP_NAME AS UNIT_NAME, C.DESCR AS CURRENCY_NAME, 
                                        D.DESCR AS DEP_NAME, MPO.DOC, MPO.COMM_AMT AS COMM_TYPE, MPO.COMM AS COMM_RATE, MPO.COMM_VAL AS COMM_AMT
                                        FROM TBL_MPO_MASTER MPO
                                        LEFT OUTER JOIN TBL_PARTY_TYPES PTC ON MPO.PARTY_CODE = PTC.PARTY_CODE AND PTC.ACT_CODE = MPO.ACT_CODE
                                        LEFT OUTER JOIN TBL_PARTY_TYPES PTS ON MPO.SPARTY_CODE = PTS.PARTY_CODE AND PTS.ACT_CODE = MPO.SACT_CODE
                                        LEFT OUTER JOIN TBL_ITEMSMASTER IT ON MPO.ITEM_CODE = IT.ITEM_CODE
                                        LEFT OUTER JOIN TBL_EMP_REG EMP ON MPO.EMP_ID = EMP.EMP_CODE
                                        LEFT OUTER JOIN TBL_PAY_TERMS PT ON MPO.TERMS = PT.GROUP_CODE
                                        LEFT OUTER JOIN TBL_UNIT U ON MPO.UNIT = U.GROUP_CODE
                                        LEFT OUTER JOIN TBL_CURRENCY C ON MPO.CURR_CODE = C.CODE
                                        LEFT OUTER JOIN TBL_ACT_GROUP D ON MPO.DEP_ID = D.CODE
                                        WHERE MPO.TRAN_ID = '{process}' AND MPO.DLT = 'T' AND MPO.BCODE = {common.Branch} AND MPO.PERIOD_ID = {common.Period} ORDER BY MPO.TRAN_ID DESC";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {

                            var row = new
                            {
                                TRAN_ID = reader["TRAN_ID"].ToString(),
                                V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                VOUCHER_NO = reader["VOUCHER_NO"].ToString(),
                                ITEM_CODE = reader["ITEM_CODE"].ToString(),
                                ITEM_NAME = reader["ITEM_NAME"].ToString(),
                                GRADE_CODE = reader["GRADE"].ToString(),
                                //PARTY_CODE = reader["PARTY_CODE"].ToString(), //yaha
                                PARTY_CODE = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                                COMM_TYPE = Convert.ToString(reader["COMM_TYPE"]),
                                COMM_RATE = reader["COMM_RATE"] != DBNull.Value ? Convert.ToDecimal(reader["COMM_RATE"]) : 0,
                                COMM_AMT = reader["COMM_AMT"] != DBNull.Value ? Convert.ToDecimal(reader["COMM_AMT"]) : 0,
                                ACT_CODE = reader["ACT_CODE"].ToString(),
                                CLIENT_NAME = reader["CLIENT_NAME"].ToString(),

                                REF = reader["REF"].ToString(),
                                JOB_NO = reader["JOB_NO"].ToString(),
                                EMP_ID = reader["EMP_ID"].ToString(),
                                ENAME = reader["ENAME"].ToString(),

                                DEP_ID = reader["DEP_ID"].ToString(),
                                DEP_NAME = reader["DEP_NAME"].ToString(),
                                SPARTY_CODE = $"{Convert.ToString(reader["SPARTY_CODE"])}{Convert.ToString(reader["SACT_CODE"])}",
                                SACT_CODE = reader["SACT_CODE"].ToString(),
                                SUPPLIER_NAME = reader["SUPPLIER_NAME"].ToString(),

                                //QTY = reader["QTY"].ToString(),
                                QTY = reader["QTY"] != DBNull.Value ? Convert.ToDecimal(reader["QTY"]) : 0,
                                UNIT = reader["UNIT"].ToString(),
                                UNIT_NAME = reader["UNIT_NAME"].ToString(),

                                //RATE = reader["RATE"].ToString(),
                                RATE = reader["RATE"] != DBNull.Value ? Convert.ToDecimal(reader["RATE"]) : 0,
                                //AMT = reader["AMT"].ToString(),
                                AMT = reader["AMT"] != DBNull.Value ? Convert.ToDecimal(reader["AMT"]) : 0,
                                //COMM = reader["COMM"].ToString(),
                                //COMM_VAL = reader["COMM_VAL"].ToString(),
                                //COMM_AMT = reader["COMM_AMT"].ToString(),
                                REMARKS = reader["REMARKS"].ToString(),
                                PICK_ID = Convert.ToInt32(reader["TRAN_ID"]),
                                DOC = Convert.ToString(reader["DOC"]),
                                STATUS = "N",

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

        public MyHttpResponseMessage Delete(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                string? table = menu.TABLE2;
                string connectionString = new SQLService().getconnstring();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"UPDATE {table} SET DLT = 'F' WHERE DT_CODE = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                    response.msgType = 1;
                    response.msg = "Record Deleted Successfully";
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

        public MyHttpResponseMessage CopyRecord(CopyRecord record, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                string? table = menu.TABLE1;
                string? table2 = menu.TABLE2;
                string connectionString = new SQLService().getconnstring();

                MpoLayout mpoLayout = new MpoLayout();
                List<MpoLayoutDetail> MpoLayoutDetailList = new List<MpoLayoutDetail>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT * FROM {table} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    string detailQuery = $@"SELECT * FROM {table2} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        mpoLayout = new MpoLayout
                        {
                            V_DATE = Convert.ToDateTime(reader["V_DATE"]),
                            //REF = Convert.ToString(reader["REF"]),
                            //PROCESS = Convert.ToInt32(reader["PROCESS"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                        };
                    }

                    reader.Close();

                    SqlCommand detail_Command = new SqlCommand(detailQuery, connection);
                    SqlDataReader detail_Reader = detail_Command.ExecuteReader();
                    while (detail_Reader.Read())
                    {
                        var row = new MpoLayoutDetail
                        {
                            //ITEM_CODE = Convert.ToInt32(detail_Reader["ITEM_CODE"]),
                            //QTY = Convert.ToDouble(detail_Reader["QTY"]),
                            //UNIT = Convert.ToInt32(detail_Reader["UNIT"]),
                            //QTY2 = Convert.ToDouble(detail_Reader["QTY2"]),
                            //BAL_QTY = Convert.ToDouble(detail_Reader["BAL_QTY"]),
                            //RATE = Convert.ToInt32(detail_Reader["RATE"]),
                            //AMT = Convert.ToInt32(detail_Reader["AMT"]),
                            ////SHIP_DATE = Convert.ToDateTime(detail_Reader["MFG_DATE"]),
                            //BOOKING_DATE = Convert.ToDateTime(detail_Reader["EXP_DATE"]),
                            //BATCH = Convert.ToString(detail_Reader["BATCH"]),
                            //DT_DESC = Convert.ToString(detail_Reader["DT_DESC"]),
                            //CHK = detail_Reader["CHK"] == DBNull.Value ? 0 : Convert.ToInt32(detail_Reader["CHK"]),
                            //PICK_ID = Convert.ToInt32(detail_Reader["PICK_ID"]),
                        };
                        MpoLayoutDetailList.Add(row);
                    }

                    detail_Reader.Close();
                    connection.Close();
                }

                var customMpoLayout = new CustomMpoLayout
                {
                    Master = mpoLayout,
                    Detail = MpoLayoutDetailList
                };

                response = this.Save(customMpoLayout, common, menu);

                if (response.msgType == 1)
                {
                    response.msg = "Record Copied Successfully";
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

        public MyHttpResponseMessage DeleteMpoLayoutDetailByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE2;
                var branch = common.Branch;
                var period = common.Period;
                string connectionString = new SQLService().getconnstring();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"UPDATE {table} SET DLT = 'F'" +
                                   $" WHERE DT_CODE = '{code}' AND BCODE = '{branch}' AND PERIOD_ID = '{period}'";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                    response.msgType = 1;
                    response.msg = "Record Deleted Successfully";
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

        //  public MyHttpResponseMessage GetDataForReport(MpoLayoutRDLCReport modelRecord, DataTable dataTable, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        //  {
        //      MyHttpResponseMessage response = new MyHttpResponseMessage();
        //      MpoLayoutRDLCReport masterData = new MpoLayoutRDLCReport();
        //      CustomMpoLayoutForPrintReport reportData = new CustomMpoLayoutForPrintReport();
        //      var Menu = _menuRepository.GetMenu(common.MenuID);
        //      string? table = string.Empty, detailTable = string.Empty, pickTable = string.Empty; string query = "";
        //      if (Menu.data != null)
        //      {
        //          var menu = (Menu)Menu.data;
        //          table = menu.TABLE1;
        //          detailTable = menu.TABLE2;
        //          pickTable = menu.PICK_TABLE_MASTER;
        //      }
        //      try
        //      {
        //          if (menuDetails.REPORT_NAME == "MpoLayout")
        //          {
        //              query = $@"SELECT D.TRAN_ID,
        //                      MPO.CLIENT_PO,MPO.JOB_NO,
        //                      M.VOUCHER_NO,M.REMARKS,
        //                      CASE 
        //                          WHEN M.ASTATUS = 'Y' THEN 'ACTIVE'
        //                          ELSE 'INACTIVE'
        //                      END AS ASTATUS,
        //                      P.PARTY_NAME  AS SUP_NAME,
        //                      PC.PARTY_NAME AS CLIENT_NAME,
        //                      MPO.V_DATE,
        //                      MPO.QTY,MB.MENU_SIG1,MENU_SIG2,MENU_SIG3,MENU_SIG4,MB.MENU_TERMS,
        //	B.B_ADDRESS,B.B_NAME,B.B_GST,B.B_NTN,B.B_WEBSITE,B.EMAIL,B.TERMS AS B_TERMS,B.B_TEL,
        //                      D.CARD_FILE_DATE, D.LAY_SUBM_DATE, D.LAY_SUBD_DATE, D.LAY_SUBD_DOC, D.LAY_APPR_DATE,D.STOFF_SUBM_DATE,D.STOFF_SUBD_DATE,D.STOFF_SUBD_DOC,
        //                      D.STOFF_APPR_DATE, D.REV_STATUS, D.REV_REF, D.CAD_FILE_DOC,D.COMMENT,D.REMARKS
        //                  FROM {table} M
        //                  LEFT JOIN {detailTable} D
        //                         ON D.TRAN_ID = M.TRAN_ID
        //                  LEFT JOIN TBL_MPO_MASTER MPO
        //                         ON MPO.TRAN_ID = M.JOB_NO
        //                  LEFT JOIN TBL_PARTY_TYPES P
        //                         ON P.PARTY_CODE = MPO.SPARTY_CODE
        //                        AND P.ACT_CODE   = MPO.SACT_CODE
        //                  LEFT JOIN TBL_PARTY_TYPES PC
        //                         ON PC.PARTY_CODE = MPO.PARTY_CODE
        //                        AND PC.ACT_CODE   = MPO.ACT_CODE
        //LEFT JOIN TBL_MENU_BUILDER MB
        //	   ON MB.ID=M.MENU_ID
        //LEFT JOIN TBL_BRANCH B
        //	   ON B.BCODE =M.BCODE
        //                  WHERE M.BCODE     = '{common.Branch}'
        //                    AND M.PERIOD_ID = '{common.Period}'
        //                    AND M.TRAN_ID   = '{modelRecord.TRAN_ID}'
        //                    AND M.DLT       = 'T'
        //                    AND D.DLT       = 'T';";
        //          }

        //          if (menuDetails.REPORT_NAME == "MpoLayout")
        //          {
        //              using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
        //              {
        //                  SqlCommand command = new SqlCommand(query, connection);
        //                  connection.Open();
        //                  SqlDataReader reader = command.ExecuteReader();

        //                  masterData.HEADER_NAME = $"{menuDetails.MD_NAME}";
        //                  masterData.REPORT_NAME = $"{menuDetails.REPORT_NAME}";
        //                  masterData.COMPANY_LOGO = currentCompany.C_LOGO;
        //                  masterData.COMPANY_NAME = currentCompany.C_NAME;


        //                  if (reader.Read())
        //                  {
        //                      //masterData.COMPANY_NAME = reader["C_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["C_NAME"]);
        //                      masterData.B_NAME = reader["B_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["B_NAME"]);
        //                      masterData.B_TERMS = reader["B_TERMS"] == DBNull.Value ? "" : Convert.ToString(reader["B_TERMS"]);
        //                      masterData.COMPANY_ADDRESS = reader["B_ADDRESS"] == DBNull.Value ? "" : Convert.ToString(reader["B_ADDRESS"]);
        //                      masterData.COMPANY_PHONE = reader["B_TEL"] == DBNull.Value ? "" : Convert.ToString(reader["B_TEL"]);
        //                      masterData.B_WEBSITE = reader["B_WEBSITE"] == DBNull.Value ? "" : Convert.ToString(reader["B_WEBSITE"]);
        //                      masterData.EMAIL = reader["EMAIL"] == DBNull.Value ? "" : Convert.ToString(reader["EMAIL"]);
        //                      masterData.B_GST = reader["B_GST"] == DBNull.Value ? "" : Convert.ToString(reader["B_GST"]);
        //                      masterData.B_NTN = reader["B_NTN"] == DBNull.Value ? "" : Convert.ToString(reader["B_NTN"]);
        //                      masterData.SIG1 = reader["MENU_SIG1"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_SIG1"]);
        //                      masterData.SIG2 = reader["MENU_SIG2"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_SIG2"]); ;
        //                      masterData.SIG3 = reader["MENU_SIG3"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_SIG3"]); ;
        //                      masterData.SIG4 = reader["MENU_SIG4"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_SIG4"]); ;
        //                      masterData.MENU_TERMS = reader["MENU_TERMS"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_TERMS"]); ;
        //                      masterData.DATE = reader["V_DATE"] == DBNull.Value ? string.Empty : (Convert.ToDateTime(reader["V_DATE"]) == new DateTime(1900, 1, 1) ? string.Empty : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy"));
        //                      masterData.INVOICE_NUMBER = reader["VOUCHER_NO"] == DBNull.Value ? string.Empty : reader["VOUCHER_NO"].ToString();
        //                      masterData.STATUS = reader["ASTATUS"] == DBNull.Value ? string.Empty : reader["ASTATUS"].ToString();
        //                      masterData.SUP_NAME = reader["SUP_NAME"] == DBNull.Value ? string.Empty : reader["SUP_NAME"].ToString();
        //                      masterData.CLIENT_NAME = reader["CLIENT_NAME"] == DBNull.Value ? string.Empty : reader["CLIENT_NAME"].ToString();
        //                      masterData.CLIENT_PO = reader["CLIENT_PO"] == DBNull.Value ? string.Empty : reader["CLIENT_PO"].ToString();
        //                      masterData.JOB_NO = reader["JOB_NO"] == DBNull.Value ? string.Empty : reader["JOB_NO"].ToString();
        //                      masterData.DESIGN = reader["REMARKS"] == DBNull.Value ? string.Empty : reader["REMARKS"].ToString();

        //                  }
        //                  reader.Close();
        //              }

        //              using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
        //              {
        //                  SqlCommand command = new SqlCommand(query, connection);
        //                  connection.Open();
        //                  SqlDataReader reader = command.ExecuteReader();
        //                  while (reader.Read())

        //                  {
        //                      DataRow dataRow = dataTable.NewRow();
        //                      dataRow["TRAN_ID"] = reader["TRAN_ID"] == DBNull.Value ? string.Empty : reader["TRAN_ID"].ToString();
        //                      dataRow["LAY_SUBM_DATE"] = reader["LAY_SUBM_DATE"] == DBNull.Value ? string.Empty : reader["LAY_SUBM_DATE"].ToString();
        //                      dataRow["LAY_SUBD_DATE"] = reader["LAY_SUBD_DATE"] == DBNull.Value ? string.Empty : reader["LAY_SUBD_DATE"].ToString();
        //                      dataRow["LAY_SUBD_DOC"] = reader["LAY_SUBD_DOC"] == DBNull.Value ? string.Empty : reader["LAY_SUBD_DOC"].ToString();
        //                      dataRow["STOFF_SUBD_DOC"] = reader["STOFF_SUBD_DOC"] == DBNull.Value ? string.Empty : reader["STOFF_SUBD_DOC"].ToString();
        //                      dataRow["CAD_FILE_DOC"] = reader["CAD_FILE_DOC"] == DBNull.Value ? string.Empty : reader["CAD_FILE_DOC"].ToString();
        //                      dataRow["LAY_APPR_DATE"] = reader["LAY_APPR_DATE"] == DBNull.Value ? string.Empty : reader["LAY_APPR_DATE"].ToString();
        //                      dataRow["STOFF_SUBM_DATE"] = reader["STOFF_SUBM_DATE"] == DBNull.Value ? string.Empty : reader["STOFF_SUBM_DATE"].ToString();
        //                      dataRow["STOFF_SUBD_DATE"] = reader["STOFF_SUBD_DATE"] == DBNull.Value ? string.Empty : reader["STOFF_SUBD_DATE"].ToString();
        //                      dataRow["STOFF_APPR_DATE"] = reader["STOFF_APPR_DATE"] == DBNull.Value ? string.Empty : reader["STOFF_APPR_DATE"].ToString();
        //                      //dataRow["LAY_SUBD_DATE"] = reader["LAY_SUBD_DATE"] == DBNull.Value ? string.Empty : reader["LAY_SUBD_DATE"].ToString();
        //                      dataRow["REV_STATUS"] = reader["REV_STATUS"] == DBNull.Value ? string.Empty : reader["REV_STATUS"].ToString();
        //                      dataRow["COMMENT"] = reader["COMMENT"] == DBNull.Value ? string.Empty : reader["COMMENT"].ToString();
        //                      dataRow["REMARKS"] = reader["REMARKS"] == DBNull.Value ? string.Empty : reader["REMARKS"].ToString();
        //                      dataTable.Rows.Add(dataRow);
        //                  }
        //                  reader.Close();
        //              }
        //          }

        //          reportData.Master = masterData;
        //          reportData.Detail = dataTable;
        //          response.data = reportData;
        //          response.msg = "";
        //          response.msgType = 1;

        //      }

        //      catch (Exception ex)
        //      {
        //          string _catchMessage = ex.Message;
        //          if (ex.InnerException != null)
        //          {
        //              _catchMessage += "<br/>" + ex.InnerException.Message;
        //          }
        //          response.msg = _catchMessage;
        //          response.msgType = 2;
        //      }
        //      return response;
        //  }

        public MyHttpResponseMessage GetDataForReport(MpoLayoutRDLCReport modelRecord, DataTable dataTable, DataTable SubReportDetails, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            MpoLayoutRDLCReport masterData = new MpoLayoutRDLCReport();
            CustomMpoLayoutForPrintReport reportData = new CustomMpoLayoutForPrintReport();

            try
            {
                if (menuDetails.REPORT_NAME == "MpoLayout")
                {
                    string query = $"EXEC PROC_PRINT '','','','','{common.Branch}','{common.Period}',{modelRecord.TRAN_ID},'ADMANI','','MpoLayout'";
                    string subReportQuery = $"EXEC PROC_PRINT '','','','','{common.Branch}','{common.Period}',{modelRecord.TRAN_ID},'ADMANI','','MpoLayout_SUB'";

                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        connection.Open();

                        // --- Part 1: Load Master Data ---
                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                masterData.HEADER_NAME = menuDetails.MD_NAME ?? "";
                                masterData.REPORT_NAME = menuDetails.REPORT_NAME ?? "";
                                masterData.COMPANY_LOGO = currentCompany.C_LOGO;
                                masterData.COMPANY_NAME = currentCompany.C_NAME;

                                if (reader.Read())
                                {
                                    masterData.B_NAME = reader["B_NAME"]?.ToString() ?? "";
                                    masterData.B_TERMS = reader["B_TERMS"]?.ToString() ?? "";
                                    masterData.COMPANY_ADDRESS = reader["B_ADDRESS"]?.ToString() ?? "";
                                    masterData.COMPANY_PHONE = reader["B_TEL"]?.ToString() ?? "";
                                    masterData.B_WEBSITE = reader["B_WEBSITE"]?.ToString() ?? "";
                                    masterData.EMAIL = reader["EMAIL"]?.ToString() ?? "";
                                    masterData.B_GST = reader["B_GST"]?.ToString() ?? "";
                                    masterData.B_NTN = reader["B_NTN"]?.ToString() ?? "";
                                    masterData.SIG1 = reader["MENU_SIG1"]?.ToString() ?? "";
                                    masterData.SIG2 = reader["MENU_SIG2"]?.ToString() ?? "";
                                    masterData.SIG3 = reader["MENU_SIG3"]?.ToString() ?? "";
                                    masterData.SIG4 = reader["MENU_SIG4"]?.ToString() ?? "";
                                    masterData.MENU_TERMS = reader["MENU_TERMS"]?.ToString() ?? "";

                                    var vDate = reader["V_DATE"];
                                    masterData.DATE = (vDate == DBNull.Value || Convert.ToDateTime(vDate) == new DateTime(1900, 1, 1))
                                                      ? "" : Convert.ToDateTime(vDate).ToString("dd-MM-yyyy");

                                    masterData.INVOICE_NUMBER = reader["VOUCHER_NO"]?.ToString() ?? "";
                                    masterData.STATUS = reader["ASTATUS"]?.ToString() ?? "";
                                    masterData.SUP_NAME = reader["SUP_NAME"]?.ToString() ?? "";
                                    masterData.CLIENT_NAME = reader["CLIENT_NAME"]?.ToString() ?? "";
                                    masterData.CLIENT_PO = reader["CLIENT_PO"]?.ToString() ?? "";
                                    masterData.JOB_NO = reader["JOB_NO"]?.ToString() ?? "";
                                    masterData.DESIGN = reader["DESIGN_NO"]?.ToString() ?? "";
                                }
                            } // Reader automatically closes here
                        }

                        // --- Part 2: Load Main Detail (DataTable) ---
                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    DataRow row = dataTable.NewRow();
                                    row["CARD_FILE_DATE"] = reader["CARD_FILE_DATE"]?.ToString() ?? "";
                                    row["LAY_SUBM_DATE"] = reader["LAY_SUBM_DATE"]?.ToString() ?? "";
                                    row["LAY_SUBD_DATE"] = reader["LAY_SUBD_DATE"]?.ToString() ?? "";
                                    row["LAY_SUBD_DOC"] = reader["LAY_SUBD_DOC"]?.ToString() ?? "";
                                    row["STOFF_SUBD_DOC"] = reader["STOFF_SUBD_DOC"]?.ToString() ?? "";
                                    row["CAD_FILE_DOC"] = reader["CAD_FILE_DOC"]?.ToString() ?? "";
                                    row["LAY_APPR_DATE"] = reader["LAY_APPR_DATE"]?.ToString() ?? "";
                                    row["STOFF_SUBM_DATE"] = reader["STOFF_SUBM_DATE"]?.ToString() ?? "";
                                    row["STOFF_SUBD_DATE"] = reader["STOFF_SUBD_DATE"]?.ToString() ?? "";
                                    row["STOFF_APPR_DATE"] = reader["STOFF_APPR_DATE"]?.ToString() ?? "";
                                    row["REV_STATUS"] = reader["REV_STATUS"]?.ToString() ?? "";
                                    row["COMMENT"] = reader["COMMENT"]?.ToString() ?? "";
                                    row["REMARKS"] = reader["REMARKS"]?.ToString() ?? "";
                                    dataTable.Rows.Add(row);
                                }
                            }
                        }

                        // --- Part 3: Load Subreport Detail (SubReportDetails) ---
                        using (SqlCommand cmd = new SqlCommand(subReportQuery, connection))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    DataRow row = SubReportDetails.NewRow();
                                    row["dtCode"] = reader["DT_CODE"]?.ToString() ?? "";
                                    row["LayDoc"] = reader["LAY_SUBD_DOC"]?.ToString() ?? "";
                                    row["bCode"] = reader["BCODE"]?.ToString() ?? "";
                                    row["period"] = reader["PERIOD_ID"]?.ToString() ?? "";
                                    row["tranId"] = reader["TRAN_ID"]?.ToString() ?? "";
                                    SubReportDetails.Rows.Add(row);
                                }
                            }
                        }
                    }
                }

                reportData.Master = masterData;
                reportData.Detail = dataTable;
                reportData.SubDetail = SubReportDetails;

                response.data = reportData;
                response.msgType = 1;
            }
            catch (Exception ex)
            {
                response.msg = ex.Message + (ex.InnerException != null ? "<br/>" + ex.InnerException.Message : "");
                response.msgType = 2;
            }
            return response;
        }


    }
}