using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using NLog;
using System.IO;
using System.Net;
using System.Configuration;
using PlatFormUpload.Common;

namespace PlatFormUpload.ServiceInterface
{
    public class TPkpmBinaryReportUpload : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> excludeFields = ConfigurationManager.AppSettings["tPkPmBinaryReport_excluded"].Split(',').ToList();
        private static List<string> numberFields = ConfigurationManager.AppSettings["tPkPmBinaryReport_Numbers"].Split(',').ToList();
        private static List<string> dateFields = ConfigurationManager.AppSettings["tPkPmBinaryReport_Dt"].Split(',').ToList();
        private static string[] primaryFields = ConfigurationManager.AppSettings["tPkPmBinaryReport_Pks"].Split(',');
        private static string exchangeName =  ConfigurationManager.AppSettings["tPkPmBinaryReport_ExchangeName"];
        private static string routingKey = ConfigurationManager.AppSettings["tPkPmBinaryReport_RoutingKey"]; 
        private static string esType = ConfigurationManager.AppSettings["tPkPmBinaryReport_EsType"];
        private static string esDocType = ConfigurationManager.AppSettings["tPkPmBinaryReport_EsDocType"]; 
        private static string largePKRFolder = ConfigurationManager.AppSettings["LargePKRDataFolder"];
        private static string elasticUrl= ConfigurationManager.AppSettings["ElasticTBPItemUrl"];
        private static int limitedSized = Int32.Parse(ConfigurationManager.AppSettings["PKR_LimitedSizeMForRavenDB"]);

        public TPkpmBinaryReportUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {
        }

        public TPkpmBinaryReportUpload(Dictionary<string, string> dicData, byte[] cmdAttach, bool needFilePath) : base(dicData, cmdAttach, needFilePath)
        {
        }

        public override JObject ConstructNeedToBePushedJson()
        {
            return ConstructJsonFromDictionary(excludeFields, numberFields, dateFields);
        }

        public override string GetPrimaryKey()
        {
            return ConstructPrimayKey(dicData, primaryFields);
        }

        public override string GetTableName()
        {
            return "t_pkpm_binaryReport ";
        }

        public override void PostConstructJson(JObject jobj)
        {
            jobj.Add(new JProperty(elasticType, esType));
            jobj.Add(new JProperty(elasticDocType, esDocType));
        }

        public override bool PrePushToRabbitMq(out string fileSavePath, out string errorMsg)
        {
            fileSavePath = string.Empty;
            errorMsg = string.Empty;

            if (string.IsNullOrWhiteSpace(customId))
            {
                errorMsg = "PKR据缺少customID";
                return false;
            }

            string reportNum = string.Empty;
            if (!dicData.TryGetValue("REPORTNUM", out reportNum))
            {
                errorMsg = "PKR数据缺少reportNum";
                return false;
            } 


            //无机构id，则reportnum拼接customid
            //接收的时候只要在reportNum值前面加注册码就可以 
            if(!reportNum.Contains(customId))
            {
                dicData["REPORTNUM"] = string.Format("{0}{1}", customId, reportNum);
            } 
           

            try
            {
                #region 附件
                //附件___blob___

                //bool hasData = false;

                if (cmdAttach != null)
                {
                    string relativePath = string.Format("{0}\\{1}\\{2}.cll",
                        FSSelector.GetSafeFilename(customId),
                        DateTime.Now.ToString("yyyy-MM"),
                        FSSelector.GetSafeFilename(reportNum));

                    string baseFileDir = FSSelector.GetFSPKRUrl(customId);
                    string filePath = Path.Combine(baseFileDir, relativePath);

                    string fileFolder = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(fileFolder))
                    {
                        Directory.CreateDirectory(fileFolder);
                    }

                    try
                    {
                        File.WriteAllBytes(filePath, cmdAttach);
                        fileSavePath = filePath;
                 
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.WriteErrorLog(logger, ex);
                        errorMsg = string.Format("[ErrorMsg:{0}]-[StackTrace:{1}]-[Source:{2}]",
                            ex.Message,
                            ex.StackTrace,
                            ex.Source);
                        return false;
                    }
                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.WriteErrorLog(logger, ex);
                errorMsg = ex.Message;
                return false;
            }
        }

        private string GetSysPrimaryFromReportNum(string reportNum)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Proxy = null;
                string tbpitemStr = wc.DownloadString(string.Format(elasticUrl, reportNum) + "&sort=UPLOADTIME:desc");

                string startStr = "\"_id\":";
                string endStr = ",\"_score\"";

                string sysPrimaryKey = string.Empty;
                int startIndex = tbpitemStr.IndexOf(startStr);
                int endIndex = tbpitemStr.IndexOf(endStr);
                if (startIndex > -1 && endIndex > startIndex)
                {
                    sysPrimaryKey = tbpitemStr.Substring(startIndex + startStr.Length, endIndex - startIndex - (startStr.Length));
                    sysPrimaryKey = sysPrimaryKey.Trim(sysPrimaryKey[0]);
                }

                return sysPrimaryKey;
            }
            catch (Exception)
            {

                //ignore
                return string.Empty;
            }
        }

        public override bool PushJsonToMq(JObject jobj, out string errorMsg)
        {
            if (string.IsNullOrWhiteSpace(customId))
            {
                errorMsg = "PKR据缺少customID";
                return false;
            }

            string reportNum = string.Empty;
            if (!dicData.TryGetValue("REPORTNUM", out reportNum))
            {
                errorMsg = "PKR数据缺少reportNum";
                return false;
            }

            return PushJsonToRabbitMq(jobj, exchangeName, routingKey, logger, out errorMsg);
        }
    }
}