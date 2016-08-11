using System;
using System.Data;
using System.Web.Mvc;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.MallAdmin.Models;

namespace BrnMall.Web.MallAdmin.Controllers
{
    /// <summary>
    /// 商城后台商品评价控制器类
    /// </summary>
    public partial class ProductReviewController : BaseMallAdminController
    {
        /// <summary>
        /// 商品评价列表
        /// </summary>
        public ActionResult ProductReviewList(string storeName, string message, string rateStartTime, string rateEndTime, string sortColumn, string sortDirection, int storeId = -1, int pid = 0, int pageNumber = 1, int pageSize = 15)
        {
            string condition = AdminProductReviews.AdminGetProductReviewListCondition(storeId, pid, message, rateStartTime, rateEndTime);
            string sort = AdminProductReviews.AdminGetProductReviewListSort(sortColumn, sortDirection);

            PageModel pageModel = new PageModel(pageSize, pageNumber, AdminProductReviews.AdminGetProductReviewCount(condition));
            ProductReviewListModel model = new ProductReviewListModel()
            {
                PageModel = pageModel,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                ProductReviewList = AdminProductReviews.AdminGetProductReviewList(pageModel.PageSize, pageModel.PageNumber, condition, sort),
                StoreId = storeId,
                StoreName = string.IsNullOrWhiteSpace(storeName) ? "全部店铺" : storeName,
                Pid = pid,
                Message = message,
                StartTime = rateStartTime,
                EndTime = rateEndTime
            };
            MallUtils.SetAdminRefererCookie(string.Format("{0}?pageNumber={1}&pageSize={2}&sortColumn={3}&sortDirection={4}&storeId={5}&storeName={6}&pid={7}&message={8}&startTime={9}&endTime={10}",
                                                            Url.Action("productreviewlist"),
                                                            pageModel.PageNumber, pageModel.PageSize,
                                                            sortColumn, sortDirection,
                                                            storeId, storeName, pid,
                                                            message, rateStartTime, rateEndTime));
            return View(model);
        }

        /// <summary>
        /// 改变商品评价的状态
        /// </summary>
        public ActionResult ChangeProductReviewState(int reviewId = -1, int state = -1)
        {
            bool result = AdminProductReviews.ChangeProductReviewState(reviewId, state);
            if (result)
            {
                AddMallAdminLog("修改商品评价状态", "修改商品评价状态,商品评价ID和状态为:" + reviewId + "_" + state);
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }

        /// <summary>
        /// 删除商品评价
        /// </summary>
        public ActionResult DelProductReview(int reviewId)
        {
            AdminProductReviews.DeleteProductReviewById(reviewId);
            AddMallAdminLog("删除商品评价", "删除商品评价,商品评价ID为:" + reviewId);
            return PromptView("商品评价删除成功");
        }

        /// <summary>
        /// 回复商品评价
        /// </summary>
        [HttpGet]
        public ActionResult Reply(int reviewid = -1)
        {
            ProductReviewInfo productReviewInfo = AdminProductReviews.AdminGetProductReviewById(reviewid);

            if (productReviewInfo == null)
            {
                return PromptView("商品评价不存在");
            }

            var childReview = ProductReviews.GetProductReviewReply(reviewid);

            ReplyProductReviewModel model = new ReplyProductReviewModel();
            model.ProductReviewInfo = productReviewInfo;
            model.ReplyMessage = childReview == null ? "" : childReview.Message;

            ViewData["referer"] = MallUtils.GetMallAdminRefererCookie();
            return View(model);
        }

        /// <summary>
        /// 回复商品评价
        /// </summary>
        [HttpPost]
        public ActionResult Reply(ReplyProductReviewModel model, int reviewid = -1)
        {
            ProductReviewInfo productReviewInfo = AdminProductReviews.AdminGetProductReviewById(reviewid);
            if (productReviewInfo == null)
            {
                return PromptView("商品评价不存在");
            }
            if (string.IsNullOrWhiteSpace(model.ReplyMessage))
            {
                return PromptView("商品评价回复不能为空");
            }
            if (ModelState.IsValid)
            {
                var childReview = ProductReviews.GetProductReviewReply(reviewid);
                if (childReview == null)
                {
                    childReview = new ProductReviewInfo()
                    {
                        ReviewId = 0,
                        Pid = productReviewInfo.Pid,
                        OPRId = productReviewInfo.OPRId,
                        Oid = productReviewInfo.Oid,
                        ParentId = productReviewInfo.ReviewId,
                        State = 0,
                        StoreId = productReviewInfo.StoreId,
                        Star = productReviewInfo.Star,
                        Quality = 0,
                        PayCredits = 0,
                        PName = productReviewInfo.PName,
                        PShowImg = productReviewInfo.PShowImg,
                        BuyTime = productReviewInfo.BuyTime

                    };
                }

                childReview.Message = WebHelper.HtmlEncode(FilterWords.HideWords(model.ReplyMessage));
                childReview.Uid = WorkContext.Uid;
                childReview.ReviewTime = DateTime.Now;
                childReview.IP = WorkContext.IP;
                AdminProductReviews.ProductReviewReply(childReview);

                AddMallAdminLog("回复商品评价", "回复商品评价,商品评价id为:" + reviewid);
                return PromptView("商品评价回复成功");
            }

            model.ProductReviewInfo = productReviewInfo;
            ViewData["referer"] = MallUtils.GetMallAdminRefererCookie();
            return View(model);
        }
    }
}
