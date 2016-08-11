using System;
using System.IO;

namespace BrnMall.Core
{
    /// <summary>
    /// BrnMall短信管理类
    /// </summary>
    public class BMAWeiXin
    {
        private static IWeiXin _iweixin = null;//短信策略
        private static ISearchStrategy _isearchstrategy = null;//搜索策略
        static BMAWeiXin()
        {
            try
            {
                _iweixin = (IWeiXin)Activator.CreateInstance(Type.GetType("BrnMall.PayPlugin.WeiXin.WeiXin,BrnMall.PayPlugin.WeiXin",false,true));
            
            }
            catch
            {
                throw new BMAException("创建微信策略对象失败");
            }
        }

        /// <summary>
        /// 微信策略实例
        /// </summary>
        public static IWeiXin Instance
        {
            get { return _iweixin; }
        }
    }
}
