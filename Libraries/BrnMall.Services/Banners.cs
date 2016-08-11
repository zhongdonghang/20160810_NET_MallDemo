using System;

using BrnMall.Core;

namespace BrnMall.Services
{
    /// <summary>
    /// banner操作管理类
    /// </summary>
    public partial class Banners
    {
        /// <summary>
        /// 获得首页banner列表
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static BannerInfo[] GetHomeBannerList(int type)
        {
            BannerInfo[] bannerList = BrnMall.Core.BMACache.Get(CacheKeys.MALL_BANNER_HOMELIST + type) as BannerInfo[];
            if (bannerList == null)
            {
                bannerList = BrnMall.Data.Banners.GetHomeBannerList(type, DateTime.Now);
                BrnMall.Core.BMACache.Insert(CacheKeys.MALL_BANNER_HOMELIST + type, bannerList);
            }
            return bannerList;
        }
    }
}
