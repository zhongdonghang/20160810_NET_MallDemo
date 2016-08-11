using System;
using System.Web.Mvc;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using System.Web.Security;

namespace BrnMall.Web.Mobile.Controllers
{
    /// <summary>
    /// 首页控制器类
    /// </summary>
    public partial class HomeController : BaseMobileController
    {

        //public string Token = "weixin";

        /// <summary>
        /// 首页
        /// </summary>
        public ActionResult Index()
        {
            //首页的数据需要在其视图文件中直接调用，所以此处不再需要视图模型
            return View();
        }


        /// <summary>
        /// 微信验证token（测试）
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidToken()
        {
            ////验证token 
            //string echoStr = Request.QueryString["echoStr"];


            //if (CheckSignature() && !string.IsNullOrEmpty(echoStr))
            //{

            //    Response.Write(echoStr);

            //    Response.End();

            //}
            return View();
        }

        /// <summary>

        /// 验证微信签名

        /// </summary>

        /// * 将token、timestamp、nonce三个参数进行字典序排序

        /// * 将三个参数字符串拼接成一个字符串进行sha1加密

        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。

        /// <returns></returns>

        //private bool CheckSignature()
        //{

        //    string signature = Request.QueryString["signature"];

        //    string timestamp = Request.QueryString["timestamp"];

        //    string nonce = Request.QueryString["nonce"];

        //    string[] arrTmp = { Token, timestamp, nonce };

        //    Array.Sort(arrTmp);

        //    string tmpStr = string.Join("", arrTmp);

        //    tmpStr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");

        //    if (tmpStr != null)
        //    {

        //        tmpStr = tmpStr.ToLower();

        //        return tmpStr == signature;

        //    }

        //    return false;

        //}
    }
}
