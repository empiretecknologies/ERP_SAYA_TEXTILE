using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Text;
using ZXing;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class SampleDevAndPricingRepository : ISampleDevAndPricingRepository
    {
        public IMenuRepository _menuRepository { get; set; }
        public ICommonRepository _commonRepository { get; set; }
        public ICommonService _commonService { get; set; }
        public IBranchRepository _branchRepository { get; set; }
        public IPeriodRepository _periodRepository { get; set; }

        public SampleDevAndPricingRepository(IMenuRepository menuRepository, IBranchRepository branchRepository, ICommonRepository commonRepository, ICommonService commonService, IPeriodRepository periodRepository)
        {
            _menuRepository = menuRepository;
            _branchRepository = branchRepository;
            _commonRepository = commonRepository;
            _commonService = commonService;
            _periodRepository = periodRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                string? detailTable = string.Empty;
                string? pickMaster = string.Empty;
                string? pickDetail = string.Empty;
                string? search = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                    pickMaster = menu.PICK_TABLE_MASTER;
                    pickDetail = menu.PICK_TABLE_DETAIL;
                    search = menu.SEARCH;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    List<object> jsonDataResult = new List<object>();

                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        if (search == "M")
                        {

                            string query = $@"SELECT M.TRAN_ID, 
                                                CASE WHEN M.ASTATUS = 'Y' THEN 'Active' ELSE 'In-Active' END AS ASTATUS,
                                                M.V_DATE, M.VOUCHER_NO, M.REF, 
                                                PT.PARTY_NAME, M.ARTICLE_DOC, M.REMARKS
                                                FROM {table} M 
                                                LEFT OUTER JOIN TBL_PARTY_TYPES PT ON PT.PARTY_CODE = M.PARTY_CODE AND PT.ACT_CODE = M.ACT_CODE 
                                                WHERE  M.DLT = 'T' AND M.BCODE = '1' AND M.PERIOD_ID = '1'
                                                ORDER BY M.TRAN_ID DESC";

                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    CODE = Convert.ToInt32(reader["TRAN_ID"]),
                                    ASTATUS = Convert.ToString(reader["ASTATUS"]),
                                    V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                    VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                                    REF = Convert.ToString(reader["REF"]),
                                    PARTY_NAME = reader["PARTY_NAME"],
                                    REMARKS = Convert.ToString(reader["REMARKS"]),
                                };
                                jsonDataResult.Add(row);
                            }
                            reader.Close();


                        }
                        else
                        {
                            var query = $@"SELECT M.TRAN_ID AS TRAN_ID, M.TRAN_ID AS CODE,M.V_DATE,M.VOUCHER_NO,M.BTYPE,M.COMM,M.PARTY_CODE,M.ACT_CODE,M.REF,D.DT_DESC,
                                            ROUND(SUM(D.NET_AMT),0) AS AMT,
                                            M.SCODE,M.SACODE,M.BCODE,M.PERIOD_ID,
                                            M.ADD_USER_ID,M.ADD_DATE,M.ADD_COMPUTER_NAME,M.ADD_IP_ADDRESS,M.EDIT_USER_ID,M.EDIT_DATE,
                                            M.EDIT_COMPUTER_NAME,M.EDIT_IP_ADDRESS,PT.PARTY_NAME,M.ADD_POSTALCODE,M.EDIT_POSTALCODE, 
                                            CASE WHEN M.ASTATUS = 'Y' THEN 'Active' ELSE 'In-Active' END AS ASTATUS 
                                            FROM {table} M 
                                            LEFT OUTER JOIN TBL_PARTY_TYPES PT ON PT.PARTY_CODE = M.PARTY_CODE AND PT.ACT_CODE = M.ACT_CODE 
                                            LEFT OUTER JOIN {detailTable} D
                                            ON D.TRAN_ID = M.TRAN_ID AND D.PERIOD_ID = M.PERIOD_ID AND D.BCODE = M.BCODE
                                            WHERE  M.DLT = 'T' AND M.BCODE = {common.Branch} AND M.PERIOD_ID = {common.Period} AND  D.DLT = 'T'
                                            GROUP BY 
                                            M.TRAN_ID  , M.TRAN_ID  ,M.V_DATE,M.VOUCHER_NO,M.BTYPE,M.COMM,M.PARTY_CODE,M.ACT_CODE,M.REF,D.DT_DESC,
                                            M.SCODE,M.SACODE,M.BCODE,M.PERIOD_ID,
                                            M.ADD_USER_ID,M.ADD_DATE,M.ADD_COMPUTER_NAME,M.ADD_IP_ADDRESS,M.EDIT_USER_ID,M.EDIT_DATE,
                                            M.EDIT_COMPUTER_NAME,M.EDIT_IP_ADDRESS,PT.PARTY_NAME,M.ADD_POSTALCODE,M.EDIT_POSTALCODE, M.ASTATUS 
                                            ORDER BY M.TRAN_ID DESC";
                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    ID = Convert.ToInt32(reader["TRAN_ID"]),
                                    CODE = Convert.ToInt32(reader["CODE"]),
                                    ASTATUS = Convert.ToString(reader["ASTATUS"]),
                                    V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                    VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                                    PARTY_CODE = reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PARTY_CODE"]),
                                    ACT_CODE = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),
                                    REF = Convert.ToString(reader["REF"]),
                                    REMARKS = Convert.ToString(reader["DT_DESC"]),
                                    AMT = Convert.ToString(reader["AMT"]),
                                    BTYPE = Convert.ToString(reader["BTYPE"]),
                                    SCODE = reader["SCODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SCODE"]),
                                    SACODE = reader["SACODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SACODE"]),
                                    ADD_USER_ID = Convert.ToString(reader["ADD_USER_ID"]),
                                    ADD_DATE = reader["ADD_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["ADD_DATE"]).ToString("yyyy-MM-dd"),
                                    ADD_COMPUTER_NAME = Convert.ToString(reader["ADD_COMPUTER_NAME"]),
                                    ADD_IP_ADDRESS = Convert.ToString(reader["ADD_IP_ADDRESS"]),
                                    EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]),
                                    EDIT_DATE = reader["EDIT_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["EDIT_DATE"]).ToString("yyyy-MM-dd"),
                                    EDIT_COMPUTER_NAME = Convert.ToString(reader["EDIT_COMPUTER_NAME"]),
                                    EDIT_IP_ADDRESS = Convert.ToString(reader["EDIT_IP_ADDRESS"]),
                                    PARTY_NAME = reader["PARTY_NAME"],
                                    ADD_POSTALCODE = Convert.ToString(reader["ADD_POSTALCODE"]),
                                    EDIT_POSTALCODE = Convert.ToString(reader["EDIT_POSTALCODE"]),
                                    COMM = Convert.ToInt32(reader["COMM"]),
                                };
                                jsonDataResult.Add(row);
                            }
                            reader.Close();
                        }
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


        public MyHttpResponseMessage GetSampleDevAndPricingByCode(int code, Common common)
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
                        string query = $@"SELECT * FROM {table} WHERE DLT = 'T' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}' AND TRAN_ID = '{code}'";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                ID = Convert.ToString(reader["TRAN_ID"]),

                                V_DATE = reader["V_DATE"] == DBNull.Value
                                         ? null
                                         : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),

                                VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                                REF = Convert.ToString(reader["REF"]),

                                REC_ON_DATE = reader["REC_ON_DATE"] == DBNull.Value ||
                                              Convert.ToDateTime(reader["REC_ON_DATE"]).Date == new DateTime(1900, 1, 1)
                                              ? null
                                              : Convert.ToDateTime(reader["REC_ON_DATE"]).ToString("yyyy-MM-dd"),

                                CLIENT_PO = Convert.ToString(reader["CLIENT_PO"]),

                                PARTY_CODE = reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PARTY_CODE"]),
                                ACT_CODE = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),

                                BPARTY_CODE = reader["BPARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BPARTY_CODE"]),
                                BACT_CODE = reader["BACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BACT_CODE"]),

                                DEPARTMENT = reader["DEPARTMENT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEPARTMENT"]),
                                CURR_CODE = reader["CURR_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CURR_CODE"]),
                                SEASON = reader["SEASON"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SEASON"]),

                                INTAKE = Convert.ToString(reader["INTAKE"]),
                                JOB_NO = Convert.ToString(reader["JOB_NO"]),

                                REMARKS = Convert.ToString(reader["REMARKS"]),
                                ARTICLE_DOC = Convert.ToString(reader["ARTICLE_DOC"]),
                                PFR_DOC = Convert.ToString(reader["PFR_DOC"]),
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

        public MyHttpResponseMessage GetPickDataBySupplier(int pCode, int actCode, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                string? detailTable = string.Empty;
                string? pickTable = string.Empty;
                string? pickDetailTable = string.Empty;
                string? pickType = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                    pickTable = menu.PICK_TABLE_MASTER;
                    pickDetailTable = menu.PICK_TABLE_DETAIL;
                    pickType = menu.PICK_TYPE;
                }


                if (pickType == "MPO")
                {
                    if (!String.IsNullOrWhiteSpace(table))
                    {
                        List<object> jsonDataResult = new List<object>();
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {

                            string query = $@"SELECT A.TRAN_ID, A.V_DATE, A.VOUCHER_NO, P.PARTY_NAME AS CLIENT_NAME, S.PARTY_NAME AS SUPP_NAME, MPO.CLIENT_PO,
                                                MPO.CURR_CODE, MPO.JOB_NO, I.ITEM_NAME, U.GROUP_NAME AS UNIT_NAME, B.RATE, 
                                                SUM((ISNULL(B.QTY, 0))) AS QTY,
                                                SUM((ISNULL(B.AMT, 0))) AS AMT,
                                                MPO.COMM_AMT AS COMM_TYPE,
                                                MPO.COMM  AS COMM_RATE,
                                                SUM(ISNULL(MPO.COMM*B.AMT/100,0)-ISNULL(TSB.COMM_AMT,0)) AS COMM_AMT, 
                                                B.DT_CODE, MPO.PARTY_CODE, MPO.ACT_CODE, MPO.ITEM_CODE, MPO.SPARTY_CODE, MPO.SACT_CODE, B.DOC, MPO.UNIT,
                                                MPO.CRATE, C.DESCR, MB.ID AS MENU_ID, MB.MENU_PAGE, MB.MENU_PARENT_CODE 
                                                FROM {pickTable} A
                                                LEFT OUTER JOIN {pickDetailTable} B ON B.TRAN_ID = A.TRAN_ID AND B.BCODE = A.BCODE AND B.PERIOD_ID = A.PERIOD_ID
                                                LEFT OUTER JOIN {detailTable} TSB ON TSB.PICK_ID = B.DT_CODE AND TSB.BCODE = A.BCODE AND TSB.PERIOD_ID = A.PERIOD_ID
                                                LEFT OUTER JOIN TBL_MPO_MASTER MPO ON MPO.TRAN_ID = A.JOB_NO AND MPO.BCODE = A.BCODE AND MPO.PERIOD_ID = A.PERIOD_ID
                                                LEFT OUTER JOIN TBL_PARTY_TYPES P ON P.PARTY_CODE = MPO.PARTY_CODE AND P.ACT_CODE = MPO.ACT_CODE
                                                LEFT OUTER JOIN TBL_PARTY_TYPES S ON S.PARTY_CODE = A.SPARTY_CODE AND S.ACT_CODE = A.SACT_CODE
                                                LEFT OUTER JOIN TBL_UNIT U ON U.GROUP_CODE = MPO.UNIT
                                                LEFT OUTER JOIN TBL_ITEMSMASTER I ON I.ITEM_CODE = MPO.ITEM_CODE
                                                LEFT OUTER JOIN TBL_CURRENCY C ON C.CODE = MPO.CURR_CODE
                                                LEFT OUTER JOIN TBL_MENU_BUILDER MB ON MB.ID = A.MENU_ID 
                                                WHERE S.PARTY_CODE = {pCode} AND S.ACT_CODE = {actCode} AND A.BCODE = {common.Branch} And A.PERIOD_ID = {common.Period} AND A.DLT = 'T' AND B.DLT = 'T' AND A.ASTATUS = 'Y'
                                                GROUP BY A.TRAN_ID, A.V_DATE, A.VOUCHER_NO, P.PARTY_NAME, S.PARTY_NAME , MPO.CLIENT_PO, MPO.CURR_CODE,
                                                MPO.JOB_NO, I.ITEM_NAME, U.GROUP_NAME, B.RATE,MPO.COMM_AMT , B.DT_CODE,MPO.COMM, MPO.PARTY_CODE, MPO.ACT_CODE, MPO.ITEM_CODE, MPO.SPARTY_CODE, MPO.SACT_CODE, B.DOC, MPO.UNIT,
                                                MPO.CRATE, C.DESCR, MB.ID, MB.MENU_PAGE, MB.MENU_PARENT_CODE 
                                                HAVING SUM(ISNULL(MPO.COMM*B.AMT/100,0)-ISNULL(TSB.COMM_AMT,0)) <> 0";



                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    ID = Convert.ToString(reader["TRAN_ID"]),
                                    LB_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy"),
                                    VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? "" : Convert.ToString(reader["VOUCHER_NO"]),
                                    PARTY_NAME = reader["CLIENT_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["CLIENT_NAME"]),
                                    SUP_NAME = reader["SUPP_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["SUPP_NAME"]),
                                    CLIENT_PO = reader["CLIENT_PO"] == DBNull.Value ? "" : Convert.ToString(reader["CLIENT_PO"]),
                                    JOB_NO = reader["JOB_NO"] == DBNull.Value ? "" : Convert.ToString(reader["JOB_NO"]),
                                    ITEM_NAME = reader["ITEM_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM_NAME"]),
                                    UNIT_NAME = reader["UNIT"] == DBNull.Value ? "" : Convert.ToString(reader["UNIT"]),
                                    RATE = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RATE"]),
                                    QTY = reader["QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QTY"]),
                                    AMT = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AMT"]),
                                    CRATE = reader["CRATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CRATE"]),
                                    DESCR = reader["DESCR"] == DBNull.Value ? "" : Convert.ToString(reader["DESCR"]),
                                    COMM_TYPE = reader["COMM_TYPE"] == DBNull.Value ? "" : Convert.ToString(reader["COMM_TYPE"]),
                                    COMM_RATE = reader["COMM_RATE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["COMM_RATE"]),
                                    //COMM_AMT = reader["COMM_AMT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COMM_AMT"]),
                                    PICK_ID_D = reader["DT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DT_CODE"]),
                                    PARTY_CODE = reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PARTY_CODE"]),
                                    ACT_CODE = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),
                                    SPARTY_CODE = reader["SPARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SPARTY_CODE"]),
                                    SACT_CODE = reader["SACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SACT_CODE"]),
                                    ITEM_CODE = reader["ITEM_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ITEM_CODE"]),
                                    CURR_CODE = reader["CURR_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CURR_CODE"]),
                                    DOC = reader["DOC"] == DBNull.Value ? "" : Convert.ToString(reader["DOC"]),
                                    PARTY_DDL = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                                    UNIT = reader["UNIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["UNIT"]),
                                    LINK = "/" + Convert.ToString(reader["MENU_PAGE"]) + "?MOID=" + Convert.ToString(reader["MENU_PARENT_CODE"]) + "&Code=" + Convert.ToString(reader["MENU_ID"]),

                                    //PARTY_NAME = reader["PARTY_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["PARTY_NAME"]),
                                    //SUP_NAME = reader["SUP_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["SUP_NAME"]),
                                    //REF = reader["REF"] == DBNull.Value ? "" : Convert.ToString(reader["REF"]),
                                }
                            ;
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
                else
                {
                    response.data = "";
                    response.msg = "Pick data is not mapped. Please contact the administrator.";
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

        public MyHttpResponseMessage GetBarcodeList()
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = $@"SELECT IM.ITEM_NAME AS ITEM_ID, IM.ITEM_CODE AS ITEM_CODE, C.GROUP_CODE AS COLOR_ID, S.GROUP_CODE AS SIZE_ID,B.CODE AS BARCODE_CODE,S.GROUP_NAME AS SIZE, C.GROUP_NAME AS COLOR, B.BARCODE, B.SRATE AS RATE FROM TBL_BARCODE B
                                    LEFT OUTER JOIN TBL_ITEMSMASTER IM
                                    ON IM.ITEM_CODE = B.ITEM_CODE
                                    LEFT OUTER JOIN TBL_SIZE S
                                    ON S.GROUP_CODE = B.SIZE
                                    LEFT OUTER JOIN TBL_COLOR C
                                    ON C.GROUP_CODE = B.COLOR
                                    WHERE B.DLT = 'T' AND IM.ASTATUS = 'Y' AND IM.ASTATUS = 'Y'
                                    ORDER BY ITEM_ID , SIZE";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            BARCODE_CODE = Convert.ToString(reader["BARCODE_CODE"]),
                            ITEM_CODE = Convert.ToString(reader["ITEM_CODE"]),
                            ITEM_ID = Convert.ToString(reader["ITEM_ID"]),
                            SIZE_NAME = Convert.ToString(reader["SIZE"]),
                            COLOR_NAME = Convert.ToString(reader["COLOR"]),
                            BARCODE = Convert.ToString(reader["BARCODE"]),
                            RATE = Convert.ToInt32(reader["RATE"]),
                            AMT = Convert.ToInt32(reader["RATE"]),
                            NET_AMT = Convert.ToInt32(reader["RATE"]),
                            COLOR = Convert.ToInt32(reader["COLOR_ID"]),
                            SIZE = Convert.ToInt32(reader["SIZE_ID"]),
                            QTY = 1,
                            BAL_QTY = 1,
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

        public MyHttpResponseMessage GetSampleDevAndPricingPickDetailByCode(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            //try
            //{
            //    var Menu = _menuRepository.GetMenu(common.MenuID);
            //    string? table = string.Empty;
            //    if (Menu.data != null)
            //    {
            //        var menu = (Menu)Menu.data;
            //        table = menu.PICK_TABLE_DETAIL;
            //    }

            //    if (!String.IsNullOrWhiteSpace(table))
            //    {
            //        List<object> jsonDataResult = new List<object>();
            //        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            //        {
            //            string query = "SELECT TRAN_ID,DT_CODE,M.ITEM_CODE, QTY,UNIT,QTY2,BAL_QTY,M.COLOR,M.SIZE, " +
            //             " RATE,AMT,DISC,DISC_AMT, TAX,TAX_AMT,ADV,ADV_AMT,NET_AMT, DT_DESC, " +
            //             " CL.GROUP_NAME AS COLORNAME,SL.GROUP_NAME AS SIZENAME,M.GRADE, WAREHOUSE,DEL_DATE,DUE_DATE, DUE_DAYS,VEH,BCODE,PERIOD_ID, M.ADD_USER_ID,M.ADD_DATE," +
            //             " M.ADD_COMPUTER_NAME,M.ADD_IP_ADDRESS, M.EDIT_USER_ID,M.EDIT_DATE,M.EDIT_COMPUTER_NAME, " +
            //             " M.EDIT_IP_ADDRESS, M.ADD_POSTALCODE,M.EDIT_POSTALCODE,M.MENU_ID,M.DLT,CHK,PICK_ID,PICK_ID_D " +
            //             " FROM {table} M LEFT OUTER JOIN TBL_BARCODE BG ON BG.CODE = M.ITEM_CODE " +
            //             " LEFT OUTER JOIN TBL_SIZE SL ON SL.GROUP_CODE = BG.SIZE LEFT OUTER JOIN TBL_COLOR CL ON CL.GROUP_CODE = BG.COLOR" +
            //             " WHERE  M.DLT = 'T' AND TRAN_ID = '" + code + "' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + "'" +
            //             " ORDER BY DT_CODE DESC";
            //            SqlCommand command = new SqlCommand(query, connection);
            //            connection.Open();
            //            SqlDataReader reader = command.ExecuteReader();
            //            while (reader.Read())
            //            {
            //                var row = new
            //                {
            //                    DT_CODE = 0,
            //                    ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
            //                    QTY = Convert.ToString(reader["QTY"]),
            //                    UNIT = Convert.ToInt32(reader["UNIT"]),
            //                    QTY2 = Convert.ToString(reader["QTY2"]),
            //                    BAL_QTY = Convert.ToString(reader["BAL_QTY"]),
            //                    RATE = Convert.ToString(reader["RATE"]),
            //                    AMT = Convert.ToString(reader["AMT"]),
            //                    DISC = Convert.ToString(reader["DISC"]),
            //                    DISC_AMT = Convert.ToString(reader["DISC_AMT"]),
            //                    TAX = Convert.ToString(reader["TAX"]),
            //                    TAX_AMT = Convert.ToString(reader["TAX_AMT"]),
            //                    ADV = Convert.ToString(reader["ADV"]),
            //                    ADV_AMT = Convert.ToString(reader["ADV_AMT"]),
            //                    NET_AMT = Convert.ToString(reader["NET_AMT"]),
            //                    DT_DESC = Convert.ToString(reader["DT_DESC"]),
            //                    COLOR = reader["COLOR"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COLOR"]),
            //                    SIZE = reader["SIZE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE"]),
            //                    GRADE = Convert.ToInt32(reader["GRADE"]),
            //                    WAREHOUSE = Convert.ToInt32(reader["WAREHOUSE"]),
            //                    DEL_DATE = Convert.ToString(reader["DEL_DATE"]),
            //                    DUE_DATE = Convert.ToString(reader["DUE_DATE"]),
            //                    DUE_DAYS = Convert.ToString(reader["DUE_DAYS"]),
            //                    VEH = Convert.ToString(reader["VEH"]),
            //                    CHK = Convert.ToString(reader["CHK"]),
            //                    CHK1 = Convert.ToString(reader["CHK"]) == "1" ? true : false,
            //                    PICK_ID = Convert.ToInt32(reader["TRAN_ID"]),
            //                    PICK_ID_D = Convert.ToInt32(reader["DT_CODE"])

            //                };
            //                jsonDataResult.Add(row);
            //            }
            //            reader.Close();
            //        }

            //        response.data = jsonDataResult;
            //        response.msg = "";
            //        response.msgType = 1;
            //    }
            //    else
            //    {
            //        response.data = "";
            //        response.msg = "Something went wrong! please try again later.";
            //        response.msgType = 2;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string _catchMessage = ex.Message;
            //    if (ex.InnerException != null)
            //    {
            //        _catchMessage += "<br/>" + ex.InnerException.Message;
            //    }
            //    response.msg = _catchMessage;
            //    response.msgType = 2;
            //}
            return response;
        }

        public MyHttpResponseMessage GetSampleDevAndPricingDetailByCode(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty, detailTable = string.Empty;
                string? pickMaster = string.Empty, pickDetail = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                    pickMaster = menu.PICK_TABLE_MASTER;
                    pickDetail = menu.PICK_TABLE_DETAIL;
                }

                if (!String.IsNullOrWhiteSpace(detailTable))
                {
                    List<object> jsonDataResult = new List<object>();
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = $@" SELECT D.DT_CODE, D.ITEM_CODE, D.FABRIC, D.STYLE, D.IMAGE, D.QUALITY, D.GRADE, D.GSM, D.SIZE, D.RATIO, D.PROJ_QTY, D.TARGET_PRICE, D.DT_DESC
                                             FROM {detailTable} D
                                             WHERE D.DLT = 'T' AND D.TRAN_ID = '{code}' AND D.BCODE = '{common.Branch}' AND D.PERIOD_ID = '{common.Period}' ORDER BY D.DT_CODE DESC";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                DT_CODE = reader["DT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DT_CODE"]),
                                ITEM_CODE = reader["ITEM_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ITEM_CODE"]),
                                FABRIC = reader["FABRIC"] == DBNull.Value ? 0 : Convert.ToInt32(reader["FABRIC"]),
                                STYLE = reader["STYLE"] == DBNull.Value ? "" : Convert.ToString(reader["STYLE"]),
                                IMAGE = reader["IMAGE"] == DBNull.Value ? "" : Convert.ToString(reader["IMAGE"]),
                                QUALITY = reader["QUALITY"] == DBNull.Value ? "" : Convert.ToString(reader["QUALITY"]),
                                GRADE = reader["GRADE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["GRADE"]),
                                GSM = reader["GSM"] == DBNull.Value ? 0 : Convert.ToInt32(reader["GSM"]),
                                SIZE = reader["SIZE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE"]),
                                RATIO = reader["RATIO"] == DBNull.Value ? "" : Convert.ToString(reader["RATIO"]),
                                PROJ_QTY = reader["PROJ_QTY"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PROJ_QTY"]),
                                TARGET_PRICE = reader["TARGET_PRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TARGET_PRICE"]),
                                DT_DESC = reader["DT_DESC"] == DBNull.Value ? "" : Convert.ToString(reader["DT_DESC"]),

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

        public MyHttpResponseMessage GetSampleDevAndPricingDetailByItem(int code, int qty, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE2;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    List<object> jsonDataResult = new List<object>();
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = "SELECT B.CODE, B.SRATE, B.COLOR, B.SIZE, IT.ITEM_ID, IT.SALE_RATE FROM TBL_BARCODE B " +
                            $"LEFT OUTER JOIN TBL_ITEMSMASTER IT ON B.ITEM_CODE = IT.ITEM_CODE " +
                            "WHERE  B.DLT = 'T' AND B.ITEM_CODE = " + code + "";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                DT_CODE = 0,
                                ITEM_CODE = Convert.ToInt32(reader["CODE"]),
                                ITEM_ID = Convert.ToString(reader["ITEM_ID"]),
                                QTY = qty,
                                UNIT = 0,
                                QTY2 = qty.ToString(),
                                BAL_QTY = qty.ToString(),
                                RATE = Convert.ToString(reader["SRATE"]),
                                AMT = (Convert.ToInt32(reader["SRATE"]) * qty).ToString(),
                                DISC = "",
                                DISC_AMT = "",
                                TAX = "",
                                TAX_AMT = "",
                                ADV = "",
                                ADV_AMT = "",
                                NET_AMT = (Convert.ToInt32(reader["SRATE"]) * qty).ToString(),
                                DT_DESC = "",
                                COLOR = reader["COLOR"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COLOR"]),
                                SIZE = reader["SIZE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE"]),
                                GRADE = 0,
                                WAREHOUSE = 0,
                                DEL_DATE = "",
                                DUE_DATE = "",
                                DUE_DAYS = "",
                                VEH = "",
                                CHK = "",
                                CHK1 = false,
                                PICK_ID = 0
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

        private int GenerateNextDetailId(Common common, SqlCommand command)
        {
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE2;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    string maxIdQuery = $"SELECT ISNULL(MAX(DT_CODE), 0) FROM {table}";
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

        public MyHttpResponseMessage Save(CustomSampleDevAndPricing modelRecord, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty, detailTable = string.Empty, b_i = string.Empty, stk_status = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                    b_i = menu.B_I;
                    stk_status = menu.STK_STATUS;
                }

                if (!String.IsNullOrWhiteSpace(table) && !String.IsNullOrWhiteSpace(detailTable))
                {
                    var Ip = common.IPAddress;
                    var Computer = common.ComputerName;
                    var Postal = common.PostalCode;
                    var username = common.Username;
                    var branch = common.Branch;
                    var period = common.Period;
                    var periodInfo = _periodRepository.GetPeriodById(Convert.ToInt32(period));
                    string startDate = ((Period)periodInfo.data).START_D.Value.ToString("yyyy-MM-dd");
                    string endDate = ((Period)periodInfo.data).START_E.Value.ToString("yyyy-MM-dd");
                    var menuID = common.MenuID;
                    bool isStockSufficient = true;
                    string InSufficientItem = "";
                    double InSufficientItemQty = 0;
                    string connectionString = new SQLService().getconnstring();


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
                            string depId = Convert.ToString(modelRecord.Master.DEPARTMENT);
                            if (modelRecord.Master.TRAN_ID == null || modelRecord.Master.TRAN_ID == 0)
                            {
                                IsNew = true;
                                code = GenerateNextId(common, command);
                                var (jobNo, depType) = GenerateJobNo(common, depId, command);

                                if (code > 0)
                                {
                                    modelRecord.Master.TRAN_ID = code;
                                    voucherNo = GenerateVoucherNo(common, code, CommonService.GetDateTime("Pakistan Standard Time"));

                                    if (String.IsNullOrWhiteSpace(voucherNo))
                                    {
                                        IsMasterAdded = false;
                                    }
                                }
                                else
                                {
                                    IsMasterAdded = false;
                                }

                                query = $"INSERT INTO {table}" +
                                "(TRAN_ID, V_DATE, VOUCHER_NO, REF, REC_ON_DATE, CLIENT_PO," +
                                "PARTY_CODE, ACT_CODE, BPARTY_CODE, BACT_CODE," +
                                "DEPARTMENT, CURR_CODE, SEASON, INTAKE, JOB_NO," +
                                "REMARKS, ARTICLE_DOC, PFR_DOC, BCODE, PERIOD_ID," +
                                "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME," +
                                "ADD_IP_ADDRESS,EDIT_USER_ID,EDIT_DATE," +
                                "EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS,ADD_POSTALCODE," +
                                "EDIT_POSTALCODE,ASTATUS,MENU_ID,DLT)" +
                                "VALUES" +
                                "('" + code + "','" + modelRecord.Master.V_DATE + "','" + voucherNo + "','" + modelRecord.Master.REF + "','" + modelRecord.Master.REC_ON_DATE + "','" + modelRecord.Master.CLIENT_PO + "'," +
                                "'" + modelRecord.Master.PARTY_CODE + "','" + modelRecord.Master.ACT_CODE + "'," +
                                "'" + modelRecord.Master.BPARTY_CODE + "','" + modelRecord.Master.BACT_CODE + "'," +
                                "'" + modelRecord.Master.DEPARTMENT + "','" + modelRecord.Master.CURRENCY + "'," +
                                "'" + modelRecord.Master.SEASON + "','" + modelRecord.Master.INTAKE + "','" + jobNo + "'," +
                                "'" + modelRecord.Master.REMARKS + "'," +
                                "'" + modelRecord.Master.ARTICLE_DOC + "','" + modelRecord.Master.PFR_DOC + "','" + branch + "','" + period + "'," +
                                "'" + username + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "','" + Computer + "'," +
                                "'" + Ip + "','" + username + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "'," +
                                "'" + Computer + "','" + Ip + "','" + Postal + "','" + Postal + "','" + modelRecord.Master.ASTATUS + "','" + menuID + "','T')";
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                query = $"UPDATE {table} SET V_DATE = '" + modelRecord.Master.V_DATE + @"',
                                    REF = '" + modelRecord.Master.REF + @"',
                                    REC_ON_DATE = '" + modelRecord.Master.REC_ON_DATE + @"',
                                    CLIENT_PO = '" + modelRecord.Master.CLIENT_PO + @"',
                                    PARTY_CODE = '" + modelRecord.Master.PARTY_CODE + @"',
                                    ACT_CODE = '" + modelRecord.Master.ACT_CODE + @"',
                                    BPARTY_CODE = '" + modelRecord.Master.BPARTY_CODE + @"',
                                    BACT_CODE = '" + modelRecord.Master.BACT_CODE + @"',
                                    DEPARTMENT = '" + modelRecord.Master.DEPARTMENT + @"',
                                    CURR_CODE = '" + modelRecord.Master.CURRENCY + @"',
                                    SEASON = '" + modelRecord.Master.SEASON + @"',
                                    INTAKE = '" + modelRecord.Master.INTAKE + @"',
                                    JOB_NO = '" + modelRecord.Master.JOB_NO + @"',
                                    REMARKS = '" + modelRecord.Master.REMARKS + @"',
                                    ARTICLE_DOC = '" + modelRecord.Master.ARTICLE_DOC + @"',
                                    PFR_DOC = '" + modelRecord.Master.PFR_DOC + @"',
                                    EDIT_USER_ID = '" + username + @"',
                                    EDIT_DATE = '" + CommonService.GetDateTime("Pakistan Standard Time") + @"',
                                    EDIT_COMPUTER_NAME = '" + Computer + @"',
                                    EDIT_IP_ADDRESS = '" + Ip + @"',
                                    EDIT_POSTALCODE = '" + Postal + @"',
                                    ASTATUS = '" + modelRecord.Master.ASTATUS + @"'
                                    WHERE TRAN_ID = '" + modelRecord.Master.TRAN_ID + @"' 
                                    AND BCODE = '" + branch + @"'
                                    AND PERIOD_ID = '" + period + "'";
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }

                            var isDetailAdded = true;

                            StringBuilder insertQueryBuilder = new StringBuilder();
                            StringBuilder updateQueryBuilder = new StringBuilder();

                            bool allowInserts = true;
                            bool hasInserts = false;
                            bool hasUpdates = false;
                            int detailCode = GenerateNextDetailId(common, command);
                            foreach (var item in modelRecord.Detail.ToList())
                            {
                                try
                                {
                                    if (item.DT_CODE == null || item.DT_CODE == 0)
                                    {
                                        detailCode++;
                                        if (detailCode > 0)
                                        {
                                            if (!hasInserts)
                                            {
                                                hasInserts = true;
                                            }

                                            insertQueryBuilder.AppendLine(
                                                $"INSERT INTO {detailTable} (TRAN_ID, DT_CODE, ITEM_CODE, FABRIC, STYLE, IMAGE, QUALITY, GRADE, GSM, SIZE, RATIO, PROJ_QTY, TARGET_PRICE," +
                                                $"DT_DESC, BCODE, PERIOD_ID," +
                                                $"ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID, EDIT_DATE, EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS, ADD_POSTALCODE, " +
                                                $"EDIT_POSTALCODE, MENU_ID, DLT) VALUES " +

                                                $"('{modelRecord.Master.TRAN_ID}','{detailCode}','{item.ITEM_CODE}','{item.FABRIC}','{item.STYLE}','{item.IMAGE}'," +
                                                $"'{item.QUALITY}', '{item.GRADE}','{item.GSM}','{item.SIZE}','{item.RATIO}','{item.PROJ_QTY}','{item.TARGET_PRICE}','{item.DT_DESC}', " +
                                                $"'{branch}', '{period}', '{username}', " +
                                                $"'{CommonService.GetDateTime("Pakistan Standard Time")}', '{Computer}', '{Ip}', '{username}', " +
                                                $"'{CommonService.GetDateTime("Pakistan Standard Time")}', '{Computer}', '{Ip}', " +
                                                $"'{Postal}', '{Postal}', '{menuID}', 'T');");
                                        }
                                        else
                                        {
                                            isDetailAdded = false;
                                        }
                                    }
                                    else
                                    {
                                        if (!hasUpdates)
                                        {
                                            hasUpdates = true;
                                        }

                                        updateQueryBuilder.AppendLine(
                                            $"UPDATE {detailTable} SET " +
                                            $"ITEM_CODE = '{item.ITEM_CODE}', FABRIC = '{item.FABRIC}', STYLE = '{item.STYLE}', IMAGE = '{item.IMAGE}', QUALITY = '{item.QUALITY}', " +
                                            $"GRADE = '{item.GRADE}', GSM = '{item.GSM}', SIZE = '{item.SIZE}', RATIO = '{item.RATIO}', PROJ_QTY = '{item.PROJ_QTY}', TARGET_PRICE = '{item.TARGET_PRICE}', " +
                                            $"DT_DESC = '{item.DT_DESC}', EDIT_USER_ID = '{username}', " +
                                            $"EDIT_DATE = '{CommonService.GetDateTime("Pakistan Standard Time")}', " +
                                            $"EDIT_COMPUTER_NAME = '{Computer}', EDIT_IP_ADDRESS = '{Ip}', " +
                                            $"EDIT_POSTALCODE = '{Postal}', DLT = 'T' " +
                                            $"WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND DT_CODE = '{item.DT_CODE}' " +
                                            $"AND BCODE = '{branch}' AND PERIOD_ID = '{period}';");
                                    }
                                }
                                catch (Exception)
                                {
                                    isDetailAdded = false;
                                }
                            }
                            if (hasInserts)
                            {
                                command.CommandText = insertQueryBuilder.ToString();
                                command.ExecuteNonQuery();
                            }
                            if (hasUpdates)
                            {
                                command.CommandText = updateQueryBuilder.ToString();
                                command.ExecuteNonQuery();
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
                response.msgType = 2;
                response.msg = _catchMessage;
            }
            return response;
        }

        public Dictionary<int?, double?> PreviousStockInBill(string table, int? TRAN_ID, string period, string branch)
        {
            List<CurrentItemsInBill> jsonDataResult = new List<CurrentItemsInBill>();
            string query;
            Dictionary<int?, double?> stock = new();
            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            {
                query = $"SELECT TRAN_ID,DT_CODE,ITEM_CODE, BAL_QTY FROM {table} " +
                    " WHERE DLT = 'T' AND TRAN_ID = '" + TRAN_ID + "' AND BCODE = '" + branch + "' AND PERIOD_ID = '" + period + "'";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var row = new CurrentItemsInBill
                    {
                        ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                        QTY = reader["BAL_QTY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["BAL_QTY"])
                    };
                    jsonDataResult.Add(row);
                }
                reader.Close();
            }
            if (jsonDataResult is not null)
            {
                foreach (var item in jsonDataResult)
                {
                    if (!stock.ContainsKey(item.ITEM_CODE))
                    {
                        stock.Add(Convert.ToInt32(item.ITEM_CODE), Convert.ToDouble(item.QTY));
                    }
                    else
                    {
                        stock[item.ITEM_CODE] += Convert.ToDouble(item.QTY);
                    }
                }
            }
            return stock;
        }

        public MyHttpResponseMessage Delete(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
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
                    if (code == 0)
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
                            string query = $"UPDATE {table} SET DLT = 'F' WHERE TRAN_ID = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                            SqlCommand command = new SqlCommand(query, connection);
                            command.ExecuteNonQuery();
                            response.msgType = 1;
                            response.msg = "Record Deleted Successfully";
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

                SampleDevAndPricing SampleDevAndPricing = new SampleDevAndPricing();
                List<SampleDevAndPricingDetail> SampleDevAndPricingDetailList = new List<SampleDevAndPricingDetail>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT * FROM {table} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    string detailQuery = $@"SELECT * FROM {table2} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        SampleDevAndPricing = new SampleDevAndPricing
                        {
                            V_DATE = Convert.ToDateTime(reader["V_DATE"]),
                            PARTY_CODE = Convert.ToInt32(reader["PARTY_CODE"]),
                            ACT_CODE = Convert.ToInt32(reader["ACT_CODE"]),
                            SCODE = Convert.ToInt32(reader["SCODE"]),
                            SACODE = Convert.ToInt32(reader["SACODE"]),
                            COMM = Convert.ToDouble(reader["COMM"]),
                            REF = Convert.ToString(reader["REF"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            BTYPE = Convert.ToString(reader["BTYPE"]),
                            BCODE = Convert.ToInt32(reader["BCODE"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            DISC = Convert.ToDouble(reader["DISC"]),
                            HS_CODE = Convert.ToString(reader["HS_CODE"]),
                            DOC = Convert.ToString(reader["DOC"]),
                            CURR_CODE = Convert.ToInt32(reader["CURR_CODE"]),
                            CRATE = Convert.ToDouble(reader["CRATE"]),
                        };
                    }

                    reader.Close();

                    SqlCommand detail_Command = new SqlCommand(detailQuery, connection);
                    SqlDataReader detail_Reader = detail_Command.ExecuteReader();
                    while (detail_Reader.Read())
                    {
                        var row = new SampleDevAndPricingDetail
                        {
                            ITEM_CODE = Convert.ToInt32(detail_Reader["ITEM_CODE"]),
                            QTY = Convert.ToDouble(detail_Reader["QTY"]),
                            UNIT = Convert.ToInt32(detail_Reader["UNIT"]),
                            QTY2 = Convert.ToDouble(detail_Reader["QTY2"]),
                            BAL_QTY = Convert.ToDouble(detail_Reader["BAL_QTY"]),
                            RATE = Convert.ToDouble(detail_Reader["RATE"]),
                            AMT = Convert.ToDouble(detail_Reader["AMT"]),
                            DISC = Convert.ToDouble(detail_Reader["DISC"]),
                            DISC_AMT = Convert.ToDouble(detail_Reader["DISC_AMT"]),
                            TAX = Convert.ToDouble(detail_Reader["TAX"]),
                            TAX_AMT = Convert.ToDouble(detail_Reader["TAX_AMT"]),
                            NET_AMT = Convert.ToDouble(detail_Reader["NET_AMT"]),
                            DT_DESC = Convert.ToString(detail_Reader["DT_DESC"]),
                            COLOR = Convert.ToInt32(detail_Reader["COLOR"]),
                            SIZE = Convert.ToInt32(detail_Reader["SIZE"]),
                            GRADE = Convert.ToInt32(detail_Reader["GRADE"]),
                            WAREHOUSE = Convert.ToInt32(detail_Reader["WAREHOUSE"]),
                            DEL_DATE = Convert.ToDateTime(detail_Reader["DEL_DATE"]),
                            DUE_DATE = Convert.ToDateTime(detail_Reader["DUE_DATE"]),
                            DUE_DAYS = Convert.ToInt32(detail_Reader["DUE_DAYS"]),
                            VEH = Convert.ToString(detail_Reader["VEH"]),
                            CHK = detail_Reader["CHK"] == DBNull.Value ? 0 : Convert.ToInt32(detail_Reader["CHK"]),
                            PICK_ID = Convert.ToInt32(detail_Reader["PICK_ID"]),
                            PICK_ID_D = detail_Reader["PICK_ID_D"] == DBNull.Value ? 0 : Convert.ToInt32(detail_Reader["PICK_ID_D"]),
                            ADV = Convert.ToDouble(detail_Reader["ADV"]),
                            ADV_AMT = Convert.ToDouble(detail_Reader["ADV_AMT"]),
                            PARTY_CODE = Convert.ToInt32(detail_Reader["PARTY_CODE"]),
                            ACT_CODE = Convert.ToInt32(detail_Reader["ACT_CODE"]),
                        };
                        SampleDevAndPricingDetailList.Add(row);
                    }

                    detail_Reader.Close();
                    connection.Close();
                }

                var customRequisition = new CustomSampleDevAndPricing
                {
                    Master = SampleDevAndPricing,
                    Detail = SampleDevAndPricingDetailList
                };

                response = this.Save(customRequisition, common);

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

        public MyHttpResponseMessage DeleteSampleDevAndPricingDetailByCode(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE2;
                }
                if (!String.IsNullOrWhiteSpace(table))
                {
                    var branch = common.Branch;
                    var period = common.Period;
                    if (code == 0)
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
                            string query = $"UPDATE {table} SET DLT = 'F'" +
                                $" WHERE DT_CODE = '{code}' AND BCODE = '{branch}' AND PERIOD_ID = '{period}'";
                            SqlCommand command = new SqlCommand(query, connection);
                            command.ExecuteNonQuery();
                            response.msgType = 1;
                            response.msg = "Record Deleted Successfully";
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

        public MyHttpResponseMessage GetDataForReport(SampleDevAndPricingRDLCReport modelRecord, CustomMenuDetail menuDetails, DataTable inspectionServiceChargesDetails, Company currentCompany, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            //CustomMenuDetail menuDetails = new CustomMenuDetail();
            SampleDevAndPricingRDLCReport masterData = new SampleDevAndPricingRDLCReport();
            CustomSampleDevAndPricingForPrintReport reportData = new CustomSampleDevAndPricingForPrintReport();
            var Menu = _menuRepository.GetMenu(common.MenuID);
            string? table = string.Empty, detailTable = string.Empty;
            string? pickMaster = string.Empty, pickDetail = string.Empty;



            if (Menu.data != null)
            {
                var menu = (Menu)Menu.data;
                table = menu.TABLE1;
                detailTable = menu.TABLE2;
                pickMaster = menu.PICK_TABLE_MASTER;
                pickDetail = menu.PICK_TABLE_DETAIL;
            }
            try
            {

                string query = $@"SELECT M.TRAN_ID, M.V_DATE, M.VOUCHER_NO, M.REF, M.CLIENT_PO, PTC.PARTY_NAME AS CLIENT_NAME, DEP.DESCR AS DEP, 
                                        M.JOB_NO, C.DESCR AS CURRENCY, PTS.PARTY_NAME AS BUYER_NAME, S.GROUP_NAME AS SEASON, M.INTAKE, M.REC_ON_DATE, M.REF, M.REMARKS,
                                        IT.ITEM_NAME, F.GROUP_NAME AS FABRIC, D.STYLE, D.QUALITY, GSM.GROUP_NAME AS GSM, SZ.GROUP_NAME AS SIZE, D.PROJ_QTY, D.TARGET_PRICE, D.DT_DESC, M.EDIT_USER_ID
                                        FROM {table} M
                                        LEFT OUTER JOIN {detailTable} D ON D.TRAN_ID = M.TRAN_ID 
                                        LEFT OUTER JOIN TBL_PARTY_TYPES PTC ON M.PARTY_CODE = PTC.PARTY_CODE AND PTC.ACT_CODE = M.ACT_CODE
                                        LEFT OUTER JOIN TBL_PARTY_TYPES PTS ON M.BPARTY_CODE = PTS.PARTY_CODE AND PTS.ACT_CODE = M.BACT_CODE
                                        LEFT OUTER JOIN TBL_ACT_GROUP DEP ON M.DEPARTMENT = DEP.CODE
                                        LEFT OUTER JOIN TBL_CURRENCY C ON M.CURR_CODE = C.CODE
                                        LEFT OUTER JOIN TBL_ITEMSMASTER IT ON D.ITEM_CODE = IT.ITEM_CODE
                                        LEFT OUTER JOIN TBL_GRADE G ON D.GRADE = G.GROUP_CODE
                                        LEFT OUTER JOIN TBL_SEASON S ON M.SEASON = S.GROUP_CODE
                                        LEFT OUTER JOIN TBL_FABRIC F ON D.FABRIC = F.GROUP_CODE
                                        LEFT OUTER JOIN TBL_GSM GSM ON D.GSM = GSM.GROUP_CODE
                                        LEFT OUTER JOIN TBL_SIZE SZ ON D.SIZE = SZ.GROUP_CODE
                                        WHERE M.TRAN_ID = '{modelRecord.TRAN_ID}' AND M.BCODE = '{common.Branch}' AND M.PERIOD_ID = '{common.Period}'";

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
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    bool firstRecord = true;

                    while (reader.Read())
                    {
                        if (firstRecord)
                        {

                            masterData.DATE = reader["V_DATE"] == DBNull.Value || Convert.ToDateTime(reader["V_DATE"]).Year <= 1900 ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MMM-yy");
                            masterData.INVOICE_NUMBER = Convert.ToString(reader["VOUCHER_NO"]);
                            masterData.CLIENT_PO = Convert.ToString(reader["CLIENT_PO"]);
                            masterData.PARTY_NAME = Convert.ToString(reader["CLIENT_NAME"]);
                            masterData.DEP = Convert.ToString(reader["DEP"]);
                            masterData.JOB_NO = Convert.ToString(reader["JOB_NO"]);
                            masterData.CURR = Convert.ToString(reader["CURRENCY"]);
                            masterData.BUYER_NAME = Convert.ToString(reader["BUYER_NAME"]);
                            masterData.SEASON = Convert.ToString(reader["SEASON"]);
                            masterData.INTAKE = Convert.ToInt32(reader["INTAKE"]);
                            masterData.REC_ON_DATE = reader["REC_ON_DATE"] == DBNull.Value || Convert.ToDateTime(reader["REC_ON_DATE"]).Year <= 1900 ? null : Convert.ToDateTime(reader["REC_ON_DATE"]).ToString("dd-MMM-yy");
                            masterData.REFERENCENO = Convert.ToString(reader["REF"]);
                            masterData.REMARKS = Convert.ToString(reader["REMARKS"]);
                            masterData.EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]);
                            firstRecord = false;
                        }

                        DataRow dataRow = inspectionServiceChargesDetails.NewRow();
                        dataRow["Item"] = Convert.ToString(reader["ITEM_NAME"]);
                        dataRow["FABRIC"] = Convert.ToString(reader["FABRIC"]);
                        dataRow["STYLE"] = Convert.ToString(reader["STYLE"]);
                        dataRow["QUALITY"] = Convert.ToString(reader["QUALITY"]);
                        dataRow["GSM"] = Convert.ToString(reader["GSM"]);
                        dataRow["SIZE"] = Convert.ToString(reader["SIZE"]);
                        dataRow["PROJ_QTY"] = reader["PROJ_QTY"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PROJ_QTY"]);
                        dataRow["TARGET_PRICE"] = reader["TARGET_PRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TARGET_PRICE"]);
                        dataRow["DT_DESC"] = Convert.ToString(reader["DT_DESC"]);
                        inspectionServiceChargesDetails.Rows.Add(dataRow);

                    }
                    reader.Close();
                }

                reportData.Master = masterData;
                reportData.Detail = inspectionServiceChargesDetails;

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

        private (string JobNo, string DepType) GenerateJobNo(Common common, string depId, SqlCommand command)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(depId))
                {
                    string query = $"SELECT DEP_TYPE FROM TBL_ACT_GROUP WHERE CODE = {depId}";
                    command.CommandText = query;
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string depType = result.ToString();

                        string secondQuery = $"SELECT COUNT(*) FROM TBL_MPO_MASTER WHERE DEP_TYPE = '{depType}';";
                        command.CommandText = secondQuery;
                        object countResult = command.ExecuteScalar();
                        int matchingCount = Convert.ToInt32(countResult);

                        string jobNo = $"{depType}-00{matchingCount + 1}";

                        return (jobNo, depType);
                    }
                }
            }
            catch (Exception ex)
            {
                // handle/log exception
            }

            return (string.Empty, string.Empty);
        }
    }
}