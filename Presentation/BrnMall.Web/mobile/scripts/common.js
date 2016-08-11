var uid = -1; //用户id
var isGuestSC = 0; //是否允许游客使用购物车(0代表不可以，1代表可以)

//返回
function pageBack() {
    var a = window.location.href;
    if (/#top/.test(a)) {
        window.history.go(-2);
        window.location.load(window.location.href)
    } else {
        window.history.back();
        window.location.load(window.location.href)
    }
}

//导航栏显示和隐藏
function navSH() {
    var navObj = document.getElementById("nav");
    if (navObj.style.display == "none") {
        navObj.style.display = "block";
    }
    else {
        navObj.style.display = "none";
    }
}
//清理搜索框
function clearSearchKeyWord(kewordid) {
    document.getElementById(kewordid).value = '';
}

//商城搜索
function mallSearch(keyword) {
    if (keyword == undefined || keyword == null || keyword.length < 1) {
        keyword = "";
    }
    window.location.href = "/mob/catalog/search?keyword=" + encodeURIComponent(keyword)+"&stype=1";
    
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
    window.location.href = "/mob/store/search?storeId=" + storeId + "&keyword=" + encodeURIComponent(keyword) + "stype=1";
}

//添加店铺到收藏夹
function addStoreToFavorite(storeId) {
    if (storeId < 1) {
        alert("请选择店铺");
    }
    else if (uid < 1) {
        alert("请先登录");
    }
    else {
        Ajax.get("/mob/ucenter/addstoretofavorite?storeId=" + storeId, false, addStoreToFavoriteResponse)
    }
}

//处理添加店铺到收藏夹的反馈信息
function addStoreToFavoriteResponse(data) {
    var result = eval("(" + data + ")");
    alert(result.content);
}

//获得分类商品列表
var cpListNextPageNumber = 2;
function getCategoryProductList(cateId, brandId, filterPrice, filterAttr, onlyStock, sortColumn, sortDirection, page) {
    document.getElementById("loadBut").style.display = "none";
    document.getElementById("loadPrompt").style.display = "block";
    Ajax.get("/mob/catalog/ajaxcategory?cateId=" + cateId + "&brandId=" + brandId + "&filterPrice=" + filterPrice + "&filterAttr=" + filterAttr + "&onlyStock=" + onlyStock + "&sortColumn=" + sortColumn + "&sortDirection=" + sortDirection + "&page=" + page, false, function (data) {
        getCategoryProductListResponse(data);
    })
}

