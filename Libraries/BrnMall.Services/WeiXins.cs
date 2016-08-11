using System;
using System.Text;

using BrnMall.Core;

namespace BrnMall.Services
{
    /// <summary>
    /// 短信操作管理类
    /// </summary>
    public partial class WeiXins
    {
        private static object _locker = new object();//锁对象
        private static IWeiXin _iweixin = null;//短信策略

        static WeiXins()
        {
            _iweixin = BMAWeiXin.Instance;
        }

 
        /// <summary>
        /// 获取当前微信Unionid
        /// </summary>
        /// <returns></returns>
        public static string GetMyWeiXinUnionid()
        {
            return _iweixin.GetMyWeiXinUnionid();
        }

        /// <summary>
        /// 获取当前微信号信息
        /// </summary>
        /// <returns></returns>
        public static WeiXinUserInfo GetMyWeiXinUserInfo()
        {
            return _iweixin.GetMyWeiXinUserInfo();
        }

    }
}
