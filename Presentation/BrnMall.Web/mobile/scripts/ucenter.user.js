var isLoading = false;
var favoriteProductListEnd = false;
var favoriteStoreListEnd = false;
var browseProductListEnd = false;

//获得收藏夹商品列表
var fpListNextPageNumber = 2;
function getFavoriteProductList(page) {
    if (favoriteProductListEnd) {
        document.getElementById("lastPagePrompt").style.display = "block";
    }
    else {
        isLoading = true;
        document.getElementById("loadPrompt").style.display = "block";
        Ajax.get("/mob/ucenter/ajaxfavoriteproductlist?page=" + page, false, getFavoriteProductListResponse)
    }
}

//处理获得收藏夹商品列表的反馈信息
function getFavoriteProductListResponse(data) {
    try {
        var result = eval("(" + data + ")");
        for (var i = 0; i < result.ProductList.length; i++) {
            var element = document.createElement("li");
            element.id = "favoriteProduct" + result.ProductList[i].pid;
            var innerHTML = "";
            innerHTML += "<a href='/mob/catalog/product?pid=" + result.ProductList[i].pid + "'>";
            innerHTML += "<img src='/upload/store/" + result.ProductList[i].storeid + "/product/show/thumb60_60/" + result.ProductList[i].showimg + "' onerror='nofindimg();' width='60px' height='60px'/></a>";
            innerHTML += "<div class='description'>";
            innerHTML += "<a href='/mob/catalog/product?pid=" + result.ProductList[i].pid + "'>";
            innerHTML += "<span class='title'>" + result.ProductList[i].name + "</span><span class='price'>¥" + result.ProductList[i].shopprice + "</span></a>";
            innerHTML += "<a href='javascript:delFavoriteProduct(" + result.ProductList[i].pid + ")' class='del delFavorite'></a>";
            innerHTML += "</div></li>";
            element.innerHTML = innerHTML;
            document.getElementById("favoriteProductListBlock").appendChild(element);
        }
        document.getElementById("loadPrompt").style.display = "none";
        if (result.PageModel.HasNextPage) {
            fpListNextPageNumber += 1;
        }
        else {
            favoriteProductListEnd = true;
            document.getElementById("lastPagePrompt").style.display = "block";
        }
        isLoading = false;
    }
    catch (ex) {
        alert("加载错误");
    }
}

//删除收藏夹中的商品
function delFavoriteProduct(pid) {
    Ajax.get("/mob/ucenter/delfavoriteproduct?pid=" + pid, false, delFavoriteProductResponse)
}

//处理删除收藏夹中的商品的反馈信息
function delFavoriteProductResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        removeNode(document.getElementById("favoriteProduct" + result.content));
        alert("删除成功");
    }
    else {
        alert(result.content);
    }
}

//获得收藏夹店铺列表
var fsListNextPageNumber = 1;
function getFavoriteStoreList(page) {
    if (favoriteStoreListEnd) {
        document.getElementById("lastPagePrompt").style.display = "block";
    }
    else {
        isLoading = true;
        document.getElementById("loadPrompt").style.display = "block";
        Ajax.get("/mob/ucenter/ajaxfavoritestorelist?page=" + page, false, getFavoriteStoreListResponse)
    }
}

