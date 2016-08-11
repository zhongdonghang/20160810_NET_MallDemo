using System;
using System.Web.Mvc;
using System.Collections.Generic;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.Mobile.Models;

namespace BrnMall.Web.Mobile.Controllers
{
    /// <summary>
    /// 分类控制器类
    /// </summary>
    public partial class CategoryController : BaseMobileController
    {
        private int pageSize = 16;
        /// <summary>
        /// 分类列表
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            var catelay1= Categories.GetCategoryListByLayer(1);
            int cateId = GetRouteInt("cateId");
            if (cateId == 0)
            {
                cateId = WebHelper.GetQueryInt("cateId");
            }

            List<CategoryListLayModel> CateLay2 = new List<CategoryListLayModel>();
            var childCategories=new List<CategoryInfo>();
            if (cateId == 0)
            {
                childCategories = Categories.GetCategoryListByLayer(1);
            }
            else
            {
                childCategories = Categories.GetChildCategoryList(cateId, 2);
            }
            foreach (var cate in childCategories)
            {
                CategoryListLayModel cateModel = new CategoryListLayModel();
                cateModel.CateId = cate.CateId;
                cateModel.CateName = cate.Name;
                cateModel.ProList = Products.GetCategoryProductList(pageSize, 1, cate.CateId, 0, 0, null, null, 0, 0, 0);
                CateLay2.Add(cateModel);
            }
            CategoryListModel model = new CategoryListModel();
            model.CateId = cateId;
            model.CateLay1 = catelay1;
            model.CateLay2 = CateLay2;
            return View(model);
        }

        /// <summary>
        /// 品牌列表
        /// </summary>
        /// <returns></returns>
        public ActionResult BrandList()
        {
            var catelay1 = Categories.GetCategoryListByLayer(1);
            int cateId = GetRouteInt("cateId");
            if (cateId == 0)
            {
                cateId = WebHelper.GetQueryInt("cateId");
            }
            BrandListModel model = new BrandListModel();
            model.CateId = cateId;
            model.CateLay1 = catelay1;
            model.BrandList = Categories.GetCategoryBrandList(cateId);
            return View(model);
        }
    }
}
