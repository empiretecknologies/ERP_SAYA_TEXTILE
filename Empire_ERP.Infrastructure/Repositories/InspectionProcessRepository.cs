using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Diagnostics;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class InspectionProcessRepository : IInspectionProcessRepository
    {
        public IBranchRepository _branchRepository { get; set; }
        public IMenuService _menuRepository { get; set; }
        public InspectionProcessRepository(IBranchRepository branchRepository, IMenuService menuRepository)
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
                string? pickTableMaster = menu.PICK_TABLE_MASTER;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {

                    //string query = $@"SELECT INS.TRAN_ID, INS.V_DATE, INS.VOUCHER_NO, INS.QA_NAME, INS.INS_DATE, INS.JOB_NO, INS.REF, INS.REMARKS , M.CLIENT_PO, M.JOB_NO, INS.EDIT_USER_ID, INS.EDIT_DATE,
                    //                    CASE WHEN INS.ASTATUS = 'Y' THEN 'Active' ELSE 'In-Active' END AS ASTATUS 
                    //                    FROM {table} INS
                    //                    LEFT OUTER JOIN TBL_MPO_MASTER M ON M.TRAN_ID = INS.JOB_NO
                    //                    WHERE INS.BCODE = {common.Branch} AND INS.PERIOD_ID = {common.Period} AND INS.MENU_NAME = '{menu.MENU_NAME}' AND INS.DLT = 'T' ";

                    string query = $@"
                                        WITH ranked AS (
                                            SELECT INS.TRAN_ID, 
                                                   INS.V_DATE, 
                                                   INS.VOUCHER_NO, 
                                                   INS.QA_NAME, 
                                                   INS.INS_DATE, 
                                                   INS.JOB_NO, 
                                                   INS.REF, 
                                                   INS.REMARKS,
                                                   M.CLIENT_PO, 
                                                   M.JOB_NO AS MPO_JOB_NO, 
                                                   INS.EDIT_USER_ID, 
                                                   INS.EDIT_DATE,
                                                   CASE WHEN INS.ASTATUS = 'Y' THEN 'Active' ELSE 'In-Active' END AS ASTATUS,
                                                   ROW_NUMBER() OVER (PARTITION BY INS.TRAN_ID ORDER BY INS.INS_DATE) AS rn
                                            FROM {table} INS
                                            LEFT OUTER JOIN TBL_MPO_MASTER M ON M.TRAN_ID = INS.JOB_NO
                                            WHERE INS.BCODE = {common.Branch} 
                                              AND INS.PERIOD_ID = {common.Period} 
                                              AND INS.MENU_PRIFIX = '{menu.PERFIX}' 
                                              AND INS.DLT = 'T'
                                        )
                                        SELECT *
                                        FROM ranked
                                        WHERE rn = 1
                                        ORDER BY TRAN_ID DESC
                                        ";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = reader["TRAN_ID"] == DBNull.Value ? null : Convert.ToString(reader["TRAN_ID"]),
                            ASTATUS = reader["ASTATUS"] == DBNull.Value ? null : Convert.ToString(reader["ASTATUS"]),
                            V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                            VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? null : Convert.ToString(reader["VOUCHER_NO"]),
                            QA_NAME = reader["QA_NAME"] == DBNull.Value ? null : Convert.ToString(reader["QA_NAME"]),
                            REF = reader["REF"] == DBNull.Value ? null : Convert.ToString(reader["REF"]),
                            INS_DATE = reader["INS_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["INS_DATE"]).ToString("yyyy-MM-dd"),
                            JOB = reader["JOB_NO"] == DBNull.Value ? null : Convert.ToString(reader["JOB_NO"]),
                            REMARKS = reader["REMARKS"] == DBNull.Value ? null : Convert.ToString(reader["REMARKS"]),
                            EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]),
                            EDIT_DATE = Convert.ToDateTime(reader["EDIT_DATE"]),

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

        public MyHttpResponseMessage Save(CustomInspectionProcess modelRecords, Common common)
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
                        // Condition: Agar TRAN_ID 0 ya null hai to New ID generate hogi (Insert Case)
                        var tranId = 0;
                        string voucherNo = modelRecords.Master.VOUCHER_NO;

                        if (modelRecords.Master.TRAN_ID == 0 || modelRecords.Master.TRAN_ID == null)
                        {
                            tranId = Convert.ToInt32(GenerateNextId(common));
                            voucherNo = GenerateVoucherNo(common, Convert.ToInt32(tranId), CommonService.GetDateTime("Pakistan Standard Time"));
                        }

                        foreach (var item in modelRecords.Detail)
                        {
                            string query = "";

                            if (modelRecords.Master.TRAN_ID == 0 || modelRecords.Master.TRAN_ID == null)
                            {
                                int dtCode = GenerateNextDetailId(command, menu);

                                query = @$"INSERT INTO {table} (
                                    [TRAN_ID], [DT_CODE], [V_DATE], [VOUCHER_NO], [QA_NAME], [INS_DATE], [JOB_NO], [REF], [REMARKS], 
                                    [OFFER_QTY], [CUTTING_QTY], [OFF_LINE], [BLIST_PCTN], [CTN_PCS], [PACK_MODE],
                                    [BCODE], [PERIOD_ID],
                                    [ADD_USER_ID], [ADD_DATE], [ADD_COMPUTER_NAME], [ADD_IP_ADDRESS], [EDIT_USER_ID],
                                    [EDIT_DATE], [EDIT_COMPUTER_NAME], [EDIT_IP_ADDRESS], [ADD_POSTALCODE],
                                    [EDIT_POSTALCODE], [MENU_ID], [DLT], [ASTATUS], [MENU_PRIFIX], [PICK_ID]
                                )
                                VALUES (
                                    '{tranId}', '{dtCode}', '{modelRecords.Master.V_DATE}', '{voucherNo}', '{common.Username}',
                                    '{modelRecords.Master.INS_DATE}', '{modelRecords.Master.JOB_NO}', '{modelRecords.Master.REF}', '{modelRecords.Master.REMARKS}',
                                    '{item.OFFER_QTY}', '{item.CUTTING_QTY}', '{item.OFF_LINE}', '{item.BLIST_PCTN}', '{item.CTN_PCS}', '{item.PACK_MODE}',
                                    '{common.Branch}',
                                    '{common.Period}', '{common.Username}', '{CommonService.GetDateTime("Pakistan Standard Time")}',
                                    '{common.ComputerName}', '{common.IPAddress}', '{common.Username}',
                                    '{CommonService.GetDateTime("Pakistan Standard Time")}', '{common.ComputerName}',
                                    '{common.IPAddress}', '{common.PostalCode}', '{common.PostalCode}', '{common.MenuID}',
                                    'T', '{modelRecords.Master.ASTATUS}', '{menu.PERFIX}', '{item.PICK_ID}');";
                            }
                            else
                            {
                                // UPDATE LOGIC
                                query = @$"UPDATE {table} SET 
                                    [V_DATE] = '{modelRecords.Master.V_DATE}',
                                    [JOB_NO] = '{modelRecords.Master.JOB_NO}',
                                    [REF] = '{modelRecords.Master.REF}',
                                    [REMARKS] = '{modelRecords.Master.REMARKS}',
                                    [OFFER_QTY] = '{item.OFFER_QTY}',
                                    [CUTTING_QTY] = '{item.CUTTING_QTY}',
                                    [OFF_LINE] = '{item.OFF_LINE}',
                                    [BLIST_PCTN] = '{item.BLIST_PCTN}',
                                    [CTN_PCS] = '{item.CTN_PCS}',
                                    [PACK_MODE] = '{item.PACK_MODE}',
                                    [EDIT_USER_ID] = '{common.Username}',
                                    [EDIT_DATE] = '{CommonService.GetDateTime("Pakistan Standard Time")}',
                                    [EDIT_COMPUTER_NAME] = '{common.ComputerName}',
                                    [EDIT_IP_ADDRESS] = '{common.IPAddress}',
                                    [EDIT_POSTALCODE] = '{common.PostalCode}',
                                    [ASTATUS] = '{modelRecords.Master.ASTATUS}'
                                   WHERE [TRAN_ID] = '{modelRecords.Master.TRAN_ID}' AND [DT_CODE] = '{item.DT_CODE}';";
                            }

                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        response.data = tranId;
                        response.msgType = 1;
                        response.msg = (modelRecords.Master.TRAN_ID == 0 || modelRecords.Master.TRAN_ID == null) ? "Record Added Successfully" : "Record Updated Successfully";
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

        public MyHttpResponseMessage SaveInspectionSheet(CustomInspectionSheet modelRecords, Common common)
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
                string table = "TBL_INSPECTION_SHEET";
                string connectionString = new SQLService().getconnstring();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;

                    try
                    {
                        int? tranId = 0;
                        bool isNewTransaction = true;

                        // Pehle iteration se pehle hi check kar lete hain ke naya record hai ya purana
                        // Hum detail ki pehli row se decide kar rahe hain
                        var firstItem = modelRecords.Detail.FirstOrDefault();

                        if (firstItem != null)
                        {

                            if (isNewTransaction == true && firstItem.TRAN_ID == 0 || firstItem.TRAN_ID == null)
                            {
                                tranId = Convert.ToInt32(GenerateNextInsSheetId(common));
                                isNewTransaction = false;
                            }
                            else
                            {
                                tranId = firstItem.TRAN_ID;
                                isNewTransaction = false;
                            }
                        }

                        foreach (var item in modelRecords.Detail)
                        {
                            string query = "";

                            if (item.DT_CODE == 0 || item.DT_CODE == null)
                            {
                                int dtCode = GenerateNextInsSheetDetailId(command, menu);

                                query = @$"INSERT INTO {table} (
                [TRAN_ID], [DT_CODE], [JOB_NO], [PTRAN_ID], [INS_SIZE_ID], [FABRICATION], [F_MIN], [F_MAJ], [STITCHING], [S_MIN], [S_MAJ], [ACCESSORIES], [A_MIN], [A_MAJ],
                [BCODE], [PERIOD_ID],
                [ADD_USER_ID], [ADD_DATE], [ADD_COMPUTER_NAME], [ADD_IP_ADDRESS], [EDIT_USER_ID],
                [EDIT_DATE], [EDIT_COMPUTER_NAME], [EDIT_IP_ADDRESS], [ADD_POSTALCODE],
                [EDIT_POSTALCODE], [MENU_ID], [DLT], [MENU_PRIFIX]
            )
            VALUES (
                '{tranId}', '{dtCode}', '{modelRecords.Master.JOB_NO}', '{modelRecords.Master.PTRAN_ID}', '{item.INS_SIZE_ID}', '{item.FAB}', '{item.F_MIN}', '{item.F_MAJ}', '{item.STI}', '{item.S_MIN}', '{item.S_MAJ}', 
                '{item.ACC}', '{item.A_MIN}', '{item.A_MAJ}',
                '{common.Branch}', '{common.Period}', '{common.Username}', '{CommonService.GetDateTime("Pakistan Standard Time")}',
                '{common.ComputerName}', '{common.IPAddress}', '{common.Username}',
                '{CommonService.GetDateTime("Pakistan Standard Time")}', '{common.ComputerName}',
                '{common.IPAddress}', '{common.PostalCode}', '{common.PostalCode}', '{common.MenuID}',
                'T', '{menu.PERFIX}');";
                            }
                            else
                            {
                                query = @$"UPDATE {table} SET 
                [FABRICATION] = '{item.FAB}',
                [F_MIN] = '{item.F_MIN}',
                [F_MAJ] = '{item.F_MAJ}',
                [STITCHING] = '{item.STI}',
                [S_MIN] = '{item.S_MIN}',
                [S_MAJ] = '{item.S_MAJ}',
                [ACCESSORIES] = '{item.ACC}',
                [A_MIN] = '{item.A_MIN}',
                [A_MAJ] = '{item.A_MAJ}',
                [EDIT_USER_ID] = '{common.Username}',
                [EDIT_DATE] = '{CommonService.GetDateTime("Pakistan Standard Time")}',
                [EDIT_COMPUTER_NAME] = '{common.ComputerName}',
                [EDIT_IP_ADDRESS] = '{common.IPAddress}',
                [EDIT_POSTALCODE] = '{common.PostalCode}'
               WHERE [TRAN_ID] = '{tranId}' AND [DT_CODE] = '{item.DT_CODE}';";
                            }

                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        response.data = tranId;
                        response.msgType = 1;
                        response.msg = isNewTransaction ? "Record Added Successfully" : "Record Updated Successfully";
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


        //public MyHttpResponseMessage SaveInspectionImages(InspectionImageRequest req, Common common)
        //{
        //    MyHttpResponseMessage response = new MyHttpResponseMessage();
        //    string connectionString = new SQLService().getconnstring();
        //    var menuResult = _menuRepository.GetMenu(common.MenuID);
        //    var menu = (Menu)menuResult.data;

        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        SqlTransaction transaction = connection.BeginTransaction();
        //        SqlCommand command = connection.CreateCommand();
        //        command.Transaction = transaction;

        //        try
        //        {
        //            int tranId = GenerateNextImageTranID(command);

        //            int dtCode = GetMaxDtCodeFromDB(command);
        //            foreach (var img in req.Images)
        //            {
        //                if (img.IsNew && !string.IsNullOrEmpty(img.SavedPath))
        //                {
        //                    dtCode++;



        //                    string query = $@"INSERT INTO [TBL_INSPECTION_IMAGES] (
        //                [TRAN_ID], [DT_CODE], [JOB_NO], [PTRAN_ID], [IMG_URL], 
        //                [BCODE], [PERIOD_ID], [ADD_USER_ID], [ADD_DATE], 
        //                [ADD_COMPUTER_NAME], [ADD_IP_ADDRESS], [MENU_ID], [DLT], [MENU_PRIFIX]
        //            ) VALUES (
        //                '{tranId}', '{dtCode}', '{req.JOB_NO}', '{req.PTRAN_ID}', '{img.SavedPath}', 
        //                '{common.Branch}', '{common.Period}', '{common.Username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', 
        //                '{common.ComputerName}', '{common.IPAddress}', '{common.MenuID}', 'T', '{menu.PERFIX}'
        //            );";

        //                    command.CommandText = query;
        //                    command.ExecuteNonQuery();
        //                }
        //            }

        //            transaction.Commit();

        //            response.msg = "Images Processed Successfully";
        //            response.msgType = 1;
        //        }
        //        catch (Exception ex)
        //        {
        //            if (transaction.Connection != null)
        //            {
        //                transaction.Rollback();
        //            }
        //            response.msg = ex.Message;
        //            response.msgType = 2;
        //        }
        //    }
        //    return response;
        //}

        public MyHttpResponseMessage SaveInspectionImages(InspectionImageRequest req, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            string connectionString = new SQLService().getconnstring();
            var menuResult = _menuRepository.GetMenu(common.MenuID);
            var menu = (Menu)menuResult.data;

            // Table name selection
            string tableName = req.IsSpecs ? "[TBL_SPECS_IMAGES]" : "[TBL_INSPECTION_IMAGES]";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                SqlCommand command = connection.CreateCommand();
                command.Transaction = transaction;

                try
                {
                    // Note: Agar dono tables k IDs alag rakhne hain to max functions mein bhi tableName pass karna hoga
                    int tranId = GenerateNextImageTranID(command, tableName);
                    int dtCode = GetMaxDtCodeFromDB(command, tableName);

                    foreach (var img in req.Images)
                    {
                        if (img.IsNew && !string.IsNullOrEmpty(img.SavedPath))
                        {
                            dtCode++;
                            string query = $@"INSERT INTO {tableName} (
                        [TRAN_ID], [DT_CODE], [JOB_NO], [PTRAN_ID], [IMG_URL], 
                        [BCODE], [PERIOD_ID], [ADD_USER_ID], [ADD_DATE], 
                        [ADD_COMPUTER_NAME], [ADD_IP_ADDRESS], [MENU_ID], [DLT], [MENU_PRIFIX]
                    ) VALUES (
                        '{tranId}', '{dtCode}', '{req.JOB_NO}', '{req.PTRAN_ID}', '{img.SavedPath}', 
                        '{common.Branch}', '{common.Period}', '{common.Username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', 
                        '{common.ComputerName}', '{common.IPAddress}', '{common.MenuID}', 'T', '{menu.PERFIX}'
                    );";

                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    response.msg = "Images Processed Successfully";
                    response.msgType = 1;
                }
                catch (Exception ex)
                {
                    if (transaction.Connection != null) transaction.Rollback();
                    response.msg = ex.Message;
                    response.msgType = 2;
                }
            }
            return response;
        }

        private int GenerateNextImageTranID(SqlCommand command, string tableName)
        {
            command.CommandText = $"SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM {tableName}";
            object result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        private int GetMaxDtCodeFromDB(SqlCommand command, string tableName)
        {
            command.CommandText = $"SELECT ISNULL(MAX(DT_CODE), 0) FROM {tableName}";
            object result = command.ExecuteScalar();
            return Convert.ToInt32(result);
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

        private int GenerateNextInsSheetDetailId(SqlCommand command, Menu menu)
        {
            try
            {
                string? table = menu.TABLE1;
                string maxIdQuery = $"SELECT ISNULL(MAX(DT_CODE), 0) + 1 FROM TBL_INSPECTION_SHEET";
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

        public string GenerateNextInsSheetId(Common common)
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
                    string maxIdQuery = "SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM TBL_INSPECTION_SHEET";
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

        public MyHttpResponseMessage GetInspectionProcessByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {

                    string query = $@"SELECT TRAN_ID, V_DATE, VOUCHER_NO, QA_NAME, INS_DATE, JOB_NO, REF, REMARKS , ASTATUS
                                    FROM {table}
                                    WHERE TRAN_ID = {code} AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period} AND MENU_PRIFIX = '{menu.PERFIX}' AND DLT = 'T'";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = Convert.ToString(reader["TRAN_ID"]),
                            V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                            VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            QA_NAME = Convert.ToString(reader["QA_NAME"]),
                            INS_DATE = (reader["INS_DATE"] == DBNull.Value || Convert.ToDateTime(reader["INS_DATE"]).Year <= 1900) ? null : Convert.ToDateTime(reader["INS_DATE"]).ToString("yyyy-MM-dd"),
                            JOB_NO = reader["JOB_NO"] == DBNull.Value ? 0 : Convert.ToInt32(reader["JOB_NO"]),
                            REF = reader["REF"] == DBNull.Value ? "" : Convert.ToString(reader["REF"]),
                            REMARKS = reader["REMARKS"] == DBNull.Value ? "" : Convert.ToString(reader["REMARKS"]),


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

        //public MyHttpResponseMessage GetInspectionImagesByCode(int ptranId, int jobNo, Common common, Menu menu)
        //{
        //    MyHttpResponseMessage response = new MyHttpResponseMessage();
        //    try
        //    {
        //        List<object> imagesList = new List<object>();
        //        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
        //        {
        //            // PTRAN_ID yahan parent record ka TRAN_ID hai (jo image save karte waqt dala tha)
        //            string query = $@"SELECT TRAN_ID, DT_CODE, IMG_URL, JOB_NO 
        //                     FROM TBL_INSPECTION_IMAGES 
        //                     WHERE PTRAN_ID = {ptranId} AND BCODE = {common.Branch} AND DLT = 'T'";

        //            SqlCommand command = new SqlCommand(query, connection);
        //            connection.Open();
        //            SqlDataReader reader = command.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                var img = new
        //                {
        //                    TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
        //                    DT_CODE = Convert.ToInt32(reader["DT_CODE"]),
        //                    IMG_URL = Convert.ToString(reader["IMG_URL"]),
        //                    JOB_NO = Convert.ToInt32(reader["JOB_NO"])
        //                };
        //                imagesList.Add(img);
        //            }
        //            reader.Close();
        //        }

        //        response.data = imagesList;
        //        response.msgType = 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.msg = ex.Message;
        //        response.msgType = 2;
        //    }
        //    return response;
        //}

        public MyHttpResponseMessage GetInspectionImagesByCode(int ptranId, int jobNo, bool isSpecs, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                // Table switch logic
                string table = isSpecs ? "TBL_SPECS_IMAGES" : "TBL_INSPECTION_IMAGES";

                List<object> imagesList = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = $@"SELECT TRAN_ID, DT_CODE, IMG_URL, JOB_NO FROM {table} WHERE PTRAN_ID = {ptranId} AND JOB_NO = '{jobNo}' AND BCODE = {common.Branch} AND DLT = 'T'";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var img = new
                        {
                            traN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                            dT_CODE = Convert.ToInt32(reader["DT_CODE"]),
                            imG_URL = Convert.ToString(reader["IMG_URL"]),
                            joB_NO = Convert.ToInt32(reader["JOB_NO"])
                        };
                        imagesList.Add(img);
                    }
                    reader.Close();
                }

                response.data = imagesList;
                response.msgType = 1;
            }
            catch (Exception ex)
            {
                response.msg = ex.Message;
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage GetAQLChart()
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<Dictionary<string, object>> jsonDataResult = new List<Dictionary<string, object>>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT * FROM TBL_INSPECTION_SIZES ORDER BY CODE";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i) == DBNull.Value ? null : reader.GetValue(i));
                        }
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
                response.msg = ex.Message;
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage GetLockGridDataByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = "TBL_MPOD_DETAIL";
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
                            //DT_CODE = Convert.ToString(reader["DT_CODE"]),
                            PICK_ID = Convert.ToString(reader["DT_CODE"]),
                            ORDER_NO = Convert.ToString(reader["ORDER_NO"]),
                            SIZE = Convert.ToInt32(reader["SIZE"]),
                            ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                            COLOR = Convert.ToInt32(reader["COLOR"]),
                            DCHANNEL_CODE = reader["DCHANNEL_CODE"] != DBNull.Value ? Convert.ToInt32(reader["DCHANNEL_CODE"]) : 0,
                            SENTITY_CODE = reader["SENTITY_CODE"] != DBNull.Value ? Convert.ToInt32(reader["SENTITY_CODE"]) : 0,
                            GRADE_CODE = reader["GRADE_CODE"] != DBNull.Value ? Convert.ToInt32(reader["GRADE_CODE"]) : 0,
                            SEASON_CODE = reader["SEASON_CODE"] != DBNull.Value ? Convert.ToInt32(reader["SEASON_CODE"]) : 0,
                            QTY = Convert.ToString(reader["QTY"]),
                            CARTON = reader["CARTON"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CARTON"]),
                            TOTAL_CARTON = reader["TOTAL_CARTON"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TOTAL_CARTON"]),
                            COMM_TYPE = Convert.ToString(reader["COMM_TYPE"]),
                            COMM_RATE = reader["COMM_RATE"] != DBNull.Value ? Convert.ToDecimal(reader["COMM_RATE"]) : 0,
                            COMM_AMT = reader["COMM_AMT"] != DBNull.Value ? Convert.ToDecimal(reader["COMM_AMT"]) : 0,
                            UNIT = Convert.ToInt32(reader["UNIT"]),
                            INTAKE = reader["INTAKE"] != DBNull.Value ? Convert.ToInt32(reader["INTAKE"]) : 0,
                            //S_NO = reader["S_NO"] != DBNull.Value ? Convert.ToInt32(reader["S_NO"]) : 0,
                            RATE = Convert.ToString(reader["RATE"]),
                            AMT = Convert.ToString(reader["AMT"]),
                            PORT = Convert.ToInt32(reader["LC_PORT"]),
                            //SHIP_DATE = Convert.ToString(reader["SHIP_DATE"]),
                            //BOOKING_DATE = Convert.ToString(reader["BOOKING_DATE"]),
                            //HANDOVER_DATE = Convert.ToString(reader["HANDOVER_DATE"]),
                            SHIP_DATE = reader["SHIP_DATE"] != DBNull.Value && Convert.ToDateTime(reader["SHIP_DATE"]).Year > 1900 ? Convert.ToDateTime(reader["SHIP_DATE"]).ToString("yyyy-MM-dd") : "",
                            BOOKING_DATE = reader["BOOKING_DATE"] != DBNull.Value && Convert.ToDateTime(reader["BOOKING_DATE"]).Year > 1900 ? Convert.ToDateTime(reader["BOOKING_DATE"]).ToString("yyyy-MM-dd") : "",
                            HANDOVER_DATE = reader["HANDOVER_DATE"] != DBNull.Value && Convert.ToDateTime(reader["HANDOVER_DATE"]).Year > 1900 ? Convert.ToDateTime(reader["HANDOVER_DATE"]).ToString("yyyy-MM-dd") : "",
                            DOC = Convert.ToString(reader["DOC"]),
                            DT_DESC = Convert.ToString(reader["DT_DESC"]),
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

        public MyHttpResponseMessage GetInspectionProcessDetailByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = "TBL_MPOD_DETAIL";
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
                                            SELECT INS.TRAN_ID,t.ORDER_NO, t.SIZE, t.COLOR, QTY, t.UNIT, t.RATE, t.AMT, t.LC_PORT, t.SHIP_DATE,
											t.BOOKING_DATE, t.DOC, t.DT_DESC, INS.BCODE, INS.PERIOD_ID, t.ITEM_CODE, t.HANDOVER_DATE, t.INTAKE, t.SENTITY_CODE, t.DCHANNEL_CODE,
											t.GRADE_CODE, t.SEASON_CODE, t.COMM_TYPE, t.COMM_RATE, t.COMM_AMT, t.CARTON, t.TOTAL_CARTON, t.REV_REF, t.DLT,
											INS.OFFER_QTY, INS.CUTTING_QTY, INS.OFF_LINE, INS.BLIST_PCTN, INS.CTN_PCS, INS.PACK_MODE, ins.DT_CODE, INS.PICK_ID
                                            FROM {table} t
                                            JOIN GroupStatus g ON t.REV_REF = g.REV_REF
                                            LEFT OUTER JOIN TBL_INSPECTION_PROCESS INS ON INS.PICK_ID = t.DT_CODE
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
                            DT_CODE = Convert.ToString(reader["DT_CODE"]),
                            PICK_ID = Convert.ToString(reader["PICK_ID"]),
                            OFFER_QTY = reader["OFFER_QTY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["OFFER_QTY"]),
                            CUTTING_QTY = reader["CUTTING_QTY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CUTTING_QTY"]),
                            OFF_LINE = reader["OFF_LINE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["OFF_LINE"]),
                            BLIST_PCTN = reader["BLIST_PCTN"] == DBNull.Value ? "" : Convert.ToString(reader["BLIST_PCTN"]),
                            CTN_PCS = reader["CTN_PCS"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CTN_PCS"]),
                            PACK_MODE = reader["PACK_MODE"] == DBNull.Value ? "" : Convert.ToString(reader["PACK_MODE"]),


                            ORDER_NO = Convert.ToString(reader["ORDER_NO"]),
                            SIZE = Convert.ToInt32(reader["SIZE"]),
                            ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                            COLOR = Convert.ToInt32(reader["COLOR"]),
                            DCHANNEL_CODE = reader["DCHANNEL_CODE"] != DBNull.Value ? Convert.ToInt32(reader["DCHANNEL_CODE"]) : 0,
                            SENTITY_CODE = reader["SENTITY_CODE"] != DBNull.Value ? Convert.ToInt32(reader["SENTITY_CODE"]) : 0,
                            GRADE_CODE = reader["GRADE_CODE"] != DBNull.Value ? Convert.ToInt32(reader["GRADE_CODE"]) : 0,
                            SEASON_CODE = reader["SEASON_CODE"] != DBNull.Value ? Convert.ToInt32(reader["SEASON_CODE"]) : 0,
                            QTY = Convert.ToString(reader["QTY"]),
                            CARTON = reader["CARTON"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CARTON"]),
                            TOTAL_CARTON = reader["TOTAL_CARTON"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TOTAL_CARTON"]),
                            COMM_TYPE = Convert.ToString(reader["COMM_TYPE"]),
                            COMM_RATE = reader["COMM_RATE"] != DBNull.Value ? Convert.ToDecimal(reader["COMM_RATE"]) : 0,
                            COMM_AMT = reader["COMM_AMT"] != DBNull.Value ? Convert.ToDecimal(reader["COMM_AMT"]) : 0,
                            UNIT = Convert.ToInt32(reader["UNIT"]),
                            INTAKE = reader["INTAKE"] != DBNull.Value ? Convert.ToInt32(reader["INTAKE"]) : 0,
                            RATE = Convert.ToString(reader["RATE"]),
                            AMT = Convert.ToString(reader["AMT"]),
                            PORT = Convert.ToInt32(reader["LC_PORT"]),
                            SHIP_DATE = reader["SHIP_DATE"] != DBNull.Value && Convert.ToDateTime(reader["SHIP_DATE"]).Year > 1900 ? Convert.ToDateTime(reader["SHIP_DATE"]).ToString("yyyy-MM-dd") : "",
                            BOOKING_DATE = reader["BOOKING_DATE"] != DBNull.Value && Convert.ToDateTime(reader["BOOKING_DATE"]).Year > 1900 ? Convert.ToDateTime(reader["BOOKING_DATE"]).ToString("yyyy-MM-dd") : "",
                            HANDOVER_DATE = reader["HANDOVER_DATE"] != DBNull.Value && Convert.ToDateTime(reader["HANDOVER_DATE"]).Year > 1900 ? Convert.ToDateTime(reader["HANDOVER_DATE"]).ToString("yyyy-MM-dd") : "",
                            DOC = Convert.ToString(reader["DOC"]),
                            DT_DESC = Convert.ToString(reader["DT_DESC"]),

                            //DT_CODE = reader["DT_CODE"] == DBNull.Value ? "" : Convert.ToString(reader["DT_CODE"]),
                            //INS_SIZE_ID = reader["INS_SIZE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["INS_SIZE_ID"]),
                            //FAB = reader["FABRICATION"] == DBNull.Value ? "" : Convert.ToString(reader["FABRICATION"]),
                            //STI = reader["STITCHING"] == DBNull.Value ? "" : Convert.ToString(reader["STITCHING"]),
                            //ACC = reader["ACCESSORIES"] == DBNull.Value ? "" : Convert.ToString(reader["ACCESSORIES"]),
                            //F_MIN = reader["F_MIN"] == DBNull.Value ? 0 : Convert.ToDouble(reader["F_MIN"]),
                            //F_MAJ = reader["F_MAJ"] == DBNull.Value ? 0 : Convert.ToDouble(reader["F_MAJ"]),
                            //S_MIN = reader["S_MIN"] == DBNull.Value ? 0 : Convert.ToDouble(reader["S_MIN"]),
                            //S_MAJ = reader["S_MAJ"] == DBNull.Value ? 0 : Convert.ToDouble(reader["S_MAJ"]),
                            //A_MIN = reader["A_MIN"] == DBNull.Value ? 0 : Convert.ToDouble(reader["A_MIN"]),
                            //A_MAJ = reader["A_MAJ"] == DBNull.Value ? 0 : Convert.ToDouble(reader["A_MAJ"]),
                            //PICK_ID = reader["PICK_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PICK_ID"])

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

        public MyHttpResponseMessage GetInspectionSheetByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = "TBL_INSPECTION_SHEET";
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {

                    string query = $@"SELECT TRAN_ID ,DT_CODE, INS_SIZE_ID, FABRICATION, F_MIN, F_MAJ, STITCHING, S_MIN, S_MAJ, ACCESSORIES, A_MIN, A_MAJ
                                        FROM {table}
                                        WHERE TRAN_ID = {code} AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period} AND MENU_PRIFIX = '{menu.PERFIX}' 
                                        AND DLT = 'T' ORDER BY INS_SIZE_ID ASC";


                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = reader["TRAN_ID"] == DBNull.Value ? "" : Convert.ToString(reader["TRAN_ID"]),
                            DT_CODE = reader["DT_CODE"] == DBNull.Value ? "" : Convert.ToString(reader["DT_CODE"]),
                            INS_SIZE_ID = reader["INS_SIZE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["INS_SIZE_ID"]),
                            FAB = reader["FABRICATION"] == DBNull.Value ? "" : Convert.ToString(reader["FABRICATION"]),
                            STI = reader["STITCHING"] == DBNull.Value ? "" : Convert.ToString(reader["STITCHING"]),
                            ACC = reader["ACCESSORIES"] == DBNull.Value ? "" : Convert.ToString(reader["ACCESSORIES"]),
                            F_MIN = reader["F_MIN"] == DBNull.Value ? 0 : Convert.ToDouble(reader["F_MIN"]),
                            F_MAJ = reader["F_MAJ"] == DBNull.Value ? 0 : Convert.ToDouble(reader["F_MAJ"]),
                            S_MIN = reader["S_MIN"] == DBNull.Value ? 0 : Convert.ToDouble(reader["S_MIN"]),
                            S_MAJ = reader["S_MAJ"] == DBNull.Value ? 0 : Convert.ToDouble(reader["S_MAJ"]),
                            A_MIN = reader["A_MIN"] == DBNull.Value ? 0 : Convert.ToDouble(reader["A_MIN"]),
                            A_MAJ = reader["A_MAJ"] == DBNull.Value ? 0 : Convert.ToDouble(reader["A_MAJ"]),
                            //PICK_ID = reader["PICK_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PICK_ID"])

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
                                        FROM {pickMasterTable} MPO
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
                string? table = "TBL_INSPECTION_SHEET";
                string connectionString = new SQLService().getconnstring();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"UPDATE {table} SET DLT = 'F' WHERE DT_CODE = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}' AND MENU_PRIFIX = '{menu.PERFIX}'";
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

        //public MyHttpResponseMessage DeleteImage(DeleteImageRequest model, Common common, Menu menu)
        //{
        //    MyHttpResponseMessage response = new MyHttpResponseMessage();
        //    try
        //    {
        //        string table = "TBL_INSPECTION_IMAGES";
        //        string connectionString = new SQLService().getconnstring();
        //        List<string> filesToDelete = new List<string>();

        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            // 1. Pehle path(s) GET karen jo delete karne hain
        //            string selectQuery = "";
        //            if (model.IsAllDelete)
        //                selectQuery = $"SELECT IMG_URL FROM {table} WHERE PTRAN_ID = {model.PTranId} AND BCODE = {common.Branch} AND MENU_PRIFIX = '{menu.PERFIX}' AND DLT = 'T'";
        //            else
        //                selectQuery = $"SELECT IMG_URL FROM {table} WHERE TRAN_ID = {model.TranId} AND DT_CODE = {model.DtCode} AND BCODE = {common.Branch} AND MENU_PRIFIX = '{menu.PERFIX}' AND DLT = 'T'";

        //            using (SqlCommand selectCmd = new SqlCommand(selectQuery, connection))
        //            {
        //                using (SqlDataReader reader = selectCmd.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        string path = reader["IMG_URL"]?.ToString();
        //                        if (!string.IsNullOrEmpty(path)) filesToDelete.Add(path);
        //                    }
        //                }
        //            }

        //            // 2. Ab DB Update (DLT = 'F') karen
        //            string updateQuery = model.IsAllDelete
        //                ? $"UPDATE {table} SET DLT = 'F' WHERE PTRAN_ID = {model.PTranId} AND JOB_NO = {model.JobNo} AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period} AND MENU_PRIFIX = '{menu.PERFIX}'"
        //                : $"UPDATE {table} SET DLT = 'F' WHERE TRAN_ID = {model.TranId} AND DT_CODE = {model.DtCode} AND JOB_NO = {model.JobNo} AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period} AND MENU_PRIFIX = '{menu.PERFIX}'";

        //            using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
        //            {
        //                int rowsAffected = updateCmd.ExecuteNonQuery();
        //                //if (rowsAffected > 0)
        //                //{
        //                //    // 3. Agar DB update ho gaya, to physical files delete karen
        //                //    foreach (var relativePath in filesToDelete)
        //                //    {
        //                //        // Path ko physical path mein convert karen (e.g., C:\inetpub\wwwroot\...)
        //                //        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));

        //                //        if (System.IO.File.Exists(fullPath))
        //                //        {
        //                //            System.IO.File.Delete(fullPath);
        //                //        }
        //                //    }

        //                //    response.msgType = 1;
        //                //    response.msg = model.IsAllDelete ? "All images deleted from DB and Server" : "Image deleted from DB and Server";
        //                //}
        //                //else
        //                //{
        //                //    response.msgType = 2;
        //                //    response.msg = "No matching records found";
        //                //}
        //                if (rowsAffected > 0)
        //                {
        //                    if (model.IsAllDelete && filesToDelete.Count > 0)
        //                    {
        //                        string firstFilePath = filesToDelete[0].TrimStart('/');
        //                        string absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", firstFilePath);

        //                        var fileInfo = new FileInfo(absoluteFilePath);
        //                        DirectoryInfo jobFolder = fileInfo.Directory?.Parent; 

        //                        if (jobFolder != null && jobFolder.Exists)
        //                        {
        //                            jobFolder.Delete(recursive: true);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        foreach (var relativePath in filesToDelete)
        //                        {
        //                            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
        //                            if (System.IO.File.Exists(fullPath))
        //                            {
        //                                System.IO.File.Delete(fullPath);
        //                            }
        //                        }
        //                    }

        //                    response.msgType = 1;
        //                    response.msg = model.IsAllDelete ? "Job folder and all images deleted" : "Image deleted successfully";
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.msg = ex.Message;
        //        response.msgType = 2;
        //    }
        //    return response;
        //}

        public MyHttpResponseMessage DeleteImage(DeleteImageRequest model, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                // Dynamic Table Selection
                string table = model.IsSpecs ? "TBL_SPECS_IMAGES" : "TBL_INSPECTION_IMAGES";
                string connectionString = new SQLService().getconnstring();
                List<string> filesToDelete = new List<string>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Path(s) GET karen database se
                    string selectQuery = model.IsAllDelete
                        ? $"SELECT IMG_URL FROM {table} WHERE PTRAN_ID = {model.PTranId} AND BCODE = {common.Branch} AND MENU_PRIFIX = '{menu.PERFIX}' AND DLT = 'T'"
                        : $"SELECT IMG_URL FROM {table} WHERE TRAN_ID = {model.TranId} AND DT_CODE = {model.DtCode} AND BCODE = {common.Branch} AND MENU_PRIFIX = '{menu.PERFIX}' AND DLT = 'T'";

                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, connection))
                    {
                        using (SqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string path = reader["IMG_URL"]?.ToString();
                                if (!string.IsNullOrEmpty(path)) filesToDelete.Add(path);
                            }
                        }
                    }

                    // 2. DB Update (DLT = 'F')
                    string updateQuery = model.IsAllDelete
                        ? $"UPDATE {table} SET DLT = 'F' WHERE PTRAN_ID = {model.PTranId} AND JOB_NO = '{model.JobNo}' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period} AND MENU_PRIFIX = '{menu.PERFIX}'"
                        : $"UPDATE {table} SET DLT = 'F' WHERE TRAN_ID = {model.TranId} AND DT_CODE = {model.DtCode} AND JOB_NO = '{model.JobNo}' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period} AND MENU_PRIFIX = '{menu.PERFIX}'";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                    {
                        int rowsAffected = updateCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // 3. Physical Deletion Logic
                            if (model.IsAllDelete && filesToDelete.Count > 0)
                            {
                                // Specific Sub-folder target karen (Findings ya Specs)
                                string firstFilePath = filesToDelete[0].TrimStart('/');
                                string absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", firstFilePath);

                                var fileInfo = new FileInfo(absoluteFilePath);
                                DirectoryInfo targetSubFolder = fileInfo.Directory; // Yeh "Specs" ya "Findings" folder hai

                                if (targetSubFolder != null && targetSubFolder.Exists)
                                {
                                    // Sirf apna folder urayen
                                    // targetSubFolder delete karne ke baad:
                                    targetSubFolder.Delete(recursive: true);

                                    // MenuID folder ko check karne ka asaan tareeqa
                                    DirectoryInfo menuFolder = targetSubFolder.Parent;
                                    if (menuFolder != null && menuFolder.Exists)
                                    {
                                        // Check if folder is empty
                                        if (menuFolder.GetFileSystemInfos().Length == 0)
                                        {
                                            menuFolder.Delete();

                                            // Mazeed parent (JobNo folder) ko bhi check kar sakte hain
                                            DirectoryInfo jobFolder = menuFolder.Parent;
                                            if (jobFolder != null && jobFolder.Exists && jobFolder.GetFileSystemInfos().Length == 0)
                                            {
                                                jobFolder.Delete();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Single file deletion
                                foreach (var relativePath in filesToDelete)
                                {
                                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
                                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                                }
                            }

                            response.msgType = 1;
                            response.msg = "Deleted Successfully";
                        }
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

                InspectionProcess InspectionProcess = new InspectionProcess();
                List<InspectionProcessDetail> InspectionProcessDetailList = new List<InspectionProcessDetail>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT * FROM {table} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    string detailQuery = $@"SELECT * FROM {table2} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        InspectionProcess = new InspectionProcess
                        {
                            V_DATE = Convert.ToDateTime(reader["V_DATE"]),
                            REF = Convert.ToString(reader["REF"]),
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
                        var row = new InspectionProcessDetail
                        {
                            //ITEM_CODE = Convert.ToInt32(detail_Reader["ITEM_CODE"]),
                            //QTY = Convert.ToDouble(detail_Reader["QTY"]),
                            //UNIT = Convert.ToInt32(detail_Reader["UNIT"]),
                            //QTY2 = Convert.ToDouble(detail_Reader["QTY2"]),
                            //BAL_QTY = Convert.ToDouble(detail_Reader["BAL_QTY"]),
                            //RATE = Convert.ToInt32(detail_Reader["RATE"]),
                            //AMT = Convert.ToInt32(detail_Reader["AMT"]),
                            //SHIP_DATE = Convert.ToDateTime(detail_Reader["MFG_DATE"]),
                            //BOOKING_DATE = Convert.ToDateTime(detail_Reader["EXP_DATE"]),
                            //BATCH = Convert.ToString(detail_Reader["BATCH"]),
                            //DT_DESC = Convert.ToString(detail_Reader["DT_DESC"]),
                            //CHK = detail_Reader["CHK"] == DBNull.Value ? 0 : Convert.ToInt32(detail_Reader["CHK"]),
                            //PICK_ID = Convert.ToInt32(detail_Reader["PICK_ID"]),
                        };
                        InspectionProcessDetailList.Add(row);
                    }

                    detail_Reader.Close();
                    connection.Close();
                }

                var customInspectionProcess = new CustomInspectionProcess
                {
                    Master = InspectionProcess,
                    Detail = InspectionProcessDetailList
                };

                response = this.Save(customInspectionProcess, common);

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

        public MyHttpResponseMessage DeleteInspectionProcessDetailByCode(int code, Common common, Menu menu)
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

        public MyHttpResponseMessage GetDataForReport(InspectionProcessRDLCReport modelRecord, DataTable dataTable, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            InspectionProcessRDLCReport masterData = new InspectionProcessRDLCReport();
            CustomInspectionProcessForPrintReport reportData = new CustomInspectionProcessForPrintReport();
            var Menu = _menuRepository.GetMenu(common.MenuID);
            string? table = string.Empty, detailTable = string.Empty, pickTable = string.Empty;
            if (Menu.data != null)
            {
                var menu = (Menu)Menu.data;
                table = menu.TABLE1;
                detailTable = menu.TABLE2;
                pickTable = menu.PICK_TABLE_MASTER;
            }
            try
            {
                string topQuery = "", query = "";

                topQuery = $@"SELECT MPO.CLIENT_PO, P.PARTY_NAME AS SUP_NAME, PC.PARTY_NAME AS CLIENT_NAME, MPO.V_DATE, T.GROUP_NAME AS TERMS, C.DESCR AS CURRENCY , MPO.QTY, M.ADD_USER_ID AS USER_NAME
                                FROM {table} M
                                LEFT OUTER JOIN {detailTable} D ON D.TRAN_ID = M.TRAN_ID
                                LEFT OUTER JOIN {pickTable} MPO ON MPO.TRAN_ID = M.JOB_NO
                                LEFT OUTER JOIN TBL_PAY_TERMS T ON MPO.TERMS = T.GROUP_CODE
                                LEFT OUTER JOIN TBL_CURRENCY C ON MPO.CURR_CODE = C.CODE
                                LEFT OUTER JOIN TBL_PARTY_TYPES P ON P.PARTY_CODE = MPO.SPARTY_CODE AND P.ACT_CODE = MPO.SACT_CODE
								LEFT OUTER JOIN TBL_PARTY_TYPES PC ON PC.PARTY_CODE = MPO.PARTY_CODE AND PC.ACT_CODE = MPO.ACT_CODE
                                WHERE M.BCODE = '{common.Branch}' And M.PERIOD_ID = '{common.Period}' AND M.TRAN_ID = '{modelRecord.TRAN_ID}' AND M.DLT = 'T' AND D.DLT = 'T'";

                query = $@"WITH GroupStatus AS
                            (
                                SELECT REV_REF,
                                    HasC = MAX(CASE WHEN REV_STATUS = 'C' THEN 1 ELSE 0 END),
                                    HasR = MAX(CASE WHEN REV_STATUS = 'R' THEN 1 ELSE 0 END)
                                FROM {detailTable}
	                             WHERE  DLT = 'T' AND TRAN_ID = '{modelRecord.TRAN_ID}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'
                                GROUP BY REV_REF
                            ),
                            Final AS
                            (
                                SELECT 
                                    SE.GROUP_NAME AS ENTITY, 
                                    D.ORDER_NO,
                                    P.GROUP_NAME AS LC_PORT, 
                                    D.SHIP_DATE,  
                                    D.BOOKING_DATE, 
                                    D.HANDOVER_DATE,
                                    S.GROUP_NAME AS SEASON, 
                                    I.ITEM_NAME, 
                                    C.GROUP_NAME AS COLOR, 
                                    U.GROUP_NAME AS UNIT, 
                                    D.QTY, 
                                    D.RATE AS AMT,
                                    CR.SHORT_NAME AS CURR,
                                    D.REV_REF,
                                    D.DT_CODE,
		                            DC.GROUP_NAME AS DCHANNEL,
		                            SI.GROUP_NAME AS SIZE
                                FROM {detailTable} D
                                LEFT JOIN {pickTable} MPO ON MPO.TRAN_ID = D.PICK_ID AND MPO.BCODE = D.BCODE AND MPO.PERIOD_ID = D.PERIOD_ID
                                LEFT JOIN TBL_ITEMSMASTER I ON I.ITEM_CODE = D.ITEM_CODE
                                LEFT JOIN TBL_COLOR C ON C.GROUP_CODE = D.COLOR 
                                LEFT JOIN TBL_UNIT U ON U.GROUP_CODE = D.UNIT 
                                LEFT JOIN TBL_S_ENTITY SE ON SE.GROUP_CODE = D.SENTITY_CODE 
                                LEFT JOIN TBL_PORT P ON P.GROUP_CODE = D.LC_PORT 
                                LEFT JOIN TBL_SEASON S ON S.GROUP_CODE = D.SEASON_CODE 
                                LEFT JOIN TBL_CURRENCY CR ON CR.CODE = MPO.CURR_CODE 
	                            LEFT JOIN TBL_D_CHANNEL DC ON DC.GROUP_CODE = D.DCHANNEL_CODE 
	                            LEFT JOIN TBL_SIZE SI ON SI.GROUP_CODE = D.SIZE 
                                JOIN GroupStatus g ON D.REV_REF = g.REV_REF
                                WHERE g.HasC = 0 AND D.DLT = 'T' AND D.TRAN_ID = '{modelRecord.TRAN_ID}' AND D.BCODE = '{common.Branch}' AND D.PERIOD_ID = '{common.Period}'
                                  AND 
                                  (
                                    (g.HasR = 1 AND D.REV_STATUS = 'R' AND D.DT_CODE = 
                                        (SELECT MAX(DT_CODE) FROM {detailTable} 
                                         WHERE REV_REF = D.REV_REF AND REV_STATUS = 'R' AND DLT = 'T' AND TRAN_ID = '{modelRecord.TRAN_ID}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'))
                                    OR 
                                    (g.HasR = 0 AND D.REV_STATUS = 'N')))
                            SELECT * FROM Final ORDER BY ENTITY ASC;";

                masterData.COMPANY_NAME = currentCompany.C_NAME;
                masterData.COMPANY_ADDRESS = currentCompany.C_ADDRESS;
                masterData.COMPANY_PHONE = currentCompany.C_TEL;
                masterData.COMPANY_LOGO = currentCompany.C_LOGO;
                masterData.COMPANY_WATER = currentCompany.C_WATER;
                masterData.HEADER_NAME = $"{menuDetails.MD_NAME}";
                masterData.REPORT_NAME = $"{menuDetails.REPORT_NAME}";
                masterData.SIG1 = $"{menuDetails.MENU_SIG1}";
                masterData.SIG2 = $"{menuDetails.MENU_SIG2}";
                masterData.SIG3 = $"{menuDetails.MENU_SIG3}";
                masterData.SIG4 = $"{menuDetails.MENU_SIG4}";
                masterData.MENU_TERMS = $"{menuDetails.MENU_TERMS}";


                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    SqlCommand command = new SqlCommand(topQuery, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        //masterData.DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                        //masterData.CLIENT_PO = Convert.ToString(reader["CLIENT_PO"]);
                        //masterData.TERMS = Convert.ToString(reader["TERMS"]);
                        //masterData.CURRENCY = Convert.ToString(reader["CURRENCY"]);

                        masterData.DATE = reader["V_DATE"] == DBNull.Value ? string.Empty : (Convert.ToDateTime(reader["V_DATE"]) == new DateTime(1900, 1, 1) ? string.Empty : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy"));
                        masterData.CLIENT_PO = reader["CLIENT_PO"] == DBNull.Value ? string.Empty : reader["CLIENT_PO"].ToString();
                        masterData.TERMS = reader["TERMS"] == DBNull.Value ? string.Empty : reader["TERMS"].ToString();
                        masterData.SUP_NAME = reader["SUP_NAME"] == DBNull.Value ? string.Empty : reader["SUP_NAME"].ToString();
                        masterData.CLIENT_NAME = reader["CLIENT_NAME"] == DBNull.Value ? string.Empty : reader["CLIENT_NAME"].ToString();
                        masterData.CURRENCY = reader["CURRENCY"] == DBNull.Value ? string.Empty : reader["CURRENCY"].ToString();
                        masterData.ORDER_QTY = reader["QTY"] == DBNull.Value ? string.Empty : reader["QTY"].ToString();
                        masterData.USER = reader["USER_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["USER_NAME"]);
                    }
                    reader.Close();
                }
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["ENTITY"] = reader["ENTITY"] == DBNull.Value ? string.Empty : reader["ENTITY"].ToString();
                        dataRow["ORDER_NO"] = reader["ORDER_NO"] == DBNull.Value ? string.Empty : reader["ORDER_NO"].ToString();
                        dataRow["LC_PORT"] = reader["LC_PORT"] == DBNull.Value ? string.Empty : reader["LC_PORT"].ToString();
                        //dataRow["ENTITY"] = Convert.ToString(reader["ENTITY"]);
                        //dataRow["ORDER_NO"] = Convert.ToString(reader["ORDER_NO"]);
                        //dataRow["LC_PORT"] = Convert.ToString(reader["LC_PORT"]);
                        //dataRow["SHIP_DATE"] = reader["SHIP_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["SHIP_DATE"]).ToString("dd-MM-yyyy");
                        if (reader["SHIP_DATE"] == DBNull.Value)
                        {
                            dataRow["SHIP_DATE"] = null;
                        }
                        else
                        {
                            DateTime shipDate = Convert.ToDateTime(reader["SHIP_DATE"]);
                            dataRow["SHIP_DATE"] = (shipDate == new DateTime(1900, 1, 1)) ? null : shipDate.ToString("dd-MM-yyyy");
                        }

                        if (reader["BOOKING_DATE"] == DBNull.Value)
                        {
                            dataRow["BOOKING_DATE"] = null;
                        }
                        else
                        {
                            DateTime shipDate = Convert.ToDateTime(reader["BOOKING_DATE"]);
                            dataRow["BOOKING_DATE"] = (shipDate == new DateTime(1900, 1, 1)) ? null : shipDate.ToString("dd-MM-yyyy");
                        }

                        if (reader["HANDOVER_DATE"] == DBNull.Value)
                        {
                            dataRow["HANDOVER_DATE"] = null;
                        }
                        else
                        {
                            DateTime shipDate = Convert.ToDateTime(reader["HANDOVER_DATE"]);
                            dataRow["HANDOVER_DATE"] = (shipDate == new DateTime(1900, 1, 1)) ? null : shipDate.ToString("dd-MM-yyyy");
                        }

                        //dataRow["SEASON"] = Convert.ToString(reader["SEASON"]);
                        //dataRow["ITEM_NAME"] = Convert.ToString(reader["ITEM_NAME"]);
                        //dataRow["COLOR"] = Convert.ToString(reader["COLOR"]);
                        //dataRow["UNIT"] = Convert.ToString(reader["UNIT"]);
                        dataRow["SEASON"] = reader["SEASON"] == DBNull.Value ? string.Empty : reader["SEASON"].ToString();
                        dataRow["ITEM_NAME"] = reader["ITEM_NAME"] == DBNull.Value ? string.Empty : reader["ITEM_NAME"].ToString();
                        dataRow["COLOR"] = reader["COLOR"] == DBNull.Value ? string.Empty : reader["COLOR"].ToString();
                        dataRow["UNIT"] = reader["UNIT"] == DBNull.Value ? string.Empty : reader["UNIT"].ToString();
                        dataRow["CURR"] = reader["CURR"] == DBNull.Value ? string.Empty : reader["CURR"].ToString();
                        dataRow["DCHANNEL"] = reader["DCHANNEL"] == DBNull.Value ? string.Empty : reader["DCHANNEL"].ToString();
                        dataRow["SIZE"] = reader["SIZE"] == DBNull.Value ? string.Empty : reader["SIZE"].ToString();
                        dataRow["QTY"] = reader["QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QTY"]);
                        dataRow["AMT"] = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AMT"]);







                        dataTable.Rows.Add(dataRow);
                    }
                    reader.Close();
                }
                reportData.Master = masterData;
                reportData.Detail = dataTable;
                response.data = reportData;
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