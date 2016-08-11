using System;
using System.Data;

namespace BrnMall.Core
{
    /// <summary>
    /// BrnMall关系数据库策略之订单分部接口
    /// </summary>
    public partial interface IRDBSStrategy
    {
        #region 订单

        /// <summary>
        /// 获得订单信息
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <returns>订单信息</returns>
        IDataReader GetOrderByOid(int oid);

        /// <summary>
        /// 获得订单信息
        /// </summary>
        /// <param name="osn">订单编号</param>
        /// <returns>订单信息</returns>
        IDataReader GetOrderByOSN(string osn);

        /// <summary>
        /// 获得订单数量
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="orderState">订单状态</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        int GetOrderCountByCondition(int storeId, int orderState, string startTime, string endTime);

        /// <summary>
        /// 获得订单列表
        /// </summary>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <param name="condition">条件</param>
        /// <param name="sort">排序</param>
        /// <returns></returns>
        DataTable GetOrderList(int pageSize, int pageNumber, string condition, string sort);

        /// <summary>
        /// 获得列表搜索条件
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="osn">订单编号</param>
        /// <param name="uid">用户id</param>
        /// <param name="consignee">收货人</param>
        /// <param name="orderState">订单状态</param>
        /// <returns></returns>
        string GetOrderListCondition(int storeId, string osn, int uid, string consignee, int orderState);

        /// <summary>
        /// 获得列表排序
        /// </summary>
        /// <param name="sortColumn">排序列</param>
        /// <param name="sortDirection">排序方向</param>
        /// <returns></returns>
        string GetOrderListSort(string sortColumn, string sortDirection);

        /// <summary>
        /// 获得订单数量
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        int GetOrderCount(string condition);

        /// <summary>
        /// 获得订单商品列表
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <returns></returns>
        IDataReader GetOrderProductList(int oid);

        /// <summary>
        /// 获得订单商品列表
        /// </summary>
        /// <param name="oidList">订单id列表</param>
        /// <returns></returns>
        IDataReader GetOrderProductList(string oidList);

        /// <summary>
        /// 获取逾期未收货列表
        /// </summary>
        /// <param name="days">逾期配置天数</param>
        DataTable GetExpireCompleteOrderList();

        /// <summary>
        /// 获取逾期未付款列表
        /// </summary>
        DataTable GetExpireCancelOrderList();

        /// <summary>
        /// 更新订单折扣
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="discount">折扣</param>
        /// <param name="surplusMoney">剩余金额</param>
        void UpdateOrderDiscount(int oid, decimal discount, decimal surplusMoney);

        /// <summary>
        /// 更新订单配送费用
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="shipFee">配送费用</param>
        /// <param name="orderAmount">订单合计</param>
        /// <param name="surplusMoney">剩余金额</param>
        void UpdateOrderShipFee(int oid, decimal shipFee, decimal orderAmount, decimal surplusMoney);

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="orderState">订单状态</param>
        void UpdateOrderState(int oid, OrderState orderState);

        /// <summary>
        /// 更新订单支付方式
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="paymode">付款模式（0：货到付款，1：线上付款）</param>
        /// <param name="paysystemname">插件code</param>
        /// <param name="payfriendname">插件名称</param>
        void UpdateOrderPayPlugin(int oid, int paymode, string paysystemname, string payfriendname);

        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="orderState">订单状态</param>
        /// <param name="shipSN">配送单号</param>
        /// <param name="shipCoId">配送公司id</param>
        /// <param name="shipCoName">配送公司名称</param>
        /// <param name="shipTime">配送时间</param>
        void SendOrderProduct(int oid, OrderState orderState, string shipSN, int shipCoId, string shipCoName, DateTime shipTime);

        /// <summary>
        /// 付款
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="orderState">订单状态</param>
        /// <param name="paySN">支付单号</param>
        /// <param name="payTime">支付时间</param>
        /// <param name="payModel">付款模式（0：货到付款，1：线上付款）</param>
        /// <param name="systemName">支付code</param>
        /// <param name="friendlyName">支付方式</param>
        void PayOrder(int oid, OrderState orderState, string paySN, DateTime payTime, int payModel, string systemName, string friendlyName);

