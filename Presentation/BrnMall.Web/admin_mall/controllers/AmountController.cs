using System;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.MallAdmin.Models;

namespace BrnMall.Web.MallAdmin.Controllers
{
    /// <summary>
    /// 商城后台广告控制器类
    /// </summary>
    public partial class AmountController : BaseMallAdminController
    {
        /// <summary>
        /// 提现日志列表
        /// </summary>
        public ActionResult WithdrawlList(string username, int state = 0, int paytype = 0, int page = 1)
        {
            int uid = 0;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var user = Users.GetPartUserByName(username.Trim());
                if (user != null)
                {
                    uid = user.Uid;
                }
            }
            PageModel pageModel = new PageModel(15, page, 0);
            List<WithdrawalLogInfo> list = new List<WithdrawalLogInfo>();
            if (string.IsNullOrWhiteSpace(username) || uid > 0)
            {
                pageModel = new PageModel(15, page, Credits.GetWithdrawalLogCount(uid, state, paytype));
                list = Credits.GetWithdrawalLogList(uid, state, paytype, pageModel.PageNumber, pageModel.PageSize);
            }
            WithdrawalLogListModel model = new WithdrawalLogListModel()
            {
                UserName = username,
                PayType = paytype,
                State = state,
                PageModel = pageModel,
                WithdrawalLogList = list
            };
            List<SelectListItem> withdrawStateList = new List<SelectListItem>();
            withdrawStateList.Add(new SelectListItem() { Text = "全部", Value = "0" });
            withdrawStateList.Add(new SelectListItem() { Text = "申请中", Value = ((int)WithdrawalState.applying).ToString() });
            withdrawStateList.Add(new SelectListItem() { Text = "不通过", Value = ((int)WithdrawalState.nopass).ToString() });
            withdrawStateList.Add(new SelectListItem() { Text = "已通过", Value = ((int)WithdrawalState.pass).ToString() });
            ViewData["withdrawStateList"] = withdrawStateList;

            List<SelectListItem> withdrawTypeList = new List<SelectListItem>();
            withdrawTypeList.Add(new SelectListItem() { Text = "全部", Value = "0" });
            withdrawTypeList.Add(new SelectListItem() { Text = "支付宝", Value = ((int)WithdrawalType.Alipay).ToString() });
            withdrawTypeList.Add(new SelectListItem() { Text = "银行卡", Value = ((int)WithdrawalType.BankCard).ToString() });
            ViewData["withdrawTypeList"] = withdrawTypeList;
            return View(model);
        }

        /// <summary>
        /// 提现操作
        /// </summary>
        /// <param name="wid">提现记录id</param>
        /// <param name="flag">true:通过 false:不通过</param>
        /// <param name="reason">通过：单号记录  不通过：不通过理由</param>
        /// <returns></returns>
        public ActionResult UpdateWithdrawl(int wid, bool flag, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                string reasonstr = flag ? "请填写支付单号" : "请填写不通过理由";
                return PromptView(Url.Action("WithdrawlList"), reasonstr);
            }

            WithdrawalLogInfo info = Credits.GetWithdrawalLogById(wid);
            if (info == null)
            {
                return PromptView(Url.Action("WithdrawlList"), "该提现记录不存在");
            }
            info.State = flag ? (int)WithdrawalState.pass : (int)WithdrawalState.nopass;
            info.Reason = reason;
            info.OperatorUid = WorkContext.Uid;
            info.OperatTime = DateTime.Now;
            Credits.UpdateWithdrawalLog(info);
            //通过了申请提现  资金减少记录
            if (flag)
            {
                AdminCredits.WithdrawalPass(wid);
            }
           
            
            return RedirectToAction("WithdrawlList");
        }

        private void Load()
        {
            List<SelectListItem> itemList = new List<SelectListItem>();
            itemList.Add(new SelectListItem() { Text = "请选择广告位置", Value = "0" });
            foreach (AdvertPositionInfo advertPositionInfo in AdminAdverts.GetAllAdvertPosition())
            {
                itemList.Add(new SelectListItem() { Text = advertPositionInfo.Title, Value = advertPositionInfo.AdPosId.ToString() });
            }
            ViewData["advertPositionList"] = itemList;
            ViewData["referer"] = MallUtils.GetMallAdminRefererCookie();
        }
    }
}
