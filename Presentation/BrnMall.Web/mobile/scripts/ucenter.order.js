//待付款
var orderstate_waitpaying = 30;
//待确认
var orderstate_confirming = 50;
// 已确认
var orderstate_confirmed = 70;
// 已备货
var orderstate_preproducting = 90;
// 已发货
var orderstate_sended = 110;
// 已收货
var orderstate_received = 140;
// 已完成
var orderstate_complete = 160;
// 退货
var orderstate_returned = 180;
// 取消
var orderstate_cancelled = 200;

//订单搜索
function orderSearch() {
    var keyword = document.getElementById("keyword").value;
    if(keyword == undefined || keyword == null || keyword.length < 1)
    {
        keyword = "";
    }
     window.location.href = "/mob/ucenter/orderlist?keyword=" + encodeURIComponent(keyword);
};


//获得订单列表
var orderListNextPageNumber = 2;
function getOrderList(startAddTime, endAddTime, orderState, page) {
    document.getElementById("loadBut").style.display = "none";
    document.getElementById("loadPrompt").style.display = "block";
    var keyword = document.getElementById("keyword").value;
    if (keyword == undefined || keyword == null || keyword.length < 1) {
        keyword = "";
    }
    Ajax.get("/mob/ucenter/ajaxorderlist?startAddTime=" + startAddTime + "&endAddTime=" + endAddTime + "&orderState=" + orderState + "&page=" + page + "&keyword=" + keyword, false, function (data) {
        getOrderListResponse(data);
    })
}

//处理获得订单列表的反馈信息
function getOrderListResponse(data) {
    try {
        var result = eval("(" + data + ")");
        var element = document.createElement("div");
        var innerHTML = "";
        for (var i = 0; i < result.OrderList.length; i++) {
            var orderProductInfo = null;
            var oid = result.OrderList[i].oid;
            var surplusmoney=result.OrderList[i].surplusmoney;
            var osn=result.OrderList[i].osn;
            var orderState = result.OrderList[i].orderstate;
            var isreview = "";
            if (result.OrderList[i].isreview==1) {
                isreview = "已评价";
            }
            var orderstatestr = "";
            switch (orderState) {
                case orderstate_waitpaying:
                    orderstatestr = "待付款";
                    break;
                case orderstate_confirming:
                    orderstatestr = "待确认";
                    break;
                case orderstate_confirmed:
                    orderstatestr = "已确认";
                    break;
                case orderstate_preproducting:
                    orderstatestr = "已备货";
                    break;
                case orderstate_sended:
                    orderstatestr = "已发货";
                    break;
                case orderstate_received:
                    orderstatestr = "已收货";
                    break;
                case orderstate_complete:
                    orderstatestr = "已完成";
                    break;
                case orderstate_returned:
                    orderstatestr = "已退货";
                    break;
                case orderstate_cancelled:
                    orderstatestr = "已取消";
                    break;
            }

            innerHTML += "<div class='proItme'>";
            innerHTML += " <div class='orderosn'><span>订单编号：" + osn + "</span><b>&nbsp;&nbsp;" + orderstatestr + "&nbsp;&nbsp;" + isreview + "</b></div>";
            for (var j = 0; j < result.OrderProductList.length; j++) {
                if (result.OrderProductList[j].Oid == oid) {
                    orderProductInfo = result.OrderProductList[j];
                    innerHTML += "<a href='/mob/catalog/product?pid=" + orderProductInfo.Pid + "'>";
                    innerHTML += "<img src='/upload/store/" + orderProductInfo.StoreId + "/product/show/thumb60_60/" + orderProductInfo.ShowImg + "' onerror='nofindimg();'>";
                    innerHTML += "<div class='order-msg'><p class='title'>" + orderProductInfo.Name + "</p><p class='price'>¥" + orderProductInfo.DiscountPrice + " X" + orderProductInfo.RealCount + "<span></span></p></div>";
                    innerHTML += "</a>";
                }
            }
            innerHTML+="<div class='clear'></div>";
            innerHTML += "<a style='margin-right:40px; float:right;  color:#ED474A; font-weight:bold; font-size:90%;'>支付金额：￥" + surplusmoney + "</a>";
             innerHTML+="<div class='clear'></div>";
            innerHTML += "<div class='proBt'>";
            innerHTML += "<a class='redBt' href='/mob/ucenter/orderinfo?oid=" + oid + "'>订单详情</a> ";
            if (orderState == orderstate_waitpaying && result.OrderList[i].paymode == 1) {
                innerHTML += "<a class='redBt' href='/mob/order/submitresult?oidList=" + oid + "' id='payOrderBut" + oid + "'>在线支付</a>";
            }
            if ((orderState == orderstate_received || orderState == orderstate_complete) && result.OrderList[i].isreview == 0) {
                innerHTML += "<a class='redBt' href='/mob/ucenter/revieworder?oid=" + oid + "'>订单评价</a>";
            }
            if (orderState == orderstate_waitpaying || (orderState == orderstate_confirming && result.OrderList[i].paymode == 0)) {
                innerHTML += "<a class='redBt' href='javascript:cancelOrder(" + oid + ", 0)' id='cancelOrderBut" + oid + "'>取消订单</a>";
            }
            if (orderState == orderstate_sended) {
                innerHTML += "<a class='redBt' href='javascript:receiveProduct(" + oid + ")' id='receiveProductBut" + oid + "'>确认收货</a>";
            }
            innerHTML += "</div>";

            innerHTML += "</div>";
        }
        element.innerHTML = innerHTML;
        document.getElementById("orderListBlock").appendChild(element);
        if (result.PageModel.HasNextPage) {
            document.getElementById("loadBut").style.display = "block";
            document.getElementById("loadPrompt").style.display = "none";
            orderListNextPageNumber += 1;
        }
        else {
            document.getElementById("loadBut").style.display = "none";
            document.getElementById("loadPrompt").style.display = "none";
            document.getElementById("lastPagePrompt").style.display = "block";
        }
    }
    catch (ex) {
        alert("加载错误");
    }
}

