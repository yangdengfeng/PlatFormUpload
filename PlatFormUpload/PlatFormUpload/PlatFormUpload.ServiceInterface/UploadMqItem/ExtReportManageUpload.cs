using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using System.IO;
using PlatFormUpload.Common;

namespace PlatFormUpload.ServiceInterface
{
    public class ExtReportManageUpload : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> excludeFields = System.Configuration.ConfigurationManager.AppSettings["extReportManage_excluded"].Split(',').ToList();
        private static List<string> numberFields = System.Configuration.ConfigurationManager.AppSettings["extReportManage_Numbers"].Split(',').ToList();
        private static List<string> dateFields = System.Configuration.ConfigurationManager.AppSettings["extReportManage_Dt"].Split(',').ToList();
        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["extReportManage_Pks"].Split(',');
        private static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["extReportManage_ExchangeName"];
        private static string routingKey = System.Configuration.ConfigurationManager.AppSettings["extReportManage_RoutingKey"]; 
        private static string esType = System.Configuration.ConfigurationManager.AppSettings["extReportManage_EsType"];
        private static string esDocType = System.Configuration.ConfigurationManager.AppSettings["extReportManage_EsDocType"]; 
        private static string largeWordReportFolder = System.Configuration.ConfigurationManager.AppSettings["LargeWordDataFolder"];
        private static int limitedSized = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["wordreportlist_LimitedSizeMForRavenDB"]);

        public ExtReportManageUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {
        }
        public ExtReportManageUpload(Dictionary<string, string> dicData, byte[] cmdAttach, bool needFilePath) : base(dicData, cmdAttach, needFilePath)
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
            return "extReportManage";
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
            try
            {
                if (string.IsNullOrWhiteSpace(customId))
                {
                    errorMsg = "wordreport缺少customID";
                    return false;
                }

                string sysPrimaryKey = string.Empty;
                if (!dicData.TryGetValue("SYSPRIMARYKEY", out sysPrimaryKey))
                {
                    errorMsg = "wordreport缺少SYSPRIMARYKEY";
                    return false;
                }

                string suffix = "doc";
                if (!dicData.TryGetValue("FILETYPE", out suffix))
                {
                    suffix = "doc";
                }

                if (string.IsNullOrWhiteSpace(suffix))
                {
                    suffix = "doc";
                }

              

                if (cmdAttach != null)
                {
                    
                    #region 附件处理

                    string relativePath = string.Format("{0}\\{1}\\{2}\\{3}.{4}",
                            FSSelector.GetSafeFilename(customId),
                            DateTime.Now.ToString("yyyy-MM-dd"),
                            FSSelector.GetSafeFilename(sysPrimaryKey),
                            FSSelector.GetSafeFilename(pk),
                            suffix);
                    string baseFileDir = FSSelector.GetFSWordUrl(customId);
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

                    #endregion 
                        
                } 
          

                return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.WriteErrorLog(logger, ex);
                errorMsg = ex.Message;
                return false;
            }
        }

        public override bool PushJsonToMq(JObject jobj, out string errorMsg)
        {
            if (string.IsNullOrWhiteSpace(customId))
            {
                errorMsg = "wordreport缺少customID";
                return false;
            }

            if (string.IsNullOrWhiteSpace(itemName))
            {
                errorMsg = "wordreport缺少itemname";
                return false;
            }

            string sysPrimaryKey = string.Empty;
            if (!dicData.TryGetValue("SYSPRIMARYKEY", out sysPrimaryKey))
            {
                errorMsg = "wordreport缺少SYSPRIMARYKEY";
                return false;
            }

            return PushJsonToRabbitMq(jobj, exchangeName, routingKey, logger, out errorMsg);
        }
    }
}