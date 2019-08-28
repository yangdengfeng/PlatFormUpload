using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using NLog;

namespace PlatFormUpload.ServiceInterface
{
    public class TBpTONOCurUpload : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> excludeFields = System.Configuration.ConfigurationManager.AppSettings["tono_cur_excluded"].Split(',').ToList();
        private static List<string> numberFields = System.Configuration.ConfigurationManager.AppSettings["tono_cur_Numbers"].Split(',').ToList();
        private static List<string> floatFields = System.Configuration.ConfigurationManager.AppSettings["tono_cur_Float"].Split(',').ToList();
        private static List<string> dateFields = System.Configuration.ConfigurationManager.AppSettings["tono_cur_Dt"].Split(',').ToList();
        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["tono_cur_Pks"].Split(',');
        private static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["tono_cur_ExchangeName"];
        private static string routingKey = System.Configuration.ConfigurationManager.AppSettings["tono_cur_RoutingKey"];
        private static string esType = System.Configuration.ConfigurationManager.AppSettings["tono_cur_EsType"];
        private static string esDocType = System.Configuration.ConfigurationManager.AppSettings["tono_cur_EsDocType"];

        public TBpTONOCurUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {
        }

        public override JObject ConstructNeedToBePushedJson()
        {
            return ConstructJsonFromDictionary(excludeFields, numberFields, dateFields, floatFields);
        }

        public override string GetPrimaryKey()
        {
            string pk = ConstructPrimayKey(dicData, primaryFields);
            return pk;
        }

        public override string GetTableName()
        {
            return "tono_cur";
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
            return true;
        }

        public override bool PushJsonToMq(JObject jobj, out string errorMsg)
        {
            return PushJsonToRabbitMq(jobj, exchangeName, routingKey, logger, out errorMsg);
        }
    }
}