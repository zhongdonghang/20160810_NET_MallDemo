//获得配送地址列表
function getShipAddressList() {
    Ajax.get("/mob/ucenter/ajaxshipaddresslist", false, getShipAddressListResponse);
}

//处理获得配送地址列表的反馈信息
function getShipAddressListResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        var shipAddressList = "";
        for (var i = 0; i < result.content.count; i++) {
            shipAddressList += "<div class='bgBlock'></div><div class='adressI'><p>" + result.content.list[i].user + "</p><p class='f14'>" + result.content.list[i].address + "</p><div class='chooseAD'><input type='checkbox' class='radio' name='shipAddressItem' value='" + result.content.list[i].saId + "' onclick='selectShipAddress(" + result.content.list[i].saId + ")'/>送到这里去</div></div>";
        }
        shipAddressList += "<a href='javascript:openAddShipAddressBlock()' class='addAddress'>+添加收货地址</a>";
        document.getElementById("mainBlock").style.display = "none";
        document.getElementById("shipAddressListBlock").style.display = "";
        document.getElementById("shipAddressListBlock").innerHTML = shipAddressList;
    }
    else {
        alert(result.content);
    }
}

//选择配送地址
function selectShipAddress(saId) {
    document.getElementById("saId").value = saId;
    document.getElementById("confirmOrderForm").submit();
}
//选择配送方式
function selectShipMode(shipmode) {
    document.getElementById("shipmode").value = shipmode;
    document.getElementById("confirmOrderForm").submit();
}
//打开添加配送地址块
function openAddShipAddressBlock() {
    document.getElementById("addShipAddressBlock").style.display = "";
}

//添加配送地址
function addShipAddress() {
    var addShipAddressForm = document.forms["addShipAddressForm"];

    var consignee = addShipAddressForm.elements["consignee"].value;
    var mobile = addShipAddressForm.elements["mobile"].value;
    var regionId = getSelectedOption(addShipAddressForm.elements["regionId"]).value;
    var address = addShipAddressForm.elements["address"].value;

    if (!verifyAddShipAddress(consignee, mobile, regionId, address)) {
        return;
    }
    document.getElementById("addShipAddressBtn").disabled = true;
    Ajax.post("/mob/ucenter/addshipaddress",
            { 'consignee': consignee, 'mobile': mobile, 'regionId': regionId, 'address': address, 'isDefault': 1 },
            false,
            addShipAddressResponse)
}

//验证添加的收货地址
function verifyAddShipAddress(consignee, mobile, regionId, address) {
    if (consignee == "") {
        alert("请填写收货人");
        return false;
    }
    if (mobile == "") {
        alert("请填写手机号");
        return false;
    }
    if (parseInt(regionId) < 1) {
        alert("请选择区域");
        return false;
    }
    if (address == "") {
        alert("请填写详细地址");
        return false;
    }
    return true;
}

//处理添加配送地址的反馈信息
function addShipAddressResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        document.getElementById("saId").value = result.content;
        document.getElementById("confirmOrderForm").submit();
    }
    else {
        var msg = "";
        for (var i = 0; i < result.content.length; i++) {
            msg += result.content[i].msg + "\n";
        }
        alert(msg)
    }
    document.getElementById("addShipAddressBtn").disabled = false;
}

//显示支付插件列表
function showPayPluginList() {
    document.getElementById("mainBlock").style.display = "none";
    document.getElementById("payPluginListBlock").style.display = "";
}

//选择支付方式
function selectPayPlugin(paySystemName) {
    document.getElementById("payName").value = paySystemName;
    document.getElementById("confirmOrderForm").submit();
}

//获得有效的优惠劵列表
function getValidCouponList() {
    Ajax.get("/mob/order/getvalidcouponlist", false, getValidCouponListResponse);
}

