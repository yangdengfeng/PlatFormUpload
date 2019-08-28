using Newtonsoft.Json.Linq;
using NLog;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatFormUpload.Common
{
    public static class RabbitMqHelper
    {
        public static bool PushJsonToRabbitMq(JObject jobj, string exchangeName, string routeName, Logger logger, out string errorMsg)
        {
            errorMsg = string.Empty;

            bool pushSucc = true;

            try
            {
                using (IConnection conn = RabbitMqFactoryHolder.RabbitFactory.CreateConnection())
                using (IModel channel = conn.CreateModel())
                {
                    var props = channel.CreateBasicProperties();
                    //props.SetPersistent(true); 过时
                    props.Persistent = true;
                    var msgBody = Encoding.UTF8.GetBytes(jobj.ToString(Newtonsoft.Json.Formatting.None));

                    channel.BasicPublish(exchangeName, routingKey: routeName, basicProperties: props, body: msgBody);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.WriteErrorLog(logger, ex);
                pushSucc = false;
                errorMsg = ex.Message;
            }

            return pushSucc;
        }
    }
}
