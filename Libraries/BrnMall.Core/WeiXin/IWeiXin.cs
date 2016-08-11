using System;

namespace BrnMall.Core
{
    /// <summary>
    /// 短信策略接口
    /// </summary>
    public partial interface IWeiXin
    {
        /// <summary>
        /// 获取当前微信Unionid
        /// </summary>
        string GetMyWeiXinUnionid();

        /// <summary>
        /// 获取当前微信用户信息
        /// </summary>
        /// <returns></returns>
        WeiXinUserInfo GetMyWeiXinUserInfo();
    }
}
