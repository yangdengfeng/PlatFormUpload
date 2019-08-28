
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlatFormUpload.HntTable
{

    public class t_bp_custom
    {

        public string Id { get; set; }
        public string NAME { get; set; }
        public string STATIONID { get; set; }
        public string POSTCODE { get; set; }
        public string TEL { get; set; }
        public string FAX { get; set; }
        public string ADDRESS { get; set; }
        public DateTime? CREATETIME { get; set; }
        public string EMAIL { get; set; }
        public string BUSINESSNUM { get; set; }
        public string BUSINESSNUMUNIT { get; set; }
        public string REGAPRICE { get; set; }
        public string ECONOMICNATURE { get; set; }
        public string MEASNUM { get; set; }
        public string MEASUNIT { get; set; }
        public string MEASNUMPATH { get; set; }
        public string FR { get; set; }
        public string JSNAME { get; set; }
        public string JSTIILE { get; set; }
        public string JSYEAR { get; set; }
        public string ZLNAME { get; set; }
        public string ZLTITLE { get; set; }
        public string ZLYEAR { get; set; }
        public string PERCOUNT { get; set; }
        public string MIDPERCOUNT { get; set; }
        public string HEIPERCOUNT { get; set; }
        public string REGYTSTA { get; set; }
        public string REGJGSTA { get; set; }
        public string INSTRUMENTPRICE { get; set; }
        public string HOUSEAREA { get; set; }
        public string DETECTAREA { get; set; }
        public string DETECTTYPE { get; set; }
        public string DETECTNUM { get; set; }
        public DateTime? APPLDATE { get; set; }
        public string DETECTPATH { get; set; }
        public string QUAINFO { get; set; }
        public string APPROVALSTATUS { get; set; }
        public string ADDDATE { get; set; }
        public string phone { get; set; }
        public DateTime? detectnumStartDate { get; set; }
        public DateTime? detectnumEndDate { get; set; }
        public DateTime? measnumStartDate { get; set; }
        public DateTime? measnumEndDate { get; set; }
        public string hasNumPerCount { get; set; }
        public string instrumentNum { get; set; }
        public string businessnumPath { get; set; }
        public string approveadvice { get; set; }
        public string subunitnum { get; set; }
        public string issubunit { get; set; }
        public string supunitcode { get; set; }
        public string subunitdutyman { get; set; }
        public string area { get; set; }
        public string detectunit { get; set; }
        public DateTime? detectappldate { get; set; }
        public string shebaopeoplelistpath { get; set; }
        public string sysarea { get; set; }
        public string yharea { get; set; }
        public string captial { get; set; }
        public string credit { get; set; }
        public string companytype { get; set; }
        public string floorarea { get; set; }
        public string yearplanproduce { get; set; }
        public string preyearproduce { get; set; }
        public string businesspermit { get; set; }
        public string businesspermitpath { get; set; }
        public string enterprisemanager { get; set; }
        public string financeman { get; set; }
        public string director { get; set; }
        public string cerfgrade { get; set; }
        public string cerfno { get; set; }
        public string cerfnopath { get; set; }
        public string sslcmj { get; set; }
        public string sslczk { get; set; }
        public string szssccnl { get; set; }
        public string fmhccnl { get; set; }
        public string chlccnl { get; set; }
        public string ytwjjccnl { get; set; }
        public string managercount { get; set; }
        public string jsglcount { get; set; }
        public string testcount { get; set; }
        public string shebaopeoplenum { get; set; }
        public string workercount { get; set; }
        public string zgcount { get; set; }
        public string instrumentpath { get; set; }
        public int? datatype { get; set; }
        public string ispile { get; set; }
        public string cmanumcerf { get; set; }
        public string SelectTel { get; set; }
        public string AppealTel { get; set; }
        public string AppealEmail { get; set; }
        public string recordStatus { get; set; }
        public DateTime? recordTime { get; set; }
        public DateTime? recordEndTime { get; set; }
        public string managersoft { get; set; }
        public string sslcfbmj { get; set; }
        public string avlstatus { get; set; }
        public int? avlid { get; set; }
        public string businessgov { get; set; }
        public DateTime? cerftime { get; set; }
        public int? isupload { get; set; }
        public DateTime? printdate { get; set; }
        public string balsh { get; set; }
        public string pid { get; set; }
        public int? isfz { get; set; }
    }

    public class t_hnt_curingrecord
    {
        public int Id { get; set; }
        public string customid { get; set; }
        public string samplenum { get; set; }
        public string reportnum { get; set; }
        public string sampletype { get; set; }
        public string phbnum { get; set; }
        public string sctzdnum { get; set; }
        public string scrwdnum { get; set; }
        public string qddj { get; set; }
        public string ksdj { get; set; }
        public string qtzb { get; set; }
        public DateTime? cxrq { get; set; }
        public string checkresult { get; set; }
        public DateTime? checktime { get; set; }
        public string remarks { get; set; }
        public DateTime? addtime { get; set; }
        public DateTime? updatetime { get; set; }
        public string YANGHUTIAOJIAN { get; set; }
        public DateTime? CHENGXINGRIQI { get; set; }
        public string CHICUN { get; set; }
        public string CUSTOMNAME { get; set; }
        public string QIANGDUDENGJI { get; set; }
        public string RWDNUM { get; set; }
        public string SAMPLETYPES { get; set; }
        public string SCPHBNUM { get; set; }
        public string SYPHBNUM { get; set; }
        public string XINGZHUANG { get; set; }
    }


    public class t_hnt_materialIn
    {

        public int Id { get; set; }
        public string customid { get; set; }
        public string customname { get; set; }
        public string inno { get; set; }
        public string typename { get; set; }
        public string itemcode { get; set; }
        public double? num { get; set; }
        public string unit { get; set; }
        public string filepath { get; set; }
        public string factory { get; set; }
        public DateTime? intime { get; set; }
        public DateTime? addtime { get; set; }
        public DateTime? updatetime { get; set; }
        public string factorynum { get; set; }
        public string REPORTNUM { get; set; }
    }


    public class t_hnt_materialcheck
    {

        public int Id { get; set; }
        public string customid { get; set; }
        public string inno { get; set; }
        public string reportnum { get; set; }
        public string checkresult { get; set; }
        public string remarks { get; set; }
        public DateTime? addtime { get; set; }
    }


    public class t_hnt_materialRecord
    {

        public int Id { get; set; }
        public string customid { get; set; }
        public string samplenum { get; set; }
        public DateTime? sampletime { get; set; }
        public string inno { get; set; }
        public string reportnum { get; set; }
        public string checkresult { get; set; }
        public DateTime? checktime { get; set; }
        public string remarks { get; set; }
        public DateTime? addtime { get; set; }
        public DateTime? updatetime { get; set; }
        public string CUSTOMNAME { get; set; }
        public string RECORDNO { get; set; }
        public string TYPENAME { get; set; }
        public string ITEMCODE { get; set; }
        public double? NUM { get; set; }
        public string UNIT { get; set; }
        public DateTime? RECORDTIME { get; set; }
    }


    public class t_hnt_strengthrecord
    {

        public int Id { get; set; }
        public string customid { get; set; }
        public string customname { get; set; }
        public string strno { get; set; }
        public DateTime? strtime { get; set; }
        public string dengji { get; set; }
        public int? samplegroup { get; set; }
        public double? biaozhunzhi { get; set; }
        public double? pingjunzhi { get; set; }
        public double? biaozhuncha { get; set; }
        public double? zuixiaozhi { get; set; }
        public double? pandingxishu { get; set; }
        public string result { get; set; }
        public DateTime? addtime { get; set; }
        public DateTime? UPDATETIME { get; set; }
    }


    public class t_hnt_blendsadvice
    {

        public int Id { get; set; }
        public string customid { get; set; }
        public string rwdh { get; set; }
        public string tzdbh { get; set; }
        public string syphbbh { get; set; }
        public string scphbbh { get; set; }
        public string llzzb { get; set; }
        public DateTime? xdsj { get; set; }
        public DateTime? sctime { get; set; }
        public string pzr { get; set; }
        public DateTime? addtime { get; set; }
        public DateTime? updatetime { get; set; }
    }


    public class t_hnt_phb
    {

        public int Id { get; set; }
        public string customid { get; set; }
        public string rwdh { get; set; }
        public string scphbbh { get; set; }
        public string syphbbh { get; set; }
        public double? sp_cemt { get; set; }
        public double? sp_water { get; set; }
        public double? sp_szi1 { get; set; }
        public double? sp_szi1_water { get; set; }
        public double? sp_szi1_sii { get; set; }
        public double? sp_szi2 { get; set; }
        public double? sp_szi2_water { get; set; }
        public double? sp_szi2_sii { get; set; }
        public double? sp_sii1 { get; set; }
        public double? sp_sii1_water { get; set; }
        public double? sp_sii2 { get; set; }
        public double? sp_sii2_water { get; set; }
        public double? sp_mt { get; set; }
        public double? sp_mtt { get; set; }
        public double? sp_wj1 { get; set; }
        public double? sp_wj2 { get; set; }
        public double? sp_qt { get; set; }
        public DateTime? addtime { get; set; }
        public DateTime? updatetime { get; set; }
        public string CUSTOMNAME { get; set; }
        public string PHBTZNO { get; set; }
        public string PHBNO { get; set; }
    }


    public class t_hnt_factorytest
    {

        public int Id { get; set; }
        public string customid { get; set; }
        public string phbbh { get; set; }
        public string qddj { get; set; }
        public string rwdh { get; set; }
        public string lzzs { get; set; }
        public string kyypbh { get; set; }
        public string kyqdz { get; set; }
        public DateTime? kylzsj { get; set; }
        public string kzypbh { get; set; }
        public string kzqdz { get; set; }
        public DateTime? kzlzsj { get; set; }
        public string ksypbh { get; set; }
        public string ksjg { get; set; }
        public DateTime? kslzsj { get; set; }
        public string llzzb { get; set; }
        public string llzjg { get; set; }
        public DateTime? llzlzsj { get; set; }
        public string qtzb { get; set; }
        public string ccjl { get; set; }
        public DateTime? addtime { get; set; }
        public DateTime? updatetime { get; set; }
    }



    public class t_hnt_productiontask
    {

        public int Id { get; set; }
        public string customid { get; set; }
        public string rwdh { get; set; }
        public string htbh { get; set; }
        public string tzdbh { get; set; }
        public string qddj { get; set; }
        public string ksdj { get; set; }
        public string kddj { get; set; }
        public string tld { get; set; }
        public string jzbw { get; set; }
        public string qtzb { get; set; }
        public string jhscsl { get; set; }
        public string scxmc { get; set; }
        public DateTime? ghsj { get; set; }
        public DateTime? xdsj { get; set; }
        public DateTime? addtime { get; set; }
        public DateTime? updatetime { get; set; }
    }


    public class t_sys_IpManager
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string CustomId { get; set; }
        public int? IsUse { get; set; }
    }

    public class t_ht_hunningtu
    {
       
        public int id { get; set; }
        public string htbah { get; set; }
        public string jf { get; set; }
        public string yf { get; set; }
        public string jsdw { get; set; }
        public string sgdw { get; set; }
        public string gcmc { get; set; }
        public string gcdd { get; set; }
        public string gyl { get; set; }
        public string sybw { get; set; }
        public DateTime? gyStartTime { get; set; }
        public DateTime? gyendTime { get; set; }
        public string hntgc { get; set; }
        public DateTime? qdsj { get; set; }
        public string htpath { get; set; }
        public string valname { get; set; }
        public DateTime? valtime { get; set; }
        public int? status { get; set; }
        public DateTime? addtime { get; set; }
        public string usercode { get; set; }
        public string avlname { get; set; }
        public DateTime? avltime { get; set; }
        public string unitcode { get; set; }
        public string hnttype { get; set; }
        public string gcbm { get; set; }
        public string tel { get; set; }
    }

    public class t_bp_productline
    {
       
        public int id { get; set; }
        public string customid { get; set; }
        public string bm { get; set; }
        public string name { get; set; }
        public string unit { get; set; }
        public string remark { get; set; }
    }

    public class t_hr_wsdRecord
    {
       
        public int id { get; set; }
        public string customid { get; set; }
        public string instrumentNum { get; set; }
        public string workRoomName { get; set; }
        public DateTime? acsdate { get; set; }
        public int? acstime { get; set; }
        public double? temperature { get; set; }
        public double? humidity { get; set; }
    }

}