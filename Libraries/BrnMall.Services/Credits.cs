using System;
using System.Data;
using System.Collections.Generic;

using BrnMall.Core;

namespace BrnMall.Services
{
    /// <summary>
    /// 积分操作管理类
    /// </summary>
    public partial class Credits
    {
        private static object _locker = new object();//锁对象
        private static CreditConfigInfo _creditconfiginfo = null;//积分配置信息

        static Credits()
        {
            _creditconfiginfo = BMAConfig.CreditConfig;
        }

        /// <summary>
        /// 重置积分配置信息
        /// </summary>
        public static void ResetCreditConfig()
        {
            lock (_locker)
            {
                _creditconfiginfo = BMAConfig.CreditConfig;
            }
        }

        /// <summary>
        /// 等级积分名称
        /// </summary>
        public static string RankCreditName
        {
            get { return _creditconfiginfo.RankCreditName; }
        }

        /// <summary>
        /// 发放登录积分
        /// </summary>
        /// <param name="partUserInfo">用户信息</param>
        /// <param name="loginTime">登录时间</param>
        public static void SendLoginCredits(ref PartUserInfo partUserInfo, DateTime loginTime)
        {
            if (_creditconfiginfo.LoginRankCredits > 0)
            {
                DateTime slcTime = TypeHelper.StringToDateTime(WebHelper.UrlDecode(MallUtils.GetBMACookie("slctime")), loginTime.Date.AddDays(-2));
                if (loginTime.Date <= slcTime.Date)
                    return;

                if (!IsSendTodayLoginCredit(partUserInfo.Uid, DateTime.Now))
                {
                    MallUtils.SetBMACookie("slctime", WebHelper.UrlEncode(loginTime.ToString()));

                    int surplusRankCredits = GetDaySurplusRankCredits(partUserInfo.Uid, loginTime.Date);
                    if (surplusRankCredits == 0)
                        return;

                
                    int rankCredits = 0;
                  
                    if (surplusRankCredits > 0)
                        rankCredits = surplusRankCredits < _creditconfiginfo.LoginRankCredits ? surplusRankCredits : _creditconfiginfo.LoginRankCredits;
                    else if (surplusRankCredits == -1)
                        rankCredits = _creditconfiginfo.LoginRankCredits;

                    partUserInfo.RankCredits += rankCredits;

                    int userRid = UserRanks.GetUserRankByCredits(partUserInfo.RankCredits).UserRid;
                    if (userRid != partUserInfo.UserRid)
                        partUserInfo.UserRid = userRid;
                    else
                        userRid = 0;

                    CreditLogInfo creditLogInfo = new CreditLogInfo();
                    creditLogInfo.Uid = partUserInfo.Uid;
                    creditLogInfo.RankCredits = rankCredits;
                    creditLogInfo.Action = (int)CreditAction.Login;
                    creditLogInfo.ActionCode = 0;
                    creditLogInfo.ActionTime = loginTime;
                    creditLogInfo.ActionDes = "登录赠送积分";
                    creditLogInfo.Operator = 0;

                    SendCredits(userRid, creditLogInfo);
                }
            }
        }

        /// <summary>
        /// 发放注册积分
        /// </summary>
        /// <param name="userInfo">用户信息</param>
        /// <param name="verifyTime">注册时间</param>
        public static void SendRegisterCredits(ref UserInfo userInfo, DateTime registerTime)
        {
            if ( _creditconfiginfo.RegisterRankCredits > 0)
            {
                int surplusRankCredits = GetDaySurplusRankCredits(userInfo.Uid, registerTime.Date);
                if (surplusRankCredits == 0)
                    return;

                int rankCredits = 0;
                if (surplusRankCredits > 0)
                    rankCredits = surplusRankCredits < _creditconfiginfo.RegisterRankCredits ? surplusRankCredits : _creditconfiginfo.RegisterRankCredits;
                else if (surplusRankCredits == -1)
                    rankCredits = _creditconfiginfo.RegisterRankCredits;

                userInfo.RankCredits += rankCredits;

                int userRid = UserRanks.GetUserRankByCredits(userInfo.RankCredits).UserRid;
                if (userRid != userInfo.UserRid)
                    userInfo.UserRid = userRid;
                else
                    userRid = 0;

                CreditLogInfo creditLogInfo = new CreditLogInfo();
                creditLogInfo.Uid = userInfo.Uid;
                creditLogInfo.RankCredits = rankCredits;
                creditLogInfo.Action = (int)CreditAction.Register;
                creditLogInfo.ActionCode = 0;
                creditLogInfo.ActionTime = registerTime;
                creditLogInfo.ActionDes = "注册赠送积分";
                creditLogInfo.Operator = 0;

                SendCredits(userRid, creditLogInfo);
            }
        }

