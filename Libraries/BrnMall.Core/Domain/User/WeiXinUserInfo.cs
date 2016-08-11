using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrnMall.Core
{
    public class WeiXinUserInfo
    {
        public string OpenId { get; set; }
        public string Unionid { get; set; }
        public string NickName { get; set; }
        public int Sex { get; set; }
        public string HeadImgUrl { get; set; }
    }
}
