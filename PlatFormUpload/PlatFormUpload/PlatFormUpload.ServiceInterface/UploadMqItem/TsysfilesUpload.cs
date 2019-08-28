using Newtonsoft.Json.Linq;
using NLog;
using PlatFormUpload.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PlatFormUpload.ServiceInterface
{
    public class TsysfilesUpload : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string arcDataFolder = System.Configuration.ConfigurationManager.AppSettings["tsysfilesDataFolder"];
        private static List<string> excludeFields = System.Configuration.ConfigurationManager.AppSettings["t_sys_files_excluded"].Split(',').ToList();
        private static List<string> numberFields = System.Configuration.ConfigurationManager.AppSettings["t_sys_files_Numbers"].Split(',').ToList();
        private static List<string> doubleFields = System.Configuration.ConfigurationManager.AppSettings["t_sys_files_Double"].Split(',').ToList();
        private static List<string> dateFields = System.Configuration.ConfigurationManager.AppSettings["t_sys_files_Dt"].Split(',').ToList();
        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["t_sys_files_Pks"].Split(',');
        private static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["t_sys_files_ExchangeName"];
        private static string routingKey = System.Configuration.ConfigurationManager.AppSettings["t_sys_files_RoutingKey"]; 
        private static string esType = System.Configuration.ConfigurationManager.AppSettings["t_sys_files_EsType"];
        private static string esDocType = System.Configuration.ConfigurationManager.AppSettings["t_sys_files_EsDocType"];


        public TsysfilesUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {
        }

        public TsysfilesUpload(Dictionary<string, string> dicData, byte[] cmdAttach, bool needFilePath) : base(dicData, cmdAttach, needFilePath)
        {
        }



        public override bool PushJsonToMq(JObject jobj, out string errorMsg)
        {
            return PushJsonToRabbitMq(jobj, exchangeName, routingKey, logger, out errorMsg);
        }

        public override JObject ConstructNeedToBePushedJson()
        {
            return ConstructJsonFromDictionary(excludeFields, numberFields, dateFields, doubleFields);
        }

        public override bool PrePushToRabbitMq(out string fileSavePath, out string errorMsg)
        {
            fileSavePath = string.Empty;
            errorMsg = string.Empty;

            string sysPrimaryKey = string.Empty;
            string unitcode = string.Empty;
            if (!dicData.TryGetValue("UNITCODE", out unitcode))
            {
                errorMsg = "t_sys_files数据缺少UNITCODE";
                return false;
            }

            string fkey = string.Empty;
            if (!dicData.TryGetValue("FKEY", out fkey))
            {
                errorMsg = "t_sys_files数据缺少fkey";
                return false;
            }

            sysPrimaryKey = unitcode + fkey;
            dicData["SYSPRIMARYKEY"] = sysPrimaryKey;

            string filename = string.Empty;
            if (!dicData.TryGetValue("FILENAME", out filename))
            {
                filename = "";
            }
            if (string.IsNullOrEmpty(filename))
            {
                filename = Guid.NewGuid() + "noname.doc";
            }
            try
            {
                #region 附件
               
                if (cmdAttach != null)
                { 
                    string relFilePath = string.Format("{0}\\{1}\\{2}",
                        FSSelector.GetSafeFilename(customId),
                        DateTime.Now.ToString("yyyy-MM"),
                        FSSelector.GetSafeFilename(filename));
                    string baseFileDir = FSSelector.GetFSYiWuNewCReportUrl(customId);
                    string filePath = Path.Combine(baseFileDir, relFilePath);

                    string fileFolder = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(fileFolder))
                    {
                        Directory.CreateDirectory(fileFolder);
                    }

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    try
                    {
                        File.WriteAllBytes(filePath, cmdAttach);
                        fileSavePath = filePath;

                        //hasData = true;
                        //dicData["REALDATAPATH"] = filePath;//平台的路径
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



        public override string GetPrimaryKey()
        {
            return ConstructPrimayKey(dicData, primaryFields);
        }

        public override string GetTableName()
        {
            return "t_sys_files";
        }

        public override void PostConstructJson(JObject jobj)
        {
            jobj.Add(new JProperty(elasticType, esType));
            jobj.Add(new JProperty(elasticDocType, esDocType));
        }
    }
}