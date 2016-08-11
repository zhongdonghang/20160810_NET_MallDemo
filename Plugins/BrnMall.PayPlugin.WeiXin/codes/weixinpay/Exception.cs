using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrnMall.PayPlugin.WeiXin.codes.weixinpay
{
    public class WxPayException : Exception
    {
        public WxPayException(string msg): base(msg)
        {

        }
    }
}
