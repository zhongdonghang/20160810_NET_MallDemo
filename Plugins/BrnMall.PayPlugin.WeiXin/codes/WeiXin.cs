using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.Core;
using BrnMall.PayPlugin.WeiXin.codes.weixinpay;
using BrnMall.PayPlugin.WeiXin.codes;
using System.Web;
using LitJson;

namespace BrnMall.PayPlugin.WeiXin
{
    public partial class WeiXin : IWeiXin
    {
        //获得当前用户Unionid
        public string GetMyWeiXinUnionid()
        {
            WeiXinUserInfo wxinfo = GetMyWeiXinUserInfo();
            if (wxinfo != null)
            {
                return wxinfo.Unionid;
            }
            return "";
        }


        /// <summary>
        /// 获得当前用户信息
        /// </summary>
        /// <returns></returns>
        public WeiXinUserInfo GetMyWeiXinUserInfo()
        {
            WeiXinUserInfo wxuser = null;
            try
            {
                //获取code
                JsApiPay jsApiPay = new JsApiPay();
                if (string.IsNullOrWhiteSpace(WebHelper.GetQueryString("code")))
                {
                    //构造网页授权获取code的URL
                    string redirect_uri = HttpUtility.UrlEncode(WebHelper.GetUrl().Trim());
                    Log.Debug(this.GetType().ToString(), "redirect_uri标记： " + redirect_uri);
                    WxPayData data = new WxPayData();
                    data.SetValue("appid", WxPayConfig.AppId);
                    data.SetValue("redirect_uri", redirect_uri);
                    data.SetValue("response_type", "code");
                    data.SetValue("scope", "snsapi_userinfo");
                    data.SetValue("state", "1#wechat_redirect");
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                    Log.Debug(this.GetType().ToString(), "Will Redirect to URL : " + url);

                    //触发微信返回code码
                    HttpContext.Current.Response.Redirect(url);//Redirect函数会抛出ThreadAbortException异常，不用处理这个异常

                }
                string code = WebHelper.GetQueryString("code");
                Log.Debug("", "code为：" + code);
                if (!string.IsNullOrWhiteSpace(code))
                {
                    //调用【网页授权获取用户信息】接口获取用户的openid和access_token
                    jsApiPay.GetOpenidAndAccessTokenFromCode(code);

                    //构造网页授权获取用户信息的URL
                    WxPayData data = new WxPayData();
                    data.SetValue("access_token", jsApiPay.access_token);
                    data.SetValue("openid", jsApiPay.openid);
                    data.SetValue("lang", "zh_CN");
                    string url = " https://api.weixin.qq.com/sns/userinfo?" + data.ToUrl();

                    //请求url以获取数据
                    string getstr = HttpService.Get(url);
                    Log.Debug("", "获取用户信息为：" + getstr);
                    JsonData jd = JsonMapper.ToObject(getstr);
                    wxuser = new WeiXinUserInfo()
                    {
                        OpenId = (string)jd["openid"],
                        NickName = (string)jd["nickname"],
                        HeadImgUrl = (string)jd["headimgurl"],
                        Sex = (int)jd["sex"],
                        Unionid = (string)jd["unionid"]
                    };
                }

                return wxuser;
            }
            catch (Exception ex)
            {
                Log.Error("", "获取用户信息出错！原因：" + ex.ToString());
                return wxuser;
            }
        }
                
    }
}
