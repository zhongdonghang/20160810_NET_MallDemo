using System;
using System.Data;

using BrnMall.Core;

namespace BrnMall.Services
{
    /// <summary>
    /// 后台订单操作管理类
    /// </summary>
    public partial class AdminOrders : Orders
    {
        /// <summary>
        /// 获得订单列表
        /// </summary>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <param name="condition">条件</param>
        /// <param name="sort">排序</param>
        /// <returns></returns>
        public static DataTable GetOrderList(int pageSize, int pageNumber, string condition, string sort)
        {
            return BrnMall.Data.Orders.GetOrderList(pageSize, pageNumber, condition, sort);
        }

        /// <summary>
        /// 获得列表搜索条件
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="osn">订单编号</param>
        /// <param name="uid">用户id</param>
        /// <param name="consignee">收货人</param>
        /// <param name="orderState">订单状态</param>
        /// <returns></returns>
        public static string GetOrderListCondition(int storeId, string osn, int uid, string consignee, int orderState)
        {
            return BrnMall.Data.Orders.GetOrderListCondition(storeId, osn, uid, consignee, orderState);
        }

        /// <summary>
        /// 获得列表排序
        /// </summary>
        /// <param name="sortColumn">排序列</param>
        /// <param name="sortDirection">排序方向</param>
        /// <returns></returns>
        public static string GetOrderListSort(string sortColumn, string sortDirection)
        {
            return BrnMall.Data.Orders.GetOrderListSort(sortColumn, sortDirection);
        }

        /// <summary>
        /// 获得订单数量
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        public static int GetOrderCount(string condition)
        {
            return BrnMall.Data.Orders.GetOrderCount(condition);
        }

        /// <summary>
        /// 获得销售商品列表
        /// </summary>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static DataTable GetSaleProductList(int pageSize, int pageNumber, string startTime, string endTime)
        {
            return BrnMall.Data.Orders.GetSaleProductList(pageSize, pageNumber, startTime, endTime);
        }

          /// <summary>
        /// 获取逾期未收货列表
        /// </summary>
        public static DataTable GetExpireCompleteOrderList()
        {
            return BrnMall.Data.Orders.GetExpireCompleteOrderList();
        }

        /// <summary>
        /// 获取逾期未付款列表
        /// </summary>
        public static DataTable GetExpireCancelOrderList()
        {
            return BrnMall.Data.Orders.GetExpireCancelOrderList();
        }

        /// <summary>
        /// 获得销售商品数量
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static int GetSaleProductCount(string startTime, string endTime)
        {
            return BrnMall.Data.Orders.GetSaleProductCount(startTime, endTime);
        }

        /// <summary>
        /// 获得销售趋势
        /// </summary>
        /// <param name="trendType">趋势类型(0代表订单数，1代表订单合计)</param>
        /// <param name="timeType">时间类型(0代表小时，1代表天，2代表月，3代表年)</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static DataTable GetSaleTrend(int trendType, int timeType, string startTime, string endTime)
        {
            return BrnMall.Data.Orders.GetSaleTrend(trendType, timeType, startTime, endTime);
        }

        /// <summary>
        /// 获得销售趋势y轴最大值
        /// </summary>
        /// <param name="trendType">趋势类型(0代表订单数，1代表订单合计)</param>
        /// <param name="timeType">时间类型(0代表小时，1代表天，2代表月，3代表年)</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static int GetMaxY(int trendType, int timeType, string startTime, string endTime)
        {
            return BrnMall.Data.Orders.GetMaxY(trendType, timeType, startTime, endTime);
        }
    }
}
