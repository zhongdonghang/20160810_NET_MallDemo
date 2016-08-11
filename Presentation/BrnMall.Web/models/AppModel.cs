using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BrnMall.Core;
using BrnMall.Web.Framework;
using System.ComponentModel.DataAnnotations;

namespace BrnMall.Web.Models
{
    #region 分类
    /// <summary>
    /// 分类列表模型类
    /// </summary>
    public class AppCategoryListModel
    {
        public int CateId { get; set; }
        public List<CategoryInfo> CateLay1 { get; set; }
        public List<AppCategoryListLayModel> CateLay2 { get; set; }
    }
    public class AppCategoryListLayModel
    {
        public string CateName { get; set; }
        public int CateId { get; set; }
        public List<StoreProductInfo> ProList { get; set; }
    }

    /// <summary>
    /// 品牌分类列表
    /// </summary>
    public class AppBrandListModel
    {
        public int CateId { get; set; }
        public List<CategoryInfo> CateLay1 { get; set; }
        public List<BrandInfo> BrandList { get; set; }
    }

    public class CategoryAppModel
    {
        public List<KeyValuePair<AttributeInfo, List<AttributeValueInfo>>> AAndVList { get; set; }
        public int BrandId { get; set; }
        public List<BrandInfo> BrandList { get; set; }
        public CategoryInfo CategoryInfo { get; set; }
        public int CateId { get; set; }
        public string[] CatePriceRangeList { get; set; }
        public string FilterAttr { get; set; }
        public int FilterPrice { get; set; }
        public int OnlyStock { get; set; }
        public PageModel PageModel { get; set; }
        public List<StoreProductInfo> ProductList { get; set; }
        public int SortColumn { get; set; }
        public int SortDirection { get; set; }
    }

    public class CateListAppModel
    {
        public int cateId;
        public string cateName;
    }
    #endregion

    #region 商品
    /// <summary>
    /// 收藏商品model
    /// </summary>
    public class FavoriteProductAppModel
    {
        public int recordid { get; set; }
        public int uid { get; set; }
        public int state { get; set; }
        public string addtime { get; set; }
        public int pid { get; set; }
        public int storeid { get; set; }
        public string name { get; set; }
        public decimal shopprice { get; set; }
        public string showimg { get; set; }
        public string storename { get; set; }
    }

    /// <summary>
    /// 商品评价model
    /// </summary>
    public class AppProReviewModel
    {
        public string NickName { get; set; }
        public DateTime ReviewTime { get; set; }
        public string Content { get; set; }
        public string UserImg { get; set; }
        public int Star { get; set; }
        public DateTime ReplyTime { get; set; }
        public string ReplyContent { get; set; }
    }

    /// <summary>
    /// 商城搜索模型类
    /// </summary>
    public class AppMallSearchModel
    {
        /// <summary>
        /// 搜索词
        /// </summary>
        public string Word { get; set; }
        /// <summary>
        /// 分类id
        /// </summary>
        public int CateId { get; set; }
        /// <summary>
        /// 品牌id
        /// </summary>
        public int BrandId { get; set; }
        /// <summary>
        /// 筛选价格
        /// </summary>
        public int FilterPrice { get; set; }
        /// <summary>
        /// 排序列
        /// </summary>
        public int SortColumn { get; set; }
        /// <summary>
        /// 排序方向
        /// </summary>
        public int SortDirection { get; set; }
        /// <summary>
        /// 品牌列表
        /// </summary>
        public List<BrandInfo> BrandList { get; set; }
        /// <summary>
        /// 价格范围列表
        /// </summary>
        public string[] PriceRangeList { get; set; }
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 商品列表
        /// </summary>
        public List<StoreProductInfo> ProductList { get; set; }
        /// <summary>
        /// 第一级分类
        /// </summary>
        public List<CategoryInfo> CateLay1List { get; set; }
    }
    #endregion

    #region 店铺
    public class FavoriteStoreAppModel
    {
        public int recordid { get; set; }
        public int uid { get; set; }
        public int storeid { get; set; }
        public string addtime { get; set; }
        public string name { get; set; }
        public string logo { get; set; }

    }
    #endregion

    #region 订单
    public class OrderListAppModel
    {
        public List<OrderModel> OrderList { get; set; }
        public PageModel PageModel { get; set; }
        public string Keyword { get; set; }
        public string OrderState { get; set; }
    }
    public class OrderModel
    {
        public int OId { get; set; }
        public string OSN { get; set; }
        public int UId { get; set; }
        public int OrderState { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal Surplusmoney { get; set; }
        public int ParentId { get; set; }
        public int IsReview { get; set; }
        public string AddTime { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int ShipCoid { get; set; }
        public string ShipCoName { get; set; }
        public string PayFriendName { get; set; }
        public int PayMode { get; set; }
        public string Consignee { get; set; }
        public List<ProductListModel> ProList { get; set; }
    }

    public class ProductListModel
    {
        public int PId { get; set; }
        public string PName { get; set; }
        public string ShowImg { get; set; }
    }

    public class ShipFreeAmountModel
    {
        public decimal ShipFree { get; set; }
        public decimal ProductAmount { get; set; }
    }

    public class PayPluginListModel
    {
        public decimal UserAmount { get; set; }
        public List<PluginInfo> pluginlist { get; set; }
    }
    #endregion

    #region 用户
    public class UserInfoAppModel
    {
        public string UserName { get; set; }
        public string RankTitle { get; set; }
        [StringLength(20, ErrorMessage = "名称长度不能大于20")]
        public string NickName { get; set; }
        [StringLength(5, ErrorMessage = "名称长度不能大于5")]
        public string RealName { get; set; }
        [Range(0, 2, ErrorMessage = "请选择确切的性别")]
        public int Gender { get; set; }
        public string Avatar { get; set; }
        [StringLength(20, ErrorMessage = "证件号码位数不能大于20")]
        public string IdCard { get; set; }
        public string BirthDay { get; set; }
        public int RegionId { get; set; }
        [StringLength(150, ErrorMessage = "密码长度不能大于125")]
        public string Bio { get; set; }
        [StringLength(75, ErrorMessage = "密码长度不能大于75")]
        public string Address { get; set; }
    }
    #endregion

    #region 资金
    /// <summary>
    /// 支付积分日志列表模型类
    /// </summary>
    public class UserAmountLogAppModel
    {
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 支付积分日志列表
        /// </summary>
        public List<CreditLogInfo> PayCreditLogList { get; set; }

        /// <summary>
        /// 最低取现额度（申请提现的额度不能低于该额度）
        /// </summary>
        public decimal MinWithCash { get; set; }
        /// <summary>
        /// 最低申请提现额度（余额必须超过该数才有资格申请提现）
        /// </summary>
        public decimal MinApplyWithCash { get; set; }

        /// <summary>
        /// 用户当前可提现金额
        /// </summary>
        public decimal UserAmount { get; set; }
        /// <summary>
        /// 用户冻结金额
        /// </summary>
        public decimal UserFrozenAmount { get; set; }
    }
#endregion
}