//处理获得收藏夹店铺列表的反馈信息
function getFavoriteStoreListResponse(data) {
    try {
        var result = eval("(" + data + ")");
        for (var i = 0; i < result.StoreList.length; i++) {
            var element = document.createElement("li");
            element.id = "favoriteStore" + result.StoreList[i].storeid;
            var innerHTML = "";
            innerHTML += "<li><a href='/mob/store?storeId=" + result.StoreList[i].storeid + "'>";
            innerHTML += "<img src='/upload/store/" + result.StoreList[i].storeid + "/logo/thumb50_50/" + result.StoreList[i].logo + "' onerror='nofindimg();' width='60px' height='60px'/>";
            innerHTML += "<div class='favDPinfo'><span class='title'>" + result.StoreList[i].name + "</span>";
            innerHTML += "<div class='heart'><span class='new-mu-heartv' style='width:95.32%'></span></div>";
            innerHTML += "</div></a><a href='javascript:delFavoriteStore(" + result.StoreList[i].storeid + ")' class='del delFavorite'></a></li>";
            element.innerHTML = innerHTML;
            document.getElementById("favoriteStoreListBlock").appendChild(element);
        }
        document.getElementById("loadPrompt").style.display = "none";
        if (result.PageModel.HasNextPage) {
            fsListNextPageNumber += 1;
        }
        else {
            favoriteStoreListEnd = true;
            document.getElementById("lastPagePrompt").style.display = "block";
        }
        isLoading = false;
    }
    catch (ex) {
        alert("加载错误");
    }
}

//删除收藏夹中的店铺
function delFavoriteStore(storeId) {
    Ajax.get("/mob/ucenter/delfavoritestore?storeId=" + storeId, false, delFavoriteStoreResponse)
}

//处理删除收藏夹中的店铺的反馈信息
function delFavoriteStoreResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        removeNode(document.getElementById("favoriteStore" + result.content));
        alert("已取消收藏！");
    }
    else {
        alert(result.content);
    }
}
//店铺搜索
function storeSearch(storeId, keyword) {
    if (storeId < 1) {
        alert("请先选择店铺");
        return false;
    }
    if (keyword == undefined || keyword == null || keyword.length < 1) {
        keyword = "";
    }
    window.location.href = "/mob/store/search?storeId=" + storeId + "&keyword=" + encodeURIComponent(keyword);

}

//商品页添加商品到收藏夹
function collectProductToFavorite(pid) {
    Ajax.get("/mob/ucenter/AddProductToFavorite?pid=" + pid, false, collectProductToFavoriteResponse)
}

//商品页添加商品到收藏夹的反馈信息
function collectProductToFavoriteResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        document.getElementById("isfavorite").style.display = "block";
        document.getElementById("notfavorite").style.display = "none";
    }
    else {
        alert(result.content);
    }
}


//商品页取消收藏
function cancelFavoriteProduct(pid) {
    Ajax.get("/mob/ucenter/DelFavoriteProduct?pid=" + pid, false, cancelFavoriteProductResponse)
}

//商品页取消收藏的反馈信息
function cancelFavoriteProductResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        document.getElementById("isfavorite").style.display = "none";
        document.getElementById("notfavorite").style.display = "block";
    }
    else {
        alert(result.content);
    }
}


//添加店铺到收藏夹
function collectStoreToFavorite(storeId) {
        Ajax.get("/mob/ucenter/addstoretofavorite?storeId=" + storeId, false, collectStoreToFavoriteResponse)
}

//处理添加店铺到收藏夹的反馈信息
function collectStoreToFavoriteResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        document.getElementById("isfavorite").style.display = "block";
        document.getElementById("notfavorite").style.display = "none";
    }
    else {
        alert(result.content);
    }
}


//店铺首页取消收藏
function cancelFavoriteStore(storeId) {
    Ajax.get("/mob/ucenter/delfavoritestore?storeId=" + storeId, false, cancelFavoriteStoreResponse)
}

//店铺首页取消收藏的反馈信息
function cancelFavoriteStoreResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        document.getElementById("isfavorite").style.display = "none";
        document.getElementById("notfavorite").style.display = "block";
    }
    else {
        alert(result.content);
    }
}


//获得浏览商品列表
var bpListNextPageNumber = 1;
function getBrowseProductList(page) {
    if (browseProductListEnd) {
        document.getElementById("lastPagePrompt").style.display = "block";
    }
    else {
        isLoading = true;
        document.getElementById("loadPrompt").style.display = "block";
        Ajax.get("/mob/ucenter/ajaxbrowseproductlist?page=" + page, false, getBrowseProductListResponse)
    }
}

