using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlatFormUpload.ServiceInterface
{
    public class RawMaterialCheckRecordUpload : UploadMqbaseItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> includeFields = System.Configuration.ConfigurationManager.AppSettings["rmcr_included"].Split(',').ToList();
        
        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["rmcr_Pks"].Split(',');
        private static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["rmcr_ExchangeName"];
        private static string routingKey = System.Configuration.ConfigurationManager.AppSettings["rmcr_RoutingKey"];
        private static string esType = System.Configuration.ConfigurationManager.AppSettings["rmcr_EsType"];
        private static string esDocType = System.Configuration.ConfigurationManager.AppSettings["rmcr_EsDocType"];
        private string TableName = string.Empty;

        public RawMaterialCheckRecordUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {
        }

        public override JObject ConstructNeedToBePushedJson()
        {
            string esDtFormat = "yyyy-MM-dd'T'HH:mm:ss";
            JObject jobj = new JObject();
            foreach (var dicKV in dicData)
            {
                if (includeFields.Contains(dicKV.Key.ToUpper()))
                {
                    jobj.Add(new JProperty(dicKV.Key, dicKV.Value));
                }
            }
            if (dataAction == "ADD")
            {
                jobj["UPLOADTIME"] = DateTime.Now.ToString(esDtFormat);
                jobj["UPDATETIME"] = DateTime.Now.ToString(esDtFormat);
            }
            if (dataAction == "UPDATE")
            {
                jobj["UPDATETIME"] = DateTime.Now.ToString(esDtFormat);
            }

            return jobj;
            // return ConstructJsonFromDictionary(excludeFields, numberFields, dateFields, floatFields);
        }

        public override string GetPrimaryKey()
        {
            string pk = ConstructPrimayKey(dicData, primaryFields);
            return pk;
        }

        public override string GetTableName()
        {
            return "rmcr";
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