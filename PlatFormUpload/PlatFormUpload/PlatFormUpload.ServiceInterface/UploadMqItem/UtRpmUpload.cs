using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using NLog;

namespace PlatFormUpload.ServiceInterface
{
    public class UtRpmUpload : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> excludeFields = System.Configuration.ConfigurationManager.AppSettings["ut_rpm_excluded"].Split(',').ToList();
        private static List<string> numberFields = System.Configuration.ConfigurationManager.AppSettings["ut_rpm_Numbers"].Split(',').ToList();
        private static List<string> floatFields = System.Configuration.ConfigurationManager.AppSettings["ut_rpm_Float"].Split(',').ToList();
        private static List<string> dateFields = System.Configuration.ConfigurationManager.AppSettings["ut_rpm_Dt"].Split(',').ToList();
        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["ut_rpm_Pks"].Split(',');
        private static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["ut_rpm_ExchangeName"];
        private static string routingKey = System.Configuration.ConfigurationManager.AppSettings["ut_rpm_RoutingKey"];
        private static string esType = System.Configuration.ConfigurationManager.AppSettings["ut_rpm_EsType"];
        private static string esDocType = System.Configuration.ConfigurationManager.AppSettings["ut_rpm_EsDocType"];
        private string TableName = string.Empty;

        public UtRpmUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {
           
        }

        public string TableToItemName(string TableName)
        {
            return GetUtRpmTableName(TableName);
        }

        private string GetUtRpmTableName(string utRpmTableName)
        {
            if (string.IsNullOrWhiteSpace(utRpmTableName))
            {
                return string.Empty;
            }

            //ut_rpm_siih  返回最后面的siih
            //去掉switch 语句，可以在原材料进场表增加之后也能正常work
            var tableNames = utRpmTableName.Split('_');

            string tableName = string.Empty;
            if (tableNames.Length == 1)
            {
                tableName = utRpmTableName.Substring(Math.Max(0, utRpmTableName.Length - 4));
                if (tableName == "tive")
                {
                    tableName = "HJSJ";
                }
            }
            else if (tableNames.Length > 1)
            {
                tableName = tableNames[tableNames.Length - 1];
                if (tableName == "additive")
                {
                    tableName = "HJSJ";
                }
            }

            return tableName;
        }

        public override JObject ConstructNeedToBePushedJson()
        {
            return ConstructJsonFromDictionary(excludeFields, numberFields, dateFields, floatFields);
        }

        public override string GetPrimaryKey()
        {
            string pk = TableName + "^" + ConstructPrimayKey(dicData, primaryFields);
            return pk;
        }

        public override string GetTableName()
        {
            return TableToItemName(TableName);
        }

        public override void PostConstructJson(JObject jobj)
        {
            string ItemName = TableToItemName(TableName);
            if (!jobj.ContainsKey("ITEMNAME"))
            {
                jobj.Add(new JProperty("ITEMNAME", ItemName.ToUpper()));
            }
            else
            {
                jobj["ITEMNAME"] = jobj["ITEMNAME"].ToString().ToUpper();//转成大写
            }
            jobj["ITEMCUSTOMNAME"] = ItemName;
            if (jobj.ContainsKey("SAMPLENAME") && jobj["SAMPLENAME"].HasValues && ((string)jobj["SAMPLENAME"]).IndexOf("膨胀剂")>-1)
            {
                jobj["ITEMCUSTOMNAME"] = "HJSJPZJ";
            }
            //增加留样数量字段 根据留样编码（多个按逗号隔开）计算 
            if (jobj.ContainsKey("SAMPLENUM"))
            {
                string Jypc = string.Empty;
                string CustomId = string.Empty;
                string EntryKey = string.Empty;//001pc00127S0001 002pc00127001 003pc00127S0001
                JToken JValue;
                if (jobj.TryGetValue("JYPC", out JValue))
                {
                    Jypc = JValue.ToString();
                }
                if (jobj.TryGetValue("CUSTOMID", out JValue))
                {
                    CustomId = JValue.ToString();
                }
                var SampleNums = jobj["SAMPLENUM"].ToString().Split(',');
                EntryKey=string.Join(" ", SampleNums.Select(s => (s + Jypc + CustomId)));
                jobj["ENTRYKEY"] = EntryKey;
                var SampleCount = SampleNums.Length;
                jobj["SAMPLECOUNT"] = SampleCount;
            }
            jobj.Add(new JProperty(elasticType, esType));
            jobj.Add(new JProperty(elasticDocType, esDocType));
        }

        public override bool PrePushToRabbitMq(out string fileSavePath, out string errorMsg)
        {
            fileSavePath = string.Empty;
            errorMsg = string.Empty;
            return true;
        }

        public override bool PushJsonToMq(JObject jobj, out string errorMsg)
        {
            return PushJsonToRabbitMq(jobj, exchangeName, routingKey, logger, out errorMsg);
        }
    }
}