//取消订单
function cancelOrder(oid, cancelReason) {
    if (confirm("确认要取消该订单？")) {
        Ajax.post("/mob/ucenter/cancelorder", { 'oid': oid, 'cancelReason': cancelReason }, false, cancelOrderResponse);
    }
}

//处理取消订单的反馈信息
function cancelOrderResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        document.getElementById("cancelOrderBut" + result.content).parentNode.innerHTML = "<a class='redBt' href='" + document.getElementById("orderDetai" + result.content).href + "'>订单详情</a>";
        alert("取消成功");
    }
    else {
        alert(result.content);
    }
}

//确认收货
function receiveProduct(oid) {
    if (confirm("确认已经收货？"))
    {
        Ajax.post("/mob/ucenter/ReceiveProduct", { 'oid': oid }, false, receiveProductResponse);
    }
}

//确认收货的反馈信息
function receiveProductResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        document.getElementById("receiveProductBut" + result.content).parentNode.innerHTML = "<a class='redBt' href='" + document.getElementById("orderDetai" + result.content).href + "'>订单详情</a><a class='redBt' href='/mob/ucenter/revieworder?oid=" + result.content + "'>订单评价</a>";
        alert("您已经确认了收货");
    }
    else {
        alert(result.content);
    }
}

//选择商品星星
function selectProductStar(i,pid) {
    var list = document.getElementsByName('star_' + pid);
    for (var j = 1; j <= 5; j++) {
        if (j <= i) {
            list[j - 1].className = "on";
        }
        else {
            list[j - 1].className = "";
        }
    }
    var reviewOrderFrom = document.forms["reviewOrderFrom"];
    reviewOrderFrom.elements["stars_"+pid].value = i;
}

//选择描述星星
function selectDescriptionStar(i) {
    var list = document.getElementById("descriptionBlock").getElementsByTagName("span");
    for (var j = 1; j <= 5; j++) {
        if (j <= i) {
            list[j - 1].className = "on";
        }
        else {
            list[j - 1].className = "";
        }
    }
    var reviewOrderFrom = document.forms["reviewOrderFrom"];
    reviewOrderFrom.elements["descriptionStar"].value = i;
}

//选择服务星星
function selectServiceStar(i) {
    var list = document.getElementById("serviceBlock").getElementsByTagName("span");
    for (var j = 1; j <= 5; j++) {
        if (j <= i) {
            list[j - 1].className = "on";
        }
        else {
            list[j - 1].className = "";
        }
    }
    var reviewOrderFrom = document.forms["reviewOrderFrom"];
    reviewOrderFrom.elements["serviceStar"].value = i;
}

//选择配送星星
function selectShipStar(i) {
    var list = document.getElementById("shipBlock").getElementsByTagName("span");
    for (var j = 1; j <= 5; j++) {
        if (j <= i) {
            list[j - 1].className = "on";
        }
        else {
            list[j - 1].className = "";
        }
    }
    var reviewOrderFrom = document.forms["reviewOrderFrom"];
    reviewOrderFrom.elements["shipStar"].value = i;
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
        var star = reviewOrderFrom.elements["stars_"+pid].value;
        if (star < 1 || star > 5) {
            alert('请选择正确的星星');
            return false;
        }
        var message = reviewOrderFrom.elements["message_" + pid].value;
        if (message.length == 0) {
           alert('请输入评价内容');
            return false;
        }
        if (message.length > 100) {
            alert('评价内容最多输入100个字');
            return false;
        }
        if (message.indexOf("#") > -1) {
           alert('订单备注不能包含特殊字符');
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
    var descriptionStar = reviewOrderFrom.elements["descriptionStar"].value;
    var serviceStar = reviewOrderFrom.elements["serviceStar"].value;
    var shipStar = reviewOrderFrom.elements["shipStar"].value;
    if (descriptionStar < 1 || descriptionStar > 5) {
        alert('请选择正确的商品描述评分');
        return false;
    }
    if (serviceStar < 1 || serviceStar > 5) {
        alert('请选择正确的商家服务评分');
        return false;
    }
    if (shipStar < 1 || shipStar > 5) {
       alert('请选择正确的商家配送评分');
        return false;
    }
    document.getElementById("submitreviewbtn").disabled = true;
    Ajax.post("/mob/ucenter/revieworderpost?oid=" + oid, { 'stars': stars, 'messages': messages, 'opids': opids, 'descriptionStar': descriptionStar, 'serviceStar': serviceStar, 'shipStar': shipStar }, false, reviewOrderResponse);
}

//处理评价订单的反馈信息
function reviewOrderResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        alert('评价成功');
        window.location.href = "/mob/ucenter/orderlist";
    }
    else {
        alert(result.content);
    }
    document.getElementById("submitreviewbtn").disabled = false;
}

//订单修改支付方式
function changePayPlugin(paySystemName, OidList) {
    Ajax.post("/mob/order/ChangeOrderPlugin",
            { 'pluginName': paySystemName, 'oids': OidList },
            false,
            changePayPluginResponse)
}

//订单修改支付方式的反馈信息
function changePayPluginResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state != "success") {

        alert(result.content);
    }
    else {
        window.location.href = result.content;
    }
}
