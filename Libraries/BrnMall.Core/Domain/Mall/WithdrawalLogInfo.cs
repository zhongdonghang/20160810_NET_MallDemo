using System;

namespace BrnMall.Core
{
    /// <summary>
    /// 提现申请日志信息类
    /// </summary>
    public class WithdrawalLogInfo
    {
        private int _recordid;//日志id
        private int _uid;//用户id
        private int _state;//提现状态
        private int _paytype;//提现方式
        private string _payaccount;//提现至的账号
        private decimal _applyamount;//申请提现金额
        private string _applyremark;//申请提现备注
        private DateTime _applytime;//申请提现时间
        private string _phone;//联系电话
        private int _operatoruid;//操作人id
        private DateTime _operattime;//操作时间
        private string _reason;//理由

        /// <summary>
        /// 日志id
        /// </summary>
        public int RecordId
        {
            get { return _recordid; }
            set { _recordid = value; }
        }
        /// <summary>
        /// 用户id
        /// </summary>
        public int Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }
        /// <summary>
        /// 提现状态
        /// </summary>
        public int State
        {
            get { return _state; }
            set { _state = value; }
        }
        /// <summary>
        /// 提现方式
        /// </summary>
        public int PayType
        {
            get { return _paytype; }
            set { _paytype = value; }
        }
        /// <summary>
        /// 提现至的账号
        /// </summary>
        public string PayAccount
        {
            get { return _payaccount; }
            set { _payaccount = value; }
        }
        /// <summary>
        /// 申请提现金额
        /// </summary>
        public decimal ApplyAmount
        {
            get { return _applyamount; }
            set { _applyamount = value; }
        }
        /// <summary>
        /// 申请提现备注
        /// </summary>
        public string ApplyRemark
        {
            get { return _applyremark; }
            set { _applyremark = value; }
        }
        /// <summary>
        /// 申请提现时间
        /// </summary>
        public DateTime ApplyTime
        {
            get { return _applytime; }
            set { _applytime = value; }
        }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }
        /// <summary>
        /// 操作人id
        /// </summary>
        public int OperatorUid
        {
            get { return _operatoruid; }
            set { _operatoruid = value; }
        }
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperatTime
        {
            get { return _operattime; }
            set { _operattime = value; }
        }
        /// <summary>
        /// 理由
        /// </summary>
        public string Reason
        {
            get { return _reason; }
            set { _reason = value; }
        }
    }
}