        /// <summary>
        /// 发放验证邮箱积分
        /// </summary>
        /// <param name="partUserInfo">用户信息</param>
        /// <param name="verifyTime">验证时间</param>
        public static void SendVerifyEmailCredits(ref PartUserInfo partUserInfo, DateTime verifyTime)
        {
            if (partUserInfo.VerifyEmail == 0 &&  _creditconfiginfo.VerifyEmailRankCredits > 0)
            {
                int surplusRankCredits = GetDaySurplusRankCredits(partUserInfo.Uid, verifyTime.Date);
                if (surplusRankCredits == 0)
                    return;

                int rankCredits = 0;
               
                if (surplusRankCredits > 0)
                    rankCredits = surplusRankCredits < _creditconfiginfo.VerifyEmailRankCredits ? surplusRankCredits : _creditconfiginfo.VerifyEmailRankCredits;
                else if (surplusRankCredits == -1)
                    rankCredits = _creditconfiginfo.VerifyEmailRankCredits;

                partUserInfo.RankCredits += rankCredits;

                int userRid = UserRanks.GetUserRankByCredits(partUserInfo.RankCredits).UserRid;
                if (userRid != partUserInfo.UserRid)
                    partUserInfo.UserRid = userRid;
                else
                    userRid = 0;

                CreditLogInfo creditLogInfo = new CreditLogInfo();
                creditLogInfo.Uid = partUserInfo.Uid;
                creditLogInfo.RankCredits = rankCredits;
                creditLogInfo.Action = (int)CreditAction.VerifyEmail;
                creditLogInfo.ActionCode = 0;
                creditLogInfo.ActionTime = verifyTime;
                creditLogInfo.ActionDes = "验证用户邮箱";
                creditLogInfo.Operator = 0;

                SendCredits(userRid, creditLogInfo);
            }
        }

        /// <summary>
        /// 发放验证手机积分
        /// </summary>
        /// <param name="partUserInfo">用户信息</param>
        /// <param name="verifyTime">验证时间</param>
        public static void SendVerifyMobileCredits(ref PartUserInfo partUserInfo, DateTime verifyTime)
        {
            if (partUserInfo.VerifyMobile == 0 &&  _creditconfiginfo.VerifyMobileRankCredits > 0)
            {
                int surplusRankCredits = GetDaySurplusRankCredits(partUserInfo.Uid, verifyTime.Date);
                if (surplusRankCredits == 0)
                {
                    return;
                }

                int rankCredits = 0;
                if (surplusRankCredits > 0)
                {
                    rankCredits = surplusRankCredits < _creditconfiginfo.VerifyMobileRankCredits ? surplusRankCredits : _creditconfiginfo.VerifyMobileRankCredits;
                }
                else if (surplusRankCredits == -1)
                {
                    rankCredits = _creditconfiginfo.VerifyMobileRankCredits;
                }

                partUserInfo.RankCredits += rankCredits;

                int userRid = UserRanks.GetUserRankByCredits(partUserInfo.RankCredits).UserRid;
                if (userRid != partUserInfo.UserRid)
                    partUserInfo.UserRid = userRid;
                else
                    userRid = 0;

                CreditLogInfo creditLogInfo = new CreditLogInfo();
                creditLogInfo.Uid = partUserInfo.Uid;
                creditLogInfo.RankCredits = rankCredits;
                creditLogInfo.Action = (int)CreditAction.VerifyMobile;
                creditLogInfo.ActionCode = 0;
                creditLogInfo.ActionTime = verifyTime;
                creditLogInfo.ActionDes = "验证用户手机";
                creditLogInfo.Operator = 0;

                SendCredits(userRid, creditLogInfo);
            }
        }

