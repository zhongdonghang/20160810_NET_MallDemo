using System;
using System.Collections.Generic;

using BrnMall.Core;

namespace BrnMall.Services
{
    /// <summary>
    /// 后台banner操作管理类
    /// </summary>
    public partial class AdminBanners : Banners
    {
        /// <summary>
        /// 后台获得banner列表
        /// </summary>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <returns></returns>
        public static List<BannerInfo> AdminGetBannerList(int pageSize, int pageNumber)
        {
            return BrnMall.Data.Banners.AdminGetBannerList(pageSize, pageNumber);
        }

        /// <summary>
        /// 后台获得banner数量
        /// </summary>
        /// <returns></returns>
        public static int AdminGetBannerCount()
        {
            return BrnMall.Data.Banners.AdminGetBannerCount();
        }

        /// <summary>
        /// 后台获得banner
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public static BannerInfo AdminGetBannerById(int id)
        {
            if (id > 0)
                return BrnMall.Data.Banners.AdminGetBannerById(id);
            return null;
        }

        /// <summary>
        /// 创建banner
        /// </summary>
        public static void CreateBanner(BannerInfo bannerInfo)
        {
            BrnMall.Data.Banners.CreateBanner(bannerInfo);
            BrnMall.Core.BMACache.Remove(CacheKeys.MALL_BANNER_HOMELIST + bannerInfo.Type);
        }

        /// <summary>
        /// 更新banner
        /// </summary>
        public static void UpdateBanner(BannerInfo bannerInfo)
        {
            BrnMall.Data.Banners.UpdateBanner(bannerInfo);
            BrnMall.Core.BMACache.Remove(CacheKeys.MALL_BANNER_HOMELIST + bannerInfo.Type);
        }

        /// <summary>
        /// 删除banner
        /// </summary>
        /// <param name="idList">id列表</param>
        public static void DeleteBannerById(int[] idList)
        {
            if (idList != null && idList.Length > 0)
            {
                BrnMall.Data.Banners.DeleteBannerById(CommonHelper.IntArrayToString(idList));
                BrnMall.Core.BMACache.Remove(CacheKeys.MALL_BANNER_HOMELIST + "0");
                BrnMall.Core.BMACache.Remove(CacheKeys.MALL_BANNER_HOMELIST + "1");
            }
        }
    }
}
