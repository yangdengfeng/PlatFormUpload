using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlatFormUpload.ServiceInterface
{
    public class HntDataUpload : UploadMqbaseItem
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> excludeFields = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_excluded"].Split(',').ToList();
        private static List<string> numberFields = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_Numbers"].Split(',').ToList();
        private static List<string> floatFields = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_Float"].Split(',').ToList();
        private static List<string> dateFields = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_Dt"].Split(',').ToList();
        private static string[] primaryFields = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_Pks"].Split(',');
        private static string exchangeName = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_ExchangeName"];
        private static string routingKey = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_RoutingKey"];
        private static string esType = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_EsType"];
        private static string esDocType = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_EsDocType"];

        private static List<string> DFPBFields = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_DFPB"].Split(',').ToList();
        private static List<string> PCFields = System.Configuration.ConfigurationManager.AppSettings["t_hnt_data_PC"].Split(',').ToList();
        public HntDataUpload(Dictionary<string, string> dicData, byte[] cmdAttach) : base(dicData, cmdAttach)
        {
        }

        public override JObject ConstructNeedToBePushedJson()
        {
            var F_FL = GetFloatFromDict("F_FL");
            //大于0才计算单方配比
            if (F_FL > 0)
            {
                foreach(var item in DFPBFields)
                {
                    var itemValue = GetFloatFromDict(item.Substring(0, item.Length - 5));
                    if (itemValue > 0)
                    {
                        dicData[item] = FloatToString(itemValue / F_FL);
                    }
                }
            }
            //偏差=(实际值-设计值)/设计值
            foreach(var item in PCFields)
            {
                var ActualValueKey = item.Substring(0, item.Length - 3);
                var DesignValueKey = ActualValueKey.Replace("_SHIJI_", "_SHEJI_");
                var DesignValue = GetFloatFromDict(DesignValueKey);
                if(DesignValue>0)//大于0才计算偏差
                {
                    var DiffValue = (GetFloatFromDict(ActualValueKey) - DesignValue);
                    if (DiffValue > 0)
                    {
                        dicData[item] = FloatToString(DiffValue / DesignValue);
                    }
                }
            }

            return ConstructJsonFromDictionary(excludeFields, numberFields, dateFields, floatFields);
        }
        private string FloatToString(float value)
        {
            return value.ToString();
        }
        private float GetFloatFromDict(string key)
        {
            string strValue = string.Empty;
            float value = 0;
            if(!dicData.TryGetValue(key,out strValue))
            {
                return 0;
            }
            if(!float.TryParse(strValue,out value))
            {
                return 0;
            }
            return value;
        }

        public override string GetPrimaryKey()
        {
            string pk = ConstructPrimayKey(dicData, primaryFields);
            return pk;
        }

        public override string GetTableName()
        {
            return "hnt_table";
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