        /// <summary>
        /// 更新订单的评价
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="isReview">是否评价</param>
        void UpdateOrderIsReview(int oid, int isReview);

        /// <summary>
        /// 获得用户订单列表
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <param name="startAddTime">添加开始时间</param>
        /// <param name="endAddTime">添加结束时间</param>
        /// <param name="orderState">订单状态(0代表全部状态,1代表待收货状态（30<state<140）)</param>
        /// <param name="keyword">搜索词</param>
        /// <returns></returns>
        DataTable GetUserOrderList(int uid, int pageSize, int pageNumber, string startAddTime, string endAddTime, int orderState,string keyword);

        /// <summary>
        /// 获得用户订单列表
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="startAddTime">添加开始时间</param>
        /// <param name="endAddTime">添加结束时间</param>
        /// <param name="orderState">订单状态(0代表全部状态,1代表待收货状态（30<state<140）)</param>
        /// <param name="keyword">搜索词</param>
        /// <returns></returns>
        int GetUserOrderCount(int uid, string startAddTime, string endAddTime, int orderState, string keyword);

        /// <summary>
        /// 获得销售商品列表
        /// </summary>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        DataTable GetSaleProductList(int pageSize, int pageNumber, string startTime, string endTime);

        /// <summary>
        /// 获得销售商品数量
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        int GetSaleProductCount(string startTime, string endTime);

        /// <summary>
        /// 获得销售趋势
        /// </summary>
        /// <param name="trendType">趋势类型(0代表订单数，1代表订单合计)</param>
        /// <param name="timeType">时间类型(0代表小时，1代表天，2代表月，3代表年)</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        DataTable GetSaleTrend(int trendType, int timeType, string startTime, string endTime);

        /// <summary>
        /// 获得销售趋势y轴最大值
        /// </summary>
        /// <param name="trendType">趋势类型(0代表订单数，1代表订单合计)</param>
        /// <param name="timeType">时间类型(0代表小时，1代表天，2代表月，3代表年)</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        int GetMaxY(int trendType, int timeType, string startTime, string endTime);

        #endregion

        #region 订单处理

        /// <summary>
        /// 创建订单处理
        /// </summary>
        /// <param name="orderActionInfo">订单处理信息</param>
        void CreateOrderAction(OrderActionInfo orderActionInfo);

        /// <summary>
        /// 获得订单处理列表
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <returns></returns>
        IDataReader GetOrderActionList(int oid);

        /// <summary>
        /// 获得订单id列表
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="orderActionType">订单操作类型</param>
        /// <returns></returns>
        DataTable GetOrderIdList(DateTime startTime, DateTime endTime, int orderActionType);

        #endregion

        #region 订单退款

        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="orderRefundInfo">订单退款信息</param>
        void ApplyRefund(OrderRefundInfo orderRefundInfo);

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="refundId">退款id</param>
        /// <param name="refundSN">退款单号</param>
        /// <param name="refundSystemName">退款方式系统名</param>
        /// <param name="refundFriendName">退款方式昵称</param>
        /// <param name="refundTime">退款时间</param>
        void RefundOrder(int refundId, string refundSN, string refundSystemName, string refundFriendName, DateTime refundTime);

        /// <summary>
        /// 获得订单退款列表
        /// </summary>
        /// <param name="pageSize">每页数</param>
        /// <param name="pageNumber">当前页数</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        IDataReader GetOrderRefundList(int pageSize, int pageNumber, string condition);

        /// <summary>
        /// 获得订单退款列表条件
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="osn">订单编号</param>
        /// <returns></returns>
        string GetOrderRefundListCondition(int storeId, string osn);

        /// <summary>
        /// 获得订单退款数量
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        int GetOrderRefundCount(string condition);

        #endregion
    }
}