//处理获得有效的优惠劵列表的反馈信息
function getValidCouponListResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        if (result.content.length < 1) {
            document.getElementById("validCouponList").innerHTML = "<div class=\"allCell\">此订单暂无可用的优惠券</div>";
        }
        else {
            var itemList = "";
            for (var i = 0; i < result.content.length; i++) {
                itemList += "<div class=\"allCell\"><span class=\"radio\" checked='false' value='" + result.content[i].couponId + "' useMode='" + result.content[i].useMode + "' onclick='checkCouponUseMode(this)'></span>" + result.content[i].name + "</div>";
            }
            document.getElementById("validCouponList").innerHTML = itemList;
        }
        document.getElementById("mainBlock").style.display = "none";
        document.getElementById("validCouponListBlcok").style.display = "";
    }
    else {
        alert(result.content);
    }
}

//检查优惠劵的使用模式
function checkCouponUseMode(obj) {
    if (obj.getAttribute("checked") == "true") {
        obj.setAttribute("checked", "false");
        obj.className = "radio";
    }
    else {
        var useMode = obj.getAttribute("useMode");
        if (useMode == "1") {
            var checkboxList = document.getElementById("validCouponList").getElementsByTagName("span");
            for (var i = 0; i < checkboxList.length; i++) {
                checkboxList[i].setAttribute("checked", "false");
                checkboxList[i].className = "radio";
            }
        }
        obj.setAttribute("checked", "true");
        obj.className = "radio checked";
    }
}

//确认选择的优惠劵
function confirmSelectedCoupon() {
    var couponList = "";
    var couponIdCheckboxList = document.getElementById("validCouponList").getElementsByTagName("span");
    for (var i = 0; i < couponIdCheckboxList.length; i++) {
        if (couponIdCheckboxList[i].getAttribute("checked") == "true") {
            couponList += "<div class='sell'><i>惠</i>" + couponIdCheckboxList[i].getAttribute("text") + "</div>";
        }
    }
    document.getElementById("selectCouponList").innerHTML = couponList;
    document.getElementById("mainBlock").style.display = "";
    document.getElementById("validCouponListBlcok").style.display = "none";
}

//提交订单
function submitOrder() {
    var selectedCartItemKeyList = document.getElementById("selectedCartItemKeyList").value
    var saId = document.getElementById("saId").value;
    var payName = document.getElementById("payName").value;
    var payCreditCount = document.getElementById("payCreditCount") && document.getElementById("payCreditCount").checked == "checked" ? document.getElementById("payCreditCount").value : 0;
    var shipmode = document.getElementById("shipmode").value;

    var couponIdList = "";
    var couponIdCheckboxList = document.getElementById("validCouponList").getElementsByTagName("span");
    for (var i = 0; i < couponIdCheckboxList.length; i++) {
        if (couponIdCheckboxList[i].getAttribute("checked") == "true") {
            couponIdList += couponIdCheckboxList[i].getAttribute("value") + ",";
        }
    }
    if (couponIdCheckboxList.length > 0)
        couponIdList = couponIdList.substring(0, couponIdCheckboxList.length - 1)

    var allFullCut = document.getElementById("allFullCut") ? document.getElementById("allFullCut").value : 0;

    if (!verifySubmitOrder(saId, payName)) {
        return;
    }
    document.getElementById("submitbtn").disabled = true;
    Ajax.post("/mob/order/submitorder",
            { 'selectedCartItemKeyList': selectedCartItemKeyList, 'saId': saId, 'payName': payName, 'payCreditCount': payCreditCount, 'couponIdList': couponIdList, 'fullCut': allFullCut,'shipmode':shipmode  },
            false,
            submitOrderResponse)
}

//验证提交订单
function verifySubmitOrder(saId, payName) {
    if (saId < 1) {
        alert("请填写收货人信息");
        return false;
    }
    if (payName.length < 1) {
        alert("配送方式不能为空");
        return false;
    }
    return true;
}

//处理提交订单的反馈信息
function submitOrderResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state != "success") {
        alert(result.content);
    }
    else {
        window.location.href = result.content;
    }
    document.getElementById("submitbtn").disabled = false;
}