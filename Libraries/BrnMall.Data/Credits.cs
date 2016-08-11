using System;
using System.Data;
using System.Collections.Generic;

using BrnMall.Core;

namespace BrnMall.Data
{
    /// <summary>
    /// 积分数据访问类
    /// </summary>
    public partial class Credits
    {
        #region 辅助方法

        /// <summary>
        /// 通过IDataReader创建CreditLogInfo信息
        /// </summary>
        public static CreditLogInfo BuildCreditLogFromReader(IDataReader reader)
        {
            CreditLogInfo creditLogInfo = new CreditLogInfo();

            creditLogInfo.LogId = TypeHelper.ObjectToInt(reader["logid"]);
            creditLogInfo.Uid = TypeHelper.ObjectToInt(reader["uid"]);
            creditLogInfo.UserAmount = TypeHelper.ObjectToDecimal(reader["useramount"]);
            creditLogInfo.FrozenAmount = TypeHelper.ObjectToDecimal(reader["frozenamount"]);
            creditLogInfo.RankCredits = TypeHelper.ObjectToInt(reader["rankcredits"]);
            creditLogInfo.Action = TypeHelper.ObjectToInt(reader["action"]);
            creditLogInfo.ActionCode = TypeHelper.ObjectToInt(reader["actioncode"]);
            creditLogInfo.ActionTime = TypeHelper.ObjectToDateTime(reader["actiontime"]);
            creditLogInfo.ActionDes = reader["actiondes"].ToString();
            creditLogInfo.Operator = TypeHelper.ObjectToInt(reader["operator"]);

            return creditLogInfo;
        }

        /// <summary>
        /// 通过IDataReader创建WithdrawalLogInfo信息
        /// </summary>
        public static WithdrawalLogInfo BuildWithdrawalLogFromReader(IDataReader reader)
        {
            WithdrawalLogInfo logInfo = new WithdrawalLogInfo();
            logInfo.RecordId = TypeHelper.ObjectToInt(reader["recordid"]);
            logInfo.Uid = TypeHelper.ObjectToInt(reader["uid"]);
            logInfo.State = TypeHelper.ObjectToInt(reader["state"]);
            logInfo.PayType = TypeHelper.ObjectToInt(reader["paytype"]);
            logInfo.PayAccount = reader["payaccount"].ToString();
            logInfo.ApplyAmount = TypeHelper.ObjectToDecimal(reader["applyamount"]);
            logInfo.ApplyRemark = reader["applyremark"].ToString();
            logInfo.Phone = reader["phone"].ToString();
            logInfo.ApplyTime = TypeHelper.ObjectToDateTime(reader["applytime"]);
            logInfo.OperatorUid = TypeHelper.ObjectToInt(reader["operatoruid"]);
            logInfo.OperatTime = TypeHelper.ObjectToDateTime(reader["operattime"]);
            logInfo.Reason = reader["reason"].ToString();

            return logInfo;
        }
        #endregion

        /// <summary>
        /// 后台获得积分日志列表
        /// </summary>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        public static DataTable AdminGetCreditLogList(int pageSize, int pageNumber, string condition)
        {
            return BrnMall.Core.BMAData.RDBS.AdminGetCreditLogList(pageSize, pageNumber, condition);
        }

        /// <summary>
        /// 后台获得积分日志列表条件
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static string AdminGetCreditLogListCondition(int uid, string startTime, string endTime)
        {
            return BrnMall.Core.BMAData.RDBS.AdminGetCreditLogListCondition(uid, startTime, endTime);
        }

        /// <summary>
        /// 后台获得积分日志数量
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        public static int AdminGetCreditLogCount(string condition)
        {
            return BrnMall.Core.BMAData.RDBS.AdminGetCreditLogCount(condition);
        }

        /// <summary>
        /// 发放积分
        /// </summary>
        /// <param name="userRid">用户等级id</param>
        /// <param name="creditLogInfo">积分日志信息</param>
        public static void SendCredits(int userRid, CreditLogInfo creditLogInfo)
        {
            BrnMall.Core.BMAData.RDBS.SendCredits(userRid, creditLogInfo);
        }