        /// <summary>
        /// 发放完善用户信息积分
        /// </summary>
        /// <param name="partUserInfo">用户信息</param>
        /// <param name="completeTime">完成时间</param>
        public static void SendCompleteUserInfoCredits(ref PartUserInfo partUserInfo, DateTime completeTime)
        {
            if ( _creditconfiginfo.CompleteUserInfoRankCredits > 0 && !IsSendCompleteUserInfoCredit(partUserInfo.Uid))
            {
                int surplusRankCredits = GetDaySurplusRankCredits(partUserInfo.Uid, completeTime.Date);
                if (surplusRankCredits == 0)
                    return;

                int rankCredits = 0;
                if (surplusRankCredits > 0)
                    rankCredits = surplusRankCredits < _creditconfiginfo.CompleteUserInfoRankCredits ? surplusRankCredits : _creditconfiginfo.CompleteUserInfoRankCredits;
                else if (surplusRankCredits == -1)
                    rankCredits = _creditconfiginfo.CompleteUserInfoRankCredits;

                partUserInfo.RankCredits += rankCredits;

                int userRid = UserRanks.GetUserRankByCredits(partUserInfo.RankCredits).UserRid;
                if (userRid != partUserInfo.UserRid)
                    partUserInfo.UserRid = userRid;
                else
                    userRid = 0;

                CreditLogInfo creditLogInfo = new CreditLogInfo();
                creditLogInfo.Uid = partUserInfo.Uid;
                creditLogInfo.RankCredits = rankCredits;
                creditLogInfo.Action = (int)CreditAction.CompleteUserInfo;
                creditLogInfo.ActionCode = 0;
                creditLogInfo.ActionTime = completeTime;
                creditLogInfo.ActionDes = "完善用户信息";
                creditLogInfo.Operator = 0;

                SendCredits(userRid, creditLogInfo);
            }
        }

        /// <summary>
        /// 发放完成订单积分
        /// </summary>
        /// <param name="partUserInfo">用户信息</param>
        /// <param name="orderInfo">订单信息</param>
        /// <param name="orderProductList">订单商品列表</param>
        /// <param name="completeTime">完成时间</param>
        public static void SendCompleteOrderCredits(ref PartUserInfo partUserInfo, OrderInfo orderInfo, List<OrderProductInfo> orderProductList, DateTime completeTime)
        {
            if ( _creditconfiginfo.CompleteOrderRankCredits > 0)
            {
                int surplusRankCredits = GetDaySurplusRankCredits(partUserInfo.Uid, completeTime.Date);

                int rankCredits = 0;
                int tempRankCredits = (int)Math.Floor(orderInfo.OrderAmount * _creditconfiginfo.CompleteOrderRankCredits / 100);
              
                if (surplusRankCredits > 0)
                    rankCredits = surplusRankCredits < tempRankCredits ? surplusRankCredits : tempRankCredits;
                else if (surplusRankCredits == -1)
                    rankCredits = tempRankCredits;

                partUserInfo.RankCredits += rankCredits;

                int userRid = UserRanks.GetUserRankByCredits(partUserInfo.RankCredits).UserRid;
                if (userRid != partUserInfo.UserRid)
                    partUserInfo.UserRid = userRid;
                else
                    userRid = 0;

                CreditLogInfo creditLogInfo = new CreditLogInfo();
                creditLogInfo.Uid = partUserInfo.Uid;
                creditLogInfo.RankCredits = rankCredits;
                creditLogInfo.Action = (int)CreditAction.CompleteOrder;
                creditLogInfo.ActionCode = orderInfo.Oid;
                creditLogInfo.ActionTime = completeTime;
                creditLogInfo.ActionDes = "完成订单:" + orderInfo.OSN;
                creditLogInfo.Operator = 0;

                SendCredits(userRid, creditLogInfo);
            }
        }

        /// <summary>
        /// 发放评价商品积分
        /// </summary>
        /// <param name="partUserInfo">用户信息</param>
        /// <param name="orderProductInfo">订单商品</param>
        /// <param name="reviewTime">评价时间</param>
        public static int SendReviewProductCredits(ref PartUserInfo partUserInfo, OrderProductInfo orderProductInfo, DateTime reviewTime)
        {
            if ( _creditconfiginfo.ReviewProductRankCredits > 0)
            {
                int surplusRankCredits = GetDaySurplusRankCredits(partUserInfo.Uid, reviewTime.Date);
                if (surplusRankCredits == 0)
                    return 0;

               
                int rankCredits = 0;
              
                if (surplusRankCredits > 0)
                    rankCredits = surplusRankCredits < _creditconfiginfo.ReviewProductRankCredits ? surplusRankCredits : _creditconfiginfo.ReviewProductRankCredits;
                else if (surplusRankCredits == -1)
                    rankCredits = _creditconfiginfo.ReviewProductRankCredits;

                partUserInfo.RankCredits += rankCredits;

                int userRid = UserRanks.GetUserRankByCredits(partUserInfo.RankCredits).UserRid;
                if (userRid != partUserInfo.UserRid)
                    partUserInfo.UserRid = userRid;
                else
                    userRid = 0;

                CreditLogInfo creditLogInfo = new CreditLogInfo();
                creditLogInfo.Uid = partUserInfo.Uid;
                creditLogInfo.RankCredits = rankCredits;
                creditLogInfo.Action = (int)CreditAction.ReviewProduct;
                creditLogInfo.ActionCode = orderProductInfo.Oid;
                creditLogInfo.ActionTime = reviewTime;
                creditLogInfo.ActionDes = "评价商品:" + orderProductInfo.Name;
                creditLogInfo.Operator = 0;

                SendCredits(userRid, creditLogInfo);

                return rankCredits;
            }
            return 0;
        }

