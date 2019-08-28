using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using NLog;
using System.IO;
using PlatFormUpload.Common;
using System.Configuration;

namespace PlatFormUpload.ServiceInterface
{
    public class WordReportListUpload : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string worldReportFolder = ConfigurationManager.AppSettings["wordReportDataFolder"];
        private static List<string> excludeFields = ConfigurationManager.AppSettings["wordreportlist_excluded"].Split(',').ToList();
        private static List<string> numberFields = ConfigurationManager.AppSettings["wordreportlist_Numbers"].Split(',').ToList();
        private static List<string> dateFields = ConfigurationManager.AppSettings["wordreportlist_Dt"].Split(',').ToList();
        private static string[] primaryFields = ConfigurationManager.AppSettings["wordreportlist_Pks"].Split(',');
        private static string exchangeName = ConfigurationManager.AppSettings["wordreportlist_ExchangeName"];
        private static string routingKey = ConfigurationManager.AppSettings["wordreportlist_RoutingKey"];
        private static string attachExchangeName= ConfigurationManager.AppSettings["wordreportlist_AttachExchangeName"];
        private static string attachRoutingKey= ConfigurationManager.AppSettings["wordreportlist_AttachRoutingKey"];
        private static string esType = ConfigurationManager.AppSettings["wordreportlist_EsType"];
        private static string esDocType = ConfigurationManager.AppSettings["wordreportlist_EsDocType"]; 
        private static string largeWordReportFolder= ConfigurationManager.AppSettings["LargeWordDataFolder"];
        private static int limitedSized = Int32.Parse(ConfigurationManager.AppSettings["wordreportlist_LimitedSizeMForRavenDB"]);


        public WordReportListUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {
        }
        public WordReportListUpload(Dictionary<string, string> dicData, byte[] cmdAttach, bool needFilePath) : base(dicData, cmdAttach, needFilePath)
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
            return "wordreportlist";
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
                if (!dicData.TryGetValue("REPORTTYPES", out suffix))
                {
                    suffix = "doc";
                }

                if(string.IsNullOrWhiteSpace(suffix))
                {
                    suffix = "doc";
                }

                //附件___blob___

                //bool hasData = false;

                if (cmdAttach != null)
                {
                    
                    #region 附件处理

                    string relativePath = string.Format("{0}\\{1}\\{2}\\{3}.{4}",
                        FSSelector.GetSafeFilename(customId),
                        DateTime.Now.ToString("yyyy-MM"),
                        FSSelector.GetSafeFilename(sysPrimaryKey),
                        FSSelector.GetSafeFilename(pk), suffix);

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

                        //hasData = true;
                        //dicData["WORDREPORTPATH"] = filePath;
                        //dicData["ISLOCALSTORE"] = "0";
                        //if (cmdAttach.Length >= limitedSized * 1024 * 1024)
                        //{
                        //    //标识为大型文件，通过wcf的stream模式传输
                        //    dicData["ISLOCALSTORE"] = "1";
                        //}
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

                #region old code

                //if (dicData.ContainsKey("IMAGE") && cmdAttach != null)
                //{
                //    var binaryIndexs = dicData["IMAGE"];
                //    if (binaryIndexs.Contains("___blob___"))
                //    {
                //        var bData = GetBinaryByIndexs(binaryIndexs);
                //        if (bData != null)
                //        {
                //            //
                //            #region 附件处理

                //            string relativePath = string.Format("{0}\\{1}\\{2}\\{3}.{4}",
                //                FSSelector.GetSafeFilename(customId),
                //                DateTime.Now.ToString("yyyy-MM"),
                //                FSSelector.GetSafeFilename(sysPrimaryKey),
                //                FSSelector.GetSafeFilename(pk),
                //                suffix);
                //            string baseFileDir = FSSelector.GetFSWordUrl(customId);
                //            string filePath = Path.Combine(baseFileDir, relativePath);

                //            string fileFolder = Path.GetDirectoryName(filePath);
                //            if (!Directory.Exists(fileFolder))
                //            {
                //                Directory.CreateDirectory(fileFolder);
                //            }

                //            try
                //            {
                //                File.WriteAllBytes(filePath, bData);
                //                hasData = true;
                //                dicData["WORDREPORTPATH"] = filePath;
                //                dicData["ISLOCALSTORE"] = "0";
                //                if (bData.Length >= limitedSized * 1024 * 1024)
                //                {
                //                    //标识为大型文件，通过wcf的stream模式传输
                //                    dicData["ISLOCALSTORE"] = "1";
                //                }
                //            }
                //            catch (Exception ex)
                //            {
                //                LoggerHelper.WriteErrorLog(logger, ex);
                //                errorMsg = string.Format("[ErrorMsg:{0}]-[StackTrace:{1}]-[Source:{2}]",
                //                    ex.Message,
                //                    ex.StackTrace,
                //                    ex.Source);
                //                return false;
                //            }


                //            #endregion

                //            //Task.Factory.StartNew(() => SaveToRavenDB(filePath, ms));
                //        }
                        
                //    }
                //}

                #endregion

                //if (!hasData)
                //{
                //    dicData["NODATA"] = "1";
                //    LoggerHelper.WriteCustomLog(logger, "无附件 " + cmdData, "CWorkNoReport", false);
                //}

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