using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class SampleDevAndPricingRepository_OLD : ISampleDevAndPricingRepository_OLD
    {
        public IMenuRepository _menuRepository { get; set; }
        public IBranchRepository _branchRepository { get; set; }
        public ICommonRepository _commonRepository { get; set; }
        public SampleDevAndPricingRepository_OLD(IMenuRepository menuRepository, IBranchRepository branchRepository, ICommonRepository commonRepository)
        {
            _menuRepository = menuRepository;
            _branchRepository = branchRepository;
            _commonRepository = commonRepository;
        }

        public MyHttpResponseMessage QuickSearch(Core.Entities.Common common)
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
                        string query = $@"SELECT SP.TRAN_ID, SP.V_DATE, SP.VOUCHER_NO, SP.REF, SP.CLIENT_PO, PTC.PARTY_NAME, SP.REC_ON_DATE, IT.ITEM_NAME,
                                            F.GROUP_NAME AS FABRIC, SP.STYLE_DESC, SP.DOC, SP.BLEND, SP.GSM, SP.SIZE_RANGE, SP.DOC_PFR, SP.RATIO, SP.PROJ_QTY,
                                            SP.TARGET_PRICE, SP.REMARKS, SP.EDIT_USER_ID, SP.EDIT_DATE,
                                            CASE WHEN SP.ASTATUS = 'Y' THEN 'Active' WHEN SP.ASTATUS = 'N' THEN 'In-Active' ELSE '' END ASTATUS
                                            FROM {table} SP
                                            LEFT OUTER JOIN TBL_PARTY_TYPES PTC ON SP.PARTY_CODE = PTC.PARTY_CODE AND PTC.ACT_CODE = SP.ACT_CODE   
                                            LEFT OUTER JOIN TBL_ITEMSMASTER IT ON SP.ITEM_CODE = IT.ITEM_CODE
                                            LEFT OUTER JOIN TBL_FABRIC F ON SP.FABRIC = F.GROUP_CODE
                                            WHERE SP.DLT = 'T' ORDER BY SP.TRAN_ID DESC";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                TRAN_ID = reader["TRAN_ID"] == DBNull.Value ? "" : Convert.ToString(reader["TRAN_ID"]),
                                V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy"),
                                VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? "" : Convert.ToString(reader["VOUCHER_NO"]),
                                REF = reader["REF"] == DBNull.Value ? "" : Convert.ToString(reader["REF"]),
                                CLIENT_PO = reader["CLIENT_PO"] == DBNull.Value ? "" : Convert.ToString(reader["CLIENT_PO"]),
                                PARTY_NAME = reader["PARTY_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["PARTY_NAME"]),
                                REC_ON_DATE = reader["REC_ON_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["REC_ON_DATE"]).ToString("dd-MM-yyyy"),
                                ITEM_NAME = reader["ITEM_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM_NAME"]),
                                FABRIC = reader["FABRIC"] == DBNull.Value ? "" : Convert.ToString(reader["FABRIC"]),
                                STYLE_DESC = reader["STYLE_DESC"] == DBNull.Value ? "" : Convert.ToString(reader["STYLE_DESC"]),
                                DOC = reader["DOC"] == DBNull.Value ? "" : Convert.ToString(reader["DOC"]),
                                BLEND = reader["BLEND"] == DBNull.Value ? "" : Convert.ToString(reader["BLEND"]),
                                GSM = reader["GSM"] == DBNull.Value ? 0 : Convert.ToDouble(reader["GSM"]),
                                SIZE_RANGE = reader["SIZE_RANGE"] == DBNull.Value ? "" : Convert.ToString(reader["SIZE_RANGE"]),
                                DOC_PFR = reader["DOC_PFR"] == DBNull.Value ? "" : Convert.ToString(reader["DOC_PFR"]),
                                RATIO = reader["RATIO"] == DBNull.Value ? "" : Convert.ToString(reader["RATIO"]),
                                PROJ_QTY = reader["PROJ_QTY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["PROJ_QTY"]),
                                TARGET_PRICE = reader["TARGET_PRICE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TARGET_PRICE"]),
                                REMARKS = reader["REMARKS"] == DBNull.Value ? "" : Convert.ToString(reader["REMARKS"]),
                                EDIT_USER_ID = reader["EDIT_USER_ID"] == DBNull.Value ? "" : Convert.ToString(reader["EDIT_USER_ID"]),
                                EDIT_DATE = reader["EDIT_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["EDIT_DATE"]).ToString("dd-MM-yyyy"),
                                ASTATUS = reader["ASTATUS"] == DBNull.Value ? "" : Convert.ToString(reader["ASTATUS"]),
                            };
                            jsonDataResult.Add(row);
                        }
                        reader.Close();

                        response.data = jsonDataResult;
                        response.msg = "";
                        response.msgType = 1;
                        return response;
                    }
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

        private int GenerateNextId(Common common, SqlCommand command)
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
                    string maxIdQuery = $"SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM {table} WHERE BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                    command.CommandText = maxIdQuery;
                    object result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
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

        public MyHttpResponseMessage Save(SampleDevAndPricing_OLD modelRecord, Core.Entities.Common common)
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
                    var Ip = common.IPAddress;
                    var Computer = common.ComputerName;
                    var Postal = common.PostalCode;
                    var userid = common.Username;
                    int code = 0;
                    string voucherNo = modelRecord.VOUCHER_NO;
                    //string depId = Convert.ToString(modelRecord.DEP_ID);
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
                            string query = "";
                            string Duplicationquery = "";
                            var partyInformation = partiesData.ToList().Where(p => p.customizedKey == modelRecord.PARTY_CODE).FirstOrDefault();
                            //var brokerInformation = partiesData.ToList().Where(p => p.customizedKey == modelRecord.SPARTY_CODE).FirstOrDefault();

                            if (partyInformation != null)
                            {
                                modelRecord.PARTY_CODE = Convert.ToString(partyInformation.key);
                                modelRecord.ACT_CODE = partyInformation.accountCode;
                            }

                            //if (brokerInformation != null)
                            //{
                            //    modelRecord.SPARTY_CODE = Convert.ToString(brokerInformation.key);
                            //    modelRecord.SACT_CODE = brokerInformation.accountCode;
                            //}
                            if (modelRecord.TRAN_ID == 0 || modelRecord.TRAN_ID is null)
                            {

                                code = GenerateNextId(common, command);
                                modelRecord.TRAN_ID = code;
                                voucherNo = GenerateVoucherNo(common, code, CommonService.GetDateTime("Pakistan Standard Time"));
                                //var (jobNo, depType) = GenerateJobNo(common, depId, command);

                                //if (String.IsNullOrWhiteSpace(jobNo) || String.IsNullOrWhiteSpace(depType))
                                //{
                                //    response.data = "";
                                //    response.msg = "Something wrong with Job No! please try another Department.";
                                //    response.msgType = 2;
                                //    return response;
                                //}

                                query = "INSERT INTO " + table + " " +
                                        "([TRAN_ID],[V_DATE],[VOUCHER_NO],[REF],[CLIENT_PO],[PARTY_CODE],[ACT_CODE],[REC_ON_DATE],[ITEM_CODE],[FABRIC]," +
                                        "[STYLE_DESC],[DOC],[BLEND],[GSM],[SIZE_RANGE],[DOC_PFR],[RATIO],[PROJ_QTY],[TARGET_PRICE],[REMARKS],[BCODE],[PERIOD_ID]," +
                                        "[ADD_USER_ID],[ADD_DATE],[ADD_COMPUTER_NAME],[ADD_IP_ADDRESS],[ADD_POSTALCODE]," +
                                        "[EDIT_USER_ID],[EDIT_DATE],[EDIT_COMPUTER_NAME],[EDIT_IP_ADDRESS],[EDIT_POSTALCODE],[ASTATUS],[MENU_ID],[DLT])" +
                                        "VALUES " +
                                        "('" + GenerateNextId(common) + "','" + modelRecord.V_DATE + "','" + voucherNo + "','" + modelRecord.REF + "','" + modelRecord.CLIENT_PO + "','" + modelRecord.PARTY_CODE + "'," +
                                        "'" + modelRecord.ACT_CODE + "','" + modelRecord.REC_ON_DATE + "','" + modelRecord.ITEM_CODE + "','" + modelRecord.FABRIC + "','" + modelRecord.STYLE_DESC + "'," +
                                        "'" + modelRecord.DOC + "','" + modelRecord.BLEND + "','" + modelRecord.GSM + "','" + modelRecord.SIZE_RANGE + "','" + modelRecord.DOC_PFR + "','" + modelRecord.RATIO + "'," +
                                        "'" + modelRecord.PROJ_QTY + "','" + modelRecord.TARGET_PRICE + "','" + modelRecord.REMARKS + "','" + common.Branch + "','" + common.Period + "','" + common.Username + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "'," +
                                        "'" + common.ComputerName + "','" + Ip + "','" + Postal + "','" + userid + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "','" + Computer + "','" + Ip + "','" + Postal + "'," +
                                        "'" + modelRecord.ASTATUS + "','" + common.MenuID + "','T')";

                                command.CommandText = query;
                                command.ExecuteNonQuery();
                                transaction.Commit();
                                response.data = code;
                                response.data2 = voucherNo;
                                response.msgType = 1;
                                response.msg = "Record Added Successfully";

                            }
                            else
                            {
                                //string checkQuery = $@"SELECT COUNT(*) FROM {table} WHERE CLIENT_PO = '{modelRecord.CLIENT_PO}' AND TRAN_ID != '{modelRecord.TRAN_ID}'";
                                //SqlCommand checkCommand = new SqlCommand(checkQuery, connection, transaction);
                                //int count = (int)checkCommand.ExecuteScalar();

                                //if (count > 0)
                                //{
                                //    response.msg = "Client PO already exists in another record!";
                                //    response.msgType = 2;
                                //    return response;
                                //}


                                query = "UPDATE " + table + " SET " +
                                        "[V_DATE] = '" + modelRecord.V_DATE + @"',
                                        [REF] = '" + modelRecord.REF + @"',
                                        [PARTY_CODE] = '" + modelRecord.PARTY_CODE + @"',
                                        [ACT_CODE] = '" + modelRecord.ACT_CODE + @"',
                                        [REC_ON_DATE] = '" + modelRecord.REC_ON_DATE + @"',
                                        [ITEM_CODE] = '" + modelRecord.ITEM_CODE + @"',
                                        [FABRIC] = '" + modelRecord.FABRIC + @"',
                                        [STYLE_DESC] = '" + modelRecord.STYLE_DESC + @"',
                                        [DOC] = '" + modelRecord.DOC + @"',
                                        [BLEND] = '" + modelRecord.BLEND + @"',
                                        [GSM] = '" + modelRecord.GSM + @"',
                                        [SIZE_RANGE] = '" + modelRecord.SIZE_RANGE + @"',
                                        [DOC_PFR] = '" + modelRecord.DOC_PFR + @"',
                                        [RATIO] = '" + modelRecord.RATIO + @"',
                                        [PROJ_QTY] = '" + modelRecord.PROJ_QTY + @"',
                                        [TARGET_PRICE] = '" + modelRecord.TARGET_PRICE + @"',
                                        [REMARKS] = '" + modelRecord.REMARKS + @"',
                                        [BCODE] = '" + common.Branch + @"',
                                        [PERIOD_ID] = '" + common.Period + @"',
		                                [EDIT_USER_ID] = '" + userid + @"',
		                                [EDIT_DATE] = '" + CommonService.GetDateTime("Pakistan Standard Time") + @"',
		                                [EDIT_COMPUTER_NAME] = '" + Computer + @"',
		                                [EDIT_IP_ADDRESS] = '" + Ip + @"',
		                                [MENU_ID] = '" + common.MenuID + @"',
		                                [EDIT_POSTALCODE] = '" + Postal + @"',
		                                [ASTATUS] = '" + modelRecord.ASTATUS + @"',
		                                [DLT] = 'T' 
                                    WHERE [TRAN_ID] = '" + modelRecord.TRAN_ID + @"'";

                                command.CommandText = query;
                                command.ExecuteNonQuery();
                                transaction.Commit();
                                response.data = modelRecord.TRAN_ID;
                                response.data2 = voucherNo;
                                response.msgType = 1;
                                response.msg = "Record Updated Successfully";
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

        public string GenerateNextId(Core.Entities.Common common)
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
                    string maxIdQuery = "SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM " + table + "";
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

        public MyHttpResponseMessage GetSampleDevAndPricingById(int id, Core.Entities.Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                object json = null;
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = "SELECT * FROM " + table + " WHERE TRAN_ID = @Id";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Id", id);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            var jsonDataResult = new
                            {
                                TRAN_ID = reader["TRAN_ID"].ToString(),
                                V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                VOUCHER_NO = reader["VOUCHER_NO"].ToString(),
                                REF = reader["REF"] == DBNull.Value ? "" : Convert.ToString(reader["REF"]),
                                CLIENT_PO = reader["CLIENT_PO"] == DBNull.Value ? "" : Convert.ToString(reader["CLIENT_PO"]),
                                PARTY_CODE = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                                ACT_CODE = reader["ACT_CODE"] == DBNull.Value ? "" : Convert.ToString(reader["ACT_CODE"]),
                                REC_ON_DATE = reader["REC_ON_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["REC_ON_DATE"]).ToString("yyyy-MM-dd"),
                                ITEM_CODE = reader["ITEM_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ITEM_CODE"]),
                                FABRIC = reader["FABRIC"] == DBNull.Value ? 0 : Convert.ToInt32(reader["FABRIC"]),

                                STYLE_DESC = reader["STYLE_DESC"] == DBNull.Value ? "" : Convert.ToString(reader["STYLE_DESC"]),
                                DOC = reader["DOC"] == DBNull.Value ? "" : Convert.ToString(reader["DOC"]),
                                BLEND = reader["BLEND"] == DBNull.Value ? "" : Convert.ToString(reader["BLEND"]),
                                GSM = reader["GSM"] == DBNull.Value ? 0 : Convert.ToInt32(reader["GSM"]),
                                SIZE_RANGE = reader["SIZE_RANGE"] == DBNull.Value ? "" : Convert.ToString(reader["SIZE_RANGE"]),
                                DOC_PFR = reader["DOC_PFR"] == DBNull.Value ? "" : Convert.ToString(reader["DOC_PFR"]),
                                RATIO = reader["RATIO"] == DBNull.Value ? "" : Convert.ToString(reader["RATIO"]),
                                PROJ_QTY = reader["PROJ_QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PROJ_QTY"]),
                                TARGET_PRICE = reader["TARGET_PRICE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TARGET_PRICE"]),
                                REMARKS = reader["REMARKS"] == DBNull.Value ? "" : Convert.ToString(reader["REMARKS"]),
                                ASTATUS = reader["ASTATUS"] == DBNull.Value ? "" : Convert.ToString(reader["ASTATUS"]),

                            };
                            json = jsonDataResult;
                        }
                        reader.Close();
                    }

                    response.msg = "";
                    response.msgType = 1;
                    response.data = json;
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

        public MyHttpResponseMessage Delete(int id, Core.Entities.Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msg = "Data not found in our records";
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
                    if (id == 0)
                    {
                        response.msg = "ID is not in numeric format";
                        response.msgType = 2;
                    }
                    else
                    {
                        string connectionString = new SQLService().getconnstring();
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = $@"UPDATE {table} SET DLT = 'F' WHERE TRAN_ID = '{id}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                            SqlCommand command = new SqlCommand(query, connection);
                            command.ExecuteNonQuery();
                            if (query != null)
                            {
                                response.msg = "Record Deleted Successfully";
                                response.msgType = 1;
                            }
                        }
                    }
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
                SampleDevAndPricing_OLD DeliveryFormat = new SampleDevAndPricing_OLD();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT * FROM {table} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    //string detailQuery = $@"SELECT * FROM {table2} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        DeliveryFormat = new SampleDevAndPricing_OLD
                        {
                            //TRAN_ID = 0,
                            //V_DATE = record.V_DATE,
                            //PARTY_CODE = Convert.ToString(reader["PARTY_CODE"]),
                            //ACT_CODE = Convert.ToInt32(reader["ACT_CODE"]),
                            //S_DATE = Convert.ToDateTime(reader["S_DATE"]),
                            //REF = Convert.ToString(reader["REF"]),
                            //BROKER_CODE = Convert.ToString(reader["BROKER_CODE"]),
                            //BD_ACT_CODE = Convert.ToInt32(reader["BD_ACT_CODE"]),
                            //GODOWN = Convert.ToString(reader["GODOWN"]),
                            //KANTA = Convert.ToString(reader["KANTA"]),
                            //COND = Convert.ToString(reader["COND"]),
                            //C_NAME = Convert.ToString(reader["C_NAME"]),
                            //CELL = Convert.ToString(reader["CELL"]),
                            //LOT_NO = Convert.ToString(reader["LOT_NO"]),
                            //ORIGIN = Convert.ToString(reader["ORIGIN"]),
                            //ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                            //TBAG = Convert.ToDouble(reader["TBAG"]),
                            //ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            //EMP = Convert.ToInt32(reader["UNIT"]),
                        };
                    }
                    reader.Close();
                    connection.Close();
                }
                response = this.Save(DeliveryFormat, common);
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

        public MyHttpResponseMessage GetDataForReport(SampleDevAndPricingReport modelRecord, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            SampleDevAndPricingReport masterData = new SampleDevAndPricingReport();
            var Menu = _menuRepository.GetMenu(common.MenuID);
            string? table = string.Empty, detailTable = string.Empty;
            if (Menu.data != null)
            {
                var menu = (Menu)Menu.data;
                table = menu.TABLE1;
            }
            try
            {
                string topQuery = "";

                topQuery = $@"SELECT MPO.TRAN_ID, MPO.V_DATE, MPO.VOUCHER_NO, MPO.REF, MPO.CLIENT_PO, PTC.PARTY_NAME AS CLIENT_NAME, D.DESCR AS DEP,
                                    PTS.PARTY_NAME AS SUPPLIER_NAME, MPO.JOB_NO, EMP.ENAME AS EMP_NAME, PT.GROUP_NAME AS TERMS_NAME, C.DESCR AS CURR, C.RATE AS CURR_RATE,
                                    IT.ITEM_NAME,  G.GROUP_NAME AS BRAND, MPO.QTY, U.GROUP_NAME AS UNIT_NAME, MPO.RATE, MPO.AMT,
                                    CASE WHEN MPO.COMM_AMT = 'PR' THEN 'Percent' WHEN MPO.COMM_AMT = 'RS' THEN 'Value' ELSE '' END COMM_UNIT, MPO.COMM, COMM_VAL, MPO.REMARKS
                                    FROM {table} MPO
                                    LEFT OUTER JOIN TBL_PARTY_TYPES PTC ON MPO.PARTY_CODE = PTC.PARTY_CODE AND PTC.ACT_CODE = MPO.ACT_CODE
                                    LEFT OUTER JOIN TBL_PARTY_TYPES PTS ON MPO.SPARTY_CODE = PTS.PARTY_CODE AND PTS.ACT_CODE = MPO.SACT_CODE
                                    LEFT OUTER JOIN TBL_ACT_GROUP D ON MPO.DEP_ID = D.CODE
                                    LEFT OUTER JOIN TBL_EMP_REG EMP ON MPO.EMP_ID = EMP.EMP_CODE
                                    LEFT OUTER JOIN TBL_PAY_TERMS PT ON MPO.TERMS = PT.GROUP_CODE
                                    LEFT OUTER JOIN TBL_CURRENCY C ON MPO.CURR_CODE = C.CODE
                                    LEFT OUTER JOIN TBL_ITEMSMASTER IT ON MPO.ITEM_CODE = IT.ITEM_CODE
                                    LEFT OUTER JOIN TBL_GRADE G ON MPO.GRADE = G.GROUP_CODE
                                    LEFT OUTER JOIN TBL_UNIT U ON MPO.UNIT = U.GROUP_CODE
                                    WHERE TRAN_ID = '{modelRecord.TRAN_ID}' AND MPO.BCODE = '{common.Branch}' AND MPO.PERIOD_ID = '{common.Period}'";

                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    SqlCommand command = new SqlCommand(topQuery, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        masterData.COMPANY_NAME = currentCompany.C_NAME;
                        masterData.COMPANY_ADDRESS = currentCompany.C_ADDRESS;
                        masterData.COMPANY_PHONE = currentCompany.C_TEL;
                        masterData.COMPANY_LOGO = currentCompany.C_LOGO;
                        masterData.HEADER_NAME = $"{menuDetails.MD_NAME}";
                        masterData.REPORT_NAME = $"{menuDetails.REPORT_NAME}";
                        masterData.USER = common.Username;
                        masterData.SIG1 = $"{menuDetails.MENU_SIG1}";
                        masterData.SIG2 = $"{menuDetails.MENU_SIG2}";
                        masterData.SIG3 = $"{menuDetails.MENU_SIG3}";
                        masterData.SIG4 = $"{menuDetails.MENU_SIG4}";
                        masterData.V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                        masterData.VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? string.Empty : reader["VOUCHER_NO"].ToString();
                        masterData.REF = reader["REF"] == DBNull.Value ? string.Empty : reader["REF"].ToString();
                        masterData.CLIENT_PO = reader["CLIENT_PO"] == DBNull.Value ? string.Empty : reader["CLIENT_PO"].ToString();
                        masterData.CLIENT_NAME = reader["CLIENT_NAME"] == DBNull.Value ? string.Empty : reader["CLIENT_NAME"].ToString();
                        masterData.DEP = reader["DEP"] == DBNull.Value ? string.Empty : reader["DEP"].ToString();
                        masterData.SUPPLIER_NAME = reader["SUPPLIER_NAME"] == DBNull.Value ? string.Empty : reader["SUPPLIER_NAME"].ToString();
                        masterData.JOB_NO = reader["JOB_NO"] == DBNull.Value ? string.Empty : reader["JOB_NO"].ToString();
                        masterData.EMP_NAME = reader["EMP_NAME"] == DBNull.Value ? string.Empty : reader["EMP_NAME"].ToString();
                        masterData.TERMS_NAME = reader["TERMS_NAME"] == DBNull.Value ? string.Empty : reader["TERMS_NAME"].ToString();
                        masterData.CURR = reader["CURR"] == DBNull.Value ? string.Empty : reader["CURR"].ToString();
                        masterData.ITEM_NAME = reader["ITEM_NAME"] == DBNull.Value ? string.Empty : reader["ITEM_NAME"].ToString();
                        masterData.UNIT_NAME = reader["UNIT_NAME"] == DBNull.Value ? string.Empty : reader["UNIT_NAME"].ToString();
                        masterData.COMM_UNIT = reader["COMM_UNIT"] == DBNull.Value ? string.Empty : reader["COMM_UNIT"].ToString();
                        masterData.CURR_RATE = reader["CURR_RATE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CURR_RATE"]);
                        masterData.QTY = reader["QTY"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["QTY"]);
                        masterData.RATE = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["RATE"]);
                        masterData.AMT = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AMT"]);
                        masterData.COMM = reader["COMM"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["COMM"]);
                        masterData.COMM_VAL = reader["COMM_VAL"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["COMM_VAL"]);
                        masterData.BRAND = reader["BRAND"] == DBNull.Value ? string.Empty : reader["BRAND"].ToString();
                        masterData.REMARKS = reader["REMARKS"] == DBNull.Value ? string.Empty : reader["REMARKS"].ToString();

                    }
                    reader.Close();
                }

                response.data = masterData;
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