//处理获得浏览商品列表的反馈信息
function getBrowseProductListResponse(data) {
    try {
        var result = eval("(" + data + ")");
        for (var i = 0; i < result.ProductList.length; i++) {
            var element = document.createElement("li");
            var innerHTML = "";
            innerHTML += "<a href='/mob/catalog/product?pid=" + result.ProductList[i].Pid + "'>";
            innerHTML += "<img src='/upload/store/" + result.ProductList[i].StoreId + "/product/show/thumb60_60/" + result.ProductList[i].ShowImg + "' onerror='nofindimg();' width='60px' height='60px'/></a>";
            innerHTML += "<div class='description'>";
            innerHTML += "<a href='/mob/catalog/product?pid=" + result.ProductList[i].Pid + "'>";
            innerHTML += "<span class='title'>" + result.ProductList[i].Name + "</span><span class='price'>¥" + result.ProductList[i].ShopPrice + "</span></a>";
            innerHTML += "</div></li>";
            element.innerHTML = innerHTML;
            document.getElementById("browseProductListBlock").appendChild(element);
        }
        document.getElementById("loadPrompt").style.display = "none";
        if (result.PageModel.HasNextPage) {
            bpListNextPageNumber += 1;
        }
        else {
            browseProductListEnd = true;
            document.getElementById("lastPagePrompt").style.display = "block";
        }
        isLoading = false;
    }
    catch (ex) {
        alert("加载错误");
    }
}

//删除配送地址
function delShipAddress(saId) {
    Ajax.get("/mob/ucenter/delshipaddress?saId=" + saId, false, delShipAddressResponse)
}

//处理删除配送地址的反馈信息
function delShipAddressResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        removeNode(document.getElementById("shipAddress" + result.content));
        alert("删除成功");
    }
    else {
        alert(result.content);
    }
}

//设置默认配送地址
function setDefaultShipAddress(saId, obj) {
    Ajax.get("/mob/ucenter/setdefaultshipaddress?saId=" + saId, false, function (data) {
        setDefaultShipAddressResponse(data, obj);
    })
}

//处理设置默认配送地址的反馈信息
function setDefaultShipAddressResponse(data, obj) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        var defaultShipAddress = document.getElementById("defaultShipAddress");
        if (defaultShipAddress != undefined) {
            defaultShipAddress.checked = "";
            defaultShipAddress.id = "";
        }
        obj.checked = "checked";
        obj.id = "defaultShipAddress";
    }
    else {
        alert(result.content);
    }
}

//添加配送地址
function addShipAddress() {
    var shipAddressForm = document.forms["shipAddressForm"];

    var consignee = shipAddressForm.elements["consignee"].value;
    var mobile = shipAddressForm.elements["mobile"].value;
    var regionId = getSelectedOption(shipAddressForm.elements["regionId"]).value;
    var address = shipAddressForm.elements["address"].value;

    if (!verifyShipAddress(consignee, mobile, regionId, address)) {
        return;
    }

    Ajax.post("/mob/ucenter/addshipaddress",
            { 'consignee': consignee, 'mobile': mobile, 'regionId': regionId, 'address': address },
            false,
            addShipAddressResponse)
}

//处理添加配送地址的反馈信息
function addShipAddressResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        window.location.href = "/mob/ucenter/shipaddresslist";
    }
    else if (result.state == "full") {
        alert("配送地址的数量已经达到系统所允许的最大值")
    }
    else if (result.state == "error") {
        var msg = "";
        for (var i = 0; i < result.content.length; i++) {
            msg += result.content[i].msg + "\n";
        }
        alert(msg)
    }
}

