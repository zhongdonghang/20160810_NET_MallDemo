using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrnMall.Core
{
    /// <summary>
    /// 提现方式
    /// </summary>
    public enum WithdrawalType
    {
        /// <summary>
        /// 支付宝
        /// </summary>
        Alipay = 1,
        /// <summary>
        /// 银行卡
        /// </summary>
         BankCard = 2
    }

    /// <summary>
    /// 提现状态
    /// </summary>
    public enum WithdrawalState
    {
        /// <summary>
        /// 申请中
        /// </summary>
        applying = 1,
        /// <summary>
        /// 不通过
        /// </summary>
        nopass = 2,
        /// <summary>
        /// 已通过
        /// </summary>
        pass = 3
    }
}
