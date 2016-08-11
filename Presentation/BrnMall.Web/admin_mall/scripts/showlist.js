var oldPageSize = 15;
var oldPageNumber = 1;
var delMessage = "您确定要删除吗？";
var batchDelMessage = "您确定要删除全部吗？";

//表单提交操作
function doPostBack(action) {
    var from = $("form:first");
    if (action != "") {
        from.attr("action", action);
    }
    from.get(0).submit();
};

$(function () {
    /*页面变量初始化*/
    oldPageSize = $("#pageSize").val();
    oldPageNumber = $("#pageNumber").val();

    //搜索按钮
    $(".submit").click(function () {
        $("input[name='pageNumber']").attr("value", 1);
        doPostBack("");
        return false;
    });

    //删除按钮
    $(".deleteOperate").click(function () {
        if (!confirm(delMessage)) {
            return false;
        }
    });

    //批量删除
    $(".batchDel").click(function () {
        if ($("input[type='checkbox'][selectItem='true']:checked").length > 0) {
            if (confirm(batchDelMessage)) {
                doPostBack($(this).attr("delUrl"));
            }
        }
        else {
            alert("没有选中任何一项!");
        }
    })

    //ajax删除
    $(".ajaxdeleteOperate").click(function () {
        var ajaxDeleteObj = $(this);
        $.jBox.confirm(delMessage, "提示", function (v, h, f) {
            if (v == 'ok') {
                $.jBox.tip("正在删除...", 'loading');
                $.get(ajaxDeleteObj.attr("url"), function (data, textStatus) {
                    if (data != "0") {
                        ajaxDeleteObj.parents("tr").remove();
                        $.jBox.tip('删除成功！', 'success');
                    } else {
                        $.jBox.error('删除失败，请联系管理员！', '删除失败');
                    }
                });
            }
            else if (v == 'cancel') {
                // 取消
            }

            return true; //close
        });
        return false;
    });

    //批量充值
    $(".batchLoadCash").click(function () {
        var returl = window.location.href;
        var aa = window.location.search;
        var ajaxObj = $(this);
        var cashamount = document.getElementById("cashAmount").value;
        if (cashamount == "") {
            $.jBox.error('请输入充值金额！', '充值失败');
            return false;
        }
        var reg = new RegExp("^\\d+(?:\\.\\d+)?$");
        if (!reg.test(cashamount)) {
            $.jBox.error('充值金额请输入数字！', '充值失败');
            return false;
        }

        var $check_boxes = $("input[type='checkbox'][selectItem='true']:checked");
        if ($check_boxes.length <= 0) {
            $.jBox.error('没有选中任何一位用户！', '充值失败');
            return false;
        }
        var message = "您确认要批量给这些客户充值：" + cashamount + "元？";
        $.jBox.confirm(message, "提示", function (v, h, f) {
            if (v == 'ok') {
                var userIds = new Array();
                $check_boxes.each(function () {
                    userIds.push($(this).val());
                });
                $.ajax({
                    url: ajaxObj.attr("url"),
                    type: "GET",
                    dataType: "json",
                    traditional: true,
                    data: { uidList: userIds, amount: cashamount },
                    success: function (data) {
                        if (data == "succ") { //充值成功
                            $.jBox.tip('充值成功！', 'success');
                        }
                        else {
                            $.jBox.error(data.toString(), '充值失败');
                        }
                    }
                });

            }
            else if (v == 'cancel') {
            }
            return true;
        });

    });

    //ajax删除
    $(".ajaxdeleteOperate").click(function () {
        var ajaxDeleteObj = $(this);
        $.jBox.confirm(delMessage, "提示", function (v, h, f) {
            if (v == 'ok') {
                $.jBox.tip("正在删除...", 'loading');
                $.get(ajaxDeleteObj.attr("url"), function (data, textStatus) {
                    if (data != "0") {
                        ajaxDeleteObj.parents("tr").remove();
                        $.jBox.tip('删除成功！', 'success');
                    } else {
                        $.jBox.error('删除失败，请联系管理员！', '删除失败');
                    }
                });
            }
            else if (v == 'cancel') {
                // 取消
            }

            return true; //close
        });
        return false;
    });



    //表格全选
    $("#allSelect").click(function () {
        $("input[type='checkbox'][selectItem='true']").attr("checked", $(this).attr("checked"));
    });

    //表格排序
    $(".dataList table thead tr th[name='sortTitle']").click(function () {
        $("#sortColumn").val($(this).attr("column"));
        $("#sortDirection").val($(this).attr("direction"));
        doPostBack("");
    });

    //排序提示
    var sortColumn = $("#sortColumn").val();
    if (sortColumn != "") {
        var sortDirection = $("#sortDirection").val();
        var flagHtml = "";
        if (sortDirection == "ASC") {
            flagHtml = "<b>↑</b>";
            sortDirection = "DESC";
        }
        else {
            flagHtml = "<b>↓</b>";
            sortDirection = "ASC";
        }
        $(".dataList th[name='sortTitle'][column='" + sortColumn + "']").attr("direction", sortDirection).append(flagHtml);
    }

    var oldDisplayOrder = 0;
    //修改排序值
    $(".sortinput").focus(function () {
        var sortInputObj = $(this);
        oldDisplayOrder = sortInputObj.val();
        sortInputObj.val("").attr("class", "selectedsortinput");
    });
    $(".sortinput").blur(function () {
        var sortInputObj = $(this);
        if (sortInputObj.val() == "") {
            sortInputObj.val(oldDisplayOrder)
        }
        else {
            var reg = /^-?\d+$/;
            if (!reg.test(sortInputObj.val())) {
                sortInputObj.val(oldDisplayOrder).attr("class", "selectedsortinput");
                alert("只能输入数字！")
                return;
            }
            else {
                if (oldDisplayOrder != sortInputObj.val()) {
                    $.jBox.tip("正在更新...", 'loading');
                    $.get(sortInputObj.attr("url") + "&displayOrder=" + sortInputObj.val(), function (data, textStatus) {
                        if (data != "0") {
                            $.jBox.tip('更新成功！', 'success');
                        } else {
                            sortInputObj.val(oldDisplayOrder);
                            $.jBox.error('更新失败，请联系管理员！', '更新失败');
                        }
                    });
                }
            }
        }
        sortInputObj.attr("class", "unselectedsortinput");
    });

    //页数按钮
    $(".dataListEdit .page .bt").click(function () {
        $("#pageNumber").val($(this).attr("page"));
        doPostBack("");
        return false;
    });

    //每页显示条数输入框
    $("#pageSize").focus(function () {
        $(this).val("");
    });
    $("#pageSize").blur(function () {
        var value = $(this).val();
        if (value == "") {
            $(this).val(oldPageSize);
        }
        else {
            var regex = /^\d+$/;
            if (!regex.test(value)) {
                alert("只能输入数字!");
                $(this).val(oldPageSize);
            }
            else if (parseInt(value) != oldPageSize) {
                doPostBack("");
            }
        }
    });

    //跳转到指定页输入框
    $("#pageNumber").focus(function () {
        $(this).val("");
    });
    $("#pageNumber").blur(function () {
        var value = $(this).val();
        if (value == "") {
            $(this).val(oldPageNumber);
        }
        else {
            var regex = /^\d+$/;
            if (!regex.test(value)) {
                alert("只能输入数字!");
                $(this).val(oldPageNumber);
            }
            else {
                var totalPages = $(this).attr("totalPages");
                if (parseInt(value) > parseInt(totalPages)) {
                    alert("跳转页数不能大于" + totalPages);
                    $(this).val(oldPageNumber);
                }
                else if (parseInt(value) != oldPageNumber) {
                    doPostBack("");
                }
            }

        }
    });

});

//找不到图片，显示上次默认图片
function nofindimg() {
    var img = event.srcElement;
    img.src = "/images/defaultImg.jpg";
    img.onerror = null; //控制不要一直跳动
}