using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlatFormUpload.ServiceInterface
{
    public class CovrlistUpload: UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> excludeFields = System.Configuration.ConfigurationManager.AppSettings["covrlist_excluded"].Split(',').ToList();
        private static List<string> numberFields = System.Configuration.ConfigurationManager.AppSettings["covrlist_Numbers"].Split(',').ToList();
        private static List<string> dateFields = System.Configuration.ConfigurationManager.AppSettings["covrlist_Dt"].Split(',').ToList();
        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["covrlist_Pks"].Split(',');
        private static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["covrlist_ExchangeName"];
        private static string routingKey = System.Configuration.ConfigurationManager.AppSettings["covrlist_RoutingKey"];
        private static string esType = System.Configuration.ConfigurationManager.AppSettings["covrlist_EsType"];
        private static string esDocType = System.Configuration.ConfigurationManager.AppSettings["covrlist_EsDocType"];

        public CovrlistUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
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
            return "covrlist";
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
                errorMsg = "现场检测数据缺少customID";
                return false;
            }
            string sysPrimaryKey = string.Empty;
            if (!dicData.TryGetValue("SYSPRIMARYKEY", out sysPrimaryKey))
            {
                errorMsg = "现场检测数据缺少sysprimarykey";
                return false;
            }
            string unitcode = string.Empty;
            if (!dicData.TryGetValue("UNITCODE", out unitcode))
            {
                errorMsg = "现场检测数据缺少unitcode";
                return false;
            }
            string Code = string.Empty;
            if (!dicData.TryGetValue("CODE", out Code))
            {
                errorMsg = "现场检测数据缺少Code";
                return false;
            }
            return true;
        }

        public override bool PushJsonToMq(JObject jobj, out string errorMsg)
        {
            return PushJsonToRabbitMq(jobj, exchangeName, routingKey, logger, out errorMsg);
        }
    }
}