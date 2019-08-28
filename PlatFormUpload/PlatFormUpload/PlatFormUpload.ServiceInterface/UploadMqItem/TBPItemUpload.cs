using Newtonsoft.Json.Linq;
using NLog;
using PlatFormUpload.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PlatFormUpload.ServiceInterface
{
    public class TBPItemUpload : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> excludeFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_item_excluded"].Split(',').ToList();
        private static List<string> numberFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_item_Numbers"].Split(',').ToList();
        private static List<string> dateFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_item_Dt"].Split(',').ToList();
        private static string esHrExchangeName = System.Configuration.ConfigurationManager.AppSettings["ESTHrItem_ExchangeName"];
        private static string esHrRoutingKey = System.Configuration.ConfigurationManager.AppSettings["ESTHrItem_RoutingKey"];
        private static string esHrEsType = System.Configuration.ConfigurationManager.AppSettings["ESTHrItem_EsType"];
        private static string esHrEsDocType = System.Configuration.ConfigurationManager.AppSettings["ESTHrItem_EsDocType"];

        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_item_Pks"].Split(',');
        public static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["t_bp_item_ExchangeName"];
        public static string routingKey = System.Configuration.ConfigurationManager.AppSettings["t_bp_item_RoutingKey"];

        private static bool IsBHRecord = System.Configuration.ConfigurationManager.AppSettings["IsBHRecord"] == "1";
        private static string BHCurTypeCode = System.Configuration.ConfigurationManager.AppSettings["BHCurTypeCode"];
        private static DateTime StartPushEntrustDate = DateTime.Parse(System.Configuration.ConfigurationManager.AppSettings["StartPushEntrustDate"]);

        public static string esType = System.Configuration.ConfigurationManager.AppSettings["t_bp_item_EsType"];

        private static List<string> cTypeItemCodes = System.Configuration.ConfigurationManager.AppSettings["CReportTypes"].Split(',').ToList();
        private static List<string> cTypeStartWiths = System.Configuration.ConfigurationManager.AppSettings["CReportStartWith"].Split(',').ToList();
        private static List<string> typeCodes = System.Configuration.ConfigurationManager.AppSettings["typeCodes"].Split(',').ToList();

        public TBPItemUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {

        }

        protected string GetDictionaryValue(string Key)
        {

            if (dicData == null)
            {
                return string.Empty;
            }

            string Value = string.Empty;
            if (!dicData.TryGetValue(Key, out Value))
            {
                Value = string.Empty;
            }

            return Value;
        }

        public override bool PushJsonToMq(JObject jobj, out string errorMsg)
        {
            string qrBar = string.Empty;
            string reportNum = string.Empty;


            if (IsBHRecord)
            {
                errorMsg = string.Empty;
                var ItemCode = GetDictionaryValue("ITEMNAME");
                if (string.IsNullOrWhiteSpace(ItemCode))
                {
                    return true;
                }
                if (BHCurTypeCode.Contains(ItemCode))
                {
                    return true;
                }
                var SAMPLEDISPOSEPHASE = GetDictionaryValue("SAMPLEDISPOSEPHASE");
                if (string.IsNullOrEmpty(SAMPLEDISPOSEPHASE) || SAMPLEDISPOSEPHASE.Substring(5, 1) == "0")
                {
                    return true;
                }
                //2018-11-2 杨灿要求2018-10月之前的不插入闭合表
                var ENTRUSTDATE = GetDictionaryValue("ENTRUSTDATE");

                var dateEntrust = DateTime.Now;
                if (DateTime.TryParse(ENTRUSTDATE, out dateEntrust))
                {
                    if (dateEntrust < StartPushEntrustDate)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
                string ConclusionCode = GetDictionaryValue("CONCLUSIONCODE");

                //string ConclusionCode = GetDictionaryValue("CONCLUSIONCODE");
                //if (dicData.ContainsKey("CONCLUSIONCODE") && dicData.TryGetValue("CONCLUSIONCODE", out ConclusionCode))
                {
                    //不合格报告插入闭合表
                    if (!string.IsNullOrEmpty(ConclusionCode) && ConclusionCode == "N")
                    {
                        var SYSPRIMARYKEY = GetDictionaryValue("SYSPRIMARYKEY");
                        string ExistSql = @"select 1 from t_bp_itemBH where sysPrimaryKey=@sysPrimaryKey";
                        SqlParameter[] ExistsParameters = new SqlParameter[] { new SqlParameter("@sysPrimaryKey", SqlDbType.VarChar) { Value = SYSPRIMARYKEY } };
                        var dtExists = SqlHelper.ExecuteDataSet(SqlHelper.ConnectionStringMSSQL, CommandType.Text, ExistSql, ExistsParameters);
                        if (dtExists != null && dtExists.Tables.Count > 0 && dtExists.Tables[0].Rows.Count > 0)
                        {
                            return true;
                        }
                        var CustomName = HttpRuntime.Cache.Get(customId);
                        if (CustomName == null)
                        {
                            string CustomSql = @"select NAME from t_bp_custom where Id=@customId";
                            SqlParameter[] CustomParameters = new SqlParameter[] { new SqlParameter("@customId", SqlDbType.VarChar) { Value = customId } };
                            var dtCustom = SqlHelper.ExecuteDataSet(SqlHelper.ConnectionStringMSSQL, CommandType.Text, CustomSql, CustomParameters);
                            if (dtCustom != null && dtCustom.Tables.Count > 0 && dtCustom.Tables[0].Rows.Count > 0 && dtCustom.Tables[0].Rows[0][0] != null)
                            {
                                CustomName = dtCustom.Tables[0].Rows[0][0].ToString();
                            }
                            else
                            {
                                CustomName = string.Empty;
                            }

                            if (CustomName != null
                                && !string.IsNullOrWhiteSpace(CustomName.ToString()))
                            {
                                HttpRuntime.Cache.Add(customId,
                               CustomName,
                               null,
                               DateTime.MaxValue,
                               TimeSpan.FromMinutes(10),
                               System.Web.Caching.CacheItemPriority.Default,
                               null);
                            }

                        }

                        var ItemCHName = HttpRuntime.Cache.Get(ItemCode);
                        if (ItemCHName == null)
                        {
                            string ItemNameSql = @"select ItemName from totalitem where itemcode=@ItemCode";
                            SqlParameter[] ItemNameParameters = new SqlParameter[] { new SqlParameter("@ItemCode", SqlDbType.VarChar) { Value = ItemCode } };
                            var dtItemName = SqlHelper.ExecuteDataSet(SqlHelper.ConnectionStringMSSQL, CommandType.Text, ItemNameSql, ItemNameParameters);
                            if (dtItemName != null && dtItemName.Tables.Count > 0 && dtItemName.Tables[0].Rows.Count > 0 && dtItemName.Tables[0].Rows[0][0] != null)
                            {
                                ItemCHName = dtItemName.Tables[0].Rows[0][0].ToString();
                            }
                            else
                            {
                                ItemCHName = string.Empty;
                            }

                            if (ItemCHName != null
                                && !string.IsNullOrWhiteSpace(ItemCHName.ToString()))
                            {
                                HttpRuntime.Cache.Add(ItemCode,
                               ItemCHName,
                               null,
                               DateTime.MaxValue,
                               TimeSpan.FromMinutes(10),
                               System.Web.Caching.CacheItemPriority.Default,
                               null);
                            }

                        }
                        bool ExistCheckDate = false;
                        DateTime dtmCheckTime = DateTime.Now;
                        string strCheckTime = GetDictionaryValue("CHECKDATE");
                        if (DateTime.TryParse(strCheckTime, out dtmCheckTime))
                        {
                            ExistCheckDate = true;
                        }
                        string sql = "INSERT INTO dbo.t_bp_itemBH(DetectUnit,DelegateUnit,TestNo,UserProjName,ProjPart,ImproperDetails,ProductCorpName,TestConclusion,TestType,{0}SYSPRIMARYKEY,show,SAMPLEDISPOSEPHASE,qrinfo,samplenum,entrustnum,reportnum,status,showDate,projectnum,WITNESSUNIT,WITNESSMAN,WITNESSMANNUM,WITNESSMANTEL,TAKESAMPLEMANNUM,TAKESAMPLEMANTEL,TAKESAMPLEMAN,Type,sendstatus,closestatus)     VALUES(@DetectUnit, @DelegateUnit, @TestNo, @UserProjName, @ProjPart, @ImproperDetails,@ProductCorpName, @TestConclusion, @TestType, {1} @SYSPRIMARYKEY, @show,@SAMPLEDISPOSEPHASE, @qrinfo, @samplenum, @entrustnum, @reportnum, @STATUS,@showDate,@projectnum, @WITNESSUNIT, @WITNESSMAN, @WITNESSMANNUM, @WITNESSMANTEL,@TAKESAMPLEMANNUM, @TAKESAMPLEMANTEL,@TAKESAMPLEMAN, @TYPE, @sendstatus, @closestatus)";
                        if (ExistCheckDate)
                        {
                            sql = string.Format(sql, "TestDate,", "@TestDate,");
                        }
                        else
                        {
                            sql = string.Format(sql, "", "");
                        }
                        SqlParameter[] parameters = new SqlParameter[]{
                new SqlParameter("@DetectUnit", SqlDbType.VarChar){ Value = CustomName },//检测机构名称
                new SqlParameter("@DelegateUnit", SqlDbType.VarChar){ Value = GetDictionaryValue("ENTRUSTUNIT")},
                new SqlParameter("@TestNo", SqlDbType.VarChar){ Value = GetDictionaryValue("REPORTNUM") },
                new SqlParameter("@UserProjName", SqlDbType.VarChar){ Value = GetDictionaryValue("PROJECTNAME") },
                new SqlParameter("@ProjPart", SqlDbType.VarChar){ Value = GetDictionaryValue("STRUCTPART") },
                new SqlParameter("@ImproperDetails", SqlDbType.VarChar){ Value = "" },
                new SqlParameter("@ProductCorpName", SqlDbType.VarChar){ Value = GetDictionaryValue("PDSTANDARDNAME") },
                new SqlParameter("@TestConclusion", SqlDbType.VarChar){ Value = GetDictionaryValue("CHECKCONCLUSION") },
                new SqlParameter("@TestType", SqlDbType.VarChar){ Value = ItemCHName },//中文名
                //new SqlParameter("@TestDate", SqlDbType.DateTime){ Value = ExistCheckDate?dtmCheckTime:DBNull.Value },
                new SqlParameter("@SYSPRIMARYKEY", SqlDbType.VarChar){ Value = SYSPRIMARYKEY },
                new SqlParameter("@show", SqlDbType.Int){ Value = 0 },
                new SqlParameter("@SAMPLEDISPOSEPHASE", SqlDbType.VarChar){ Value = GetDictionaryValue("SAMPLEDISPOSEPHASE") },
                new SqlParameter("@qrinfo", SqlDbType.VarChar){ Value = GetDictionaryValue("QRCODEBAR") },
                new SqlParameter("@samplenum", SqlDbType.VarChar){ Value = GetDictionaryValue("SAMPLENUM") },
                new SqlParameter("@entrustnum", SqlDbType.VarChar){ Value = GetDictionaryValue("ENTRUSTNUM") },
                new SqlParameter("@reportnum", SqlDbType.VarChar){ Value = GetDictionaryValue("REPORTNUM") },

                //new SqlParameter("@slclosedman", SqlDbType.VarChar){ Value = null },
                //new SqlParameter("@slcloseddate", SqlDbType.DateTime){ Value = null },
                //new SqlParameter("@spnclosedman", SqlDbType.VarChar){ Value = null },
                //new SqlParameter("@spncloseddate", SqlDbType.DateTime){ Value = null },

                new SqlParameter("@status", SqlDbType.Int){ Value = 0},
                new SqlParameter("@showDate", SqlDbType.DateTime){ Value = DateTime.Now },
                new SqlParameter("@projectnum", SqlDbType.VarChar){ Value = GetDictionaryValue("PROJECTNUM") },
                new SqlParameter("@WITNESSUNIT", SqlDbType.VarChar){ Value = GetDictionaryValue("SUPERUNIT") },
                new SqlParameter("@WITNESSMAN", SqlDbType.VarChar){ Value = GetDictionaryValue("WITNESSMAN") },
                new SqlParameter("@WITNESSMANNUM", SqlDbType.VarChar){ Value = "" },
                new SqlParameter("@WITNESSMANTEL", SqlDbType.VarChar){ Value = GetDictionaryValue("WITNESSMANTEL") },
                new SqlParameter("@TAKESAMPLEMANNUM", SqlDbType.VarChar){ Value = ""},
                new SqlParameter("@TAKESAMPLEMANTEL", SqlDbType.VarChar){ Value =GetDictionaryValue("TAKESAMPLEMANTEL") },
                new SqlParameter("@TAKESAMPLEMAN", SqlDbType.VarChar){ Value = GetDictionaryValue("TAKESAMPLEMAN") },

                new SqlParameter("@TYPE", SqlDbType.VarChar){ Value = GetDictionaryValue("ITEMNAME") },
                new SqlParameter("@sendstatus", SqlDbType.Int){ Value = 0 },
                new SqlParameter("@closestatus", SqlDbType.Int){ Value = 0 },


                    };
                        if (ExistCheckDate)
                        {
                            var listPara = parameters.ToList();
                            listPara.Add(new SqlParameter("@TestDate", SqlDbType.DateTime) { Value = dtmCheckTime });
                            parameters = listPara.ToArray();
                        }
                        var insertRsult = SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionStringMSSQL, CommandType.Text, sql, parameters);

                        LoggerHelper.WriteCustomLog(logger,
                            string.Format("闭合处理成功,SysPrimarykey:[{0}]. ReportNum:[{1}]", pk, reportNum),
                            "TBPItem-BH",
                            insertRsult >= 1);
                        LoggerHelper.WriteCustomLog(logger,
                                 string.Format("原始记录:{0}", dicData),
                                 customId + "BH-",
                                false);
                    }
                }
            } 

            return PushJsonToRabbitMq(jobj, exchangeName, routingKey, logger, out errorMsg);
        }

        public override JObject ConstructNeedToBePushedJson()
        {
            if (dicData.ContainsKey("SAMPLEDISPOSEPHASE"))
            {
                dicData["SAMPLEDISPOSEPHASEORIGIN"] = dicData["SAMPLEDISPOSEPHASE"];
                string samplePhase = dicData["SAMPLEDISPOSEPHASE"];
                int sLength = samplePhase.Length;
                char oneChar = '1';
                if (samplePhase.Count(c => c == oneChar) > 1)
                {
                    List<int> oneIndexs = new List<int>();

                    for (int i = 0; i < samplePhase.Length; i++)
                    {
                        if (samplePhase[i] == oneChar)
                        {
                            oneIndexs.Add(i);
                        }
                    }

                    //将 1100011
                    //转成  1000000 0100000 0000010 0000001

                    List<string> output = oneIndexs.Select(d => string.Format("{0}{1}{2}",
                        d == 0 ? string.Empty : new string('0', d),
                        '1',
                        d == (sLength - 1) ? string.Empty : new string('0', sLength - d - 1))).ToList();

                    string phases = string.Join(" ", output);

                    dicData["SAMPLEDISPOSEPHASE"] = phases;
                }
            }

            //解析181规范 
            //解析出3个字段 TYPECODE SUBITEMCODE PARMCODE
            if (dicData.ContainsKey("REPORTNUM"))
            {
                int customFlagLength = 3;  // 031
                int typeCodeLength = 3;  //Q03
                int itemCodeLength = 5;   //Q0301
                int paramCodeLength = 7;  //Q030100
                int yearLength = 2; // 17
                int seqNoLength = 5; //01045

                string reportNum = dicData["REPORTNUM"];

                int typeCodeIndex = GetTypeCodeStartIndex(reportNum, customFlagLength);

                string typeCode = GetSafeSubstring(reportNum, typeCodeIndex, typeCodeLength);
                if (!string.IsNullOrWhiteSpace(typeCode))
                {
                    dicData["TYPECODE"] = typeCode;
                }

                string itemcode = GetSafeSubstring(reportNum, typeCodeIndex, itemCodeLength);
                if (!string.IsNullOrWhiteSpace(itemcode))
                {

                    dicData["SUBITEMCODE"] = itemcode;
                }

                string parmaCode = GetSafeSubstring(reportNum, typeCodeIndex, paramCodeLength);
                if (!string.IsNullOrWhiteSpace(parmaCode))
                {
                    int aIndex = reportNum.IndexOf("补");
                    if (aIndex > -1 && aIndex < typeCodeIndex)
                    {
                        string additonStr = GetSafeSubstring(reportNum, aIndex, typeCodeIndex - aIndex);
                        parmaCode = string.Format("{0}{1}", additonStr, parmaCode);
                    }

                    dicData["PARMCODE"] = parmaCode;
                }

                string seqNoStr = GetSafeSubstring(reportNum, typeCodeIndex + paramCodeLength + yearLength + 1, seqNoLength);

                //支持6位流水号
                string additonSeqNoStr = GetSafeSubstring(reportNum, typeCodeIndex + paramCodeLength + yearLength + seqNoLength + 1, 1);
                if (!string.IsNullOrWhiteSpace(additonSeqNoStr)
                    && additonSeqNoStr.Length == 1
                    && Char.IsNumber(additonSeqNoStr[0]))
                {
                    seqNoStr = string.Format("{0}{1}", seqNoStr, additonSeqNoStr);
                }

                int seqno = 0;
                if (int.TryParse(seqNoStr, out seqno))
                {
                    dicData["REPSEQNO"] = seqno.ToString();

                }

                if (!string.IsNullOrWhiteSpace(seqNoStr))
                {
                    string reporNumWithOutSeqNo = reportNum.Replace(seqNoStr, string.Empty);

                    if (!string.IsNullOrWhiteSpace(reporNumWithOutSeqNo))
                    {
                        dicData["REPORMNUMWITHOUTSEQ"] = reporNumWithOutSeqNo;
                    }
                }
                else
                {
                    dicData["REPORMNUMWITHOUTSEQ"] = reportNum;
                }
            }

            dicData["ISCREPORT"] = "0";
            if (!string.IsNullOrWhiteSpace(itemName))
            {
                if (cTypeItemCodes.Contains(itemName))
                {
                    dicData["ISCREPORT"] = "1";
                }
                else
                {
                    if (itemName.Length >= 2 && cTypeStartWiths.Exists(c => itemName.StartsWith(c)))
                    {
                        //GE02
                        if (char.IsNumber(itemName.Remove(0, 2)[0]))
                        {
                            dicData["ISCREPORT"] = "1";
                        }
                    }
                }
            }

            return ConstructJsonFromDictionary(excludeFields, numberFields, dateFields);
        }

        private string GetSafeSubstring(string str,int startIndex,int length)
        {
            if(string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }

            try
            {
                return str.Substring(startIndex, length);
            }
            catch (ArgumentOutOfRangeException)
            {
                return string.Empty;
            }
        }

        private int GetTypeCodeStartIndex(string reportNum, int customLength)
        {
            foreach (var item in typeCodes)
            {
                if (reportNum.Contains(item))
                {
                    return reportNum.IndexOf(item);
                }
            }
            //没有 默认从 机构后面开始
            return customLength;
        }

        public override bool PrePushToRabbitMq(out string fileSavePath, out string errorMsg)
        {
            fileSavePath = string.Empty;
            errorMsg = string.Empty;
            return true;
        }

        public override string GetPrimaryKey()
        {
            return ConstructPrimayKey(dicData, primaryFields);
        }

        public override string GetTableName()
        {
            return "T_BP_ITEM";
        }

        public override void PostConstructJson(JObject jobj)
        {
            jobj.Add(new JProperty(elasticType, esType));
        }
    }
}