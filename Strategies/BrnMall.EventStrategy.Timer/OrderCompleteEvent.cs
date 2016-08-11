using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.Core;
using BrnMall.Services;
using System.Data;

namespace BrnMall.EventStrategy.Timer
{
    public class OrderCompleteEvent : IEvent
    {
        /// <summary>
        /// 执行事件
        /// </summary>
        /// <param name="eventInfo">事件信息</param>
        public void Execute(object eventInfo)
        {
            EventInfo e = (EventInfo)eventInfo;

            //发货后15天自动收货
            DataTable orderlist = AdminOrders.GetExpireCompleteOrderList();
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
                Orders.ReceiveOrder(oid);
                //创建订单处理
                OrderActions.CreateOrderAction(new OrderActionInfo()
                {
                    Oid = oid,
                    Uid = 0,
                    RealName = "系统",
                    ActionType = (int)OrderActionType.Receive,
                    ActionTime = DateTime.Now,
                    ActionDes = "发货后未在规定时间内（15天）确认收货，系统自动收货"
                });
            }

            EventLogs.CreateEventLog(e.Key, e.Title, Environment.MachineName, DateTime.Now);
        }
    }
}
