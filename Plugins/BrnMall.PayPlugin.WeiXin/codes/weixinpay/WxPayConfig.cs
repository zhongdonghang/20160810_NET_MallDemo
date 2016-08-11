using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrnMall.PayPlugin.WeiXin.codes.weixinpay
{
    /**
     * 	配置账号信息
     */
    public class WxPayConfig
    {
        private static string _appid = "";//绑定支付的APPID（必须配置）
        private static string _mchid = "";//商户号（必须配置）
        private static string _iosappid = "";//绑定支付的APPID（必须配置）
        private static string _iosmchid = "";//商户号（必须配置）
        private static string _androidappid = "";//绑定支付的APPID（必须配置）
        private static string _androidmchid = "";//商户号（必须配置）
        private static string _key = ""; //商户支付密钥，参考开户邮件设置（必须配置）
        private static string _appsecret = "";//公众帐号secert（仅JSAPI支付的时候需要配置）

         //=======【证书路径设置】===================================== 
        /* 证书路径,注意应该填写绝对路径（仅退款、撤销订单时需要）
        */
        public const string _sslcert_path = "cert/apiclient_cert.p12";
        public const string _sslcert_password = "1233410002";

          //=======【支付结果通知url】===================================== 
        /* 支付结果通知回调url，用于商户接收支付结果
        */
        public const string _notify_url = "weixin/payresult";

         //=======【商户系统后台机器IP】===================================== 
        /* 此参数可手动配置也可在程序中自动获取
        */
        public const string _ip = "8.8.8.8";

          //=======【代理服务器设置】===================================
        /* 默认IP和端口号分别为0.0.0.0和0，此时不开启代理（如有需要才设置）
        */
        public const string _proxy_url = "http://10.152.18.220:8080";

        //=======【上报信息配置】===================================
        /* 测速上报等级，0.关闭上报; 1.仅错误时上报; 2.全量上报
        */
        public const int _report_levenl = 2;

        //=======【日志级别】===================================
        /* 日志等级，0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息
        */
        public const int _log_levenl = 3;




        static WxPayConfig()
        {
            _appid = PluginUtils.GetPluginSet().APPID;
            _mchid = PluginUtils.GetPluginSet().MCHID;
            _iosappid = PluginUtils.GetPluginSet().IOSAPPID;
            _iosmchid = PluginUtils.GetPluginSet().IOSMCHID;
            _androidappid = PluginUtils.GetPluginSet().ANDROIDAPPID;
            _androidmchid = PluginUtils.GetPluginSet().ANDROIDMCHID;
            _key = PluginUtils.GetPluginSet().Key;
            _appsecret = PluginUtils.GetPluginSet().APPSECRET;


        }
        /// <summary>
        /// 重置微信配置
        /// </summary>
        public static void ReSet()
        {
            _appid = PluginUtils.GetPluginSet().APPID;
            _mchid = PluginUtils.GetPluginSet().MCHID;
            _iosappid = PluginUtils.GetPluginSet().IOSAPPID;
            _iosmchid = PluginUtils.GetPluginSet().IOSMCHID;
            _androidappid = PluginUtils.GetPluginSet().ANDROIDAPPID;
            _androidmchid = PluginUtils.GetPluginSet().ANDROIDMCHID;
            _key = PluginUtils.GetPluginSet().Key;
            _appsecret = PluginUtils.GetPluginSet().APPSECRET;
        }


        /// <summary>
        /// 绑定支付的APPID
        /// </summary>
        public static string AppId
        {
            get { return _appid; }
        }

        /// <summary>
        /// 商户号
        /// </summary>
        public static string MchId
        {
            get { return _mchid; }
        }

        /// <summary>
        /// 绑定支付的APPID
        /// </summary>
        public static string IosAppId
        {
            get { return _iosappid; }
        }

        /// <summary>
        /// 商户号
        /// </summary>
        public static string IosMchId
        {
            get { return _iosmchid; }
        }

        /// <summary>
        /// 绑定支付的APPID
        /// </summary>
        public static string AndroidAppId
        {
            get { return _androidappid; }
        }

        /// <summary>
        /// 商户号
        /// </summary>
        public static string AndroidMchId
        {
            get { return _androidmchid; }
        }

        /// <summary>
        /// 商户支付密钥
        /// </summary>
        public static string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// 公众帐号secert
        /// </summary>
        public static string AppSecret
        {
            get { return _appsecret; }
        }

    }
}