//处理获得分类商品列表的反馈信息
function getCategoryProductListResponse(data) {
    try {
        var result = eval("(" + data + ")");
        var element = document.createElement("div");
        element.className = "proItme";
        var innerHTML = "";
        for (var i = 0; i < result.ProductList.length; i++) {
            var reviewLayer = 100;
            var goodStars = result.ProductList[i].Star3 + result.ProductList[i].Star4 + result.ProductList[i].Star5;
            var allStars = goodStars + result.ProductList[i].Star1 + result.ProductList[i].Star2;
            if (allStars != 0) {
                reviewLayer = Math.round(goodStars * 100 / allStars);
            }
            innerHTML += "<a href='/mob/catalog/product?pid=" + result.ProductList[i].Pid + "'>";
            innerHTML += "<img src='/upload/store/" + result.ProductList[i].StoreId + "/product/show/thumb100_100/" + result.ProductList[i].ShowImg + "' onerror='nofindimg();' width='100' height='100' class='img' /></a>";
            innerHTML += "<span class='proDt'>";
            innerHTML += "<strong class='proDD DD1'><a href='/mob/catalog/product?pid=" + result.ProductList[i].Pid + "'>" + result.ProductList[i].Name + "</a></strong>";
            innerHTML += "<b class='proDD DD3'>￥" + result.ProductList[i].ShopPrice + "<input type='button' onclick='addProductToCart(" + result.ProductList[i].Pid + ", 1,0)' style='right: 20px; position: absolute;' class='OrangeBtn' value='加入购物车'/></b>";
            innerHTML += "<p class='proDD DD4'>" + result.ProductList[i].ReviewCount + " 人评价，" + reviewLayer + "%好评</p>";
            innerHTML += "</span></a>";
        }
        element.innerHTML = innerHTML;
        document.getElementById("categoryProductListBlock").appendChild(element);
        if (result.PageModel.HasNextPage) {
            document.getElementById("loadBut").style.display = "block";
            document.getElementById("loadPrompt").style.display = "none";
            cpListNextPageNumber += 1;
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

//获得商城搜索商品列表
var mspListNextPageNumber = 2;
function getMallSearchProductList(keyword, cateId, brandId, filterPrice, filterAttr, onlyStock, sortColumn, sortDirection, page) {
    document.getElementById("loadBut").style.display = "none";
    document.getElementById("loadPrompt").style.display = "block";
    Ajax.get("/mob/catalog/ajaxsearch?keyword=" + encodeURIComponent(keyword) + "&cateId=" + cateId + "&brandId=" + brandId + "&filterPrice=" + filterPrice + "&filterAttr=" + filterAttr + "&onlyStock=" + onlyStock + "&sortColumn=" + sortColumn + "&sortDirection=" + sortDirection + "&page=" + page, false, function (data) {
        getMallSearchProductListResponse(data);
    })
}

//处理获得商城搜索商品列表的反馈信息
function getMallSearchProductListResponse(data) {
    try {
        var result = eval("(" + data + ")");
        var element = document.createElement("div");
        element.className = "proItme";
        var innerHTML = "";
        for (var i = 0; i < result.ProductList.length; i++) {
            innerHTML += "<div class='proItme'>";
            var reviewLayer = 100;
            var goodStars = result.ProductList[i].Star3 + result.ProductList[i].Star4 + result.ProductList[i].Star5;
            var allStars = goodStars + result.ProductList[i].Star1 + result.ProductList[i].Star2;
            if (allStars != 0) {
                reviewLayer = Math.round(goodStars * 100 / allStars);
            }
            innerHTML += "<a href='/mob/catalog/product?pid=" + result.ProductList[i].Pid +"'>";
            innerHTML += "<img src='/upload/store/" + result.ProductList[i].StoreId + "/product/show/thumb100_100/" + result.ProductList[i].ShowImg + "' onerror='nofindimg();' width='100' height='100' class='img' /></a>";
            innerHTML += "<span class='proDt'>";
            innerHTML += "<strong class='proDD DD1'><a href='/mob/catalog/product?pid=" + result.ProductList[i].Pid +"'>" + result.ProductList[i].Name + "</a></strong>";
            innerHTML += "<b class='proDD DD3'>￥" + result.ProductList[i].ShopPrice + "<input type='button' onclick='addProductToCart("+result.ProductList[i].Pid+", 1,0)' style='right: 20px; position: absolute;' class='OrangeBtn' value='加入购物车'/></b>";
            innerHTML += "<p class='proDD DD4'>" + result.ProductList[i].ReviewCount + " 人评价，" + reviewLayer + "%好评</p>";
            innerHTML += "</span></a>";

            innerHTML += "</div>";
        }
        element.innerHTML = innerHTML;
        document.getElementById("mallSearchProductListBlock").appendChild(element);
        if (result.PageModel.HasNextPage) {
            document.getElementById("loadBut").style.display = "block";
            document.getElementById("loadPrompt").style.display = "none";
            mspListNextPageNumber += 1;
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

//获取优惠劵
function getCoupon(couponTypeId) {
    if (uid < 1) {
        alert("请先登录");
    }
    else if (couponTypeId < 1) {
        alert("请选择优惠劵");
    }
    else {
        Ajax.get("/mob/coupon/getcoupon?couponTypeId=" + couponTypeId, false, getCouponResponse)
    }
}

//处理获取优惠劵的反馈信息
function getCouponResponse(data) {
    var result = eval("(" + data + ")");
    if (result.state == "success") {
        alert("领取成功");
    }
    else {
        alert(result.content);
    }
}


// json时间格式化
function ChangeDateFormat(jsondate) {
    jsondate = jsondate.replace("/Date(", "").replace(")/", "");
    if (jsondate.indexOf("+") > 0) {
        jsondate = jsondate.substring(0, jsondate.indexOf("+"));
    }
    else if (jsondate.indexOf("-") > 0) {
        jsondate = jsondate.substring(0, jsondate.indexOf("-"));
    }
    var date = new Date(parseInt(jsondate, 10));
    var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
    var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
    var hours = date.getHours() < 10 ? "0" + date.getHours() : date.getHours();
    var minutes = date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
    var seconds = date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds();
    return date.getFullYear() + "-" + month + "-" + currentDate + " " + hours + ":" + minutes + ":" + seconds;
}