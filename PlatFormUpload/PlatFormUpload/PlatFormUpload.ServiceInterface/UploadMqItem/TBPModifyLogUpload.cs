using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlatFormUpload.ServiceInterface
{
    public class TBPModifyLogUpload : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> excludeFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_modify_log_excluded"].Split(',').ToList();
        private static List<string> numberFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_modify_log_Numbers"].Split(',').ToList();
        private static List<string> dateFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_modify_log_Dt"].Split(',').ToList();
        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["t_bp_modify_log_Pks"].Split(',');
        private static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["t_bp_modify_log_ExchangeName"];
        private static string routingKey = System.Configuration.ConfigurationManager.AppSettings["t_bp_modify_log_RoutingKey"];
        private static string esType = System.Configuration.ConfigurationManager.AppSettings["t_bp_modify_log_EsType"];
        private static string esDocType = System.Configuration.ConfigurationManager.AppSettings["t_bp_modify_log_EsDocType"];


        public TBPModifyLogUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {

        }


        public override bool PushJsonToMq(JObject jobj, out string errorMsg)
        {
            return PushJsonToRabbitMq(jobj, exchangeName, routingKey, logger, out errorMsg);
        }

        public override JObject ConstructNeedToBePushedJson()
        {
            return ConstructJsonFromDictionary(excludeFields, numberFields, dateFields);
        }

        public override bool PrePushToRabbitMq(out string fileSavePath, out string errorMsg)
        {
            fileSavePath = string.Empty;
            errorMsg = string.Empty;
            return true;
        }

        public override string GetPrimaryKey()
        {
            string pk = ConstructPrimayKey(dicData, primaryFields);
            return pk;
        }

        public override string GetTableName()
        {
            return "t_bp_modify_log";
        }

        public override void PostConstructJson(JObject jobj)
        {
            jobj.Add(new JProperty(elasticType, esType));
            jobj.Add(new JProperty(elasticDocType, esDocType));
        }
    }
}