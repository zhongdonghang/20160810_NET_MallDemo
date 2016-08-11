//取消订单
function cancelOrder(oid, cancelReason) {
    Showbo.Msg.confirm("确认要取消该订单？", function () {
        Ajax.post("/ucenter/cancelorder", { 'oid': oid, 'cancelReason': cancelReason }, false, cancelOrderResponse);
    });
}

//处理取消订单的反馈信息
function cancelOrderResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        document.getElementById("orderState" + result.content).innerHTML = "取消";
        removeNode(document.getElementById("payOrderBut" + result.content));
        removeNode(document.getElementById("cancelOrderBut" + result.content));
        Showbo.Msg.alert('取消成功');
    }
    else {
        Showbo.Msg.alert(result.content);
    }
}

//确认收货
function receiveProduct(oid) {
    Showbo.Msg.confirm("确认已经收货？", function () {
        Ajax.post("/ucenter/ReceiveProduct", { 'oid': oid }, false, receiveProductResponse);
    });
}

//确认收货的反馈信息
function receiveProductResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        document.getElementById("orderState" + result.content).innerHTML = "已收货";
        removeNode(document.getElementById("receiveProductBut" + result.content));
        Showbo.Msg.alert('您已经确认了收货');
    }
    else {
        Showbo.Msg.alert(result.content);
    }
}

//评价订单
function reviewOrder() {
    var reviewOrderFrom = document.forms["reviewOrderFrom"];

    var oid = reviewOrderFrom.elements["oid"].value;
    var stars = "";
    var messages = "";
    var opids = "";
    //商品评价检查
    var pids = document.getElementsByName("proid");
    for (var i = 0; i < pids.length; i++) {
        var pid = pids[i].value;
        var star = getSelectedRadio(reviewOrderFrom.elements["star_" + pid]).value;
        if (star < 1 || star > 5) {
            Showbo.Msg.alert('请选择正确的星星');
            return false;
        }
        var message = reviewOrderFrom.elements["message_" + pid].value;
        if (message.length == 0) {
            Showbo.Msg.alert('请输入评价内容');
            return false;
        }
        if (message.length > 100) {
            Showbo.Msg.alert('评价内容最多输入100个字');
            return false;
        }
        if (message.indexOf("#") > -1) {
            Showbo.Msg.alert('订单备注不能包含特殊字符');
            return false;
        }
        stars += star + "#";
        messages += message + "#";
        opids += pid + "#";
    }
    stars = stars.substring(0, stars.length - 1);
    messages = messages.substring(0, messages.length - 1);
    opids = opids.substring(0, opids.length - 1);

    //商家评价检查
    var descriptionStar = getSelectedRadio(reviewOrderFrom.elements["descriptionStar"]).value;
    var serviceStar = getSelectedRadio(reviewOrderFrom.elements["serviceStar"]).value;
    var shipStar = getSelectedRadio(reviewOrderFrom.elements["shipStar"]).value;
    if (descriptionStar < 1 || descriptionStar > 5) {
        Showbo.Msg.alert('请选择正确的商品描述评分');
        return false;
    }
    if (serviceStar < 1 || serviceStar > 5) {
        Showbo.Msg.alert('请选择正确的商家服务评分');
        return false;
    }
    if (shipStar < 1 || shipStar > 5) {
        Showbo.Msg.alert('请选择正确的商家配送评分');
        return false;
    }

    document.getElementById("reviewOrderBut").disabled = true;
    Ajax.post("/ucenter/revieworderpost?oid=" + oid, {'stars':stars,'messages':messages, 'opids':opids, 'descriptionStar': descriptionStar, 'serviceStar': serviceStar, 'shipStar': shipStar}, false, reviewOrderResponse);
}

//处理评价订单的反馈信息
function reviewOrderResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        window.location.href = "/ucenter/productreviewlist";
    }
    else {
        Showbo.Msg.alert(result.content);
    }
    document.getElementById("reviewOrderBut").disabled = false;
}

//订单搜索
function orderSearch() {
    var keyword = document.getElementById("orderkeyword").value;
    if (keyword == undefined || keyword == null || keyword.length < 1) {
        keyword = "";
    }
    var orderListForm = document.forms["orderListForm"];
    var orderState = getSelectedOption(orderListForm.elements["OrderState"]).value;
    window.location.href = "/ucenter/orderlist?orderState=" + orderState+"&keyword=" + encodeURIComponent(keyword);
};