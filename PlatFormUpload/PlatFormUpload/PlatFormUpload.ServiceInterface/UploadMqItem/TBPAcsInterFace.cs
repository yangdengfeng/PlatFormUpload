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
    public class TBPAcsInterFace : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string arcDataFolder = System.Configuration.ConfigurationManager.AppSettings["arcDataFolder"];
        private static List<string> excludeFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_acsInterFace_excluded"].Split(',').ToList();
        private static List<string> numberFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_acsInterFace_Numbers"].Split(',').ToList();
        private static List<string> doubleFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_acsInterFace_Double"].Split(',').ToList();
        private static List<string> dateFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_acsInterFace_Dt"].Split(',').ToList();
        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_acsInterFace_Pks"].Split(',');
        private static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["t_bp_acsInterFace_ExchangeName"];
        private static string routingKey = System.Configuration.ConfigurationManager.AppSettings["t_bp_acsInterFace_RoutingKey"]; 
        private static string esType = System.Configuration.ConfigurationManager.AppSettings["t_bp_acsInterFace_EsType"];
        private static string esDocType = System.Configuration.ConfigurationManager.AppSettings["t_bp_acsInterFace_EsDocType"];

        public TBPAcsInterFace(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {
        }
        public TBPAcsInterFace(Dictionary<string, string> dicData, byte[] cmdAttach, bool needFilePath) : base(dicData, cmdAttach, needFilePath)
        {
        }
       

        public override bool PushJsonToMq(JObject jobj, out string errorMsg)
        {
            if (string.IsNullOrWhiteSpace(customId))
            {
                errorMsg = "曲线数据缺少customID";
                return false;
            }

            if (string.IsNullOrWhiteSpace(itemName))
            {
                errorMsg = "曲线数据缺少检测项目";
                return false;
            }

            string sysPrimaryKey = string.Empty;
            if (!dicData.TryGetValue("SYSPRIMARYKEY", out sysPrimaryKey))
            {
                errorMsg = "曲线数据缺少SYSPRIMARYKEY";
                return false;
            }

            string acsTime = string.Empty;
            DateTime acsDt = DateTime.Today;
            if (!dicData.TryGetValue("ACSTIME", out acsTime))
            {
                errorMsg = "曲线数据缺少曲线时间";
                return false;
            }
            else
            {
                if (!DateTime.TryParse(acsTime, out acsDt))
                {
                    errorMsg = "曲线时间不正确！";
                }
            }

            string sampleName = string.Empty;
            if (!dicData.TryGetValue("SAMPLENUM", out sampleName))
            {
                errorMsg = "曲线数据缺少样本编号";
                return false;
            }

            return PushJsonToRabbitMq(jobj, exchangeName, routingKey, logger, out errorMsg);
        }

        public override JObject ConstructNeedToBePushedJson()
        {
            return ConstructJsonFromDictionary( excludeFields, numberFields, dateFields, doubleFields);
        }

        public override bool PrePushToRabbitMq(out string fileSavePath, out string errorMsg)
        {
            fileSavePath = string.Empty;
            errorMsg = string.Empty; 
            if (string.IsNullOrWhiteSpace(customId))
            {
                errorMsg = "曲线数据缺少customID";
                return false;
            }

            if (string.IsNullOrWhiteSpace(itemName))
            {
                errorMsg = "曲线数据缺少检测项目";
                return false;
            }

            string sampleName = string.Empty;
            if(!dicData.TryGetValue("SAMPLENUM",out sampleName))
            {
                errorMsg = "曲线数据缺少样本编号";
                return false;
            }  

            try
            {
                #region 附件
               
                if (cmdAttach != null)
                {
                    sampleName = FSSelector.GetSafeFilename(sampleName);

                    // 270001/2017-10/CEMT/17Q030200-OO1111/PK 格式
                    string relFilePath = string.Format("{0}\\{1}\\{2}\\{3}\\{4}.txt",
                        FSSelector.GetSafeFilename(customId),
                        DateTime.Now.ToString("yyyy-MM"),
                        FSSelector.GetSafeFilename(itemName),
                        FSSelector.GetSafeFilename(sampleName),
                        FSSelector.GetSafeFilename(pk));
                    string baseFileDir = FSSelector.GetFSAcsUrl(customId);
                    string filePath = Path.Combine(baseFileDir, relFilePath);

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

       

        public override string GetPrimaryKey()
        {
            return ConstructPrimayKey(dicData, primaryFields);
        }

        public override string GetTableName()
        {
            return "t_bp_acsInterFace";
        }

        public override void PostConstructJson(JObject jobj)
        {
            jobj.Add(new JProperty(elasticType, esType));
            jobj.Add(new JProperty(elasticDocType, esDocType));
        }
    }
}