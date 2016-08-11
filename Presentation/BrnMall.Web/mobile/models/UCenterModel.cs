using System;
using System.Data;
using System.Collections.Generic;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using System.ComponentModel.DataAnnotations;

namespace BrnMall.Web.Mobile.Models
{
    /// <summary>
    /// 用户信息模型类
    /// </summary>
    public class UserInfoModel
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
        public DateTime BirthDay { get; set; }
        public int RegionId { get; set; }
        [StringLength(150, ErrorMessage = "密码长度不能大于125")]
        public string Bio { get; set; }
        [StringLength(75, ErrorMessage = "密码长度不能大于75")]
        public string Address { get; set; }
    }

    /// <summary>
    /// 安全验证模型类
    /// </summary>
    public class SafeVerifyModel
    {
        /// <summary>
        /// 动作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 验证方式
        /// </summary>
        public string Mode { get; set; }
    }

    /// <summary>
    /// 安全更新模型类
    /// </summary>
    public class SafeUpdateModel
    {
        /// <summary>
        /// 动作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// V
        /// </summary>
        public string V { get; set; }
    }

    /// <summary>
    /// 安全成功模型类
    /// </summary>
    public class SafeSuccessModel
    {
        /// <summary>
        /// 动作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// 订单列表模型类
    /// </summary>
    public class OrderListModel
    {
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 订单列表
        /// </summary>
        public DataTable OrderList { get; set; }
        /// <summary>
        /// 订单商品列表
        /// </summary>
        public List<OrderProductInfo> OrderProductList { get; set; }
        /// <summary>
        /// 开始添加时间
        /// </summary>
        public string StartAddTime { get; set; }
        /// <summary>
        /// 结束添加时间
        /// </summary>
        public string EndAddTime { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public int OrderState { get; set; }
        /// <summary>
        /// 搜索词
        /// </summary>
        public string KeyWord { get; set; }
    }

    /// <summary>
    /// 订单列表模型类
    /// </summary>
    public class AjaxOrderListModel
    {
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 订单列表
        /// </summary>
        public List<Dictionary<string, object>> OrderList { get; set; }
        /// <summary>
        /// 订单商品列表
        /// </summary>
        public List<OrderProductInfo> OrderProductList { get; set; }
    }

    /// <summary>
    /// 订单信息模型类
    /// </summary>
    public class OrderInfoModel
    {
        /// <summary>
        /// 订单信息
        /// </summary>
        public OrderInfo OrderInfo { get; set; }
        /// <summary>
        /// 区域信息
        /// </summary>
        public RegionInfo RegionInfo { get; set; }
        /// <summary>
        /// 订单商品列表
        /// </summary>
        public List<OrderProductInfo> OrderProductList { get; set; }
    }

    /// <summary>
    /// 订单动作列表模型类
    /// </summary>
    public class OrderActionListModel
    {
        /// <summary>
        /// 订单信息
        /// </summary>
        public OrderInfo OrderInfo { get; set; }
        /// <summary>
        /// 订单处理列表
        /// </summary>
        public List<OrderActionInfo> OrderActionList { get; set; }
    }

    /// <summary>
    /// 收藏夹商品列表模型类
    /// </summary>
    public class AjaxFavoriteProductListModel
    {
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 商品列表
        /// </summary>
        public List<Dictionary<string, object>> ProductList { get; set; }
    }

    /// <summary>
    /// 收藏夹店铺列表模型类
    /// </summary>
    public class AjaxFavoriteStoreListModel
    {
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 店铺列表
        /// </summary>
        public List<Dictionary<string, object>> StoreList { get; set; }
    }

    /// <summary>
    /// 浏览商品列表模型类
    /// </summary>
    public class AjaxBrowseProductListModel
    {
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 商品列表
        /// </summary>
        public List<PartProductInfo> ProductList { get; set; }
    }

    /// <summary>
    /// 配送地址列表模型类
    /// </summary>
    public class ShipAddressListModel
    {
        /// <summary>
        /// 配送地址列表
        /// </summary>
        public List<FullShipAddressInfo> ShipAddressList { get; set; }
        /// <summary>
        /// 配送地址数量
        /// </summary>
        public int ShipAddressCount { get; set; }
    }

    /// <summary>
    /// 配送地址模型类
    /// </summary>
    public class ShipAddressModel
    {
        public string Alias { get; set; }
        public string Consignee { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ZipCode { get; set; }
        public int ProvinceId { get; set; }
        public int CityId { get; set; }
        public int RegionId { get; set; }
        public string Address { get; set; }
        public int IsDefault { get; set; }
    }

    /// <summary>
    /// 优惠劵列表模型类
    /// </summary>
    public class CouponListModel
    {
        /// <summary>
        /// 列表类型
        /// </summary>
        public int ListType { get; set; }
        /// <summary>
        /// 优惠劵列表
        /// </summary>
        public DataTable CouponList { get; set; }
    }

    /// <summary>
    /// 评价订单模型类
    /// </summary>
    public class ReviewOrderModel
    {
        /// <summary>
        /// 订单信息
        /// </summary>
        public OrderInfo OrderInfo { get; set; }
        /// <summary>
        /// 订单商品列表
        /// </summary>
        public List<OrderProductInfo> OrderProductList { get; set; }
        /// <summary>
        /// 店铺评价信息
        /// </summary>
        public StoreReviewInfo StoreReviewInfo { get; set; }
    }

    /// <summary>
    /// 推荐者模型
    /// </summary>
    public class IntroduceShowModel
    {
        /// <summary>
        /// 我的推荐者
        /// </summary>
        public IntroduceModel introducer { get; set; }

        /// <summary>
        /// 我推荐的会员列表
        /// </summary>
        public List<IntroduceModel> MyIntroducers { get; set; }

        public int introduceCount { get; set; }
    }
    public class IntroduceModel
    {
        public string NickName { get; set; }
        public string UserName { get; set; }
        public string AddTime { get; set; }
    }

    /// <summary>
    /// 提现记录列表
    /// </summary>
    public class WithdrawalLogListModel
    {
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 支付积分日志列表
        /// </summary>
        public List<WithdrawalLogInfo> WithdrawalLogList { get; set; }
    }


    /// <summary>
    /// 提现记录模型
    /// </summary>
    public class WithdrawalLogModel
    {
        /// <summary>
        /// 日志id
        /// </summary>
        public int RecordId { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public int Uid { get; set; }
        /// <summary>
        /// 提现状态
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 提现方式
        /// </summary>
        [Required(ErrorMessage = "提现方式不能为空")]
        public int PayType { get; set; }
        /// <summary>
        /// 提现至的账号
        /// </summary>
        [Required(ErrorMessage = "账号不能为空")]
        [StringLength(50, ErrorMessage = "账号长度不能大于50")]
        public string PayAccount { get; set; }

        /// <summary>
        /// 申请提现金额
        /// </summary>
        [Required(ErrorMessage = "申请提现金额不能为空")]
        public decimal ApplyAmount { get; set; }
        /// <summary>
        /// 账号信息
        /// </summary>
        [Required(ErrorMessage = "账号信息不能为空")]
        [StringLength(50, ErrorMessage = "账号信息长度不能大于100")]
        public string ApplyRemark { get; set; }
        /// <summary>
        /// 申请提现时间
        /// </summary>
        public DateTime ApplyTime { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        [Required(ErrorMessage = "联系方式不能为空")]
        [StringLength(50, ErrorMessage = "联系方式不能大于20")]
        public string Phone { get; set; }
        /// <summary>
        /// 操作人id
        /// </summary>
        public int OperatorUid { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperatTime { get; set; }
        /// <summary>
        /// 理由
        /// </summary>
        public string Reason { get; set; }
    }

    /// <summary>
    /// 支付积分日志列表模型类
    /// </summary>
    public class UserAmountLogModel
    {
        /// <summary>
        /// 分页对象
        /// </summary>
        public PageModel PageModel { get; set; }
        /// <summary>
        /// 支付积分日志列表
        /// </summary>
        public List<CreditLogInfo> PayCreditLogList { get; set; }
    }


}