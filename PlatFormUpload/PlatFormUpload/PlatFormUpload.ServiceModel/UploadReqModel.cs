using ServiceStack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatFormUpload.ServiceModel
{
    [Description("指定数据上传接口")]
    [Route("/UploadAssignData", "POST")]
    public class UploadAssignDataRequest : IReturn<UploadAssignDataResponse>
    {
        public List<AssignDataModel> Model { get; set; }
    }

    public class UploadAssignDataResponse
    {
        [Description("是否都成功")]
        public bool AllSucc { get; set; }

        [Description("失败信息等")]
        public string ErrMsg { get; set; }

        [Description("上传结果")]
        public List<ItemUploadResult> Result { get; set; }
    }

    [Description("统一数据上传接口")]
    [Route("/UploadUnifiedData", "POST")]
    public class UploadUnifiedDataRequest : IReturn<UploadUnifiedDataResponse>
    {
        public UnifiedDataModel Model { get; set; }
    }

    public class UploadUnifiedDataResponse
    {
        [Description("是否都成功")]
        public bool AllSucc { get; set; }

        [Description("失败信息等")]
        public string ErrMsg { get; set; }

        [Description("上传结果")]
        public List<ItemUploadResult> Result { get; set; }
    }

    [Description("单独附件上传接口")]
    [Route("/UploadAttachment", "POST")]
    public class UploadAttachmentRequest : IReturn<UploadAttachmentResponse>
    {
        public AttachmentModel Attachment { get; set; }
    }

    public class UploadAttachmentResponse
    {
        [Description("是否成功")]
        public bool IsSucc { get; set; }

        [Description("附件上传后的路径")]
        public string FilePath { get; set; }

        [Description("失败信息等")]
        public string ErrMsg { get; set; }
    }

    [Description("批量附件上传接口")]
    [Route("/UploadBatchAttachment", "POST")]
    public class UploadBatchAttachmentRequest : IReturn<UploadBatchAttachmentResponse>
    {
        public List<AttachmentModel> BatchAttachment { get; set; }
    }

    public class UploadBatchAttachmentResponse
    {
        [Description("是否都成功")]
        public bool AllSucc { get; set; }

        [Description("失败信息等")]
        public string ErrMsg { get; set; }

        [Description("上传结果")]
        public List<ItemUploadResult> Result { get; set; }
    }


    /// <summary>
    /// 指定表名数据
    /// </summary>
    public class AssignDataModel
    {
        /// <summary>
        /// 表名
        /// </summary>
        [Description("表名")]
        public string Table { get; set; }

        /// <summary>
        /// 上传的数据key为字段名，value为字段值
        /// </summary>
        [Description("上传的数据key为字段名，value为字段值")]
        public Dictionary<string,string> Data { get; set; }
    }

    /// <summary>
    /// 主表，修改记录，曲线记录，pkr记录，参数记录，工程表  json
    /// </summary>
    public class UnifiedDataModel
    {
        /// <summary>
        /// 上传的数据key为字段名，value为字段值
        /// </summary>
        [Description("主表数据-dict")]
        public Dictionary<string,string> MainData { get; set; }

        /// <summary>
        /// 修改记录数据
        /// </summary>
        [Description("修改记录数据-dict")]
        public List<Dictionary<string,string>> ModifyDatas { get; set; }

        /// <summary>
        /// 曲线记录数据
        /// </summary>
        [Description("曲线记录数据-list<dict>")]
        public List<Dictionary<string,string>> AcsDatas { get; set; }

        /// <summary>
        /// pkr记录数据
        /// </summary>
        [Description("pkr记录数据-dict")]
        public Dictionary<string,string> PKRData { get; set; }

        /// <summary>
        /// 工程项目数据
        /// </summary>
        [Description("工程项目数据-dict")]
        public Dictionary<string,string> ItemListData { get; set; }

        /// <summary>
        /// 工程数据
        /// </summary>
        [Description("工程数据-dict")]
        public Dictionary<string,string> ProjectData { get; set; }
    }


    


    public class AttachmentModel
    {
        /// <summary>
        /// 表名
        /// </summary>
        [Description("表名")]
        public string Table { get; set; }

        /// <summary>
        /// 上传的数据key为字段名，value为字段值
        /// </summary>
        [Description("上传的数据key为字段名，value为字段值(对应表必须字段:CUSTOMID,ITEMNAME,SYSPRIMARYKEY,REPORTNUM...)")]
        public Dictionary<string,string> Data { get; set; }

        /// <summary>
        /// 附件数据
        /// </summary>
        [Description("附件数据（byte[]）")]
        public byte[] Attachment { get; set; }

        /// <summary>
        /// 附件缓存Key 唯一 客户端要保存
        /// </summary>
        [Description("附件缓存唯一Key，客户端要保存")]
        public string CacheKey { get; set; }

        /// <summary>
        /// 附件状态
        /// </summary>
        [Description("附件状态 0：第一段 1：中间段 2：结束段（只有一段传2）")]
        public int Status { get; set; }
    }

    /// <summary>
    /// 单项上传结果
    /// </summary>
    public class ItemUploadResult
    {
        [Description("表名")]
        public string Table { get; set; }
        [Description("附件上传后的路径")]
        public string FilePath { get; set; }
        [Description("是否成功")]
        public bool IsSuc { get; set; }
        [Description("失败信息等")]
        public string msg { get; set; }
    }


    /// <summary>
    /// 附件上传状态
    /// </summary>
    public enum AttachmentStatus
    {
        /// <summary>
        /// 第一段
        /// </summary>
        Begin = 0, 
        /// <summary>
        /// 中间段
        /// </summary>
        Middle = 1,
        /// <summary>
        /// 最后一段
        /// </summary>
        End = 2 
    }



}
