using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class SampleSubmissionRepository : ISampleSubmissionRepository
    {
        public IBranchRepository _branchRepository { get; set; }
        public IMenuService _menuRepository { get; set; }
        public SampleSubmissionRepository(IBranchRepository branchRepository, IMenuService menuRepository)
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
                    string query = $@"SELECT A.TRAN_ID, A.V_DATE, A.VOUCHER_NO, A.CLIENT_PO, A.REF, A.REMARKS, A.PARTY_CODE, A.ACT_CODE, PT.PARTY_NAME, A.ADD_USER_ID, A.ADD_DATE, 
                                        CASE WHEN A.ASTATUS = 'Y' THEN 'Active' ELSE 'In-Active' END AS ASTATUS 
                                        FROM {table} A 
                                        --LEFT OUTER JOIN TBL_SAMPLE_PRICING MPO ON MPO.PARTY_CODE = A.PARTY_CODE AND MPO.ACT_CODE = A.ACT_CODE AND MPO.CLIENT_PO = A.CLIENT_PO
                                        LEFT OUTER JOIN TBL_PARTY_TYPES PT ON  PT.PARTY_CODE = A.PARTY_CODE AND PT.ACT_CODE = A.ACT_CODE
                                        WHERE A.DLT = 'T' AND A.BCODE = '{common.Branch}' AND A.PERIOD_ID = '{common.Period}' ORDER BY A.TRAN_ID DESC";

                    SqlCommand command = new SqlCommand(query, connection);
					connection.Open();
					SqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
					{
						var row = new
						{
                            TRAN_ID = reader["TRAN_ID"] == DBNull.Value ? null : Convert.ToString(reader["TRAN_ID"]),
                            V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                            VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? null : Convert.ToString(reader["VOUCHER_NO"]),
                            CLIENT_PO = Convert.ToString(reader["CLIENT_PO"]),
                            REF = reader["REF"] == DBNull.Value ? null : Convert.ToString(reader["REF"]),
                            REMARKS = reader["REMARKS"] == DBNull.Value ? null : Convert.ToString(reader["REMARKS"]),
                            PARTY_NAME = reader["PARTY_NAME"] == DBNull.Value ? null : Convert.ToString(reader["PARTY_NAME"]),
                            ADD_USER_ID = Convert.ToString(reader["ADD_USER_ID"]),
                            ADD_DATE = Convert.ToDateTime(reader["ADD_DATE"]),
                            PARTY_CODE = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                            ASTATUS = reader["ASTATUS"] == DBNull.Value ? null : Convert.ToString(reader["ASTATUS"]),
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

        public MyHttpResponseMessage Save(CustomSampleSubmission modelRecord, Common common, Menu menu)
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
                        var partyInformation = partiesData.FirstOrDefault(p => int.TryParse(p.customizedKey, out int key) && key == modelRecord.Master.PARTY_CODE);

                        if (partyInformation != null)
                        {
                            modelRecord.Master.PARTY_CODE = partyInformation.key;
                            modelRecord.Master.ACT_CODE = partyInformation.accountCode;
                        }

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
                                    "(TRAN_ID, V_DATE, VOUCHER_NO, PARTY_CODE, ACT_CODE, CLIENT_PO, REF, REMARKS, BCODE, " +
                                    "PERIOD_ID, ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID, EDIT_DATE, EDIT_COMPUTER_NAME, " +
                                    "EDIT_IP_ADDRESS, ADD_POSTALCODE, EDIT_POSTALCODE, ASTATUS,JOB_NO, " +
                                    "MENU_ID, DLT) " +
                                    $"VALUES " +
                                    $"('{code}', '{modelRecord.Master.V_DATE}', '{voucherNo}','{modelRecord.Master.PARTY_CODE}','{modelRecord.Master.ACT_CODE}','{modelRecord.Master.CLIENT_PO}', " +
                                    $"'{modelRecord.Master.REF}','{modelRecord.Master.REMARKS}' , '{branch}', '{periodID}'," +
                                    $"'{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}'," +
                                    $"'{ip}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}'," +
                                    $"'{computerName}', '{ip}', " +
                                    $"'{postalCode}', '{postalCode}', " +
                                    $"'{modelRecord.Master.ASTATUS}','{modelRecord.Master.JOBNO}', '{menuID}', 'T')";

                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            query = $"UPDATE {table} " +
                                    $"SET V_DATE = '{modelRecord.Master.V_DATE}', " +
                                    $"PARTY_CODE = '{modelRecord.Master.PARTY_CODE}', " +
                                    $"ACT_CODE = '{modelRecord.Master.ACT_CODE}', " +
                                    $"CLIENT_PO = '{modelRecord.Master.CLIENT_PO}', " +
                                    $"REF = '{modelRecord.Master.REF}', " +
                                    $"JOB_NO = '{modelRecord.Master.JOBNO}', " +
                                    $"REMARKS = '{modelRecord.Master.REMARKS}', " +
                                    $"EDIT_USER_ID = '{username}', " +
                                    $"EDIT_DATE = '{CommonService.GetDateTime("Pakistan Standard Time")}', " +
                                    $"EDIT_COMPUTER_NAME = '{computerName}', " +
                                    $"EDIT_IP_ADDRESS = '{ip}', " +
                                    $"EDIT_POSTALCODE = '{postalCode}', " +
                                    $"ASTATUS = '{modelRecord.Master.ASTATUS}' " +
                                    $"WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND BCODE = '{branch}' AND PERIOD_ID = '{periodID}'";

                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }

                        var isDetailAdded = true;

                        //if (modelRecord.Detail.Count > 0)
                        //{
                        //    detailQuery = $"UPDATE {detailTable} SET DLT = 'F'" +
                        //    $" WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND BCODE = '{branch}' AND PERIOD_ID = '{periodID}'";
                        //    command.CommandText = detailQuery;
                        //    command.ExecuteNonQuery();
                        //}
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
                                                       (TRAN_ID, DT_CODE, SPARTY_CODE, SACT_CODE, YARN_CODE, COMM_RATE, SO_DATE,RO_DATE, COMMENTS, AWB, ORIGINAL_SAMPLE_DOC, RATE, PICK_ID, REV_STATUS, REV_REF,
                                                       BCODE, PERIOD_ID, ADD_USER_ID, ADD_DATE,ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID,
                                                       EDIT_DATE, EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS,ADD_POSTALCODE, EDIT_POSTALCODE, MENU_ID, DLT)
                                                       VALUES
                                                       ('{modelRecord.Master.TRAN_ID}', '{detailCode}', '{item.SPARTY_CODE}', '{item.SACT_CODE}', '{item.YARN_CODE}', '{item.COMM_RATE}', '{item.SO_DATE}','{item.RO_DATE}', '{item.COMMENTS}', '{item.AWB}','{item.ORIGINAL_SAMPLE_DOC}',
                                                        '{item.RATE}','{item.PICK_ID}','{item.REV_STATUS}',{detailCode},'{branch}', '{periodID}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', 
                                                        '{computerName}', '{ip}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}', '{ip}', '{postalCode}',
                                                        '{postalCode}', '{menuID}', 'T')";

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
                                                       (TRAN_ID, DT_CODE, SPARTY_CODE, SACT_CODE, YARN_CODE, COMM_RATE, SO_DATE,RO_DATE, COMMENTS, AWB, ORIGINAL_SAMPLE_DOC, RATE, PICK_ID, REV_STATUS, REV_REF,
                                                       BCODE, PERIOD_ID, ADD_USER_ID, ADD_DATE,ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID,
                                                       EDIT_DATE, EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS,ADD_POSTALCODE, EDIT_POSTALCODE, MENU_ID, DLT)
                                                       VALUES
                                                       ('{modelRecord.Master.TRAN_ID}', '{detailCode}', '{item.SPARTY_CODE}', '{item.SACT_CODE}', '{item.YARN_CODE}', '{item.COMM_RATE}', '{item.SO_DATE}', '{item.RO_DATE}', '{item.COMMENTS}', '{item.AWB}', '{item.ORIGINAL_SAMPLE_DOC}',
                                                        '{item.RATE}','{item.PICK_ID}','{item.REV_STATUS}',{item.REV_REF},'{branch}', '{periodID}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', 
                                                        '{computerName}', '{ip}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}', '{ip}', '{postalCode}',
                                                        '{postalCode}', '{menuID}', 'T')";

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
                                    string revStatusCheckQuery = $@"SELECT REV_STATUS FROM TBL_MPOD_DETAIL WHERE DT_CODE = '{item.DT_CODE}'";
                                    command.CommandText = revStatusCheckQuery;
                                    object revStatus = command.ExecuteScalar();
                                    string revStatusStr = revStatus?.ToString().Trim();


                                    if (revStatusStr != item.REV_STATUS)
                                    {
                                        response.msgType = 2;
                                        response.msg = $"You Cannot change Revised entry to New entry.";
                                        return response;
                                    }

                                    detailQuery = $"UPDATE {detailTable} " +
                                                  $"SET SPARTY_CODE = '{item.SPARTY_CODE}', " +
                                                  $"SACT_CODE = '{item.SACT_CODE}', " +
                                                  $"YARN_CODE = '{item.YARN_CODE}', " +
                                                  $"COMM_RATE = '{item.COMM_RATE}', " +
                                                  $"RATE = '{item.RATE}', " +
                                                  $"SO_DATE = '{item.SO_DATE}', " +
                                                  $"RO_DATE = '{item.RO_DATE}', " +
                                                  $"COMMENTS = '{item.COMMENTS}', " +
                                                  $"AWB = '{item.AWB}', " +
                                                  $"ORIGINAL_SAMPLE_DOC = '{item.ORIGINAL_SAMPLE_DOC}', " +
                                                  $"REV_STATUS = '{item.REV_STATUS}', " +
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

        public MyHttpResponseMessage GetSampleSubmissionByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {

                    string query = $@"SELECT VP.TRAN_ID, VP.V_DATE, VP.VOUCHER_NO, VP.PARTY_CODE, VP.ACT_CODE, VP.CLIENT_PO, VP.REF, VP.REMARKS, VP.ASTATUS,VP.JOB_NO
                                    FROM {table} VP
                                    WHERE VP.DLT = 'T' AND VP.TRAN_ID = '{code}' AND VP.BCODE = '{common.Branch}' AND VP.PERIOD_ID = '{common.Period}'";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = Convert.ToString(reader["TRAN_ID"]),
                            V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                            JOB_NO = reader["JOB_NO"] == DBNull.Value ? 0 : Convert.ToInt32(reader["JOB_NO"]),
                            VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                            PARTY_CODE = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                            CLIENT_PO = Convert.ToString(reader["CLIENT_PO"]),
                            REF = Convert.ToString(reader["REF"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),

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

        public MyHttpResponseMessage GetSampleSubmissionDetailByCode(int code, Common common, Menu menu)
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
                            SO_DATE = reader["SO_DATE"] == DBNull.Value || Convert.ToDateTime(reader["SO_DATE"]) == new DateTime(1900, 1, 1) ? null : Convert.ToDateTime(reader["SO_DATE"]).ToString("yyyy-MM-dd"),
                            RO_DATE = reader["RO_DATE"] == DBNull.Value || Convert.ToDateTime(reader["RO_DATE"]) == new DateTime(1900, 1, 1) ? null : Convert.ToDateTime(reader["RO_DATE"]).ToString("yyyy-MM-dd"),
                            DT_CODE = Convert.ToString(reader["DT_CODE"]),
                            PICK_ID = Convert.ToString(reader["PICK_ID"]),
                            SPARTY_CODE = Convert.ToString(reader["SPARTY_CODE"]),
                            SACT_CODE = Convert.ToString(reader["SACT_CODE"]),
                            YARN_CODE = Convert.ToString(reader["YARN_CODE"]),
                            COMMENTS = Convert.ToString(reader["COMMENTS"]),
                            AWB = Convert.ToString(reader["AWB"]),
                            ORIGINAL_SAMPLE_DOC = Convert.ToString(reader["ORIGINAL_SAMPLE_DOC"]),
                            COMM_RATE = reader["COMM_RATE"] != DBNull.Value ? Convert.ToInt32(reader["COMM_RATE"]) : 0,
                            RATE = reader["RATE"] != DBNull.Value ? Convert.ToDecimal(reader["RATE"]) : 0,
                            SUPPLIER_CODE = $"{Convert.ToString(reader["SPARTY_CODE"])}{Convert.ToString(reader["SACT_CODE"])}",
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

        public MyHttpResponseMessage GetBatchDetailByProcess(SampleSubmission model, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty, detailTable = string.Empty, pickMasterTable = string.Empty, pickDetailTable = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                    pickMasterTable = menu.PICK_TABLE_MASTER;
                    pickDetailTable = menu.PICK_TABLE_DETAIL;
                }

                if (!String.IsNullOrWhiteSpace(table) && !String.IsNullOrWhiteSpace(detailTable) && !String.IsNullOrWhiteSpace(pickMasterTable) && !String.IsNullOrWhiteSpace(pickDetailTable))
                {
                    List<object> jsonDataResult = new List<object>();
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = $@"WITH LatestRecords AS (
                                                    SELECT 
                                                        REV_REF, 
                                                        MAX(CASE WHEN REV_STATUS = 'C' THEN NULL ELSE DT_CODE END) as Max_DT_CODE,
                                                        MAX(CASE WHEN REV_STATUS = 'C' THEN 1 ELSE 0 END) as IsCancelled
                                                    FROM {pickDetailTable}
                                                    WHERE DLT = 'T'
                                                    GROUP BY REV_REF
                                                )
                                                SELECT 
                                                    M.TRAN_ID, M.V_DATE, M.VOUCHER_NO, M.PARTY_CODE, M.ACT_CODE, 
                                                    PT.PARTY_NAME AS CLIENT_NAME, M.CLIENT_PO, M.REF, M.REMARKS,
                                                    D.DT_CODE AS PICK_ID, D.SPARTY_CODE, D.SACT_CODE, SPT.PARTY_NAME AS SUPPLIER, D.YARN_CODE, D.PQ_DATE, Y.GROUP_NAME AS YARN, D.COMM_RATE, D.RATE
                                                FROM {pickMasterTable} M
                                                INNER JOIN {pickDetailTable} D ON D.TRAN_ID = M.TRAN_ID
                                                INNER JOIN LatestRecords L ON D.REV_REF = L.REV_REF AND D.DT_CODE = L.Max_DT_CODE
                                                LEFT OUTER JOIN TBL_PARTY_TYPES PT ON M.PARTY_CODE = PT.PARTY_CODE AND PT.ACT_CODE = M.ACT_CODE
                                                LEFT OUTER JOIN TBL_PARTY_TYPES SPT ON D.SPARTY_CODE = SPT.PARTY_CODE AND SPT.ACT_CODE = D.SACT_CODE
												LEFT OUTER JOIN TBL_YARN Y ON Y.GROUP_CODE = D.YARN_CODE
                                                WHERE 
                                                    M.CLIENT_PO = '{model.CLIENT_PO}' 
                                                    AND M.PARTY_CODE = '{model.PARTY_CODE}' 
                                                    AND M.ACT_CODE = '{model.ACT_CODE}' 
                                                    AND M.JOB_NO = '{model.JOBNO}' 
                                                    AND M.DLT = 'T' 
                                                    AND M.BCODE = {common.Branch} 
                                                    AND M.PERIOD_ID = {common.Period}
                                                    AND L.IsCancelled = 0 -- Jis group mein 'C' hai wo poora group bahar nikal diya
                                                ORDER BY D.DT_CODE DESC;";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            
                            var row = new
                            {
                                TRAN_ID = reader["TRAN_ID"].ToString(),
                                V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                PQ_DATE = reader["PQ_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["PQ_DATE"]).ToString("yyyy-MM-dd"),
                                VOUCHER_NO = reader["VOUCHER_NO"].ToString(),
                                PARTY_CODE = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                                CLIENT_NAME = reader["CLIENT_NAME"].ToString(),
                                CLIENT_PO = reader["CLIENT_PO"].ToString(),
                                REF = reader["REF"].ToString(),
                                REMARKS = reader["REMARKS"].ToString(),
                                PICK_ID = Convert.ToInt32(reader["PICK_ID"]),
                                SUPPLIER = reader["SUPPLIER"].ToString(),
                                SUPPLIER_CODE = $"{Convert.ToString(reader["SPARTY_CODE"])}{Convert.ToString(reader["SACT_CODE"])}",
                                SPARTY_CODE = Convert.ToString(reader["SPARTY_CODE"]),
                                SACT_CODE = Convert.ToString(reader["SACT_CODE"]),
                                YARN_CODE = reader["YARN_CODE"].ToString(),
                                YARN = reader["YARN"].ToString(),
                                COMM_RATE = reader["COMM_RATE"] != DBNull.Value ? Convert.ToDecimal(reader["COMM_RATE"]) : 0,
                                RATE = reader["RATE"] != DBNull.Value ? Convert.ToDecimal(reader["RATE"]) : 0,
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

        //public MyHttpResponseMessage Delete(int code, Common common, Menu menu)
        //{
        //    MyHttpResponseMessage response = new MyHttpResponseMessage();
        //    response.msgType = 2;
        //    response.msg = "Data not found in our records";
        //    try
        //    {
        //        string? masterTable = menu.TABLE1;
        //        string? detailTable = menu.TABLE2;
        //        string connectionString = new SQLService().getconnstring();
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            string query = $"UPDATE {detailTable} SET DLT = 'F' WHERE DT_CODE = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
        //            SqlCommand command = new SqlCommand(query, connection);
        //            command.ExecuteNonQuery();
        //            response.msgType = 1;
        //            response.msg = "Record Deleted Successfully";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string _catchMessage = ex.Message;
        //        if (ex.InnerException != null)
        //        {
        //            _catchMessage += "<br/>" + ex.InnerException.Message;
        //        }
        //        response.msg = _catchMessage;
        //        response.msgType = 2;
        //    }
        //    return response;
        //}

        public MyHttpResponseMessage Delete(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                string? masterTable = menu.TABLE1;
                string? detailTable = menu.TABLE2;
                string connectionString = new SQLService().getconnstring();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $@"UPDATE {detailTable} SET DLT = 'F' WHERE TRAN_ID = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}';
                             UPDATE {masterTable} SET DLT = 'F' WHERE TRAN_ID = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}';";

                    SqlCommand command = new SqlCommand(query, connection);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        response.msgType = 1;
                        response.msg = "Record Deleted Successfully";
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

                SampleSubmission SampleSubmission = new SampleSubmission();
                List<SampleSubmissionDetail> SampleSubmissionDetailList = new List<SampleSubmissionDetail>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT * FROM {table} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    string detailQuery = $@"SELECT * FROM {table2} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        SampleSubmission = new SampleSubmission
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
                        var row = new SampleSubmissionDetail
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
                        };
                        SampleSubmissionDetailList.Add(row);
                    }

                    detail_Reader.Close();
                    connection.Close();
                }

                var customSampleSubmission = new CustomSampleSubmission
                {
                    Master = SampleSubmission,
                    Detail = SampleSubmissionDetailList
                };

                response = this.Save(customSampleSubmission, common, menu);

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

        public MyHttpResponseMessage DeleteSampleSubmissionDetailByCode(int code, Common common, Menu menu)
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

        public MyHttpResponseMessage GetDataForReport(SampleSubmissionRDLCReport modelRecord, DataTable dataTable, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            SampleSubmissionRDLCReport masterData = new SampleSubmissionRDLCReport();
            CustomSampleSubmissionForPrintReport reportData = new CustomSampleSubmissionForPrintReport();
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

                topQuery = $@"SELECT M.CLIENT_PO, PC.PARTY_NAME AS CLIENT_NAME, M.V_DATE, M.EDIT_USER_ID, M.REF, M.REMARKS, M.VOUCHER_NO
                                FROM {table} M
                                LEFT OUTER JOIN {detailTable} D ON D.TRAN_ID = M.TRAN_ID
                                LEFT OUTER JOIN {pickTable} MPO ON MPO.TRAN_ID = M.JOB_NO
								LEFT OUTER JOIN TBL_PARTY_TYPES PC ON PC.PARTY_CODE = M.PARTY_CODE AND PC.ACT_CODE = M.ACT_CODE
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
                                    P.PARTY_NAME,
									Y.GROUP_NAME AS YARN,
									D.COMM_RATE,
									D.RATE,
                                    D.REV_REF,
                                    D.DT_CODE

                                FROM {detailTable} D
                                LEFT JOIN {pickTable} MPO ON MPO.TRAN_ID = D.PICK_ID AND MPO.BCODE = D.BCODE AND MPO.PERIOD_ID = D.PERIOD_ID

                                LEFT OUTER JOIN TBL_PARTY_TYPES P ON P.PARTY_CODE = D.SPARTY_CODE AND P.ACT_CODE = D.SACT_CODE
								LEFT OUTER JOIN TBL_YARN Y ON Y.GROUP_CODE = D.YARN_CODE

                                JOIN GroupStatus g ON D.REV_REF = g.REV_REF
                                WHERE g.HasC = 0 AND D.DLT = 'T' AND D.TRAN_ID = '{modelRecord.TRAN_ID}' AND D.BCODE = '{common.Branch}' AND D.PERIOD_ID = '{common.Period}'
                                  AND 
                                  (
                                    (g.HasR = 1 AND D.REV_STATUS = 'R' AND D.DT_CODE = 
                                        (SELECT MAX(DT_CODE) FROM {detailTable} 
                                         WHERE REV_REF = D.REV_REF AND REV_STATUS = 'R' AND DLT = 'T' AND TRAN_ID = '{modelRecord.TRAN_ID}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'))
                                    OR 
                                    (g.HasR = 0 AND D.REV_STATUS = 'N')))
                            SELECT * FROM Final ORDER BY DT_CODE ASC;";

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
                        masterData.CLIENT_PO = Convert.ToString(reader["CLIENT_PO"]);
                        masterData.PARTY_NAME = Convert.ToString(reader["CLIENT_NAME"]);
                        masterData.DATE = reader["V_DATE"] == DBNull.Value || Convert.ToDateTime(reader["V_DATE"]).Year <= 1900 ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MMM-yy");
                        masterData.USER = reader["EDIT_USER_ID"] == DBNull.Value ? "" : Convert.ToString(reader["EDIT_USER_ID"]);

                        masterData.INVOICE_NUMBER = Convert.ToString(reader["VOUCHER_NO"]);
                        //masterData.CURRENCY = reader["CURRENCY"] == DBNull.Value ? string.Empty : reader["CURRENCY"].ToString();
                        //masterData.JOB_NO = Convert.ToString(reader["JOB_NO"]);
                        masterData.REF = Convert.ToString(reader["REF"]);
                        masterData.REMARKS = Convert.ToString(reader["REMARKS"]);
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
                        dataRow["BUYER"] = reader["PARTY_NAME"] == DBNull.Value ? string.Empty : Convert.ToString(reader["PARTY_NAME"]);
                        dataRow["YARN"] = reader["YARN"] == DBNull.Value ? string.Empty : Convert.ToString(reader["YARN"]);
                        dataRow["COMM_RATE"] = reader["COMM_RATE"] == DBNull.Value ? string.Empty : Convert.ToDecimal(reader["COMM_RATE"]);
                        dataRow["RATE"] = reader["RATE"] == DBNull.Value ? string.Empty : Convert.ToDecimal(reader["RATE"]);

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