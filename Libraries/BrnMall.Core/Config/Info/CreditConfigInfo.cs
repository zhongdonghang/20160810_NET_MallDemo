using System;

namespace BrnMall.Core
{
    /// <summary>
    /// 积分配置信息类
    /// </summary>
    [Serializable]
    public class CreditConfigInfo : IConfigInfo
    {
        private decimal _point;//抽水百分比
        private decimal _minamount;//提现最低条件额度
        private decimal _minwithdrawal;//提现最低值

        private string _rankcreditname;//等级积分名称
        private int _daymaxsendrankcredits;//每天最大发放等级积分
        private int _registerrankcredits;//注册等级积分
        private int _loginrankcredits;//每天登录等级积分
        private int _verifyemailrankcredits;//验证邮箱等级积分
        private int _verifymobilerankcredits;//验证手机等级积分
        private int _completeuserinforankcredits;//完善用户信息等级积分
        private int _completeorderrankcredits;//完成订单等级积分(以订单金额的百分比计算)
        private int _reviewproductrankcredits;//评价商品等级积分


        /// <summary>
        /// 抽水百分比
        /// </summary>
        public decimal Point
        {
            get { return _point; }
            set { _point = value; }
        }
        /// <summary>
        /// 提现最低条件额度
        /// </summary>
        public decimal MinAmount
        {
            get { return _minamount; }
            set { _minamount = value; }
        }
         /// <summary>
        /// 提现最低额度
        /// </summary>
        public decimal MinWithdrawal
        {
            get { return _minwithdrawal; }
            set { _minwithdrawal = value; }
        }
        
       
        /// <summary>
        /// 等级积分名称
        /// </summary>
        public string RankCreditName
        {
            get { return _rankcreditname; }
            set { _rankcreditname = value; }
        }
        /// <summary>
        /// 每天最大发放等级积分
        /// </summary>
        public int DayMaxSendRankCredits
        {
            get { return _daymaxsendrankcredits; }
            set { _daymaxsendrankcredits = value; }
        }
        /// <summary>
        /// 注册等级积分
        /// </summary>
        public int RegisterRankCredits
        {
            get { return _registerrankcredits; }
            set { _registerrankcredits = value; }
        }
        /// <summary>
        /// 每天登录等级积分
        /// </summary>
        public int LoginRankCredits
        {
            get { return _loginrankcredits; }
            set { _loginrankcredits = value; }
        }
        /// <summary>
        /// 验证邮箱等级积分
        /// </summary>
        public int VerifyEmailRankCredits
        {
            get { return _verifyemailrankcredits; }
            set { _verifyemailrankcredits = value; }
        }
        /// <summary>
        /// 验证手机等级积分
        /// </summary>
        public int VerifyMobileRankCredits
        {
            get { return _verifymobilerankcredits; }
            set { _verifymobilerankcredits = value; }
        }
        /// <summary>
        /// 完善用户信息等级积分
        /// </summary>
        public int CompleteUserInfoRankCredits
        {
            get { return _completeuserinforankcredits; }
            set { _completeuserinforankcredits = value; }
        }
        /// <summary>
        /// 完成订单等级积分(以订单金额的百分比计算)
        /// </summary>
        public int CompleteOrderRankCredits
        {
            get { return _completeorderrankcredits; }
            set { _completeorderrankcredits = value; }
        }
        /// <summary>
        /// 评价商品等级积分
        /// </summary>
        public int ReviewProductRankCredits
        {
            get { return _reviewproductrankcredits; }
            set { _reviewproductrankcredits = value; }
        }
    }
}
