using System;
using System.Web.Mvc;
using System.Collections.Generic;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.StoreAdmin.Models;

namespace BrnMall.Web.StoreAdmin.Controllers
{
    /// <summary>
    /// 店铺后台商品咨询控制器类
    /// </summary>
    public partial class ProductConsultController : BaseStoreAdminController
    {
        /// <summary>
        /// 商品咨询列表
        /// </summary>
        public ActionResult ProductConsultList(string accountName, string productName, string consultMessage, string consultStartTime, string consultEndTime, string sortColumn, string sortDirection, int pid = -1, int consultTypeId = -1, int pageNumber = 1, int pageSize = 15)
        {
            if (!SecureHelper.IsSafeSqlString(consultMessage)) consultMessage = "";
            if (!SecureHelper.IsSafeSqlString(consultStartTime)) consultStartTime = "";
            if (!SecureHelper.IsSafeSqlString(consultEndTime)) consultEndTime = "";
            if (!SecureHelper.IsSafeSqlString(sortColumn)) sortColumn = "";
            if (!SecureHelper.IsSafeSqlString(sortDirection)) sortDirection = "";
            int uid = AdminUsers.GetUidByAccountName(accountName);

            string condition = AdminProductConsults.AdminGetProductConsultListCondition(consultTypeId, WorkContext.StoreId, pid, uid, consultMessage, consultStartTime, consultEndTime);
            string sort = AdminProductConsults.AdminGetProductConsultListSort(sortColumn, sortDirection);

            PageModel pageModel = new PageModel(pageSize, pageNumber, AdminProductConsults.AdminGetProductConsultCount(condition));
            ProductConsultListModel model = new ProductConsultListModel()
            {
                PageModel = pageModel,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                ProductConsultList = AdminProductConsults.AdminGetProductConsultList(pageModel.PageSize, pageModel.PageNumber, condition, sort),
                AccountName = accountName,
                Pid = pid,
                ProductName = string.IsNullOrWhiteSpace(productName) ? "选择商品" : productName,
                ConsultTypeId = consultTypeId,
                ConsultMessage = consultMessage,
                ConsultStartTime = consultStartTime,
                ConsultEndTime = consultEndTime
            };

            List<SelectListItem> productConsultTypeList = new List<SelectListItem>();
            productConsultTypeList.Add(new SelectListItem() { Text = "全部类型", Value = "0" });
            foreach (ProductConsultTypeInfo productConsultTypeInfo in AdminProductConsults.GetProductConsultTypeList())
            {
                productConsultTypeList.Add(new SelectListItem() { Text = productConsultTypeInfo.Title, Value = productConsultTypeInfo.ConsultTypeId.ToString() });
            }
            ViewData["productConsultTypeList"] = productConsultTypeList;

            MallUtils.SetAdminRefererCookie(string.Format("{0}?pageNumber={1}&pageSize={2}&sortColumn={3}&sortDirection={4}&consultMessage={5}&pid={6}&productName={7}&consultStartTime={8}&consultEndTime={9}&consultTypeId={10}&accountName={11}",
                                                           Url.Action("productconsultlist"),
                                                           pageModel.PageNumber, pageModel.PageSize,
                                                           sortColumn, sortDirection,
                                                           consultMessage,
                                                           pid, productName,
                                                           consultStartTime, consultEndTime,
                                                           consultTypeId, accountName));
            return View(model);
        }

        /// <summary>
        /// 更新商品咨询状态
        /// </summary>
        /// <param name="consultId">商品咨询id</param>
        /// <param name="state">状态</param>
        /// <returns></returns>
        public ActionResult UpdateProductConsultState(int consultId = -1, int state = -1)
        {
            ProductConsultInfo productConsultInfo = AdminProductConsults.AdminGetProductConsultById(consultId);
            if (productConsultInfo == null || productConsultInfo.StoreId != WorkContext.StoreId)
                return Content("0");

            bool result = AdminProductConsults.UpdateProductConsultState(consultId, state);
            if (result)
            {
                AddStoreAdminLog("更新商品咨询状态", "更新商品咨询状态,咨询ID和状态为:" + consultId + "_" + state);
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }

        /// <summary>
        /// 回复商品咨询
        /// </summary>
        [HttpGet]
        public ActionResult Reply(int consultId = -1)
        {
            ProductConsultInfo productConsultInfo = AdminProductConsults.AdminGetProductConsultById(consultId);
            if (productConsultInfo == null)
                return PromptView("商品咨询不存在");
            if (productConsultInfo.StoreId != WorkContext.StoreId)
                return PromptView("不能回复其它店铺的商品咨询");

            ReplyProductConsultModel model = new ReplyProductConsultModel();
            model.ProductConsultInfo = productConsultInfo;
            model.ReplyMessage = productConsultInfo.ReplyMessage;

            ViewData["referer"] = MallUtils.GetStoreAdminRefererCookie();
            return View(model);
        }

        /// <summary>
        /// 回复商品咨询
        /// </summary>
        [HttpPost]
        public ActionResult Reply(ReplyProductConsultModel model, int consultId = -1)
        {
            ProductConsultInfo productConsultInfo = AdminProductConsults.AdminGetProductConsultById(consultId);
            if (productConsultInfo == null)
                return PromptView("商品咨询不存在");
            if (productConsultInfo.StoreId != WorkContext.StoreId)
                return PromptView("不能回复其它店铺的商品咨询");

            if (ModelState.IsValid)
            {
                AdminProductConsults.ReplyProductConsult(consultId, WorkContext.Uid, DateTime.Now, model.ReplyMessage, WorkContext.NickName, WorkContext.IP);
                AddStoreAdminLog("回复商品咨询", "回复商品咨询,商品咨询为:" + consultId);
                return PromptView("商品咨询回复成功");
            }

            model.ProductConsultInfo = productConsultInfo;
            ViewData["referer"] = MallUtils.GetStoreAdminRefererCookie();
            return View(model);
        }

        /// <summary>
        /// 删除商品咨询
        /// </summary>
        /// <param name="consultIdList">商品咨询id列表</param>
        /// <returns></returns>
        public ActionResult DelProductConsult(int[] consultIdList)
        {
            if (!AdminProductConsults.IsStoreConsultByConsultId(WorkContext.StoreId, consultIdList))
                return PromptView("不能删除其它店铺的商品咨询");

            AdminProductConsults.DeleteProductConsultById(consultIdList);
            AddStoreAdminLog("删除商品咨询", "删除商品咨询,商品咨询ID为:" + CommonHelper.IntArrayToString(consultIdList));
            return PromptView("商品咨询删除成功");
        }
    }
}
