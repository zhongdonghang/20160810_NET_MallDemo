using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrnMall.Core
{
    public class SMSLogInfo
    {
        private int _id;//编号
        private string _phone;//发送手机号
        private int _type;//发送类型（0：短信验证，1：找回密码）
        private DateTime _sendTime;//发送时间
        private bool _isSendSuccess;//是否已发送成功
        private bool _codeUsed;//验证码是否已经使用
        private string _smsContent;//发送内容
        private string _ip;//IP地址
        private string _code;//验证码

        /// <summary>
        /// 编号
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        /// <summary>
        /// 发送手机号
        /// </summary>
        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }
        /// <summary>
        /// 发送类型（0：短信验证，1：找回密码）
        /// </summary>
        public int Type
        {
            get { return _type; }
            set { _type = value; }
        }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime
        {
            get { return _sendTime; }
            set { _sendTime = value; }
        }
        /// <summary>
        /// 是否已发送成功
        /// </summary>
        public bool IsSendSuccess
        {
            get { return _isSendSuccess; }
            set { _isSendSuccess = value; }
        }
        /// <summary>
        /// 验证码是否已经使用
        /// </summary>
        public bool CodeUsed
        {
            get { return _codeUsed; }
            set { _codeUsed = value; }
        }
        /// <summary>
        /// 发送内容
        /// </summary>
        public string SMSContent
        {
            get { return _smsContent; }
            set { _smsContent = value; }
        }
        /// <summary>
        /// ip地址
        /// </summary>
        public string IP
        {
            get { return _ip; }
            set { _ip = value; }
        }
        /// <summary>
        /// 验证码
        /// </summary>
        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }
    }
}
