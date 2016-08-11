using System;
using System.Data;
using System.Collections.Generic;

using BrnMall.Core;
using System.Net;
using System.IO;
using System.Text;

namespace BrnMall.Services
{
    /// <summary>
    /// 订单处理操作管理类
    /// </summary>
    public partial class OrderActions
    {
        /// <summary>
        /// 创建订单处理
        /// </summary>
        /// <param name="orderActionInfo">订单处理信息</param>
        public static void CreateOrderAction(OrderActionInfo orderActionInfo)
        {
            BrnMall.Data.OrderActions.CreateOrderAction(orderActionInfo);
        }

        /// <summary>
        /// 获得订单处理列表
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <returns></returns>
        public static List<OrderActionInfo> GetOrderActionList(int oid)
        {
            OrderInfo order = Orders.GetOrderByOid(oid);
            if (order == null)
            {
                return null;
            }
            var list = BrnMall.Data.OrderActions.GetOrderActionList(oid);

            //如果物流订单（已发货、已收货、已完成）且发货1个月内，可查询物流信息
            if ((order.OrderState == (int)OrderState.Sended ||  order.OrderState == (int)OrderState.Received || order.OrderState == (int)OrderState.Complete) && order.ShipTime.AddMonths(1) > DateTime.Now && order.ShipMode==(int)ShipMode.Express)
            {
                var shipcmp = ShipCompanies.GetShipCompanyById(order.ShipCoId);
                if (shipcmp != null)
                {
                    List<OrderActionInfo> logilist = GetOrderLogistics(shipcmp.Code, order.ShipSN);
                    list.AddRange(logilist);
                }
            }
            list.Sort();

            return list;
        }

        /// <summary>
        /// 获得订单id列表
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="orderActionType">订单操作类型</param>
        /// <returns></returns>
        public static DataTable GetOrderIdList(DateTime startTime, DateTime endTime, int orderActionType)
        {
            return BrnMall.Data.OrderActions.GetOrderIdList(startTime, endTime, orderActionType);
        }

        /// <summary>
        /// 获取物流信息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="shipsn"></param>
        /// <returns></returns>
        public static List<OrderActionInfo> GetOrderLogistics(string code, string shipsn)
        {
            List<OrderActionInfo> list = new List<OrderActionInfo>();
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    code = "auto";
                }
                string ApiKey = "7064c5b6c91feb09";
                string apiurl = "http://api.jisuapi.com/express/query?appkey=" + ApiKey + "&type=" + code + "&number=" + shipsn;
                WebRequest request = WebRequest.Create(@apiurl);
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encode = Encoding.UTF8;
                StreamReader reader = new StreamReader(stream, encode);
                string detail = reader.ReadToEnd();
                LogisticsModelList model = LitJson.JsonMapper.ToObject<LogisticsModelList>(detail);
                if (model.result != null && model.result.list != null && model.result.list.Count > 0)
                {
                    var rl = model.result.list;
                    for (int i = rl.Count - 1; i < rl.Count; i--)
                    {
                        OrderActionInfo ac = new OrderActionInfo()
                        {
                            ActionTime = TypeHelper.StringToDateTime(rl[i].time),
                            ActionDes = rl[i].status
                        };
                        list.Add(ac);
                    }
                }
                return list;
            }
            catch (Exception)
            {
                return list;
            }
        }
        public class LogisticsModelList
        {
            public string status { get; set; }
            public string msg { get; set; }
            public ResultModel result { get; set; }
        }
        public class ResultModel
        {
            public string number { get; set; }
            public string type { get; set; }
            public List<LogisticsModel> list { get; set; }
        }
        public class LogisticsModel
        {
            public string time { get; set; }
            public string status { get; set; }
        }
    }
}
