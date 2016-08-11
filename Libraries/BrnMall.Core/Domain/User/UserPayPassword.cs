using System;

namespace BrnMall.Core
{
    /// <summary>
    /// 用户支付密码类
    /// </summary>
    public class UserPayPassword
    {
        private int _uid;//用户id
        private string _password;//密码
        private string _salt;//盐值

        ///<summary>
        ///用户等级id
        ///</summary>
        public int Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }
    
        ///<summary>
        ///用户等级标题
        ///</summary>
        public string Password
        {
            get { return _password; }
            set { _password = value.TrimEnd(); }
        }
        /// <summary>
        /// 用户等级头像
        /// </summary>
        public string Salt
        {
            get { return _salt; }
            set { _salt = value.TrimEnd(); }
        }
    }
}
