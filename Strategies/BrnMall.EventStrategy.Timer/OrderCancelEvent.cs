using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.Core;
using BrnMall.Services;
using System.Data;

namespace BrnMall.EventStrategy.Timer
{
    public class OrderCancelEvent : IEvent
    {
        /// <summary>
        /// 执行事件
        /// </summary>
        /// <param name="eventInfo">事件信息</param>
        public void Execute(object eventInfo)
        {
            EventInfo e = (EventInfo)eventInfo;
            //线上订单后多久未支付自动取消
            DataTable orderlist = AdminOrders.GetExpireCancelOrderList();
            foreach (DataRow row in orderlist.Rows)
            {
                int oid = TypeHelper.ObjectToInt(row["oid"]);
                int uid = TypeHelper.ObjectToInt(row["uid"]);
                OrderInfo order = Orders.GetOrderByOid(oid);
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null || order == null)
                {
                    continue;
                }
                //系统自动确认收货
                Orders.CancelOrder(ref user, order, 0, DateTime.Now);
                //创建订单处理
                OrderActions.CreateOrderAction(new OrderActionInfo()
                {
                    Oid = oid,
                    Uid = 0,
                    RealName = "系统",
                    ActionType = (int)OrderActionType.Cancel,
                    ActionTime = DateTime.Now,
                    ActionDes = "下单后未在规定时间内（" + BMAConfig.MallConfig.OrderAutoCancel.ToString() + "分钟）支付订单，系统自动取消订单"
                });
            }

            EventLogs.CreateEventLog(e.Key, e.Title, Environment.MachineName, DateTime.Now);
        }
    }
}
