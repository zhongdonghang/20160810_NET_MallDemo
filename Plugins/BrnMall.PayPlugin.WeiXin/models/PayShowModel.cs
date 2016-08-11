using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.Core;

namespace BrnMall.PayPlugin.WeiXin.models
{
   public  class PayShowModel
    {
        public string OidList { get; set; }
        public List<OrderInfo> OrderList { get; set; }
        public string ShowView { get; set; }
        public int AllSurplusMoney { get; set; }
        public string ImgUrl { get; set; }
        public DateTime AddTime { get; set; }
        public string PaySystemName { get; set; }
    }
}
