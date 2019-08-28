
using Newtonsoft.Json.Linq;
using NLog;
using PlatFormUpload.Common;
using PlatFormUpload.ServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatFormUpload.ServiceInterface
{
    public class SyncPKPMJCDataService : Service
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static bool updateNow = System.Configuration.ConfigurationManager.AppSettings["UpdateNow"].ToString() == "0" ? false : true;
        public static string dataFolder = System.Configuration.ConfigurationManager.AppSettings["TempDataFolder"];

        /// <summary>
        /// 指定数据上传接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public UploadAssignDataResponse POST(UploadAssignDataRequest req)
        {
            List<ItemUploadResult> results = new List<ItemUploadResult>();
            try
            {
                string tableName = string.Empty; 
                byte[] cmdAttach = null;
                StringBuilder sb = new StringBuilder();

                if (updateNow)
                {
                    return new UploadAssignDataResponse { AllSucc = false, ErrMsg = "服务器正在处理数据，暂时不接收数据!" };
                }

                if(req.Model == null || req.Model.Count == 0)
                {
                    return new UploadAssignDataResponse { AllSucc = false, ErrMsg = "上传数据为空!" };
                }

                foreach (var item in req.Model)
                {
                    tableName = item.Table;
                        
                    if (string.IsNullOrWhiteSpace(tableName))
                    {
                        results.Add(new ItemUploadResult { IsSuc = false, Table = tableName, msg = "表名不能为空！" });
                        continue;
                    }

                    if (item.Data.IsNullOrEmpty())
                    {
                        results.Add(new ItemUploadResult { IsSuc = false, Table = tableName, msg = "所传数据为空,不再进行处理！" });
                        continue;
                    } 
                     
                    var syncResult = SyncPKPMJCData(tableName, item.Data, cmdAttach);
                    results.Add(new ItemUploadResult { IsSuc = syncResult.IsSuc, Table = tableName, msg = syncResult.Info });
                    if (!syncResult.IsSuc)
                    {
                        sb.AppendLine(string.Format("isSucc:{0} tableName:{1} cmdData:{2} ErrMsg:{3}", syncResult.IsSuc, tableName, item.Data, syncResult.Info));
                    }
                }

                if (!string.IsNullOrEmpty(sb.ToString()))
                {
                    LoggerHelper.WriteCustomLog(logger, sb.ToString(), "UploadAssignData", false);
                    return new UploadAssignDataResponse { AllSucc = false, ErrMsg = "存在上传失败的数据！", Result = results };
                }
                else
                {
                    return new UploadAssignDataResponse { AllSucc = true, ErrMsg = "上传成功！" };
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.WriteErrorLog(logger, ex);
                return new UploadAssignDataResponse { AllSucc = false, ErrMsg = ex.Message, Result = results };
            }
        }

        /// <summary>
        /// 统一数据上传接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public UploadUnifiedDataResponse POST(UploadUnifiedDataRequest req)
        {
            List<ItemUploadResult> results = new List<ItemUploadResult>();
            try
            {
                StringBuilder sb = new StringBuilder();
                string cmdData = string.Empty;
                byte[] cmdAttach = null;
                
                if (updateNow)
                {
                    return new UploadUnifiedDataResponse { AllSucc = false, ErrMsg = "服务器正在处理数据，暂时不接收数据!" };
                }

                if (req.Model != null)
                {
                    UploadMqbaseItem item = null;

                    ResultBase<string> syncResult_Main = null;
                    ResultBase<string> syncResult_Modify = null;
                    ResultBase<string> syncResult_Acs = null;
                    ResultBase<string> syncResult_Pkr = null;
                    ResultBase<string> syncResult_ItemList = null;
                    ResultBase<string> syncResult_Project = null;

                    if (req.Model.MainData.IsNullOrEmpty())
                    {
                        return new UploadUnifiedDataResponse { AllSucc = false, ErrMsg = "主表数据必须上传!" };
                    }

                    if (req.Model.PKRData.IsNullOrEmpty())
                    {
                        return new UploadUnifiedDataResponse { AllSucc = false, ErrMsg = "PKR数据必须上传!" };
                    }

                    #region 主表记录

                    item = new TBPItemUpload(req.Model.MainData, cmdAttach);
                    syncResult_Main = item.StartUploadDataItem(logger);
                    results.Add(new ItemUploadResult { IsSuc = syncResult_Main.IsSuc, Table = "Main", msg = syncResult_Main.Info });
                    if (syncResult_Main.IsSuc)
                    {
                        sb.AppendLine(string.Format("isSucc:{0} tableName:{1} cmdData:{2} ErrMsg:{3}", syncResult_Main.IsSuc, "Main", cmdData, syncResult_Main.Info));
                    }

                    #endregion

                    #region PKR记录

                    item = new TPkpmBinaryReportUpload(req.Model.PKRData, cmdAttach, true);
                    syncResult_Pkr = item.StartUploadDataItem(logger);
                    results.Add(new ItemUploadResult { IsSuc = syncResult_Pkr.IsSuc, Table = "Pkr", msg = syncResult_Pkr.Info });
                    if (!syncResult_Pkr.IsSuc)
                    {
                        sb.AppendLine(string.Format("isSucc:{0} tableName:{1} cmdData:{2} ErrMsg:{3}", syncResult_Pkr.IsSuc, "Pkr", cmdData, syncResult_Pkr.Info));
                    }

                    #endregion

                    #region 修改记录

                    if (!req.Model.ModifyDatas.IsNullOrEmpty())
                    {
                        foreach (var modifyData in req.Model.ModifyDatas)
                        {
                            item = new TBPModifyLogUpload(modifyData, cmdAttach);
                            syncResult_Modify = item.StartUploadDataItem(logger);
                            results.Add(new ItemUploadResult { IsSuc = syncResult_Modify.IsSuc, Table = "Modify", msg = syncResult_Modify.Info });
                            if (!syncResult_Modify.IsSuc)
                            {
                                sb.AppendLine(string.Format("isSucc:{0} tableName:{1} cmdData:{2} ErrMsg:{3}", syncResult_Modify.IsSuc, "Modify", cmdData, syncResult_Modify.Info));
                            }
                        }
                    }

                    #endregion

                    #region 曲线记录

                    if (!req.Model.AcsDatas.IsNullOrEmpty())
                    {
                        foreach (var acsData in req.Model.AcsDatas)
                        {
                            item = new TBPAcsInterFace(acsData, cmdAttach, true);
                            syncResult_Acs = item.StartUploadDataItem(logger);
                            results.Add(new ItemUploadResult { IsSuc = syncResult_Acs.IsSuc, Table = "Acs", msg = syncResult_Acs.Info });
                            if (!syncResult_Acs.IsSuc)
                            {
                                sb.AppendLine(string.Format("isSucc:{0} tableName:{1} cmdData:{2} ErrMsg:{3}", syncResult_Acs.IsSuc, "Acs", cmdData, syncResult_Acs.Info));
                            }
                        } 
                    }

                    #endregion

                    #region 工程项目记录

                    if (!req.Model.ItemListData.IsNullOrEmpty())
                    {
                        item = new TBPItemListUpload(req.Model.ItemListData, cmdAttach);
                        syncResult_ItemList = item.StartUploadDataItem(logger);
                        results.Add(new ItemUploadResult { IsSuc = syncResult_ItemList.IsSuc, Table = "ItemList", msg = syncResult_ItemList.Info });
                        if (!syncResult_Pkr.IsSuc)
                        {
                            sb.AppendLine(string.Format("isSucc:{0} tableName:{1} cmdData:{2} ErrMsg:{3}", syncResult_ItemList.IsSuc, "ItemList", cmdData, syncResult_ItemList.Info));
                        }
                    }

                    #endregion

                    #region 工程记录

                    if (!req.Model.ProjectData.IsNullOrEmpty())
                    {
                        //工程记录不需要传，这里只是要添加到主表的一些字段如 工程名称，工程编号，工程地区，监督机构等等
                        item = new TBPProjectUpload(req.Model.ProjectData, cmdAttach);
                        syncResult_Project = item.StartUploadDataItem(logger);
                        results.Add(new ItemUploadResult { IsSuc = syncResult_Project.IsSuc, Table = "Project", msg = syncResult_Project.Info });
                        if (!syncResult_Pkr.IsSuc)
                        {
                            sb.AppendLine(string.Format("isSucc:{0} tableName:{1} cmdData:{2} ErrMsg:{3}", syncResult_Project.IsSuc, "Project", cmdData, syncResult_Project.Info));
                        }
                    }

                    #endregion

                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        LoggerHelper.WriteCustomLog(logger, sb.ToString(), "UploadUnifiedData", false);
                        return new UploadUnifiedDataResponse { AllSucc = false, ErrMsg = "存在上传失败的数据！", Result = results };
                    }
                    else
                    {
                        return new UploadUnifiedDataResponse { AllSucc = true, ErrMsg = "上传成功" };
                    }
                }
                else
                {
                    return new UploadUnifiedDataResponse { AllSucc = false, ErrMsg = "上传数据为空！" };
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.WriteErrorLog(logger, ex);
                return new UploadUnifiedDataResponse { AllSucc = false, ErrMsg = ex.Message, Result = results };
            }
        }

        /// <summary>
        /// 单独附件上传接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public UploadAttachmentResponse POST(UploadAttachmentRequest req)
        {

            //如果分段的前几段数据，则先缓存
            //否则调用S1(string d,byte[] b)来存储数据
            //缓存放在数据存放文件夹下面的TempData文件夹
            //如果isEnd的值为"0",则向文件追加数据
            //如果isEnd的值为"1"，则向文件追加数据之后调用S1进行数据保存之后删除缓存文件            
            //cacheKey需对每台工作站每条数据唯一

            if (updateNow)
            {
                return new UploadAttachmentResponse { IsSucc = false, ErrMsg = "服务器正在处理数据，暂时不接收数据!" } ;
            }

            try
            {
                string tempDir = "TempData";
                string tempDirPath = Path.Combine(dataFolder, tempDir);
                string filePath = Path.Combine(tempDirPath, string.Format("{0}.txt", req.Attachment.CacheKey));

                if (!Directory.Exists(tempDirPath))
                {
                    Directory.CreateDirectory(tempDirPath);
                }

                if (req.Attachment.Status == (int)AttachmentStatus.Begin)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                CommonUtil.WriteFileBytes(filePath, req.Attachment.Attachment);

                if (req.Attachment.Status == (int)AttachmentStatus.Begin || req.Attachment.Status == (int)AttachmentStatus.Middle) //第一段和中间段二进制数据
                {
                    return new UploadAttachmentResponse { IsSucc = true, ErrMsg = string.Empty };
                }
                else if (req.Attachment.Status == (int)AttachmentStatus.End) //最后一段二进制数据或附件大小只需要上次一次的
                {
                    byte[] cmdAttach = File.ReadAllBytes(filePath);

                    File.Delete(filePath);

                    UploadMqbaseItem item = GetUploadItemByTableName(req.Attachment.Table, req.Attachment.Data, cmdAttach);

                    var result = item.StartUploadAttachmentItem(logger);

                    //还回附件保存的路径给客户端，客户端在下次上传数据的时候要带上来 校验
                    if (result)
                    {
                        return new UploadAttachmentResponse { IsSucc = true, FilePath = result.Info, ErrMsg = "上传成功!" };
                    }
                    else
                    {
                        LoggerHelper.WriteCustomLog(logger, string.Format("Table:{0} Data:{1} Status:{2} CacheKey:{3} ErrMsg:{4}", req.Attachment.Table, req.Attachment.Data, req.Attachment.Status, req.Attachment.CacheKey, result.Info), "UploadAttachment", false);
                        return new UploadAttachmentResponse { IsSucc = false, ErrMsg = result.Info };
                    }
                }
                else
                {
                    return new UploadAttachmentResponse { IsSucc = false, ErrMsg = "未知状态[Status]" };
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.WriteErrorLog(logger, ex);
                return new UploadAttachmentResponse { IsSucc = false, ErrMsg = ex.Message }; ;
            }
        }

        /// <summary>
        /// 批量附件上传接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public UploadBatchAttachmentResponse POST(UploadBatchAttachmentRequest req)
        {
            List<ItemUploadResult> results = new List<ItemUploadResult>();
            StringBuilder sb = new StringBuilder();

            if (updateNow)
            {
                return new UploadBatchAttachmentResponse { AllSucc = false, ErrMsg = "服务器正在处理数据，暂时不接收数据!" };
            }

            if (req.BatchAttachment == null || req.BatchAttachment.Count == 0)
            {
                return new UploadBatchAttachmentResponse { AllSucc = false, ErrMsg = "上传数据为空，请检查!" };
            }

            try
            {
                bool allSucc = true;
                foreach (var attachment in req.BatchAttachment)
                {
                    var uploadResult = POST(new UploadAttachmentRequest { Attachment = attachment });

                    results.Add(new ItemUploadResult { IsSuc = uploadResult.IsSucc, Table = attachment.Table, FilePath = uploadResult.FilePath, msg = uploadResult.IsSucc ? "上传成功" : string.Format("CacheKey:{0}&ErrMsg:{1}", attachment.CacheKey, uploadResult.ErrMsg) });

                    if (!uploadResult.IsSucc) allSucc = false;
                }

                if (!allSucc)
                {
                    return new UploadBatchAttachmentResponse { AllSucc = false, ErrMsg = "本次批量存在上传失败的附件！", Result = results };
                }
                else
                {
                    return new UploadBatchAttachmentResponse { AllSucc = true, ErrMsg = "批量上传成功!" };
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.WriteErrorLog(logger, ex);
                return new UploadBatchAttachmentResponse { AllSucc = false, ErrMsg = ex.Message, Result = results }; ;
            }
        }


        public ResultBase<string> SyncPKPMJCData(string tableName, Dictionary<string,string> cmdData, byte[] cmdAttach)
        {
            if (updateNow)
            {
                return ResultBase<string>.Failure("服务器在处理数据，暂时不接收数据！");
            }

            try
            {
                string retStr = string.Empty;
                tableName = GetPlatTableName(tableName);
                UploadMqbaseItem item = GetUploadItemByTableName(tableName, cmdData, cmdAttach);
                if (item == null)
                {
                    string message = string.Format("[{0}]-[{1}]", "无配置的表上传", tableName, cmdData);
                    LoggerHelper.WriteCustomLog(logger, message, "NoTableConfig", false);
                    return ResultBase<string>.Failure(message);
                }
                else
                {
                    return item.StartUploadDataItem(logger);
                }
            }
            catch (Exception ex)
            {
                string retStr = ex.Message.ToString() + ex.StackTrace.ToString() + ex.Source.ToString();
                LoggerHelper.WriteErrorLog(logger, ex);
                return ResultBase<string>.Failure(retStr);
            }
        }

        /// <summary>
        /// 转换成PKPMBS对应的表
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private string GetPlatTableName(string t)
        {
            t = t.ToUpper();
            string ret = t;
            string noTranTable = "NOTRANTABLE";
            switch (t)
            {
                case "SYSCURRENTRECORD":
                case "SYSRECORD":
                case "SYSBACKUPRECORD": //数据总表
                    ret = "T_BP_ITEM";
                    break;
                case "ACSINTERFACE_GRAPH": //曲线数据表
                    ret = "t_bp_acsInterFace".ToUpper();
                    break;
                case "AUTOCOLUMNLIST":
                    //ret = "t_bp_autoColumn".ToUpper(); break;
                    ret = noTranTable; break;
                case "SYSCUSTOMINFOMATION":
                    //ret = "t_bp_customJZ".ToUpper(); break;
                    ret = noTranTable; break;
                case "INFODEPARTMENT":
                    //ret = "t_bp_departMentJZ".ToUpper(); break;
                    ret = noTranTable; break;
                case "DEPARTMENTFUNPOWER":
                    //ret = "t_bp_deptfp".ToUpper(); break;
                    ret = noTranTable; break;
                case "DEPARTMENTPOWER":
                    //ret = "t_bp_deptip".ToUpper(); break;
                    ret = noTranTable; break;
                case "INSTRUMENTLIST":
                    //ret = "t_bp_instrumentJZ".ToUpper(); break;
                    ret = noTranTable; break;
                case "ITEMCOLUMNLIST": //字段名表
                    ret = "t_bp_itemColumn".ToUpper(); break;
                //ret = noTranTable; break;
                case "SYSITEMLIST": //检测项目表
                    ret = "t_bp_itemList".ToUpper(); break;
                case "USERMODIFYDATALOG": //修改记录表
                    ret = "t_bp_modify_log".ToUpper(); break;
                case "SYSPROJECTINFOMATIONLIB": //工程信息表
                case "SYSPROJECT":
                    ret = "t_bp_project".ToUpper(); break;
                case "QUESTIONRECORDLIST": //修改原因表
                    ret = "t_bp_question".ToUpper(); break;
                case "QUESTIONRECORDFIELDLIST":
                    ret = "t_bp_questionField".ToUpper(); break;
                case "OPERATIONUSERFUNPOWERS":
                    //ret = "t_bp_userfp".ToUpper(); break;
                    ret = noTranTable; break;
                case "OPERATIONUSERITEMPOWERS":
                    //ret = "t_bp_userip".ToUpper(); break;
                    ret = noTranTable; break;
                case "OPERATIONUSERS":
                    //ret = "t_bp_peopleJZ".ToUpper(); break;
                    ret = noTranTable; break;
                case "ITEMAMENDPARMLIST":
                    //ret = "t_bp_peopleJZ".ToUpper(); break;
                    ret = noTranTable; break;
                case "AUTOTESTCOLUMNLIST":
                    ret = "T_BP_AUTOCOLUMN"; break;
                //ret = noTranTable; break;
                case "PBCATU":
                case "MODIFYLOG": //修改记录表
                    ret = "t_bp_modify_log".ToUpper(); break;
                case "PBCATQ":
                case "QUESTIONLIST": //修改原因表
                    ret = "t_bp_question".ToUpper(); break;
                case "PBCATF":
                    ret = "t_bp_questionField".ToUpper(); break;
                case "WORDREPORTLIST": //Word报告表
                    ret = "WORDREPORTLIST".ToUpper(); break;
                case "EXTREPORTMANAGE": //Word报告表
                    ret = "EXTREPORTMANAGE".ToUpper(); break;
                case "UNITCERT":
                    ret = "T_BP_UNITCERT"; break;
                case "PEOPLECERT":
                    ret = "T_BP_PEOPLECERT"; break;
                case "SUBITEMPARM": //项目参数表
                    ret = noTranTable; break;
                case "DB_BINARYREPORT..BINARYREPORT": //二进制报告表
                    ret = "T_binaryReport"; break;
                case "T_PKPM_BINARYREPORT": //二进制报告表
                    ret = "T_pkpm_binaryReport"; break;
                case "subitempd":
                    ret = "subitempd";
                    break;
                case "covrlist":
                    ret = "covrlist";
                    break;
                case "T_Sys_Files":
                    ret = "t_sys_files";
                    break;
                case "T_HNT_DATA":
                    ret = "t_hnt_data";
                    break;
                //case "UT_RPM_SIIH":
                //case "UT_RPM_ADDITIVE":
                //case "UT_RPM_CEMT":
                //case "UT_RPM_GLKZ":
                //case "UT_RPM_SZIH":
                //case "UT_RPM_CFMH":
                //case "UT_RPM_JBSH":
                //    ret = "ut_rpm_";
                //    break;

                default:
                    ret = t.ToUpper();
                    break;
            }
            
            if (t.Length == 8 && t.Substring(4, 4).ToUpper() == "_SAV")
            {
                ret = t.Substring(0, 4) + "_CUR";
            }

            switch (ret)
            {
                case "SIIH_CUR":
                case "HJSJ_CUR":
                case "HPZJ_CUR":
                case "CEMT_CUR":
                case "GLKZ_CUR":
                case "SZIH_CUR":
                case "CFMH_CUR":
                case "JBSH_CUR":
                    ret = "RawMaterialCheckRecord";
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 处理数据主方法
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cmdData"></param>
        /// <param name="cmdAttach"></param>
        /// <returns></returns>
        private UploadMqbaseItem GetUploadItemByTableName(string tableName,Dictionary<string,string> dicData, byte[] cmdAttach)
        {

            if (string.Compare(tableName, "t_bp_acsInterFace", true) == 0)
            {
                //曲线表
                return new TBPAcsInterFace(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "T_BP_ITEM", true) == 0)
            {
                //主表
                return new TBPItemUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "t_bp_modify_log", true) == 0)
            {
                //修改记录
                return new TBPModifyLogUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "t_bp_question", true) == 0)
            {
                //修改原因
                return new TBPQuestionUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "t_bp_itemList", true) == 0)
            {
                //参数 工程项目
                return new TBPItemListUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "t_bp_project", true) == 0)
            {
                //工程
                return new TBPProjectUpload(dicData, cmdAttach);
            }
            //else if (string.Compare(tableName, "subitempd", true) == 0)
            //{
            //    //subitmpd 
            //    return new SubItemPbUpload(cmdData, cmdAttach);
            //}
            else if (string.Compare(tableName, "WORDREPORTLIST", true) == 0)
            {
                //word 报告表 
                return new WordReportListUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "EXTREPORTMANAGE", true) == 0)
            {
                //word 报告表 EXTREPORTMANAGE 
                return new ExtReportManageUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "T_pkpm_binaryReport", true) == 0)
            {
                //PKR报告 T_binaryReport
                return new TPkpmBinaryReportUpload(dicData, cmdAttach);
            }
            //else if (string.Compare(tableName, "covrlist", true) == 0)
            //{
            //    //现场检测covrlist
            //    return new CovrlistUpload(cmdData, cmdAttach);
            //}
            else if (string.Compare(tableName, "t_sys_files", true) == 0)
            {
                //义乌新的C类表
                return new TsysfilesUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "t_hnt_data", true) == 0)
            {
                //混凝土生产数据
                return new HntDataUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "tono_cur", true) == 0)
            {
                //混凝土配合比
                return new TBpTONOCurUpload(dicData, cmdAttach);
            }
            else if (tableName.IndexOf("UT_RPM_") > -1)
            {
                //原材料进场记录
                return new UtRpmUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "hntqdtj", true) == 0)
            {
                //强度评定
                return new HntQdtjUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "HPPD_CUR", true) == 0)
            {
                //出厂合格
                return new HntHPPDCurUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "HNKY_CUR", true) == 0)
            {
                //抗压
                return new HntHNKYUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "HNKZ_CUR", true) == 0)
            {
                //抗折
                return new HntHNKZUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "HNKS_CUR", true) == 0)
            {
                //抗渗
                return new HntHNKSUpload(dicData, cmdAttach);
            }
            else if (string.Compare(tableName, "RawMaterialCheckRecord", true) == 0)
            {
                //抗渗
                return new RawMaterialCheckRecordUpload(dicData, cmdAttach);
            }

            return null;
        }



    }
}
