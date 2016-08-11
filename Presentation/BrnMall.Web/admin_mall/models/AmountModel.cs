using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.Web.Framework;
using BrnMall.Core;

namespace BrnMall.Web.MallAdmin.Models
{
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
        /// <summary>
        /// 申请人
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 申请提现类型
        /// </summary>
        public int PayType { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }
             
    }
}
