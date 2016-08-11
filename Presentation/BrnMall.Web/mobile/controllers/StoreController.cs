using System;
using System.Web.Mvc;
using System.Web.Routing;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.Mobile.Models;

namespace BrnMall.Web.Mobile.Controllers
{
    /// <summary>
    /// 店铺控制器类
    /// </summary>
    public partial class StoreController : BaseMobileController
    {
        /// <summary>
        /// 店铺首页
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 优惠劵类型列表
        /// </summary>
        public ActionResult CouponTypeList()
        {
            CouponTypeListModel model = new CouponTypeListModel();
            model.CouponTypeList = Coupons.GetSendingCouponTypeList(WorkContext.StoreId, DateTime.Now);
            return View(model);
        }

        /// <summary>
        /// 店铺分类
        /// </summary>
        public ActionResult Class()
        {
            //店铺分类id
            int storeCid = GetRouteInt("storeCid");
            if (storeCid == 0)
                storeCid = WebHelper.GetQueryInt("storeCid");
            //排序列
            int sortColumn = GetRouteInt("sortColumn");
            if (sortColumn == 0)
                sortColumn = WebHelper.GetQueryInt("sortColumn");
            //排序方向
            int sortDirection = GetRouteInt("sortDirection");
            if (sortDirection == 0)
                sortDirection = WebHelper.GetQueryInt("sortDirection");
            //当前页数
            int page = GetRouteInt("page");
            if (page == 0)
                page = WebHelper.GetQueryInt("page");

            //店铺分类信息
            StoreClassInfo storeClassInfo = Stores.GetStoreClassByStoreIdAndStoreCid(WorkContext.StoreId, storeCid);
            if (storeClassInfo == null)
                return View("~/mobile/views/shared/prompt.cshtml", new PromptModel("/", "此店铺分类不存在"));

            //分页对象
            PageModel pageModel = new PageModel(20, page, Products.GetStoreClassProductCount(storeCid, 0, 0));
            //视图对象
            StoreClassModel model = new StoreClassModel()
            {
                StoreCid = storeCid,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                PageModel = pageModel,
                ProductList = Products.GetStoreClassProductList(pageModel.PageSize, pageModel.PageNumber, storeCid, 0, 0, sortColumn, sortDirection),
                StoreClassInfo = storeClassInfo
            };

            return View(model);
        }

        /// <summary>
        /// 店铺搜索
        /// </summary>
        public ActionResult Search()
        {
            //搜索词
            string keyword = WebHelper.GetQueryString("keyword");
            if (!SecureHelper.IsSafeSqlString(keyword))
            {
                return View("~/mobile/views/shared/prompt.cshtml", new PromptModel(WorkContext.UrlReferrer, "您搜索的商品不存在"));
            }

            //判断搜索词是否为店铺分类名称，如果是则重定向到店铺分类页面
            int storeCid = Stores.GetStoreCidByStoreIdAndName(WorkContext.StoreId, keyword);
            if (storeCid > 0)
                return Redirect(Url.Action("class", new RouteValueDictionary { { "storeId", WorkContext.StoreId }, { "storeCid", storeCid } }));

            //排序列
            int sortColumn = WebHelper.GetQueryInt("sortColumn");
            //排序方向
            int sortDirection = WebHelper.GetQueryInt("sortDirection");
            //当前页数
            int page = WebHelper.GetQueryInt("page");

            //分页对象
            PageModel pageModel = new PageModel(20, page, Searches.GetSearchStoreProductCount(keyword, WorkContext.StoreId, 0, 0, 0));
            //视图对象
            StoreSearchModel model = new StoreSearchModel()
            {
                Word = keyword,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                PageModel = pageModel,
                ProductList = Searches.SearchStoreProducts(pageModel.PageSize, pageModel.PageNumber, keyword, WorkContext.StoreId, 0, 0, 0, sortColumn, sortDirection)
            };

            //异步保存搜索历史
            //通过搜索框进行搜索（stype=1）的全部记录
            int type = GetRouteInt("stype");
            if (type == 0)
            {
                type = WebHelper.GetQueryInt("stype");
            }
            if (type == 1)
            {
                Asyn.UpdateSearchHistory(WorkContext.Uid, keyword);
            }

            return View(model);
        }

        protected sealed override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            //店铺id
            WorkContext.StoreId = GetRouteInt("storeId");
            if (WorkContext.StoreId < 1)
            {
                WorkContext.StoreId = WebHelper.GetQueryInt("storeId");
            }

            //店铺信息
            WorkContext.StoreInfo = Stores.GetStoreById(WorkContext.StoreId);

            //验证店铺状态
            if (WorkContext.StoreInfo == null || WorkContext.StoreInfo.State != (int)StoreState.Open)
            {
                filterContext.Result = PromptView(Url.Action("index", "home"), "你访问的店铺不存在");
            }
        }

    }
}
