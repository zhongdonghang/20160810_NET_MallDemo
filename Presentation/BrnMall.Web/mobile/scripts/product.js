var reviewListNextPageNumber = 2;
//增加商品数量
function addProuctCount() {
    var buyCountInput = document.getElementById("buyCount");
    var buyCount = buyCountInput.value;
    if (!isInt(buyCount)) {
        alert('请输入数字');
        return false;
    }
    buyCountInput.value = parseInt(buyCount) + 1;
}

//减少商品数量
function cutProductCount() {
    var buyCountInput = document.getElementById("buyCount");
    var buyCount = buyCountInput.value;
    if (!isInt(buyCount)) {
        alert('请输入数字');
        return false;
    }
    var count = parseInt(buyCount);
    if (count > 1) {
        buyCountInput.value = count - 1;
    }
}

//添加商品到收藏夹
function addProductToFavorite(pid) {
    if (pid < 1) {
        alert("请选择商品");
    }
    else if (uid < 1) {
        alert("请先登录");
    }
    else {
        Ajax.get("/mob/ucenter/addproducttofavorite?pid=" + pid, false, addProductToFavoriteResponse)
    }
}

//处理添加商品到收藏夹的反馈信息
function addProductToFavoriteResponse(data) {
    var result = eval("(" + data + ")");
    alert(result.content);
}

//添加商品到购物车
function addProductToCart(pid, buyCount, type) {
    if (pid < 1) {
        alert("请选择商品");
    }
    else if (isGuestSC == 0 && uid < 1) {
        alert("请先登录");
    }
    else if (buyCount < 1) {
        alert("请填写购买数量");
    }
    else {
        Ajax.get("/mob/cart/addproduct?pid=" + pid + "&buyCount=" + buyCount, false, function (data) {
            addProductToCartResponse(type, data);
        });
    }
}

//处理添加商品到购物车的反馈信息
function addProductToCartResponse(type, data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        if (type == 0) {
            window.location.href = "/mob/cart/index";
        }
        else {
            document.getElementById("addResult1").style.display = "block";
            document.getElementById("addResult2").style.display = "block";
        }
    }
    else {
        alert(result.content);
    }
}

//添加套装到购物车
function addSuitToCart(pmId, buyCount, type) {
    if (pmId < 1) {
        alert("请选择套装");
    }
    else if (isGuestSC == 0 && uid < 1) {
        alert("请先登录");
    }
    else if (buyCount < 1) {
        alert("请填写购买数量");
    }
    else {
        Ajax.get("/mob/cart/addsuit?pmId=" + pmId + "&buyCount=" + buyCount, false, function (data) {
            addSuitToCartResponse(type, data);
        });
    }
}

//处理添加套装到购物车的反馈信息
function addSuitToCartResponse(type, data) {
    var result = eval("(" + data + ")");
    if (result.state != "stockout") {
        if (type == 0) {
            window.location.href = "/mob/cart/index";
        }
        else {
            document.getElementById("addResult1").style.display = "block";
            document.getElementById("addResult2").style.display = "block";
        }
    }
    else {
        alert("商品库存不足");
    }
}

//获得商品评价列表
function getProductReviewList(pid, page) {
    document.getElementById("loadBut").style.display = "none";
    document.getElementById("loadPrompt").style.display = "block";
    Ajax.get("/mob/catalog/ajaxproductreviewlist?pid=" + pid + "&page=" + page, false, getProductReviewListResponse);
}


//处理获得商品评价的反馈信息
function getProductReviewListResponse(data) {
    try {
        var result = eval("(" + data + ")");
        for (var i = 0; i < result.ProductReviewList.length; i++) {
            var element = document.createElement("tr");
            element.className = "prclass";
            var element1 = document.createElement("tr");
            element1.className = "prclass1";
            var element2 = document.createElement("tr");
            element2.className = "prclass2";
            var element3 = document.createElement("tr");
            element3.className = "prclass1";
            var innerHTML = "";
            var innerHTML1 = "";
            var innerHTML2 = "";
            var innerHTML3 = "";
            var star = parseInt(result.ProductReviewList[i].star)*100/5;
            var nickname = result.ProductReviewList[i].nickname;
            var buytime = ChangeDateFormat(result.ProductReviewList[i].buytime);
            var message = result.ProductReviewList[i].message;
            var reply = result.ProductReviewList[i].replymessage;

            innerHTML += "<td style='color:Gray; font-size:smaller; float:left'>" + nickname + "</td>";
            innerHTML += "<td style='color:Gray; font-size:smaller; float:right'>" + buytime + "</td>";

            innerHTML1 += "<td style='float:left; max-width:65%; font-size:small;' align='left'>" + message + "</td>";
            innerHTML1 += "<td style='float:right'><div class='start'><b style='width:" + star.toString() + "%;'></b></div></td>";

            innerHTML3 += "<td style='float:left; max-width:65%; font-size:small;color:Red' align='left'>" + reply + "</td>";
            innerHTML3 += "<td style='float:right'></td>";

            innerHTML2 += "<td>&nbsp;</td>";
            innerHTML2 += "<td>&nbsp;</td>";
            element.innerHTML = innerHTML;
            element1.innerHTML = innerHTML1;
            element2.innerHTML = innerHTML2;
            element3.innerHTML = innerHTML3;
            document.getElementById("reviewBlock").appendChild(element);
            document.getElementById("reviewBlock").appendChild(element1);
            if (reply != null && reply != "") {
                document.getElementById("reviewBlock").appendChild(element3);
            }
            document.getElementById("reviewBlock").appendChild(element2);
        }
       
        if (result.ReviewpageModel.HasNextPage) {
            document.getElementById("loadBut").style.display = "block";
            document.getElementById("loadPrompt").style.display = "none";
            reviewListNextPageNumber += 1;
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

//获得商品咨询列表
function getProductConsultList(pid, consultTypeId, consultMessage, page) {
    Ajax.get("/mob/catalog/ajaxproductconsultlist?pid=" + pid + "&consultTypeId=" + consultTypeId + "&consultMessage=" + encodeURIComponent(consultMessage) + "&page=" + page, false, getProductConsultListResponse)
}

//处理获得商品咨询的反馈信息
function getProductConsultListResponse(data) {
    document.getElementById("productConsultList").innerHTML = data;
}