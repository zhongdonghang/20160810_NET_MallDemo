using System;
using System.Collections.Generic;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;

namespace BrnMall.Web.Mobile.Models
{
    /// <summary>
    /// 分类列表模型类
    /// </summary>
    public class CategoryListModel
    {
        public int CateId { get; set; }
        public List<CategoryInfo> CateLay1 { get; set; }
        public List<CategoryListLayModel> CateLay2 { get; set; }
    }
    public class CategoryListLayModel
    {
        public string CateName { get; set; }
        public int CateId { get; set; }
        public List<StoreProductInfo> ProList { get; set; }
    }

    /// <summary>
    /// 品牌分类列表
    /// </summary>
    public class BrandListModel
    {
        public int CateId { get; set; }
        public List<CategoryInfo> CateLay1 { get; set; }
        public List<BrandInfo> BrandList { get; set; }
    }
}