        ///// <summary>
        ///// 支付订单
        ///// </summary>
        ///// <param name="partUserInfo">用户信息</param>
        ///// <param name="orderInfo">订单信息</param>
        ///// <param name="credits">积分数量</param>
        ///// <param name="payTime">支付时间</param>
        //public static void PayOrder(ref PartUserInfo partUserInfo, OrderInfo orderInfo, int credits, DateTime payTime)
        //{
        //    if (credits > 0)
        //    {
        //        partUserInfo.PayCredits -= credits;

        //        CreditLogInfo creditLogInfo = new CreditLogInfo();
        //        creditLogInfo.Uid = partUserInfo.Uid;
        //        creditLogInfo.PayCredits = -1 * credits;
        //        creditLogInfo.RankCredits = 0;
        //        creditLogInfo.Action = (int)CreditAction.PayOrder;
        //        creditLogInfo.ActionCode = orderInfo.Oid;
        //        creditLogInfo.ActionTime = payTime;
        //        creditLogInfo.ActionDes = "支付订单:" + orderInfo.OSN;
        //        creditLogInfo.Operator = partUserInfo.Uid;

        //        SendCredits(0, creditLogInfo);
        //    }
        //}

        ///// <summary>
        ///// 退回用户订单使用的积分
        ///// </summary>
        ///// <param name="partUserInfo">用户信息</param>
        ///// <param name="orderInfo">订单信息</param>
        ///// <param name="operatorId">操作人</param>
        ///// <param name="returnTime">退回时间</param>
        //public static void ReturnUserOrderUseCredits(ref PartUserInfo partUserInfo, OrderInfo orderInfo, int operatorId, DateTime returnTime)
        //{
        //    partUserInfo.PayCredits += orderInfo.PayCreditCount;

        //    CreditLogInfo creditLogInfo = new CreditLogInfo();
        //    creditLogInfo.Uid = orderInfo.Uid;
        //    creditLogInfo.PayCredits = orderInfo.PayCreditCount;
        //    creditLogInfo.RankCredits = 0;
        //    creditLogInfo.Action = (int)CreditAction.ReturnOrderUse;
        //    creditLogInfo.ActionCode = orderInfo.Oid;
        //    creditLogInfo.ActionTime = returnTime;
        //    creditLogInfo.ActionDes = "退回用户订单使用的积分:" + orderInfo.OSN;
        //    creditLogInfo.Operator = operatorId;

        //    SendCredits(0, creditLogInfo);
        //}

        ///// <summary>
        ///// 退回用户订单发放的积分
        ///// </summary>
        ///// <param name="partUserInfo">用户信息</param>
        ///// <param name="orderInfo">订单信息</param>
        ///// <param name="payCredits">支付积分</param>
        ///// <param name="rankCredits">等级积分</param>
        ///// <param name="operatorId">操作人</param>
        ///// <param name="returnTime">退回时间</param>
        //public static void ReturnUserOrderSendCredits(ref PartUserInfo partUserInfo, OrderInfo orderInfo, int payCredits, int rankCredits, int operatorId, DateTime returnTime)
        //{
        //    partUserInfo.PayCredits -= payCredits;
        //    partUserInfo.RankCredits -= rankCredits;

        //    int userRid = UserRanks.GetUserRankByCredits(partUserInfo.RankCredits).UserRid;
        //    if (userRid != partUserInfo.UserRid)
        //        partUserInfo.UserRid = userRid;
        //    else
        //        userRid = 0;

        //    CreditLogInfo creditLogInfo = new CreditLogInfo();
        //    creditLogInfo.Uid = orderInfo.Uid;
        //    creditLogInfo.PayCredits = -1 * payCredits;
        //    creditLogInfo.RankCredits = -1 * rankCredits;
        //    creditLogInfo.Action = (int)CreditAction.ReturnOrderSend;
        //    creditLogInfo.ActionCode = orderInfo.Oid;
        //    creditLogInfo.ActionTime = returnTime;
        //    creditLogInfo.ActionDes = "收回订单发放的积分:" + orderInfo.OSN;
        //    creditLogInfo.Operator = operatorId;