//编辑配送地址
function editShipAddress() {
    var shipAddressForm = document.forms["shipAddressForm"];

    var saId = shipAddressForm.elements["saId"].value;
    var consignee = shipAddressForm.elements["consignee"].value;
    var mobile = shipAddressForm.elements["mobile"].value;
    var regionId = getSelectedOption(shipAddressForm.elements["regionId"]).value;
    var address = shipAddressForm.elements["address"].value;

    if (saId < 1) {
        alert("请选择配送地址");
        return;
    }
    if (!verifyShipAddress(consignee, mobile, regionId, address)) {
        return;
    }

    Ajax.post("/mob/ucenter/editshipaddress?saId=" + saId,
            { 'consignee': consignee, 'mobile': mobile, 'regionId': regionId, 'address': address },
            false,
            editShipAddressResponse)
}

//处理编辑配送地址的反馈信息
function editShipAddressResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        window.location.href = "/mob/ucenter/shipaddresslist";
    }
    else if (result.state == "noexist") {
        alert("配送地址不存在");
    }
    else if (result.state == "error") {
        var msg = "";
        for (var i = 0; i < result.content.length; i++) {
            msg += result.content[i].msg + "\n";
        }
        alert(msg)
    }
}

//验证配送地址
function verifyShipAddress(consignee, mobile, regionId, address) {
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

//获得资产详细列表
var userAmountLogNextPageNumber = 2;
function getUserAmountLogList(page) {
    document.getElementById("loadBut").style.display = "none";
    document.getElementById("loadPrompt").style.display = "block";
    Ajax.get("/mob/ucenter/ajaxpaycredit?page=" + page, false, function (data) {
        getUserAmountLogListResponse(data);
    })
}

//处理获得资产详细列表的反馈信息
function getUserAmountLogListResponse(data) {
    try {

        var result = eval("(" + data + ")");
        for (var i = 0; i < result.PayCreditLogList.length; i++) {
            var element = document.createElement("tr");
            var innerHTML = "";

            var time = ChangeDateFormat(result.PayCreditLogList[i].ActionTime);
            innerHTML += "<td>" + result.PayCreditLogList[i].UserAmount + "</td>";
            innerHTML += "<td>" + result.PayCreditLogList[i].FrozenAmount + "</td>";
            innerHTML += "<td  align='left'>" + result.PayCreditLogList[i].ActionDes + "<br/>" + time + "</td>";
         
            element.innerHTML = innerHTML;

            document.getElementById("uatable").appendChild(element);
        }
        if (result.PageModel.HasNextPage) {
            document.getElementById("loadBut").style.display = "block";
            document.getElementById("loadPrompt").style.display = "none";
            userAmountLogNextPageNumber += 1;
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

//获得提现列表
var userWithdrawlListPageNumber = 2;
function getWithdrawlList(page) {
    document.getElementById("loadBut").style.display = "none";
    document.getElementById("loadPrompt").style.display = "block";
    Ajax.get("/mob/ucenter/ajaxwithdrawllist?page=" + page, false, function (data) {
        getWithdrawlListResponse(data);
    })
}

//处理获得提现列表的反馈信息
function getWithdrawlListResponse(data) {
    try {

        var result = eval("(" + data + ")");
        for (var i = 0; i < result.WithdrawalLogList.length; i++) {
            var element = document.createElement("tr");
            var innerHTML = "";

            var time = ChangeDateFormat(result.WithdrawalLogList[i].ApplyTime);
            innerHTML += "<td>" + result.WithdrawalLogList[i].ApplyAmount.toFixed(2) + "</td>";
            innerHTML += "<td>" + getWithdrawalState(result.WithdrawalLogList[i].State) + "</td>";
            innerHTML += "<td>" + time + "</td>";
            innerHTML += "<td><a href='/mob/ucenter/withdrawaldetail?wid="+result.WithdrawalLogList[i].RecordId+"' class='redBt'>详情</a></td>";
            element.innerHTML = innerHTML;

            document.getElementById("wltable").appendChild(element);
        }
        if (result.PageModel.HasNextPage) {
            document.getElementById("loadBut").style.display = "block";
            document.getElementById("loadPrompt").style.display = "none";
            userWithdrawlListPageNumber += 1;
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