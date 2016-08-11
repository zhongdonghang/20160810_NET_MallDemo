using System;
using System.Collections.Generic;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;

namespace BrnMall.Web.Models
{
    /// <summary>
    /// 店铺分类模型类
    /// </summary>
    public class StoreClassModel
    {
        /// <summary>
        /// 店铺分类id
        /// </summary>
        public int StoreCid { get; set; }
        /// <summary>
        /// 排序列
        /// </summary>
        public int SortColumn { get; set; }
        /// <summary>
        /// 排序方向
        /// </summary>
        public int SortDirection { get; set; }
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 商品列表
        /// </summary>
        public List<PartProductInfo> ProductList { get; set; }
        /// <summary>
        /// 店铺分类信息
        /// </summary>
        public StoreClassInfo StoreClassInfo { get; set; }
    }

    /// <summary>
    /// 店铺搜索模型类
    /// </summary>
    public class StoreSearchModel
    {
        /// <summary>
        /// 搜索词
        /// </summary>
        public string Word { get; set; }
        /// <summary>
        /// 店铺分类id
        /// </summary>
        public int StoreCid { get; set; }
        /// <summary>
        /// 开始价格
        /// </summary>
        public int StartPrice { get; set; }
        /// <summary>
        /// 结束价格
        /// </summary>
        public int EndPrice { get; set; }
        /// <summary>
        /// 排序列
        /// </summary>
        public int SortColumn { get; set; }
        /// <summary>
        /// 排序方向
        /// </summary>
        public int SortDirection { get; set; }
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 商品列表
        /// </summary>
        public List<PartProductInfo> ProductList { get; set; }
        /// <summary>
        /// 店铺分类信息
        /// </summary>
        public StoreClassInfo StoreClassInfo { get; set; }
    }

    /// <summary>
    /// 申请开通店铺模型
    /// </summary>
    public class storeApplyStep1Model
    {
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 店铺类型(0代表个人,1代表公司)
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 店长名称
        /// </summary>
        public string StoreKeeperName { get; set; }

        /// <summary>
        /// 标识号
        /// </summary>
        public string IdCard { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
    }

    public class storeApplyStep2Model
    {
     /// <summary>
        /// 区域id
        /// </summary>
        public int RegionId { get; set; }

        /// <summary>
        /// 行业id
        /// </summary>
        public int StoreIid { get; set; }

        /// <summary>
        /// logo
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 固定电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// qq
        /// </summary>
        public string QQ { get; set; }

        /// <summary>
        /// 支付宝账号
        /// </summary>
        public string WW { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Banner
        /// </summary>
        public string Banner { get; set; }

        /// <summary>
        /// 公告
        /// </summary>
        public string Announcement { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}