using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.Core;
using BrnMall.PayPlugin.WeiXin.codes.weixinpay;

namespace BrnMall.PayPlugin.WeiXin.codes
{
    /// <summary>
    /// 插件工具类
    /// </summary>
    public class PluginUtils
    {
        private static object _locker = new object();//锁对象
        private static PluginSetInfo _pluginsetinfo = null;//插件设置信息
        private static string _dbfilepath = "/plugins/BrnMall.PayPlugin.WeiXin/db.config";//数据文件路径

        // <summary>
        //获得插件设置
        // </summary>
        // <returns></returns>
        public static PluginSetInfo GetPluginSet()
        {
            if (_pluginsetinfo == null)
            {
                lock (_locker)
                {
                    if (_pluginsetinfo == null)
                    {
                        _pluginsetinfo = (PluginSetInfo)IOHelper.DeserializeFromXML(typeof(PluginSetInfo), IOHelper.GetMapPath(_dbfilepath));
                    }
                }
            }
            return _pluginsetinfo;
        }

         //<summary>
         //保存插件设置
         //</summary>
        public static void SavePluginSet(PluginSetInfo pluginSetInfo)
        {
            lock (_locker)
            {
                IOHelper.SerializeToXml(pluginSetInfo, IOHelper.GetMapPath(_dbfilepath));
                _pluginsetinfo = null;
                WxPayConfig.ReSet();
            }
        }

    }


    /// <summary>
    /// 插件设置信息类
    /// </summary>
    public class PluginSetInfo
    {

        private static string _appid = "";//绑定支付的APPID（必须配置）
        private static string _mchid = "";//商户号（必须配置）
        private static string _iosappid = "";//绑定支付的APPID（必须配置）
        private static string _iosmchid = "";//商户号（必须配置）
        private static string _androidappid = "";//绑定支付的APPID（必须配置）
        private static string _androidmchid = "";//商户号（必须配置）
        private static string _key = ""; //商户支付密钥，参考开户邮件设置（必须配置）
        private static string _appsecret = "";//公众帐号secert（仅JSAPI支付的时候需要配置）
        /// <summary>
        /// 绑定支付的APPID
        /// </summary>
        public string APPID
        {
            get { return _appid; }
            set { _appid = value; }
        }

        /// <summary>
        /// 商户号（必须配置）
        /// </summary>
        public string MCHID
        {
            get { return _mchid; }
            set { _mchid = value; }
        }

        /// <summary>
        /// 绑定支付的APPID
        /// </summary>
        public string IOSAPPID
        {
            get { return _iosappid; }
            set { _iosappid = value; }
        }

        /// <summary>
        /// 商户号（必须配置）
        /// </summary>
        public string IOSMCHID
        {
            get { return _iosmchid; }
            set { _iosmchid = value; }
        }

        /// <summary>
        /// 绑定支付的APPID
        /// </summary>
        public string ANDROIDAPPID
        {
            get { return _androidappid; }
            set { _androidappid = value; }
        }

        /// <summary>
        /// 商户号（必须配置）
        /// </summary>
        public string ANDROIDMCHID
        {
            get { return _androidmchid; }
            set { _androidmchid = value; }
        }

        /// <summary>
        /// 商户支付密钥，参考开户邮件设置（必须配置）
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }
        /// <summary>
        /// 公众帐号secert（仅JSAPI支付的时候需要配置）
        /// </summary>
        public string APPSECRET
        {
            get { return _appsecret; }
            set { _appsecret = value; }
        }

    }

}
