using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatFormUpload.Common
{
    public class LoggerHelper
    {    
        /// <summary>
        /// 自定义写log日志
        /// </summary>
        /// <param name="logger">nlog</param>
        /// <param name="message">日志消息</param>
        /// <param name="customId">自定义目录名称</param>
        /// <param name="isSucc">是否成功</param>
        public static void WriteCustomLog(Logger logger, string message, string customId, bool isSucc)
        {
            if (string.IsNullOrWhiteSpace(customId))
            {
                customId = "NoCustomId";
            }
            else
            {
                customId = string.Format("{0}-{1}", customId, isSucc ? "Succ" : "Fail");
            }

            if (message == null)
            {
                message = string.Empty;
            }

            LogEventInfo theEvent = new LogEventInfo(LogLevel.Info, "", message);
            theEvent.Properties["FPath"] = customId;
            if (isSucc)
            {
                //成功记录debug,后续方便关闭
                //Debug, Info, Warn, Error and Fatal 顺序
                theEvent.Level = LogLevel.Debug;
            }


            logger.Log(theEvent);


        }

        public static void WriteErrorLog(Logger logger, Exception ex)
        {
            string filePath = "SystemException";
            LogEventInfo theEvent = new LogEventInfo(LogLevel.Error, "", null, string.Empty, null, ex);
            theEvent.Properties["FPath"] = filePath;

            logger.Log(theEvent);
        }
    }
}
