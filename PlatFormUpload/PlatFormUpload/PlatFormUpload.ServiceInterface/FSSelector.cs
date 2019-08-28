using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PlatFormUpload.ServiceInterface
{
    public class FSSelector
    {
        private static string ravenDbFsAcsUril = System.Configuration.ConfigurationManager.AppSettings["RavenDBFSAcsUri"];
        private static string ravenDbFsWordUril = System.Configuration.ConfigurationManager.AppSettings["RavenDBFSWordsUri"];
        private static string ravenDbFsPKRUril = System.Configuration.ConfigurationManager.AppSettings["RavenDBFSPKRUri"];

        private static List<string> ravenDBFsAcsSchemas = System.Configuration.ConfigurationManager.AppSettings["AcsCustomSchema"].Split(',').ToList();
        private static List<string> ravenDBFsWordSchemas = System.Configuration.ConfigurationManager.AppSettings["WordCustomSchema"].Split(',').ToList();
        private static List<string> ravenDBFsPKRSchemas = System.Configuration.ConfigurationManager.AppSettings["PKRCustomSchema"].Split(',').ToList();
        private static List<string> ravenDBFsYiWuNewCReportSchemas = System.Configuration.ConfigurationManager.AppSettings["YiWuNewCReportCustomSchema"].Split(',').ToList();//义乌新c类报告

        
        public static string GetFSAcsUrl(string customId)
        {
            var customPrefix = ravenDBFsAcsSchemas.Where(u => customId.StartsWith(u)).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(customPrefix))
            {
                customPrefix = ravenDBFsAcsSchemas.Last();
            }

            return Path.Combine(ravenDbFsAcsUril, customPrefix);
        }

        public static string GetFSYiWuNewCReportUrl(string customId)
        {
            var customPrefix = ravenDBFsYiWuNewCReportSchemas.Where(u => customId.StartsWith(u)).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(customPrefix))
            {
                customPrefix = ravenDBFsYiWuNewCReportSchemas.Last();
            }

            return Path.Combine(ravenDbFsWordUril, customPrefix);
        }

        public static string GetFSWordUrl(string customId)
        {
            string customPrefix = string.Empty;

            foreach (var item in ravenDBFsWordSchemas)
            {
                if (item.Contains("-") && item.Split('-').ToList().Exists(i => customId.StartsWith(i)))
                {
                    customPrefix = item;
                    break;
                }
                else
                {
                    if (customId.StartsWith(item))
                    {
                        customId = item;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(customPrefix))
            {
                customPrefix = ravenDBFsWordSchemas.Last();
            }

            return Path.Combine(ravenDbFsWordUril, customPrefix);
        }

        public static string GetFSPKRUrl(string customId)
        {
            string customPrefix = string.Empty;

            foreach (var item in ravenDBFsPKRSchemas)
            {
                if (item.Contains("-") && item.Split('-').ToList().Exists(i => customId.StartsWith(i)))
                {
                    customPrefix = item;
                    break;
                }
                else
                {
                    if (customId.StartsWith(item))
                    {
                        customId = item;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(customPrefix))
            {
                customPrefix = ravenDBFsPKRSchemas.Last();
            }

            return Path.Combine(ravenDbFsPKRUril, customPrefix);
        }

        public static string GetSafeFilename(string filename)
        {
            return string.Join("-", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}