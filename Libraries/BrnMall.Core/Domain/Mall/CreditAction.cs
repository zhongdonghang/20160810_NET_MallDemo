using System;

namespace BrnMall.Core
{
    /// <summary>
    /// 积分动作枚举
    /// </summary>
    public enum CreditAction
    {
        /// <summary>
        /// 管理员发放
        /// </summary>
        AdminSend = 0,
        /// <summary>
        /// 注册
        /// </summary>
        Register = 1,
        /// <summary>
        /// 登录
        /// </summary>
        Login = 2,
        /// <summary>
        /// 验证邮箱
        /// </summary>
        VerifyEmail = 3,
        /// <summary>
        /// 验证手机
        /// </summary>
        VerifyMobile = 4,
        /// <summary>
        /// 完善用户资料
        /// </summary>
        CompleteUserInfo = 5,
        /// <summary>
        /// 支付订单
        /// </summary>
        PayOrder = 6,
        /// <summary>
        /// 完成订单
        /// </summary>
        CompleteOrder = 7,
        /// <summary>
        /// 评价商品
        /// </summary>
        ReviewProduct = 8,
        /// <summary>
        /// 单品促销活动
        /// </summary>
        SinglePromotion = 9,
        /// <summary>
        /// 退回订单使用
        /// </summary>
        ReturnOrderUse = 10,
        /// <summary>
        /// 退回订单发放
        /// </summary>
        ReturnOrderSend = 11,
        /// <summary>
        /// 发送冻结订单金额
        /// </summary>
        SendFrozenAmount=12,
        /// <summary>
        /// 解冻订单金额
        /// </summary>
        ThawAmount=13,
        /// <summary>
        /// 返还订单抽水
        /// </summary>
        RerurnOrderAmount=14,
        /// <summary>
        /// 货到付款完成直接抽水
        /// </summary>
        CodOrderAmount = 15,
        /// <summary>
        /// 提现
        /// </summary>
        WithdrawCash=16,
        /// <summary>
        /// 会员充值
        /// </summary>
        UserLoadCash=17,
        /// <summary>
        /// 余额订单支付
        /// </summary>
        UserOrderCreditPay=18
    }
}