        //    SendCredits(userRid, creditLogInfo);
        //}

        /// <summary>
        /// 发放积分
        /// </summary>
        /// <param name="userRid">用户等级id，不更改则为0</param>
        /// <param name="creditLogInfo">积分日志信息</param>
        public static void SendCredits(int userRid, CreditLogInfo creditLogInfo)
        {
            BrnMall.Data.Credits.SendCredits(userRid, creditLogInfo);
        }

        /// <summary>
        /// 获得支付积分日志列表
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="type">类型(2代表全部类型，0代表收入，1代表支出)</param>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <returns></returns>
        public static List<CreditLogInfo> GetUserAmountLogList(int uid,int pageSize, int pageNumber)
        {
            return BrnMall.Data.Credits.GetUserAmountLogList(uid, pageSize, pageNumber);
        }

        /// <summary>
        /// 获得用户资金日志数量
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <returns></returns>
        public static int GetUserAmountLogCount(int uid)
        {
            return BrnMall.Data.Credits.GetUserAmountLogCount(uid);
        }

        /// <summary>
        /// 根据id获取提现记录
        /// </summary>
        /// <param name="id">提现记录id</param>
        /// <returns></returns>
        public static WithdrawalLogInfo GetWithdrawalLogById(int id)
        {
            return BrnMall.Data.Credits.GetWithdrawalLogById(id);
        }

        /// <summary>
        /// 获得用户提现日志数量
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="state">提现记录状态</param>
        /// <param name="state">提现类型</param>
        /// <returns></returns>
        public static int GetWithdrawalLogCount(int uid, int state,int paytype)
        {
            return BrnMall.Data.Credits.GetWithdrawalLogCount(uid, state, paytype);
        }

                /// <summary>
        /// 获得用户提现日志列表
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="state">提现记录状态</param>
        /// <param name="state">提现类型</param>
        /// <param name="state">第几页</param>
        /// <param name="state">每页数码</param>
        /// <returns></returns>
        public static List<WithdrawalLogInfo> GetWithdrawalLogList(int uid, int state, int paytype, int pagenumber, int pagesize)
        {

            return BrnMall.Data.Credits.GetWithdrawalLogList(uid, state, paytype,pagenumber,pagesize);
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        /// <param name="info"></param>
        public static void CreateWithdrawalLog(WithdrawalLogInfo info)
        {
             BrnMall.Data.Credits.CreateWithdrawalLog(info);
        }

        /// <summary>
        /// 修改提现申请
        /// </summary>
        /// <param name="info"></param>
        public static void UpdateWithdrawalLog(WithdrawalLogInfo info)
        {
             BrnMall.Data.Credits.UpdateWithdrawalLog(info);
        }


        /// <summary>
        /// 获得今天发放的等级积分
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="today">今天日期</param>
        /// <returns></returns>
        public static int GetTodaySendRankCredits(int uid, DateTime today)
        {
            return BrnMall.Data.Credits.GetTodaySendRankCredits(uid, today);
        }

        /// <summary>
        /// 获得今天剩余的积分发放数
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="today">今天日期</param>
        /// <returns></returns>
        public static int GetDaySurplusRankCredits(int uid, DateTime today)
        {
            if (_creditconfiginfo.DayMaxSendRankCredits > 0)
            {
                int todaySendCredits = GetTodaySendRankCredits(uid, today);
                if (todaySendCredits < _creditconfiginfo.DayMaxSendRankCredits)
                    return _creditconfiginfo.DayMaxSendRankCredits - todaySendCredits;
                else
                    return 0;
            }
            return -1;
        }

        /// <summary>
        /// 是否发放了今天的登录积分
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="today">今天日期</param>
        /// <returns></returns>
        public static bool IsSendTodayLoginCredit(int uid, DateTime today)
        {
            return BrnMall.Data.Credits.IsSendTodayLoginCredit(uid, today);
        }

        /// <summary>
        /// 是否发放了完成用户信息积分
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <returns></returns>
        public static bool IsSendCompleteUserInfoCredit(int uid)
        {
            return BrnMall.Data.Credits.IsSendCompleteUserInfoCredit(uid);
        }


        /// <summary>
        /// 获得用户订单发放的积分
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <returns></returns>
        public static DataTable GetUserOrderSendCredits(int oid)
        {
            return BrnMall.Data.Credits.GetUserOrderSendCredits(oid);
        }

        /// <summary>
        /// 获得订单支付时的资金记录
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static CreditLogInfo GetOrderAmountLog(int oid, int type)
        {
            return BrnMall.Data.Credits.GetOrderAmountLog(oid, type);
        }
    }
}
