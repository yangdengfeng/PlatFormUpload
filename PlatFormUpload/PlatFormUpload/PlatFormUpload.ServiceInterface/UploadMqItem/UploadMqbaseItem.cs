
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using PlatFormUpload.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PlatFormUpload.ServiceInterface
{
    public abstract class UploadMqbaseItem
    {
       
        protected byte[] cmdAttach;
        protected string customId;
        protected string itemName;
        protected string filePath;
        protected bool needFilePath;
        protected string dataAction;
        protected Dictionary<string, string> dicData;
        protected string pk;

       
        

        /// <summary>
        /// 用了区分logstash多个output
        /// </summary>
        protected static string elasticType= System.Configuration.ConfigurationManager.AppSettings["ElasticType"];

        /// <summary>
        /// elasticSearch中的document type
        /// </summary>
        protected static string elasticDocType = System.Configuration.ConfigurationManager.AppSettings["ElasticDocType"];

        private static bool whiteListEnabled = System.Configuration.ConfigurationManager.AppSettings["WhiteListEnable"] == "1";
        private static bool blackListEnabled = System.Configuration.ConfigurationManager.AppSettings["BlackListEnable"] == "1";

        private static HashSet<string> customWhiteList = new HashSet<string>(System.Configuration.ConfigurationManager.AppSettings["CustomIdWhiteList"].Split('|'));
        private static HashSet<string> customBlackList = new HashSet<string>(System.Configuration.ConfigurationManager.AppSettings["CustomIdBlackList"].Split('|'));

        public UploadMqbaseItem(Dictionary<string, string> dicData, byte[] cmdAttach)
        {
            
            this.cmdAttach = cmdAttach;
            this.needFilePath = false;
            this.dicData = dicData;
            customId = string.Empty;
            itemName = string.Empty;
            filePath = string.Empty;
            dataAction = string.Empty;
            pk = string.Empty;
        }

        public UploadMqbaseItem(Dictionary<string, string> dicData, byte[] cmdAttach, bool needFilePath)
        {
         
            this.cmdAttach = cmdAttach;
            this.needFilePath = needFilePath;
            this.dicData = dicData;
            customId = string.Empty;
            itemName = string.Empty;
            filePath = string.Empty;
            dataAction = string.Empty;
            pk = string.Empty;
        }

        protected string GetCustomId()
        {
            if (dicData == null)
            {
                return string.Empty;
            }

            string customId = string.Empty;
            if (!dicData.TryGetValue("CUSTOMID", out customId))
            {
                customId = string.Empty;
            }

            if (customId.Length == 0)
            {
                string sysPrimaryKey = string.Empty;
                if (!dicData.TryGetValue("SYSPRIMARYKEY", out sysPrimaryKey))
                {
                    customId = string.Empty;
                }
                else
                {
                    if (sysPrimaryKey != null && sysPrimaryKey.Length > 7)
                    {
                        customId = sysPrimaryKey.Substring(0, 7);
                    }
                    else
                    {
                        customId = string.Empty;
                    }
                }
            }
            if (customId.Length == 0)//混凝土生产数据
            {
                if(!dicData.TryGetValue("WORKSTATIONID", out customId))
                {
                    customId = string.Empty;
                }
            }
            if (customId.Length == 0)//原材料进场
            {
                if (!dicData.TryGetValue("CUSTOMID", out customId))
                {
                    customId = string.Empty;
                }
            }
            return customId;
        }

        protected string GetItemName()
        {

            if (dicData == null)
            {
                return string.Empty;
            }

            string itemName = string.Empty;
            if (!dicData.TryGetValue("ITEMNAME", out itemName))
            {
                if (!dicData.TryGetValue("ITEMTABLENAME", out itemName))
                {
                    itemName = string.Empty;
                }
            }

            return itemName;
        }

        protected string GetFilePath()
        {

            if (dicData == null)
            {
                return string.Empty;
            }

            string filePath = string.Empty;
            if (!dicData.TryGetValue("FILEPATH", out filePath))
            {
                filePath = string.Empty;
            }

            return filePath;
        }

        protected bool CanClientUploadData(string customId)
        {
            if (string.IsNullOrWhiteSpace(customId))
            {
                return false;
            }

            //黑名单
            if (blackListEnabled)
            {
                if (customBlackList.Contains(customId))
                {
                    return false;
                }
            }

            //先白名单
            if (whiteListEnabled)
            {
                if (customWhiteList.Contains(customId))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        protected string GetSysPrimaryKey(Dictionary<string, string> dicData)
        {
            if (dicData == null)
            {
                return string.Empty;
            }

            string sysPrimayKey = string.Empty;
            if (!dicData.TryGetValue("SYSPRIMARYKEY", out sysPrimayKey))
            {
                sysPrimayKey = string.Empty;
            }

            return sysPrimayKey;
        }

        protected string ConstructPrimayKey(Dictionary<string, string> dicData, params string[] keyColumns)
        {
            if (dicData == null)
            {
                return string.Empty;
            }

            List<string> keyValues = new List<string>();
            bool primaryKeyExist = true;
            foreach (var item in keyColumns)
            {
                string keyValue = string.Empty;

                if(dicData.TryGetValue(item,out keyValue))
                {
                    keyValues.Add(keyValue);
                }
                else
                {
                    primaryKeyExist = false;
                    break;
                }
            }

            if(!primaryKeyExist)
            {
                return string.Empty;
            }
            else
            {
                return string.Join("^", keyValues);
            }
        }

        protected JObject ConstructJsonFromDictionary(
            List<string> excludeFields,
            List<string> numberFields,
            List<string> dateFields,
            List<string> doubleFields = null)
        {
            JObject jobj = new JObject();

            string esDtFormat = "yyyy-MM-dd'T'HH:mm:ss";
            foreach (var dicKV in dicData)
            {
                if (string.IsNullOrWhiteSpace(dicKV.Key)
                    || string.IsNullOrWhiteSpace(dicKV.Value))
                {
                    continue;
                }

                //字段排除
                if (excludeFields.Contains(dicKV.Key.ToUpper()))
                {
                    continue;
                }


                //int 类型的处理
                if (numberFields.Contains(dicKV.Key.ToUpper()))
                {
                    int i = 0;
                    if (int.TryParse(dicKV.Value, out i))
                    {
                        jobj.Add(new JProperty(dicKV.Key, i));
                    }
                    continue;
                }

                //double 类型处理
                if (doubleFields != null && doubleFields.Count > 0)
                {
                    if (doubleFields.Contains(dicKV.Key.ToUpper()))
                    {
                        double d = 0.0;
                        if (double.TryParse(dicKV.Value, out d))
                        {
                            jobj.Add(new JProperty(dicKV.Key, d));
                        }

                        continue;
                    }
                }


                //datetime类型进行自动处理，不需要从配置文件中读取，以便于后续新增字段自动转换为datetime

                //通过regex先匹配 过滤 376.5也会转换为datetime的情况
                // yyyy-mm-dd
                // yyyy/mm/dd
                bool isMath = Regex.IsMatch(dicKV.Value, @"(\d{4}[/-](0[1-9]|[1-9]|10|11|12)[/-](0[1-9]|[1-9]|[12][0-9]|3[01]))", RegexOptions.IgnoreCase);
                if (isMath)
                {
                    DateTime dt = DateTime.Today;
                    if (DateTime.TryParse(dicKV.Value, out dt))
                    {
                        jobj.Add(new JProperty(dicKV.Key, dt.ToString(esDtFormat)));
                        continue;
                    }
                }
                
                jobj.Add(new JProperty(dicKV.Key, dicKV.Value));
            }

            //去掉了 logstash的@timetamp，这里手动加上上传时间
            if (dataAction == "ADD")
            {
                jobj.Add(new JProperty("UPLOADTIME", DateTime.Now.ToString(esDtFormat)));
                jobj.Add(new JProperty("UPDATETIME", DateTime.Now.ToString(esDtFormat)));
            }
            if (dataAction == "UPDATE")
            {
                jobj.Add(new JProperty("UPDATETIME", DateTime.Now.ToString(esDtFormat)));
            }


            return jobj;
        }

        /// <summary>
        /// 推送数据到MQ 不处理附件
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public ResultBase<string> StartUploadDataItem(Logger logger)
        {
           
            if (dicData == null || dicData.Count == 0)
            {
                return ResultBase<string>.Failure("json数据为空或者数据格式错误");
            }

            customId = GetCustomId();
            itemName = GetItemName();
            filePath = GetFilePath();
            pk = GetPrimaryKey();

            //判断能否上传
            bool canUpload = CanClientUploadData(customId);
            if (!canUpload)
            {
                string error = "Blacklist! Restrict Upload!";
                string message = string.Format("[{0}]-[{1}]-[{2}]", error, GetTableName(), dicData);
                LoggerHelper.WriteCustomLog(logger, message, customId, false);
                return ResultBase<string>.Failure(error);
            }

            //判断是否有主键
            if (string.IsNullOrWhiteSpace(pk))
            {
                string error = "Data Has No PrimaryKey";
                string message = string.Format("[{0}]-[{1}]-[{2}]", error, GetTableName(), dicData);
                LoggerHelper.WriteCustomLog(logger, message, customId, false);
                return ResultBase<string>.Failure(error);
            }

            //是否有附件的上传保存路径
            if (needFilePath && string.IsNullOrWhiteSpace(filePath))
            {
                string error = "Attachment FilePath Is Empty";
                string message = string.Format("[{0}]-[{1}]-[{2}]", error, GetTableName(), dicData);
                LoggerHelper.WriteCustomLog(logger, message, customId, false);
                return ResultBase<string>.Failure(error);
            }

            JObject jobj = ConstructNeedToBePushedJson();
            if (!string.IsNullOrWhiteSpace(customId))
            {
                jobj["CUSTOMID"] = customId;
            }
            jobj.Add(new JProperty("PK", pk));
            PostConstructJson(jobj);

            string errorMsg = string.Empty;
            bool isPushToMqSucc = PushJsonToMq(jobj, out errorMsg);
            if (!isPushToMqSucc)
            {
                string message = string.Format("[{0}]-[{1}]-[{2}]", errorMsg, GetTableName(), dicData);
                LoggerHelper.WriteCustomLog(logger, message, customId, false);
                return ResultBase<string>.Failure(errorMsg);
            }
            else
            {
                string message = string.Format("[{0}]-[{1}]", GetTableName(), dicData);
                LoggerHelper.WriteCustomLog(logger, message, customId, true);
                return ResultBase<string>.Success();
            }
        }

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public ResultBase<string> StartUploadAttachmentItem(Logger logger)
        {
            
            if (dicData == null || dicData.Count == 0)
            {
                return ResultBase<string>.Failure("json数据为空或者数据格式错误");
            }

            customId = GetCustomId();
            itemName = GetItemName();
            pk = GetPrimaryKey();

            //判断能否上传
            bool canUpload = CanClientUploadData(customId);
            if (!canUpload)
            {
                string error = "Blacklist! Restrict Upload!";
                string message = string.Format("[{0}]-[{1}]-[{2}]", error, GetTableName(), dicData);
                LoggerHelper.WriteCustomLog(logger, message, customId, false);
                return ResultBase<string>.Failure(error);
            }

            //判断是否有主键
            if (string.IsNullOrWhiteSpace(pk))
            {
                string error = "Data Has No PrimaryKey";
                string message = string.Format("[{0}]-[{1}]-[{2}]", error, GetTableName(), dicData);
                LoggerHelper.WriteCustomLog(logger, message, customId, false);
                return ResultBase<string>.Failure(error);
            }

            string filePath = string.Empty;
            string errorMsg = string.Empty;
            bool isPrePushMqSucc = PrePushToRabbitMq(out filePath, out errorMsg);
            if (!isPrePushMqSucc)
            {
                string message = string.Format("[{0}]-[{1}]-[{2}]", errorMsg, GetTableName(), dicData);
                LoggerHelper.WriteCustomLog(logger, message, customId, false);
                return ResultBase<string>.Failure(errorMsg);
            }
            else
            {
                string message = string.Format("[{0}]-[{1}]", GetTableName(), dicData);
                LoggerHelper.WriteCustomLog(logger, message, customId, true);
                return ResultBase<string>.Success(filePath);
            }
        }


        private bool CanPassWhenNoAttach()
        {
            if (dicData.ContainsKey("NODATA"))
            {
                return dicData["NODATA"] == "1";
            }

            return false;
        }

        public abstract string GetTableName();

        /// <summary>
        /// 构建json之后需要的其他操作入口
        /// </summary>
        /// <param name="jobj">已经构建好的json</param>
        public abstract void PostConstructJson(JObject jobj);

        /// <summary>
        /// 将json对象push到mq队列中
        /// </summary>
        /// <param name="jobj">json 对象</param>
        /// <param name="errorMsg">失败的错误信息</param>
        /// <returns>操作是否成功</returns>
        public abstract bool PushJsonToMq(JObject jobj, out string errorMsg);

        /// <summary>
        /// 从Dic构建Json对象
        /// </summary>
        /// <param name="dicData">Dic对象</param>
        /// <returns>Json对象</returns>
        public abstract JObject ConstructNeedToBePushedJson();

        /// <summary>
        /// 在将json对象推送到mq之前的操作，一般是二进制文件的保持
        /// </summary>
        /// <param name="filePath">附件上传存放路径</param>
        /// <param name="errorMsg">失败的错误信息</param>
        /// <returns>操作是否成功</returns>
        public abstract bool PrePushToRabbitMq(out string filePath, out string errorMsg);

        /// <summary>
        /// 获取主键
        /// </summary>
        /// <param name="dicData">Dic对象</param>
        /// <returns>主键</returns>
        public abstract string GetPrimaryKey();

        protected bool PushJsonToRabbitMq(JObject jobj, string exchangeName, string routeName, Logger logger, out string errorMsg)
        {
            errorMsg = string.Empty;

            bool pushSucc = true;

            pushSucc = RabbitMqHelper.PushJsonToRabbitMq(jobj, exchangeName, routeName, logger, out errorMsg);

            return pushSucc;
        }

        //protected bool PushAttachToRabbitMq(AttachMqMessage attachMessage, string exchangeName, string routeName, Logger logger, out string errorMsg)
        //{
        //    errorMsg = string.Empty;

        //    bool pushSucc = true;

        //    try
        //    {
        //        using (IConnection conn = RabbitMqFactoryHolder.RabbitFactory.CreateConnection())
        //        using (IModel channel = conn.CreateModel())
        //        {
        //            var props = channel.CreateBasicProperties();
        //            props.SetPersistent(true);

        //            var jsonStr = JsonConvert.SerializeObject(attachMessage);

        //            var msgBody = Encoding.UTF8.GetBytes(jsonStr);

        //            channel.BasicPublish(exchangeName, routingKey: routeName, basicProperties: props, body: msgBody);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggerHelper.WriteErrorLog(logger, ex);
        //        pushSucc = false;
        //        errorMsg = ex.Message;
        //    }

        //    return pushSucc;
        //}


        protected byte[] GetBinaryByIndexs(string bIndexStr)
        {
            string[] binaryIndexs = bIndexStr.Split(new string[] { "___blob___" }, StringSplitOptions.RemoveEmptyEntries);
            int begin = 0; //Convert.ToInt32(myAr[0]);
            int end = 0; //Convert.ToInt32(myAr[1]);
            byte[] data = null;

            if (binaryIndexs.Length < 2)
            {
                return data;
            }

            if (int.TryParse(binaryIndexs[0], out begin)
                && int.TryParse(binaryIndexs[1], out end))
            {
                if (cmdAttach.Length > end)
                {
                    data = new byte[(end - begin) + 1];
                    Buffer.BlockCopy(cmdAttach, begin, data, 0, end - begin + 1);
                }
            }


            return data;
        }

        protected Dictionary<string, string> ParseDataFromJsonStr(string data)
        {
           // "Data": {
           //     "CUSTOMID_": "26J109A",
           //     "STATIONID": "1910021",
           //     "QUESTIONPRIMARYKEY": "26J109A1900000000101",
           //     "SYSPRIMARYKEY": "26J109A1900000000352"
           //  }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(data))
            {
                return dic;
            }

            dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

            return dic;
        }

        protected Dictionary<string, string> ParseDataFromStr(string data)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(data))
            {
                return dic;
            }

            string[] outerArrs = data.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var outItem in outerArrs)
            {
                string[] innerArrs = outItem.Split(new string[] { "=" }, StringSplitOptions.None);
                if (innerArrs.Length < 2)
                {
                    continue;
                }
                string key = innerArrs[0].ToUpper().Trim();
                string value = f_replaceValue(innerArrs[1], 0);
                if (value == "NAN")
                {
                    continue;
                }
                value = value.Replace("''", "____$____").Replace("'", "''").Replace("____$____", "''");
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, value.Trim());
                }
            }
            return dic;
        }

        protected string f_replaceValue(string text, int type)
        {
            if (Regex.IsMatch(text, @"\d{4}[-/]\d{1,2}[-/]\d{1,2} \d{1,2}:\d{1,2}:\d{1,2}[\.?\d{0,3}]"))
            {
                return text;
            }
            if (type == 0)
            {
                return text.Replace("&", "___blob___").Replace("%3D", "=").Replace("%2C", ",").Replace("%26", "&").Replace("%3E", "''");
            }
            else
            {
                return text.Replace("=", "%3D").Replace(",", "%2C").Replace("&", "%26").Replace("'", "%3E");
            }
        }
    }
}