        /// <summary>
        /// 获得今天发放的等级积分
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="today">今天日期</param>
        /// <returns></returns>
        public static int GetTodaySendRankCredits(int uid, DateTime today)
        {
            return BrnMall.Core.BMAData.RDBS.GetTodaySendRankCredits(uid, today);
        }

        /// <summary>
        /// 是否发放了今天的登录积分
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="today">今天日期</param>
        /// <returns></returns>
        public static bool IsSendTodayLoginCredit(int uid, DateTime today)
        {
            return BrnMall.Core.BMAData.RDBS.IsSendTodayLoginCredit(uid, today);
        }

        /// <summary>
        /// 是否发放了完成用户信息积分
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <returns></returns>
        public static bool IsSendCompleteUserInfoCredit(int uid)
        {
            return BrnMall.Core.BMAData.RDBS.IsSendCompleteUserInfoCredit(uid);
        }

        /// <summary>
        /// 获得支付积分日志列表
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <returns></returns>
        public static List<CreditLogInfo> GetUserAmountLogList(int uid, int pageSize, int pageNumber)
        {
            List<CreditLogInfo> creditLogList = new List<CreditLogInfo>();
            IDataReader reader = BrnMall.Core.BMAData.RDBS.GetUserAmountLogList(uid, pageSize, pageNumber);
            while (reader.Read())
            {
                CreditLogInfo creditLogInfo = BuildCreditLogFromReader(reader);
                creditLogList.Add(creditLogInfo);
            }
            reader.Close();
            return creditLogList;
        }

        /// <summary>
        /// 获得用户资金日志数量
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <returns></returns>
        public static int GetUserAmountLogCount(int uid)
        {
            return BrnMall.Core.BMAData.RDBS.GetUserAmountLogCount(uid);
        }

        /// <summary>
        /// 根据id获取提现记录
        /// </summary>
        /// <param name="id">提现记录id</param>
        /// <returns></returns>
        public static WithdrawalLogInfo GetWithdrawalLogById(int id)
        {
            WithdrawalLogInfo infolog = new WithdrawalLogInfo();
            IDataReader reader = BrnMall.Core.BMAData.RDBS.GetWithdrawalLogById(id);
            while (reader.Read())
            {
                infolog = BuildWithdrawalLogFromReader(reader);
            }
            reader.Close();
            return infolog;
        }

        /// <summary>
        /// 获得用户提现日志数量
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="state">提现记录状态</param>
        /// <param name="state">提现类型</param>
        /// <returns></returns>
        public static int GetWithdrawalLogCount(int uid, int state, int paytype)
        {
            return BrnMall.Core.BMAData.RDBS.GetWithdrawalLogCount(uid, state, paytype);
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
            List<WithdrawalLogInfo> withdrawalLogList = new List<WithdrawalLogInfo>();
            IDataReader reader = BrnMall.Core.BMAData.RDBS.GetWithdrawalLogList(uid, state, paytype, pagenumber,pagesize);
            while (reader.Read())
            {
                WithdrawalLogInfo logInfo = BuildWithdrawalLogFromReader(reader);
                withdrawalLogList.Add(logInfo);
            }
            reader.Close();
            return withdrawalLogList;
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        /// <param name="info"></param>
        public static void CreateWithdrawalLog(WithdrawalLogInfo info)
        {
            BrnMall.Core.BMAData.RDBS.CreateWithdrawalLog(info);
        }

        /// <summary>
        /// 修改提现申请
        /// </summary>
        /// <param name="info"></param>
        public static void UpdateWithdrawalLog(WithdrawalLogInfo info)
        {
            BrnMall.Core.BMAData.RDBS.UpdateWithdrawalLog(info);
        }

        /// <summary>
        /// 获得用户订单发放的积分
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <returns></returns>
        public static DataTable GetUserOrderSendCredits(int oid)
        {
            return BrnMall.Core.BMAData.RDBS.GetUserOrderSendCredits(oid);
        }

        /// <summary>
        /// 获得订单支付时的资金记录
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static CreditLogInfo GetOrderAmountLog(int oid, int type)
        {
            CreditLogInfo creditLogInfo = null;
            IDataReader reader = BrnMall.Core.BMAData.RDBS.GetOrderAmountLog(oid, type);
            while (reader.Read())
            {
                creditLogInfo = BuildCreditLogFromReader(reader);
            }
            reader.Close();
            return creditLogInfo;
        }
    }
}
