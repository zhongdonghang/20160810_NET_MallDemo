using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using BrnMall.Web.Framework;
using BrnMall.Core;
using BrnMall.Services;
using System.Text;
using System.Data;
using BrnMall.Web.Models;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BrnMall.Web.api
{
    /// <summary>
    /// ajax 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class app : System.Web.Services.WebService
    {

        BaseWebController baseWebController = new BaseWebController();

        #region 注册登录

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="accountName">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="returnUrl">返回链接</param>
        [WebMethod(Description = "登录入口")]
        public void Login(string accountName, string password, string returnUrl = "")
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"登录失败！\"}]}";
            try
            {
                var WorkContext = baseWebController.WorkContext;
                if (WorkContext.MallConfig.LoginType == "")
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"商城目前暂时关闭了登录功能！\"}]}";
                    SendData(result);
                }
                //验证账户名
                if (string.IsNullOrWhiteSpace(accountName))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"用户名不能为空！\"}]}";
                    SendData(result);
                }
                if ((!SecureHelper.IsSafeSqlString(accountName, false)))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"用户名不存在！\"}]}";
                    SendData(result);
                }

                //验证密码
                if (string.IsNullOrWhiteSpace(password))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"密码不能为空！\"}]}";
                    SendData(result);
                }

                //当以上验证全部通过时
                PartUserInfo partUserInfo = Users.GetPartUserByName(accountName);
                if (partUserInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"用户名不存在！\"}]}";
                    SendData(result);
                }

                //判断密码是否正确
                if (partUserInfo != null && Users.CreateUserPassword(password, partUserInfo.Salt) != partUserInfo.Password)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"密码不正确！\"}]}";
                    SendData(result);
                }
                //当用户等级是禁止访问等级时
                if (partUserInfo.UserRid == 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"您的账号当前被锁定,不能访问！\"}]}";
                    SendData(result);
                }

                UserInfoModel model = new UserInfoModel();
                model.UserInfo = Users.GetUserById(partUserInfo.Uid);
                model.UserRankInfo = UserRanks.GetUserRankById(partUserInfo.UserRid);
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";

                //更新用户最后访问
                string ip = string.IsNullOrEmpty(WorkContext.IP) ? "" : WorkContext.IP;
                Users.UpdateUserLastVisit(partUserInfo.Uid, DateTime.Now, ip, WorkContext.RegionId);

            }
            catch (Exception)
            {

            }
            SendData(result);
        }

        /// <summary>
        /// app注册
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="password"></param>
        /// <param name="confirmPwd"></param>
        /// <param name="introduceName"></param>
        [WebMethod(Description = "注册入口")]
        public void Register(string accountName, string password, string confirmPwd, string introduceName)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"注册失败！\"}]}";
            var WorkContext = baseWebController.WorkContext;
            //账号验证
            if (string.IsNullOrEmpty(accountName) || !ValidateHelper.IsMobile(accountName))
            {
                result = "{\"result\":false,\"data\":[{\"msg\":\"用户名必须为11位有效手机号码！\"}]}";
                SendData(result);
            }
            if (Users.IsExistUserName(accountName))
            {
                result = "{\"result\":false,\"data\":[{\"msg\":\"该账号用户名已经注册！\"}]}";
                SendData(result);
            }

            //推荐者验证
            if (!string.IsNullOrEmpty(introduceName) && !Users.IsSurperMember(introduceName))
            {
                result = "{\"result\":false,\"data\":[{\"msg\":\"该推荐者不存在！\"}]}";
            }

            //密码验证
            if (string.IsNullOrWhiteSpace(password))
            {
                result = "{\"result\":false,\"data\":[{\"msg\":\"密码不能为空！\"}]}";
                SendData(result);
            }
            else if (password.Length < 4 || password.Length > 32)
            {
                result = "{\"result\":false,\"data\":[{\"msg\":\"密码必须大于3且不大于32个字符！\"}]}";
                SendData(result);
            }
            else if (password != confirmPwd)
            {
                result = "{\"result\":false,\"data\":[{\"msg\":\"两次输入的密码不一样！\"}]}";
                SendData(result);
            }

            //当以上验证都通过时
            try
            {
                UserInfo userInfo = new UserInfo();
                userInfo.UserName = accountName;
                userInfo.Email = string.Empty;
                userInfo.Mobile = accountName;

                userInfo.UserRid = UserRanks.GetLowestUserRank().UserRid;
                userInfo.Salt = Randoms.CreateRandomValue(6);
                userInfo.Password = Users.CreateUserPassword(password, userInfo.Salt);
                userInfo.StoreId = 0;
                userInfo.MallAGid = 1;//非管理员组
                userInfo.NickName = "jsy" + Randoms.CreateRandomValue(7);
                userInfo.Avatar = "";
                userInfo.FrozenAmount = 0;
                userInfo.UserAmount = 0;
                userInfo.RankCredits = 0;
                userInfo.VerifyEmail = 0;
                userInfo.VerifyMobile = 1;

                userInfo.LastVisitIP = "";
                userInfo.LastVisitRgId = WorkContext.RegionId;
                userInfo.LastVisitTime = DateTime.Now;
                userInfo.RegisterIP = "";
                userInfo.RegisterRgId = WorkContext.RegionId;
                userInfo.RegisterTime = DateTime.Now;

                userInfo.Gender = 0;
                userInfo.RealName = "";
                userInfo.Bday = new DateTime(1900, 1, 1);
                userInfo.IdCard = "";
                userInfo.RegionId = 0;
                userInfo.Address = "";
                userInfo.Bio = "";


                //创建用户
                userInfo.Uid = Users.CreateUser(userInfo);

                //添加用户失败
                if (userInfo.Uid < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"创建用户失败,请联系管理员！\"}]}";
                    SendData(result);
                }
                //添加用户推荐者
                if (!string.IsNullOrEmpty(introduceName))
                {
                    int introduceId = Users.GetUidByUserName(introduceName);
                    if (introduceId > 0)
                    {
                        Users.CreateUserIntroduceId(userInfo.Uid, introduceId);
                    }
                }

                //发放注册积分
                Credits.SendRegisterCredits(ref userInfo, DateTime.Now);

                UserInfoModel model = new UserInfoModel();
                model.UserInfo = userInfo;
                model.UserRankInfo = UserRanks.GetUserRankById(userInfo.UserRid);
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <param name="type">0：注册验证  1：找回密码验证</param>
        [WebMethod(Description = "发送短信验证码")]
        public void SendPhoneVerifyCode(string phoneNumber, int type = 0)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"发送短信验证失败！\"}]}";
            int uid = Users.GetUidByUserName(phoneNumber);
            StringBuilder body = null;
            switch (type)
            {
                case 0:

                    if (uid > 0)
                    {
                        result = "{\"result\":false,\"data\":[{\"msg\":\"该手机号已经被注册！\"}]}";
                        SendData(result);
                    }
                    body = new StringBuilder(BMAConfig.SMSConfig.SCVerifyBody);
                    break;
                case 1:
                    if (uid < 1)
                    {
                        result = "{\"result\":false,\"data\":[{\"msg\":\"该手机还未注册过！\"}]}";
                        SendData(result);
                    }
                    body = new StringBuilder(BMAConfig.SMSConfig.FindPwdBody);
                    break;
                default:
                    break;
            }
            //短信内容
            string moibleCode = Randoms.CreateRandomValue(6);
            body.Replace("{mallname}", BMAConfig.MallConfig.MallName);
            body.Replace("{code}", moibleCode);
            string content = body.ToString();
            bool sendresult = false;
            try
            {
                sendresult = SMSes.SendSCVerifySMS(phoneNumber, content);
                if (sendresult)
                {
                    result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(moibleCode, JsonTimeFormat()) + "}";
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                //记录短信log
                var WorkContext = baseWebController.WorkContext;
                SMSLogInfo loginfo = new SMSLogInfo()
                {
                    CodeUsed = false,
                    IP = WorkContext.IP,
                    IsSendSuccess = sendresult,
                    SMSContent = content,
                    SendTime = DateTime.Now,
                    Phone = phoneNumber,
                    Type = type,
                    Code = moibleCode
                };
                SMSes.CreateSMSLog(loginfo);
                SendData(result);
            }
        }

        #endregion

        #region 分类
        /// <summary>
        /// 根据分类名称获取分类
        /// </summary>
        /// <param name="name">分类名称</param>
        [WebMethod(Description = "根据分类名称获取分类")]
        public void GetCategoryByCateName(string name)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"分类不存在！\"}]}";
            try
            {
                CategoryInfo info = Categories.GetCategoryByName(name);
                if (info == null)
                {
                    SendData(result);
                }
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(info, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 获取第一层分类
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "app首页获取第一层分类")]
        public void GetCategoryLay1()
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取分类失败！\"}]}";
            try
            {
                var lay1 = Categories.GetCategoryListByLayer(1);
                if (lay1 != null && lay1.Count() > 0)
                {
                    List<CateListAppModel> list = new List<CateListAppModel>();
                    foreach (var cateinfo in lay1)
                    {
                        CateListAppModel model = new CateListAppModel();
                        model.cateId = cateinfo.CateId;
                        model.cateName = cateinfo.Name;
                        list.Add(model);
                    }

                    result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(list, JsonTimeFormat()) + "}";

                }
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 根据分类Id获取商品列表
        /// </summary>
        /// <param name="cateId"></param>
        [WebMethod(Description = "根据分类Id获取商品列表")]
        public void GetProductListByCateId(int cateId, int pageNumber, int pageSize, string brandid, string filterprice, string onlystock, string sortcolumn, string sortdirection)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取分类商品列表失败！\"}]}";
            try
            {

                int brandId = TypeHelper.StringToInt(brandid);
                int filterPrice = TypeHelper.StringToInt(filterprice);
                int onlyStock = TypeHelper.StringToInt(onlystock);
                int sortColumn = TypeHelper.StringToInt(sortcolumn);
                int sortDirection = TypeHelper.StringToInt(sortdirection);
                //筛选属性
                string filterAttr = "";
                //分类信息
                CategoryInfo categoryInfo = Categories.GetCategoryById(cateId);
                //分类关联品牌列表
                List<BrandInfo> brandList = Categories.GetCategoryBrandList(cateId);
                //分类筛选属性及其值列表
                List<KeyValuePair<AttributeInfo, List<AttributeValueInfo>>> cateAAndVList = Categories.GetCategoryFilterAAndVList(cateId);
                //分类价格范围列表
                string[] catePriceRangeList = StringHelper.SplitString(categoryInfo.PriceRange, "\r\n");

                //筛选属性处理
                List<int> attrValueIdList = new List<int>();
                string[] filterAttrValueIdList = StringHelper.SplitString(filterAttr, "-");
                if (filterAttrValueIdList.Length != cateAAndVList.Count)//当筛选属性和分类的筛选属性数目不对应时，重置筛选属性
                {
                    if (cateAAndVList.Count == 0)
                    {
                        filterAttr = "0";
                    }
                    else
                    {
                        int count = cateAAndVList.Count;
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < count; i++)
                            sb.Append("0-");
                        filterAttr = sb.Remove(sb.Length - 1, 1).ToString();
                    }
                }
                else
                {
                    foreach (string attrValueId in filterAttrValueIdList)
                    {
                        int temp = TypeHelper.StringToInt(attrValueId);
                        if (temp > 0) attrValueIdList.Add(temp);
                    }
                }
                //分页对象
                PageModel pageModel = new PageModel(pageSize, pageNumber, Products.GetCategoryProductCount(cateId, brandId, filterPrice, catePriceRangeList, attrValueIdList, onlyStock));
                //视图对象
                CategoryAppModel model = new CategoryAppModel()
                {
                    CateId = cateId,
                    BrandId = brandId,
                    FilterPrice = filterPrice,
                    FilterAttr = filterAttr,
                    OnlyStock = onlyStock,
                    SortColumn = sortColumn,
                    SortDirection = sortDirection,
                    CategoryInfo = categoryInfo,
                    BrandList = brandList,
                    CatePriceRangeList = catePriceRangeList,
                    AAndVList = cateAAndVList,
                    PageModel = pageModel,
                    ProductList = Products.GetCategoryProductList(pageModel.PageSize, pageModel.PageNumber, cateId, brandId, filterPrice, catePriceRangeList, attrValueIdList, onlyStock, sortColumn, sortDirection)
                };
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 分类列表页
        /// </summary>
        /// <param name="cateId">第一层分类id</param>
        [WebMethod(Description = "分类列表页")]
        public void GetCateProList(string cateId)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取分类列表失败！\"}]}";
            try
            {
                int cateid = TypeHelper.StringToInt(cateId);
                var catelay1 = Categories.GetCategoryListByLayer(1);
                //如果分类为0，自动分配第一个分类id
                if (cateid == 0)
                {
                    cateid = catelay1.ToArray()[0].CateId;
                }

                List<AppCategoryListLayModel> CateLay2 = new List<AppCategoryListLayModel>();
                var childCategories = Categories.GetChildCategoryList(cateid, 2);
                foreach (var cate in childCategories)
                {
                    AppCategoryListLayModel cateModel = new AppCategoryListLayModel();
                    cateModel.CateId = cate.CateId;
                    cateModel.CateName = cate.Name;
                    cateModel.ProList = Products.GetCategoryProductList(9, 1, cate.CateId, 0, 0, null, null, 0, 0, 0);
                    CateLay2.Add(cateModel);
                }
                AppCategoryListModel model = new AppCategoryListModel();
                model.CateId = cateid;
                model.CateLay1 = catelay1;
                model.CateLay2 = CateLay2;
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }


        [WebMethod(Description = "品牌列表页")]
        public void GetCateBrandList(string cateId)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取品牌列表失败！\"}]}";
            try
            {
                int cateid = TypeHelper.StringToInt(cateId);
                var catelay1 = Categories.GetCategoryListByLayer(1);
                //如果分类为0，自动分配第一个分类id
                if (cateid == 0)
                {
                    cateid = catelay1.ToArray()[0].CateId;
                }

                AppBrandListModel model = new AppBrandListModel();
                model.CateId = cateid;
                model.CateLay1 = catelay1;
                model.BrandList = Categories.GetCategoryBrandList(cateid);
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }
        #endregion

        #region 资产管理
        /// <summary>
        /// 资金日志
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageNumber">页码</param>
        [WebMethod(Description = "资金日志")]
        public void PayCreditList(int uid, int pageSize, int pageNumber)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取资金日志失败！\"}]}";
            try
            {
                //用户信息
                PartUserInfo user = Users.GetPartUserById (uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                PageModel pageModel = new PageModel(pageSize, pageNumber, Credits.GetUserAmountLogCount(uid));
                UserAmountLogAppModel model = new UserAmountLogAppModel()
                {
                    PageModel = pageModel,
                    PayCreditLogList = Credits.GetUserAmountLogList(uid, pageModel.PageSize, pageModel.PageNumber),
                    MinApplyWithCash=BMAConfig.CreditConfig.MinAmount,
                    MinWithCash=BMAConfig.CreditConfig.MinWithdrawal,
                    UserAmount=user.UserAmount,
                    UserFrozenAmount=user.FrozenAmount
                };
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 申请提现列表
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageNumber">页码</param>
        [WebMethod(Description = "申请提现列表")]
        public void WithdrawlList(int uid, int pageSize, int pageNumber)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取申请提现列表失败！\"}]}";
            try
            {
                //用户信息
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }

                PageModel pageModel = new PageModel(pageSize, pageNumber, Credits.GetWithdrawalLogCount(uid, 0, 0));
                WithdrawalLogListModel model = new WithdrawalLogListModel()
                {
                    PageModel = pageModel,
                    WithdrawalLogList = Credits.GetWithdrawalLogList(uid, 0, 0, pageModel.PageNumber, pageModel.PageSize)
                };
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }


        [WebMethod(Description = "提现详情")]
        public void WithdrawalDetail(int uid, int wid)
        {

            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取提现详情失败！\"}]}";
            try
            {
                //用户信息
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }

                WithdrawalLogInfo info = Credits.GetWithdrawalLogById(wid);
                if (info == null || info.Uid != uid)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该提现申请不存在！\"}]}";
                    SendData(result);
                }
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(info, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        [WebMethod(Description = "申请提现")]
        public void WithdrawlApply(int uid, decimal applyAmount, string payAccount, string amountRemark, int payType, string phone)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"申请提现失败！\"}]}";
            try
            {
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"用户信息错误！\"}]}";
                    SendData(result);
                }
                if (user.UserAmount < BMAConfig.CreditConfig.MinAmount)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"您还没有满足申请提现的条件！\"}]}";
                    SendData(result);
                }
                var list = Credits.GetWithdrawalLogList(uid, (int)WithdrawalState.applying, 0, 1, 1);
                if (list != null && list.Count > 0)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"您已经有在审核中的提现申请了，当前不能申请提现！\"}]}";
                    SendData(result);
                }
                if (applyAmount > user.UserAmount)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"您剩余的提现额度不足！\"}]}";
                    SendData(result);
                }
                WithdrawalLogInfo winfo = new WithdrawalLogInfo()
              {
                  ApplyAmount = applyAmount,
                  ApplyRemark = amountRemark,
                  ApplyTime = DateTime.Now,
                  PayAccount = payAccount,
                  PayType = payType,
                  Phone = phone,
                  State = (int)WithdrawalState.applying,
                  OperatTime = DateTime.Now,
                  Uid = uid
              };
                Credits.CreateWithdrawalLog(winfo);
                result = "{\"result\":true,\"data\":[{\"msg\":\"申请提现成功！\"}]}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }
        #endregion

        #region 商品

        /// <summary>
        ///商品详情页
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "app商品详情页")]
        public void Product(int pid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取商品失败！\"}]}";
            try
            {
                //判断商品是否存在
                ProductInfo productInfo = Products.GetProductById(pid);
                productInfo.Description = productInfo.Description.Replace("/upload/store/", BMAConfig.MallConfig.SiteUrl + "upload/store/");

                //店铺信息
                StoreInfo storeInfo = Stores.GetStoreById(productInfo.StoreId);
                if (storeInfo.State != (int)StoreState.Open)
                {
                    SendData(result);
                }

                //商品存在时
                ProductModel model = new ProductModel();
                //商品id
                model.Pid = pid;
                //商品信息
                model.ProductInfo = productInfo;
                //商品分类
                model.CategoryInfo = Categories.GetCategoryById(productInfo.CateId);
                //商品品牌
                model.BrandInfo = Brands.GetBrandById(productInfo.BrandId);
                //店铺信息
                model.StoreInfo = storeInfo;
                //店长信息
                model.StoreKeeperInfo = Stores.GetStoreKeeperById(storeInfo.StoreId);
                //店铺区域
                model.StoreRegion = Regions.GetRegionById(storeInfo.RegionId);
                //店铺等级信息
                model.StoreRankInfo = StoreRanks.GetStoreRankById(storeInfo.StoreRid);
                //商品图片列表
                model.ProductImageList = Products.GetProductImageList(pid);
                //扩展商品属性列表
                model.ExtProductAttributeList = Products.GetExtProductAttributeList(pid);
                //商品SKU列表
                model.ProductSKUList = Products.GetProductSKUListBySKUGid(productInfo.SKUGid);
                //商品库存数量
                model.StockNumber = Products.GetProductStockNumberByPid(pid);


                //单品促销
                model.SinglePromotionInfo = Promotions.GetSinglePromotionByPidAndTime(pid, DateTime.Now);
                //买送促销活动列表
                model.BuySendPromotionList = Promotions.GetBuySendPromotionList(productInfo.StoreId, pid, DateTime.Now);
                //赠品促销活动
                model.GiftPromotionInfo = Promotions.GetGiftPromotionByPidAndTime(pid, DateTime.Now);
                //赠品列表
                if (model.GiftPromotionInfo != null)
                    model.ExtGiftList = Promotions.GetExtGiftList(model.GiftPromotionInfo.PmId);
                //套装商品列表
                model.SuitProductList = Promotions.GetProductAllSuitPromotion(pid, DateTime.Now);
                //满赠促销活动
                model.FullSendPromotionInfo = Promotions.GetFullSendPromotionByStoreIdAndPidAndTime(productInfo.StoreId, pid, DateTime.Now);
                //满减促销活动
                model.FullCutPromotionInfo = Promotions.GetFullCutPromotionByStoreIdAndPidAndTime(productInfo.StoreId, pid, DateTime.Now);

                //广告语
                model.Slogan = model.SinglePromotionInfo == null ? "" : model.SinglePromotionInfo.Slogan;
                //商品促销信息
                model.PromotionMsg = Promotions.GeneratePromotionMsg(model.SinglePromotionInfo, model.BuySendPromotionList, model.FullSendPromotionInfo, model.FullCutPromotionInfo);
                //商品折扣价格
                model.DiscountPrice = Promotions.ComputeDiscountPrice(model.ProductInfo.ShopPrice, model.SinglePromotionInfo);

                //店铺推荐商品列表
                model.RelateProductList = Products.GetStoreTraitProductList(4, model.StoreInfo.StoreId, 0, 0);
                var partpro = model.RelateProductList.Find(m => m.Pid == model.Pid);
                if (partpro != null)
                {
                    model.RelateProductList.Remove(partpro);
                }

                //商品咨询类型列表
                model.ProductConsultTypeList = ProductConsults.GetProductConsultTypeList();

                ////更新商品统计
                //Asyn.UpdateProductStat(pid, WorkContext.RegionId);
                //    result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(list) + "}";

                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {

            }

            SendData(result);
        }

        /// <summary>
        /// 我的商品收藏夹列表
        /// </summary>
        /// <param name="uid">用户id</param>
        [WebMethod(Description = "我的商品收藏夹列表")]
        public void FavoriteProductList(int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取商品收藏夹列表失败！\"}]}";
            try
            {
                //用户信息
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                List<FavoriteProductAppModel> list = new List<FavoriteProductAppModel>();
                DataTable table = FavoriteProducts.GetFavoriteProductList(int.MaxValue, 1, uid);
                foreach (DataRow row in table.Rows)
                {
                    FavoriteProductAppModel model = new FavoriteProductAppModel();
                    model.uid = TypeHelper.ObjectToInt(row["uid"]);
                    model.addtime = row["addtime"].ToString();
                    model.name = row["name"].ToString();
                    model.pid = TypeHelper.ObjectToInt(row["pid"]);
                    model.recordid = TypeHelper.ObjectToInt(row["recordid"]);
                    model.shopprice = TypeHelper.ObjectToDecimal(row["shopprice"]);
                    model.showimg = row["showimg"].ToString();
                    model.state = TypeHelper.ObjectToInt(row["state"]);
                    model.storeid = TypeHelper.ObjectToInt(row["storeid"]);
                    model.storename = row["storename"].ToString();
                    list.Add(model);
                }
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(list, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 添加商品到收藏夹
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="pid">商品id</param>
        [WebMethod(Description = "添加商品到收藏夹")]
        public void AddProductToFavorite(int uid, int pid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"收藏商品失败！\"}]}";
            try
            {
                //用户信息
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                //商品信息
                PartProductInfo partProductInfo = Products.GetPartProductById(pid);
                if (partProductInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"获取商品失败！\"}]}";
                    SendData(result);
                }
                //店铺信息
                StoreInfo storeInfo = Stores.GetStoreById(partProductInfo.StoreId);
                if (storeInfo.State != (int)StoreState.Open)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"店铺不存在！\"}]}";
                    SendData(result);
                }

                //当收藏夹中已经存在此商品时
                if (FavoriteProducts.IsExistFavoriteProduct(uid, pid))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该商品已收藏过！\"}]}";
                    SendData(result);
                }

                //收藏夹已满时
                if (baseWebController.WorkContext.MallConfig.FavoriteProductCount <= FavoriteProducts.GetFavoriteProductCount(uid))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"收藏夹已满！\"}]}";
                    SendData(result);
                }

                bool addresult = FavoriteProducts.AddProductToFavorite(uid, pid, 0, DateTime.Now);
                //添加成功
                if (addresult)
                {
                    result = "{\"result\":true,\"data\":[{\"msg\":\"添加收藏夹成功！\"}]}";
                }
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 删除商品收藏
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="pid">商品id</param>
        [WebMethod(Description = "删除商品收藏")]
        public void DeleteFavoriteProduct(int uid, int pid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"删除商品收藏失败！\"}]}";
            try
            {
                //用户信息
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                //商品信息
                if (!FavoriteProducts.IsExistFavoriteProduct(uid, pid))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"获取商品失败！\"}]}";
                    SendData(result);
                }
                bool delResult = FavoriteProducts.DeleteFavoriteProductByUidAndPid(uid, pid);
                //删除成功
                if (delResult)
                {
                    result = "{\"result\":true,\"data\":[{\"msg\":\"删除商品收藏成功！\"}]}";
                }
            }
            catch (Exception)
            {

                throw;
            }
            SendData(result);
        }


        /// <summary>
        /// 获取会员商品折扣价
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="uid"></param>
        [WebMethod(Description = "获取商品会员折扣价")]
        public void GetProDiscountPrice(int pid, int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取商品折扣价失败！\"}]}";
            try
            {
                //检查商品
                PartProductInfo product = Products.GetPartProductById(pid);
                if (product == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该商品不存在！\"}]}";
                    SendData(result);
                }

                decimal dicountprice = Products.GetUserProductPrice(pid, uid);
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(dicountprice) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }


        /// <summary>
        /// 商城搜索
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <param name="uid">用户id</param>
        /// <param name="cateid">分类id</param>
        /// <param name="brandid">品牌id</param>
        /// <param name="filterprice">价格区间（1：0~200 2：200~500 3：500~2000 4：2000~5000 5：5000以上）</param>
        /// <param name="sortcolumn">以哪个来做升降序（默认为权重值（0），1：销售数量，2：商品价格，3：评价数量，4：商品添加时间 5：访问量）</param>
        /// <param name="sortdirection">升降序方向（默认降序，1：升序）</param>
        /// <param name="pagenumber">当前页码</param>
        /// <param name="pagesize">每页数量</param>
        /// <param name="type">为1：系统做搜索记录</param>
        [WebMethod(Description = "商城搜索")]
        public void MallSearch(string keyword, string uid, string cateid, string brandid, string filterprice, string sortcolumn, string sortdirection, string pagenumber, string pagesize, string type)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"您搜索的商品不存在！\"}]}";
            try
            {
                if (!SecureHelper.IsSafeSqlString(keyword))
                {
                    SendData(result);
                }

                int stype = TypeHelper.StringToInt(type);
                //异步保存搜索历史
                if (!string.IsNullOrWhiteSpace(keyword) && stype == 1)
                {
                    int userid = TypeHelper.StringToInt(uid);
                    Asyn.UpdateSearchHistory(userid, keyword);
                }



                int procount = 0;
                List<StoreProductInfo> storeProlist = new List<StoreProductInfo>();
                //分类id
                int cateId = TypeHelper.StringToInt(cateid);
                //筛选价格
                int filterPrice = TypeHelper.StringToInt(filterprice);
                //排序列
                int sortColumn = TypeHelper.StringToInt(sortcolumn);
                //排序方向
                int sortDirection = TypeHelper.StringToInt(sortdirection);
                //当前页数
                int page = TypeHelper.StringToInt(pagenumber);
                if (page < 1)
                {
                    page = 1;
                }
                //每页数量
                int pageSize = TypeHelper.StringToInt(pagesize);
                if (pageSize < 1)
                {
                    pageSize = 20;
                }
                //价格范围列表
                string[] priceRangeList = StringHelper.SplitString("0-200,200-500,500-2000,2000-5000,5000", ",");
                //第一级分类列表
                List<CategoryInfo> cateList = Categories.GetCategoryListByLayer(1);

                //品牌列表
                List<BrandInfo> brandList = new List<BrandInfo>();

                //品牌id
                int brandId = Brands.GetBrandIdByName(keyword);
                //当搜索词为品牌名称时,搜出品牌下所以的商品
                if (brandId > 0)
                {
                    brandList.Add(Brands.GetBrandById(brandId));
                    procount = Searches.GetSearchMallProductCount("", 0, brandId, filterPrice, priceRangeList, null, 0);
                    storeProlist = Searches.SearchMallProducts(pageSize, page, "", 0, brandId, filterPrice, priceRangeList, null, 0, sortColumn, sortDirection);
                }
                else
                {
                    brandId = TypeHelper.StringToInt(brandid);
                    //根据商品关键词获取分类相关的品牌
                    brandList = Searches.GetCategoryBrandListByKeyword(cateId, keyword);
                    procount = Searches.GetSearchMallProductCount(keyword, cateId, brandId, filterPrice, priceRangeList, null, 0);
                    storeProlist = Searches.SearchMallProducts(pageSize, page, keyword, cateId, brandId, filterPrice, priceRangeList, null, 0, sortColumn, sortDirection);
                }
                //分页对象
                PageModel pageModel = new PageModel(pageSize, page, procount); ;
                //视图对象
                AppMallSearchModel model = new AppMallSearchModel()
                {
                    Word = keyword,
                    CateId = cateId,
                    BrandId = brandId,
                    FilterPrice = filterPrice,
                    SortColumn = sortColumn,
                    SortDirection = sortDirection,
                    CateLay1List = cateList,
                    BrandList = brandList,
                    PriceRangeList = priceRangeList,
                    PageModel = pageModel,
                    ProductList = storeProlist
                };

                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        

        #endregion

        #region 商品评价

         /// <summary>
        /// 订单评价
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="uid">用户id</param>
        /// <param name="stars">商品评分s，以#分隔</param>
        /// <param name="messages">评价s，以#分隔</param>
        /// <param name="opids">商品ids，以#分隔</param>
        /// <param name="descriptionStar">商品描述星数</param>
        /// <param name="serviceStar">商家服务星数</param>
        /// <param name="shipStar">商家配送星数</param>
        [WebMethod(Description = "订单评价")]
        public void ReviewOrder(int oid, int uid, string stars,string messages,string opids,int descriptionStar, int serviceStar, int shipStar)
        {
              string result = "{\"result\":false,\"data\":[{\"msg\":\"订单评价失败！\"}]}";
              try
              {
                  OrderInfo orderInfo = Orders.GetOrderByOid(oid);
                  if (orderInfo == null || orderInfo.Uid != uid)
                  {
                      result = "{\"result\":false,\"data\":[{\"msg\":\"订单不存在！\"}]}";
                      SendData(result);
                  }
                  if (orderInfo.OrderState != (int)OrderState.Complete && orderInfo.OrderState != (int)OrderState.Received)
                  {
                      result = "{\"result\":false,\"data\":[{\"msg\":\"订单当前不能评价！\"}]}";
                      SendData(result);
                  }
                  if (orderInfo.IsReview == 1)
                  {
                      result = "{\"result\":false,\"data\":[{\"msg\":\"此订单已经评价！\"}]}";
                      SendData(result);
                  }
                  string[] opidss = opids.Split('#');
                  string[] starss = stars.Split('#');
                  string[] messagess = messages.Split('#');
                 
                  for (int i = 0; i < opidss.Length; i++)
                  {
                      int opid = TypeHelper.StringToInt(opidss[i]);
                      if (opid == 0)
                      { 
                          continue; 
                      }
                      int star = TypeHelper.StringToInt(starss[i]);
                      if (star > 5 || star < 1)
                      {
                          result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的星星！\"}]}";
                          SendData(result);
                      }
                      if (messagess[i].Length == 0)
                      {
                          result = "{\"result\":false,\"data\":[{\"msg\":\"请填写评价内容！\"}]}";
                          SendData(result);
                      }
                      if (messagess[i].Length > 100)
                      {
                          result = "{\"result\":false,\"data\":[{\"msg\":\"评价内容最多输入100个字！\"}]}";
                          SendData(result);
                      }
                      //禁止词
                      string bannedWord = FilterWords.GetWord(messagess[i]);
                      if (bannedWord != "")
                      {
                          result = "{\"result\":false,\"data\":[{\"msg\":\"评价内容中不能包含违禁词！\"}]}";
                          SendData(result);
                      }
                  }
                  if (opidss.Length != starss.Length && starss.Length != messages.Length)
                  {
                      result = "{\"result\":false,\"data\":[{\"msg\":\"评价信息错误，请重新评价！\"}]}";
                      SendData(result);
                  }
                  if (descriptionStar > 5 || descriptionStar < 1)
                  {
                      result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的商品描述星星！\"}]}";
                      SendData(result);
                  }
                  if (serviceStar > 5 || serviceStar < 1)
                  {
                      result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的商家服务星星！\"}]}";
                      SendData(result);
                  }
                  if (shipStar > 5 || shipStar < 1)
                  {
                      result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的商家配送星星！\"}]}";
                      SendData(result);
                  }

                  //评价商品
                  for (int i = 0; i < opidss.Length; i++)
                  {
                      int opid = TypeHelper.StringToInt(opidss[i]);
                      if (opid == 0)
                      {
                          continue;
                      }
                      OrderProductInfo orderProductInfo = null;
                      List<OrderProductInfo> orderProductList = Orders.GetOrderProductList(oid);
                      foreach (OrderProductInfo item in orderProductList)
                      {
                          if (item.Pid == opid)
                          {
                              orderProductInfo = item;
                              break;
                          }
                      }
                      if (orderProductInfo == null)
                      {
                          result = "{\"result\":false,\"data\":[{\"msg\":\"商品不存在！\"}]}";
                          SendData(result);
                      }
                      PartUserInfo user = Users.GetPartUserById(uid);
                      int payCredits = Credits.SendReviewProductCredits(ref user, orderProductInfo, DateTime.Now);
                      ProductReviewInfo productReviewInfo = new ProductReviewInfo()
                      {
                          Pid = orderProductInfo.Pid,
                          Uid = orderProductInfo.Uid,
                          OPRId = orderProductInfo.RecordId,
                          Oid = orderProductInfo.Oid,
                          ParentId = 0,
                          State = 0,
                          StoreId = orderProductInfo.StoreId,
                          Star = TypeHelper.StringToInt(starss[i]),
                          Quality = 0,
                          Message = WebHelper.HtmlEncode(FilterWords.HideWords(messagess[i])),
                          ReviewTime = DateTime.Now,
                          PayCredits = payCredits,
                          PName = orderProductInfo.Name,
                          PShowImg = orderProductInfo.ShowImg,
                          BuyTime = orderProductInfo.AddTime,
                          IP = ""
                      };
                      ProductReviews.ReviewProduct(productReviewInfo);

                  }

                  //评价店铺
                  StoreReviewInfo storeReviewInfo = new StoreReviewInfo()
                  {
                      Oid = oid,
                      StoreId = orderInfo.StoreId,
                      DescriptionStar = descriptionStar,
                      ServiceStar = serviceStar,
                      ShipStar = shipStar,
                      Uid =uid,
                      ReviewTime = DateTime.Now,
                      IP = ""
                  };
                  Stores.CreateStoreReview(storeReviewInfo);

                  //订单已评价
                  Orders.UpdateOrderIsReview(oid, 1);
                  result = "{\"result\":true,\"data\":[{\"msg\":\"订单评价成功！\"}]}";
              }
              catch (Exception)
              {
              }
              SendData(result);
        }

        /// <summary>
        /// 店铺评价
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="uid">用户id</param>
        /// <param name="descriptionStar">商品描述星数</param>
        /// <param name="serviceStar">商家服务星数</param>
        /// <param name="shipStar">商家配送星数</param>
        [WebMethod(Description = "店铺评价")]
        public void ReviewStore(int oid, int uid, int descriptionStar, int serviceStar, int shipStar)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取分类失败！\"}]}";
            try
            {
                //订单验证
                OrderInfo orderInfo = Orders.GetOrderByOid(oid);
                if (orderInfo == null || orderInfo.Uid != uid)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"订单不存在！\"}]}";
                    SendData(result);
                }
                if (orderInfo.OrderState != (int)OrderState.Complete && orderInfo.OrderState != (int)OrderState.Received)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"订单当前不能评价！\"}]}";
                    SendData(result);
                }
                //评分验证
                if (descriptionStar > 5 || descriptionStar < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的商品描述星星！\"}]}";
                    SendData(result);
                }
                if (serviceStar > 5 || serviceStar < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的商家服务星星！\"}]}";
                    SendData(result);
                }
                if (shipStar > 5 || shipStar < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的商家配送星星！\"}]}";
                    SendData(result);
                }
                //评价验证
                StoreReviewInfo storeReviewInfo = Stores.GetStoreReviewByOid(oid);
                if (storeReviewInfo != null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"店铺已经评价！\"}]}";
                    SendData(result);
                }

                //生成评价
                storeReviewInfo = new StoreReviewInfo()
                {
                    Oid = oid,
                    StoreId = orderInfo.StoreId,
                    DescriptionStar = descriptionStar,
                    ServiceStar = serviceStar,
                    ShipStar = shipStar,
                    Uid = uid,
                    ReviewTime = DateTime.Now,
                    IP = ""
                };
                Stores.CreateStoreReview(storeReviewInfo);
                if (Orders.IsReviewAllOrderProduct(Orders.GetOrderProductList(oid)))
                {
                    Orders.UpdateOrderIsReview(oid, 1);
                }
                result = "{\"result\":true,\"data\":[{\"msg\":\"订单评价成功！\"}]}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 商品评价
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="uid">用户id</param>
        /// <param name="recordid">商品订单id</param>
        /// <param name="star">星数</param>
        /// <param name="message">评价内容</param>
        [WebMethod(Description = "商品评价")]
        public void ReviewProduct(int oid, int uid, int recordid, int star, string message)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取分类失败！\"}]}";
            try
            {
                //评价星数
                if (star > 5 || star < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的星星！\"}]}";
                    SendData(result);
                }
                //评价内容
                if (message.Length < 1 || message.Length > 100)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"评价内容不能为空且最多输入100个字！\"}]}";
                    SendData(result);
                }
                //禁止词
                string bannedWord = FilterWords.GetWord(message);
                if (bannedWord != "")
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"评价内容中不能包含违禁词！\"}]}";
                    SendData(result);
                }
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"用户信息错误！\"}]}";
                    SendData(result);
                }
                //订单验证
                OrderInfo orderInfo = Orders.GetOrderByOid(oid);
                if (orderInfo == null || orderInfo.Uid != uid)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"订单不存在！\"}]}";
                    SendData(result);
                }
                if (orderInfo.OrderState != (int)OrderState.Complete && orderInfo.OrderState != (int)OrderState.Received)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"订单当前不能评价！\"}]}";
                    SendData(result);
                }

                OrderProductInfo orderProductInfo = null;
                List<OrderProductInfo> orderProductList = Orders.GetOrderProductList(oid);
                foreach (OrderProductInfo item in orderProductList)
                {
                    if (item.RecordId == recordid)
                    {
                        orderProductInfo = item;
                        break;
                    }
                }
                if (orderProductInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"商品不存在！\"}]}";
                    SendData(result);
                }
                //商品已评价
                if (orderProductInfo.IsReview == 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"商品已经评价！\"}]}";
                    SendData(result);
                }

                int payCredits = Credits.SendReviewProductCredits(ref user, orderProductInfo, DateTime.Now);
                ProductReviewInfo productReviewInfo = new ProductReviewInfo()
                {
                    Pid = orderProductInfo.Pid,
                    Uid = orderProductInfo.Uid,
                    OPRId = orderProductInfo.RecordId,
                    Oid = orderProductInfo.Oid,
                    ParentId = 0,
                    State = 0,
                    StoreId = orderProductInfo.StoreId,
                    Star = star,
                    Quality = 0,
                    Message = WebHelper.HtmlEncode(FilterWords.HideWords(message)),
                    ReviewTime = DateTime.Now,
                    PayCredits = payCredits,
                    PName = orderProductInfo.Name,
                    PShowImg = orderProductInfo.ShowImg,
                    BuyTime = orderProductInfo.AddTime,
                    IP = ""
                };
                ProductReviews.ReviewProduct(productReviewInfo);

                if (Orders.IsReviewAllOrderProduct(orderProductList) && Stores.GetStoreReviewByOid(oid) != null)
                {
                    Orders.UpdateOrderIsReview(oid, 1);
                }

                result = "{\"result\":true,\"data\":[{\"msg\":\"商品评价成功！\"}]}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 获取商品评价列表
        /// </summary>
        /// <param name="pid">商品id</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageNumber">页码</param>
        [WebMethod(Description = "商品评价列表")]
        public void ProductReviewList(int pid, int pageSize, int pageNumber)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取商品评价列表失败！\"}]}";
            try
            {
                //判断商品是否存在
                PartProductInfo productInfo = Products.GetPartProductById(pid);
                if (productInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"你访问的商品不存在！\"}]}";
                    SendData(result);
                }
                //店铺信息
                StoreInfo storeInfo = Stores.GetStoreById(productInfo.StoreId);
                if (storeInfo == null || storeInfo.State != (int)StoreState.Open)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"你访问的商品不存在！\"}]}";
                    SendData(result);
                }
                List<AppProReviewModel> modelList = new List<AppProReviewModel>();
                DataTable rlist = ProductReviews.GetProductReviewList(pid, 0, pageSize, pageNumber);
                foreach (DataRow row in rlist.Rows)
                {
                    AppProReviewModel model = new AppProReviewModel();
                    model.NickName = row["nickname"].ToString();
                    model.ReviewTime =TypeHelper.ObjectToDateTime(row["reviewtime"]);
                    model.Star = TypeHelper.ObjectToInt(row["star"]);
                    model.UserImg = row["avatar"].ToString();
                    model.Content = row["message"].ToString();
                    model.ReplyContent = row["replymessage"].ToString();
                    model.ReplyTime = TypeHelper.ObjectToDateTime(row["replytime"]);
                    modelList.Add(model);
                }
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(modelList, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        #endregion

        #region 店铺

        /// <summary>
        /// 店铺详细信息
        /// </summary>
        /// <param name="storeId">店铺id</param>
        [WebMethod(Description = "店铺详细信息")]
        public void GetStoreInfo(int storeId)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取店铺信息失败！\"}]}";
            try
            {
                //店铺信息
                StoreInfo storeInfo = Stores.GetStoreById(storeId);
                if (storeInfo == null || storeInfo.State != (int)StoreState.Open)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"店铺不存在！\"}]}";
                    SendData(result);
                }
                var model = Stores.GetStoreById(storeId);
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 店铺分类列表
        /// </summary>
        /// <param name="storeId">店铺id</param>
        [WebMethod(Description = "店铺分类列表")]
        public void GetStoreClassList(int storeId)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取店铺信息失败！\"}]}";
            try
            {
                //店铺信息
                StoreInfo storeInfo = Stores.GetStoreById(storeId);
                if (storeInfo == null || storeInfo.State != (int)StoreState.Open)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"店铺不存在！\"}]}";
                    SendData(result);
                }
                var model = Stores.GetStoreClassList(storeId);
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 店铺商品搜索
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageNumber">第几页</param>
        /// <param name="storecid">店铺分类id</param>
        /// <param name="keyword">搜索关键词</param>
        /// <param name="sortcolumn">排序列</param>
        /// <param name="sortdirection">方向</param>
        [WebMethod(Description = "店铺商品搜索")]
        public void StoreProductSearch(int storeId, int pageSize, int pageNumber, string storecid, string keyword, string sortcolumn, string sortdirection)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取店铺信息失败！\"}]}";
            try
            {

                int storeCid = TypeHelper.StringToInt(storecid);
                int sortDirection = TypeHelper.StringToInt(sortdirection);
                int sortColumn = TypeHelper.StringToInt(sortcolumn);

                //店铺信息
                StoreInfo storeInfo = Stores.GetStoreById(storeId);
                if (storeInfo == null || storeInfo.State != (int)StoreState.Open)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"店铺不存在！\"}]}";
                    SendData(result);
                }

                //店铺分类信息
                StoreClassInfo storeClassInfo = null;
                if (storeCid > 0)
                {
                    storeClassInfo = Stores.GetStoreClassByStoreIdAndStoreCid(storeId, storeCid);
                    if (storeClassInfo == null)
                    {
                        result = "{\"result\":false,\"data\":[{\"msg\":\"此店铺分类不存在！\"}]}";
                        SendData(result);
                    }
                }


                //分页对象
                PageModel pageModel = new PageModel(pageSize, pageNumber, Searches.GetSearchStoreProductCount(keyword, storeId, storeCid, 0, 0));
                //视图对象
                StoreSearchModel model = new StoreSearchModel()
                {
                    Word = keyword,
                    StoreCid = storeCid,
                    StartPrice = 0,
                    EndPrice = 0,
                    SortColumn = sortColumn,
                    SortDirection = sortDirection,
                    PageModel = pageModel,
                    ProductList = Searches.SearchStoreProducts(pageModel.PageSize, pageModel.PageNumber, keyword, storeId, storeCid, 0, 0, sortColumn, sortDirection),
                    StoreClassInfo = storeClassInfo
                };
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 我的店铺收藏夹列表
        /// </summary>
        /// <param name="uid">用户id</param>
        [WebMethod(Description = "我的店铺收藏夹列表")]
        public void FavoriteStoreList(int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取商品收藏夹列表失败！\"}]}";
            try
            {
                //用户信息
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }

                List<FavoriteStoreAppModel> list = new List<FavoriteStoreAppModel>();
                DataTable table = FavoriteStores.GetFavoriteStoreList(int.MaxValue, 1, uid);
                foreach (DataRow row in table.Rows)
                {
                    FavoriteStoreAppModel model = new FavoriteStoreAppModel();
                    model.uid = TypeHelper.ObjectToInt(row["uid"]);
                    model.addtime = row["addtime"].ToString();
                    model.name = row["name"].ToString();
                    model.recordid = TypeHelper.ObjectToInt(row["recordid"]);
                    model.logo = row["logo"].ToString();
                    model.storeid = TypeHelper.ObjectToInt(row["storeid"]);
                    list.Add(model);
                }

                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(list, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 添加店铺到收藏夹
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="storeId">店铺id</param>
        [WebMethod(Description = "添加店铺到收藏夹")]
        public void AddStoreToFavorite(int uid, int storeId)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"添加店铺到收藏夹失败！\"}]}";
            try
            {
                //用户信息
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                //店铺信息
                StoreInfo storeInfo = Stores.GetStoreById(storeId);
                if (storeInfo == null || storeInfo.State != (int)StoreState.Open)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"店铺不存在！\"}]}";
                    SendData(result);
                }

                //当收藏夹中已经存在此店铺时
                if (FavoriteStores.IsExistFavoriteStore(uid, storeId))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该店铺已收藏过！\"}]}";
                    SendData(result);
                }

                //收藏夹已满时
                if (baseWebController.WorkContext.MallConfig.FavoriteStoreCount <= FavoriteStores.GetFavoriteStoreCount(uid))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"收藏夹已满！\"}]}";
                    SendData(result);
                }

                bool addResult = FavoriteStores.AddStoreToFavorite(uid, storeId, DateTime.Now);
                //添加成功
                if (addResult)
                {
                    result = "{\"result\":true,\"data\":[{\"msg\":\"添加成功！\"}]}";
                }
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 删除店铺收藏
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="storeId">店铺id</param>
        [WebMethod(Description = "删除店铺收藏")]
        public void DeleteFavoriteStore(int uid, int storeId)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"删除店铺收藏！\"}]}";
            try
            {
                //用户信息
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                //店铺信息
                if (!FavoriteStores.IsExistFavoriteStore(uid, storeId))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"店铺不存在！\"}]}";
                    SendData(result);
                }
                bool delResult = FavoriteStores.DeleteFavoriteStoreByUidAndStoreId(uid, storeId);
                //删除成功
                if (delResult)
                {
                    result = "{\"result\":true,\"data\":[{\"msg\":\"删除店铺收藏成功！\"}]}";
                }
            }
            catch (Exception)
            {
            }
            SendData(result);
        }
        #endregion

        #region 购物车

        /// <summary>
        /// 我的购物车
        /// </summary>
        /// <param name="uid">用户名</param>
        [WebMethod(Description = "我的购物车")]
        public void GetMyCart(int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取购物车失败！\"}]}";
            try
            {

                //当商城不允许游客使用购物车时
                if (baseWebController.WorkContext.MallConfig.IsGuestSC == 0 && uid < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"商城不允许游客使用购物车！\"}]}";
                    SendData(result);
                }
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                //购物车商品列表
                List<OrderProductInfo> orderProductList = Carts.GetCartProductList(uid);
                //商品数量
                int pCount = Carts.SumOrderProductCount(orderProductList);
                //店铺购物车列表
                List<StoreCartInfo> storeCartList = Carts.TidyMallOrderProductList(orderProductList);

                //商品总数量
                int totalCount = Carts.SumMallCartOrderProductCount(storeCartList);
                //商品合计
                decimal productAmount = Carts.SumMallCartOrderProductAmount(storeCartList, uid);
                //满减折扣
                int fullCut = Carts.SumMallCartFullCut(storeCartList);
                //订单合计
                decimal orderAmount = productAmount - fullCut;

                CartModel model = new CartModel
                {
                    TotalCount = totalCount,
                    ProductAmount = productAmount,
                    FullCut = fullCut,
                    OrderAmount = orderAmount,
                    StoreCartList = storeCartList
                };
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }

            catch (Exception)
            {

            }
            SendData(result);
        }

        /// <summary>
        /// 添加商品到购物车
        /// </summary>
        /// <param name="pid">商品id</param>
        /// <param name="uid">用户id</param>
        /// <param name="count">商品数量</param>
        [WebMethod(Description = "添加商品到购物车")]
        public void AddProductToCart(int pid, int uid, int count)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"添加商品到购物车失败！\"}]}";
            try
            {

                //当商城不允许游客使用购物车时
                if (baseWebController.WorkContext.MallConfig.IsGuestSC == 0 && uid < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"商城不允许游客使用购物车！\"}]}";
                    SendData(result);
                }

                //判断用户是否存在
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                //判断商品是否存在
                PartProductInfo partProductInfo = Products.GetPartProductById(pid);
                if (partProductInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该商品不存在！\"}]}";
                    SendData(result);
                }
                //店铺状态是否营业中
                StoreInfo storeInfo = Stores.GetStoreById(partProductInfo.StoreId);
                if (storeInfo.State != (int)StoreState.Open)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该商品不存在！\"}]}";
                    SendData(result);
                }
                //商品库存检查
                int stockNumber = Products.GetProductStockNumberByPid(pid);
                if (stockNumber < count)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该商品库存不足！\"}]}";
                    SendData(result);
                }

                //购物车中已经存在的商品列表
                List<OrderProductInfo> orderProductList = Carts.GetCartProductList(uid);
                OrderProductInfo orderProductInfo = Carts.GetCommonOrderProductByPid(pid, orderProductList);
                if (orderProductInfo == null)
                {
                    if (orderProductList.Count >= baseWebController.WorkContext.MallConfig.MemberSCCount)
                    {
                        result = "{\"result\":false,\"data\":[{\"msg\":\"该购物车已满,请先结账！\"}]}";
                        SendData(result);
                    }
                }

                count = orderProductInfo == null ? count : orderProductInfo.BuyCount + count;

                //购买数量不能小于1
                if (count < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"购买数量不能小于1！\"}]}";
                    SendData(result);
                }

                //将商品添加到购物车
                Carts.AddProductToCart(ref orderProductList, count, partProductInfo, "", uid, DateTime.Now);
                result = "{\"result\":true,\"data\":[{\"msg\":\"添加购物车成功！\"}]}";
            }
            catch (Exception)
            {

            }
            SendData(result);
        }

        /// <summary>
        /// 删除购物车中的商品
        /// </summary>
        /// <param name="pid">商品id</param>
        /// <param name="uid">用户id</param>
        [WebMethod(Description = "删除购物车中的商品")]
        public void DeleteCartProduct(int pid, int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"删除失败！\"}]}";
            try
            {
                //当商城不允许游客使用购物车时
                if (baseWebController.WorkContext.MallConfig.IsGuestSC == 0 && uid < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"商城不允许游客使用购物车！\"}]}";
                    SendData(result);
                }

                //判断用户是否存在
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }

                //判断购物车中商品是否存在
                List<OrderProductInfo> orderProductList = Carts.GetCartProductList(uid);
                OrderProductInfo orderProductInfo = Carts.GetCommonOrderProductByPid(pid, orderProductList);
                if (orderProductInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"购物车中没有该商品！\"}]}";
                    SendData(result);
                }

                //删除
                Carts.DeleteCartProduct(ref orderProductList, orderProductInfo);
                result = "{\"result\":true,\"data\":[{\"msg\":\"删除成功！\"}]}";
            }
            catch (Exception)
            {

            }
            SendData(result);
        }

        #endregion

        #region 订单

        /// <summary>
        /// 我的全部订单
        /// </summary>
        /// <param name="uid">用户名id</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageNumber">第几页</param>
        /// <param name="state">订单状态（0代表全部）</param>
        [WebMethod(Description = "我的全部订单")]
        public void GetMyOrderList(int uid, int pageSize, int pageNumber, string state)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取订单列表失败！\"}]}";
            try
            {
                int orderState = 0;
                int.TryParse(state, out orderState);

                //判断用户
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }

                PageModel pageModel = new PageModel(pageSize, pageNumber, Orders.GetUserOrderCount(uid, "", "", orderState,""));

                List<OrderModel> orderlist = new List<OrderModel>();
                DataTable orderTable = Orders.GetUserOrderList(uid, pageModel.PageSize, pageModel.PageNumber, "", "", orderState,"");
                foreach (DataRow row in orderTable.Rows)
                {
                    OrderModel order = new OrderModel();
                    order.OId = Convert.ToInt32(row["oid"]);
                    order.OSN = row["osn"].ToString();
                    order.UId = Convert.ToInt32(row["uid"]);
                    order.OrderState = Convert.ToInt32(row["orderstate"]);
                    order.OrderAmount = Convert.ToDecimal(row["orderamount"]);
                    order.Surplusmoney = Convert.ToDecimal(row["surplusmoney"]);
                    order.ParentId = Convert.ToInt32(row["parentid"]);
                    order.IsReview = Convert.ToInt32(row["isreview"]);
                    order.AddTime = row["addtime"].ToString();
                    order.StoreId = (int)row["storeid"];
                    order.StoreName = row["storename"].ToString();
                    order.ShipCoid = Convert.ToInt32(row["shipcoid"]);
                    order.ShipCoName = row["shipconame"].ToString();
                    order.PayFriendName = row["payfriendname"].ToString();
                    order.PayMode = Convert.ToInt32(row["paymode"]);
                    order.Consignee = row["consignee"].ToString();

                    List<ProductListModel> prolistmodel = new List<ProductListModel>();
                    var prolist = Orders.GetOrderProductList(order.OId);
                    foreach (var pro in prolist)
                    {
                        ProductListModel promodel = new ProductListModel();
                        promodel.PId = pro.Pid;
                        promodel.PName = pro.Name;
                        promodel.ShowImg = pro.ShowImg;
                        prolistmodel.Add(promodel);
                    }
                    order.ProList = prolistmodel;

                    orderlist.Add(order);

                }


                OrderListAppModel model = new OrderListAppModel()
                {
                    PageModel = pageModel,
                    OrderList = orderlist
                };

                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }

            catch (Exception)
            {

            }
            SendData(result);
        }

        /// <summary>
        /// 我的全部订单
        /// </summary>
        /// <param name="uid">用户名id</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageNumber">第几页</param>
        /// <param name="state">订单状态（0代表全部,,1代表待收货状态（30<state<140）,其他表状态）</param>
        /// <param name="keyword">搜索词（订单编号或者商品名）</param>
        [WebMethod(Description = "我的全部订单（最新接口）")]
        public void GetMyOrderListNew(int uid, int pageSize, int pageNumber, string state,string keyword)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取订单列表失败！\"}]}";
            try
            {
                int orderState = 0;
                int.TryParse(state, out orderState);

                //判断用户
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }

                PageModel pageModel = new PageModel(pageSize, pageNumber, Orders.GetUserOrderCount(uid, "", "", orderState, keyword));

                List<OrderModel> orderlist = new List<OrderModel>();
                DataTable orderTable = Orders.GetUserOrderList(uid, pageModel.PageSize, pageModel.PageNumber, "", "", orderState, keyword);
                foreach (DataRow row in orderTable.Rows)
                {
                    OrderModel order = new OrderModel();
                    order.OId = Convert.ToInt32(row["oid"]);
                    order.OSN = row["osn"].ToString();
                    order.UId = Convert.ToInt32(row["uid"]);
                    order.OrderState = Convert.ToInt32(row["orderstate"]);
                    order.OrderAmount = Convert.ToDecimal(row["orderamount"]);
                    order.Surplusmoney = Convert.ToDecimal(row["surplusmoney"]);
                    order.ParentId = Convert.ToInt32(row["parentid"]);
                    order.IsReview = Convert.ToInt32(row["isreview"]);
                    order.AddTime = row["addtime"].ToString();
                    order.StoreId = (int)row["storeid"];
                    order.StoreName = row["storename"].ToString();
                    order.ShipCoid = Convert.ToInt32(row["shipcoid"]);
                    order.ShipCoName = row["shipconame"].ToString();
                    order.PayFriendName = row["payfriendname"].ToString();
                    order.PayMode = Convert.ToInt32(row["paymode"]);
                    order.Consignee = row["consignee"].ToString();

                    List<ProductListModel> prolistmodel = new List<ProductListModel>();
                    var prolist = Orders.GetOrderProductList(order.OId);
                    foreach (var pro in prolist)
                    {
                        ProductListModel promodel = new ProductListModel();
                        promodel.PId = pro.Pid;
                        promodel.PName = pro.Name;
                        promodel.ShowImg = pro.ShowImg;
                        prolistmodel.Add(promodel);
                    }
                    order.ProList = prolistmodel;

                    orderlist.Add(order);

                }

                OrderListAppModel model = new OrderListAppModel()
                {
                    PageModel = pageModel,
                    OrderList = orderlist,
                    Keyword=keyword,
                    OrderState=state
                };

                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }

            catch (Exception)
            {

            }
            SendData(result);
        }

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="oid">订单id</param>
        [WebMethod(Description = "订单详情")]
        public void OrderDetail(int uid, int oid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取订单详情失败！\"}]}";
            try
            {
                //检查用户
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }

                //验证订单
                OrderInfo orderInfo = Orders.GetOrderByOid(oid);
                if (orderInfo == null || orderInfo.Uid != uid)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该订单不存在！\"}]}";
                    SendData(result);
                }

                OrderInfoModel model = new OrderInfoModel();
                model.OrderInfo = orderInfo;
                model.RegionInfo = Regions.GetRegionById(orderInfo.RegionId);
                model.OrderProductList = AdminOrders.GetOrderProductList(oid);
                model.OrderActionList = OrderActions.GetOrderActionList(oid);
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="oid">订单id</param>
        [WebMethod(Description = "取消订单")]
        public void OrderCancel(int uid, int oid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取订单详情失败！\"}]}";
            try
            {
                //检查用户
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }

                //验证订单
                OrderInfo orderInfo = Orders.GetOrderByOid(oid);
                if (orderInfo == null || orderInfo.Uid != uid)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该订单不存在！\"}]}";
                    SendData(result);
                }

                //不是等待付款或者货到付款订单确认中状态时，不能取消订单
                if (!(orderInfo.OrderState == (int)OrderState.WaitPaying || (orderInfo.OrderState == (int)OrderState.Confirming && orderInfo.PayMode == 0)))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"只有等待付款或者货到付款确认中状态的订单才可取消！\"}]}";
                    SendData(result);
                }

                //取消订单
                Orders.CancelOrder(ref user, orderInfo, uid, DateTime.Now);
                //创建订单处理
                OrderActions.CreateOrderAction(new OrderActionInfo()
                {
                    Oid = oid,
                    Uid = uid,
                    RealName = "本人",
                    ActionType = (int)OrderActionType.Cancel,
                    ActionTime = DateTime.Now,
                    ActionDes = "您取消了订单"
                });
                result = "{\"result\":true,\"data\":[{\"msg\":\"取消订单成功！\"}]}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="saId">配送地址id </param>
        /// <param name="selectedCartItemKeyList">选中的购物车项键列表  为商品类型_商品id 例如："0_99,0_109,1_87"类型(0代表项为购物车商品,1代表项为购物车套装,2代表项为购物车满赠,3代表项为购物车满减)</param>
        /// <param name="payName">支付方式名称</param>
        /// <param name="buyerRemark">买家备注</param>
        /// <param name="payCreditCount">支付积分</param>
        /// <param name="couponIdList">客户已经激活的优惠劵</param>
        /// <param name="couponSNList">客户还未激活的优惠劵</param>
        /// <param name="fullCut">满减金额</param>
        /// <param name="bestTime">最佳配送时间</param>
        /// <param name="ip">ip地址</param>
        /// <param name="shipmode">配送方式</param>
        [WebMethod(Description = "生成订单")]
        public void OrderCreateNew(int uid, int saId, string selectedCartItemKeyList, string payName, string buyerRemark, string paycreditcount, string couponidlist, string couponsnlist, string fullcut, string bestTime, string ip,int shipmode)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"生成订单失败！\"}]}";
            try
            {
                //验证用户
                PartUserInfo puinfo = Users.GetPartUserById(uid);
                if (puinfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"用户信息错误，请重新登录！\"}]}";
                    SendData(result);
                }

                //验证买家备注的内容长度
                if (StringHelper.GetStringLength(buyerRemark) > 125)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"备注最多填写125个字！\"}]}";
                    SendData(result);
                }
                //验证支付方式是否为空
                PluginInfo payPluginInfo = Plugins.GetPayPluginBySystemName(payName);
                if (payPluginInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择支付方式！\"}]}";
                    SendData(result);
                }
                //验证配送地址是否为空
                FullShipAddressInfo fullShipAddressInfo = ShipAddresses.GetFullShipAddressBySAId(saId, uid);
                if (fullShipAddressInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择配送地址！\"}]}";
                    SendData(result);
                }

                //购物车商品列表
                List<OrderProductInfo> orderProductList = Carts.GetCartProductList(uid);
                if (orderProductList.Count < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"购物车中没有商品！\"}]}";
                    SendData(result);
                }

                //店铺购物车列表
                List<StoreCartInfo> storeCartList = Carts.TidyMallOrderProductList(StringHelper.SplitString(selectedCartItemKeyList), orderProductList);
                if (Carts.SumMallCartOrderProductCount(storeCartList) < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"购物车中没有选中的商品！\"}]}";
                    SendData(result);
                }

                #region 验证支付积分
                int payCreditCount = TypeHelper.StringToInt(paycreditcount);
               
                #endregion

                #region 验证优惠劵
                List<CouponInfo> couponList = new List<CouponInfo>();
                #endregion

                string pidList = Carts.GetMallCartPidList(storeCartList);//商品id列表
                List<ProductStockInfo> productStockList = Products.GetProductStockList(pidList);//商品库存列表
                List<SinglePromotionInfo> singlePromotionList = new List<SinglePromotionInfo>();//单品促销活动列表
                //循环店铺购物车列表
                foreach (StoreCartInfo storeCartInfo in storeCartList)
                {
                    //循环购物车项列表，依次验证
                    foreach (CartItemInfo cartItemInfo in storeCartInfo.CartItemList)
                    {
                        var cartItem = (CartProductInfo)cartItemInfo.Item;
                        if (!cartItem.Selected)
                        {
                            continue;
                        }
                        #region 购物车商品类型
                        if (cartItemInfo.Type == 0)
                        {
                            CartProductInfo cartProductInfo = cartItem;

                            OrderProductInfo orderProductInfo = cartProductInfo.OrderProductInfo;

                            //验证商品信息
                            PartProductInfo partProductInfo = Products.GetPartProductById(orderProductInfo.Pid);
                            if (partProductInfo == null)
                            {
                                result = "{\"result\":false,\"data\":[{\"msg\":\"商品" + partProductInfo.Name + "已经下架，请删除此商品！\"}]}";
                                SendData(result);
                            }
                            if (orderProductInfo.Name != partProductInfo.Name || orderProductInfo.ShopPrice != partProductInfo.ShopPrice || orderProductInfo.MarketPrice != partProductInfo.MarketPrice || orderProductInfo.CostPrice != partProductInfo.CostPrice || orderProductInfo.Weight != partProductInfo.Weight || orderProductInfo.PSN != partProductInfo.PSN)
                            {
                                result = "{\"result\":false,\"data\":[{\"msg\":\"商品" + partProductInfo.Name + "信息有变化，请删除后重新添加！\"}]}";
                                SendData(result);
                            }

                            //验证商品库存
                            ProductStockInfo productStockInfo = Products.GetProductStock(orderProductInfo.Pid, productStockList);
                            if (productStockInfo.Number < orderProductInfo.RealCount)
                            {
                                result = "{\"result\":false,\"data\":[{\"msg\":\"商品" + partProductInfo.Name + "库存不足！\"}]}";
                                SendData(result);
                            }
                            else
                            {
                                productStockInfo.Number -= orderProductInfo.RealCount;
                            }
                            
                        }
                        #endregion

                    }
                }

                if (Carts.SumMallCartFullCut(storeCartList) != TypeHelper.StringToInt(fullcut))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"满减金额不正确,请刷新页面重新提交！\"}]}";
                    SendData(result);
                }

                //验证已经通过,进行订单保存
                int pCount = 0;
                string oidList = "";
                foreach (StoreCartInfo storeCartInfo in storeCartList)
                {
                    if (storeCartInfo.SelectedOrderProductList.Count < 1)
                    {
                        continue;
                    }
                    List<SinglePromotionInfo> storeSinglePromotionList = singlePromotionList.FindAll(x => x.StoreId == storeCartInfo.StoreInfo.StoreId);
                    List<CouponInfo> storeCouponList = couponList.FindAll(x => x.StoreId == storeCartInfo.StoreInfo.StoreId);
                    int storeFullCut = Carts.SumFullCut(storeCartInfo.CartItemList);
                    string ipstr = ip == null ? "" : ip;
                    OrderInfo orderInfo = Orders.CreateOrder(puinfo, storeCartInfo.StoreInfo, storeCartInfo.SelectedOrderProductList, storeSinglePromotionList, fullShipAddressInfo, payPluginInfo, ref payCreditCount, storeCouponList, storeFullCut, buyerRemark, TypeHelper.StringToDateTime(bestTime, new DateTime(1970, 1, 1)), ipstr,shipmode);
                    if (orderInfo != null)
                    {
                        oidList += orderInfo.Oid + ",";
                        //删除剩余的满赠赠品
                        if (storeCartInfo.RemainedOrderProductList.Count > 0)
                        {
                            List<OrderProductInfo> delOrderProductList = Carts.GetFullSendMinorOrderProductList(storeCartInfo.RemainedOrderProductList);
                            if (delOrderProductList.Count > 0)
                            {
                                Carts.DeleteOrderProductList(delOrderProductList);
                                pCount += Carts.SumOrderProductCount(storeCartInfo.RemainedOrderProductList) - delOrderProductList.Count;
                            }
                        }
                        //创建订单处理
                        OrderActions.CreateOrderAction(new OrderActionInfo()
                        {
                            Oid = orderInfo.Oid,
                            Uid = uid,
                            RealName = "本人",
                            ActionType = (int)OrderActionType.Submit,
                            ActionTime = DateTime.Now,
                            ActionDes = orderInfo.OrderState == (int)OrderState.WaitPaying ? "您提交了订单，等待您付款" : "您提交了订单，请等待系统确认"
                        });
                        result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(orderInfo, JsonTimeFormat()) + "}";
                    }
                    else
                    {
                        result = "{\"result\":false,\"data\":[{\"msg\":\"提交失败，请联系管理员！\"}]}";
                        SendData(result);
                    }
                }
                Carts.SetCartProductCountCookie(pCount);

            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="saId">配送地址id </param>
        /// <param name="selectedCartItemKeyList">选中的购物车项键列表  为商品类型_商品id 例如："0_99,0_109,1_87"类型(0代表项为购物车商品,1代表项为购物车套装,2代表项为购物车满赠,3代表项为购物车满减)</param>
        /// <param name="payName">支付方式名称</param>
        /// <param name="buyerRemark">买家备注</param>
        /// <param name="payCreditCount">支付积分</param>
        /// <param name="couponIdList">客户已经激活的优惠劵</param>
        /// <param name="couponSNList">客户还未激活的优惠劵</param>
        /// <param name="fullCut">满减金额</param>
        /// <param name="bestTime">最佳配送时间</param>
        /// <param name="ip">ip地址</param>
        [WebMethod(Description = "生成订单")]
        public void OrderCreate(int uid, int saId, string selectedCartItemKeyList, string payName, string buyerRemark, string paycreditcount, string couponidlist, string couponsnlist, string fullcut, string bestTime, string ip)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"生成订单失败！\"}]}";
            try
            {
                //验证用户
                PartUserInfo puinfo = Users.GetPartUserById(uid);
                if (puinfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"用户信息错误，请重新登录！\"}]}";
                    SendData(result);
                }

                //验证买家备注的内容长度
                if (StringHelper.GetStringLength(buyerRemark) > 125)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"备注最多填写125个字！\"}]}";
                    SendData(result);
                }
                //验证支付方式是否为空
                PluginInfo payPluginInfo = Plugins.GetPayPluginBySystemName(payName);
                if (payPluginInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择支付方式！\"}]}";
                    SendData(result);
                }
                //验证配送地址是否为空
                FullShipAddressInfo fullShipAddressInfo = ShipAddresses.GetFullShipAddressBySAId(saId, uid);
                if (fullShipAddressInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择配送地址！\"}]}";
                    SendData(result);
                }

                //购物车商品列表
                List<OrderProductInfo> orderProductList = Carts.GetCartProductList(uid);
                if (orderProductList.Count < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"购物车中没有商品！\"}]}";
                    SendData(result);
                }

                //店铺购物车列表
                List<StoreCartInfo> storeCartList = Carts.TidyMallOrderProductList(StringHelper.SplitString(selectedCartItemKeyList), orderProductList);
                if (Carts.SumMallCartOrderProductCount(storeCartList) < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"购物车中没有选中的商品！\"}]}";
                    SendData(result);
                }

                #region 验证支付积分
                int payCreditCount = TypeHelper.StringToInt(paycreditcount);

                #endregion

                #region 验证优惠劵
                List<CouponInfo> couponList = new List<CouponInfo>();
                #endregion

                string pidList = Carts.GetMallCartPidList(storeCartList);//商品id列表
                List<ProductStockInfo> productStockList = Products.GetProductStockList(pidList);//商品库存列表
                List<SinglePromotionInfo> singlePromotionList = new List<SinglePromotionInfo>();//单品促销活动列表
                //循环店铺购物车列表
                foreach (StoreCartInfo storeCartInfo in storeCartList)
                {
                    //循环购物车项列表，依次验证
                    foreach (CartItemInfo cartItemInfo in storeCartInfo.CartItemList)
                    {
                        var cartItem = (CartProductInfo)cartItemInfo.Item;
                        if (!cartItem.Selected)
                        {
                            continue;
                        }
                        #region 购物车商品类型
                        if (cartItemInfo.Type == 0)
                        {
                            CartProductInfo cartProductInfo = cartItem;

                            OrderProductInfo orderProductInfo = cartProductInfo.OrderProductInfo;

                            //验证商品信息
                            PartProductInfo partProductInfo = Products.GetPartProductById(orderProductInfo.Pid);
                            if (partProductInfo == null)
                            {
                                result = "{\"result\":false,\"data\":[{\"msg\":\"商品" + partProductInfo.Name + "已经下架，请删除此商品！\"}]}";
                                SendData(result);
                            }
                            if (orderProductInfo.Name != partProductInfo.Name || orderProductInfo.ShopPrice != partProductInfo.ShopPrice || orderProductInfo.MarketPrice != partProductInfo.MarketPrice || orderProductInfo.CostPrice != partProductInfo.CostPrice || orderProductInfo.Weight != partProductInfo.Weight || orderProductInfo.PSN != partProductInfo.PSN)
                            {
                                result = "{\"result\":false,\"data\":[{\"msg\":\"商品" + partProductInfo.Name + "信息有变化，请删除后重新添加！\"}]}";
                                SendData(result);
                            }

                            //验证商品库存
                            ProductStockInfo productStockInfo = Products.GetProductStock(orderProductInfo.Pid, productStockList);
                            if (productStockInfo.Number < orderProductInfo.RealCount)
                            {
                                result = "{\"result\":false,\"data\":[{\"msg\":\"商品" + partProductInfo.Name + "库存不足！\"}]}";
                                SendData(result);
                            }
                            else
                            {
                                productStockInfo.Number -= orderProductInfo.RealCount;
                            }

                        }
                        #endregion

                    }
                }

                if (Carts.SumMallCartFullCut(storeCartList) != TypeHelper.StringToInt(fullcut))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"满减金额不正确,请刷新页面重新提交！\"}]}";
                    SendData(result);
                }

                //验证已经通过,进行订单保存
                int pCount = 0;
                string oidList = "";
                foreach (StoreCartInfo storeCartInfo in storeCartList)
                {
                    if (storeCartInfo.SelectedOrderProductList.Count < 1)
                    {
                        continue;
                    }
                    List<SinglePromotionInfo> storeSinglePromotionList = singlePromotionList.FindAll(x => x.StoreId == storeCartInfo.StoreInfo.StoreId);
                    List<CouponInfo> storeCouponList = couponList.FindAll(x => x.StoreId == storeCartInfo.StoreInfo.StoreId);
                    int storeFullCut = Carts.SumFullCut(storeCartInfo.CartItemList);
                    string ipstr = ip == null ? "" : ip;
                    OrderInfo orderInfo = Orders.CreateOrder(puinfo, storeCartInfo.StoreInfo, storeCartInfo.SelectedOrderProductList, storeSinglePromotionList, fullShipAddressInfo, payPluginInfo, ref payCreditCount, storeCouponList, storeFullCut, buyerRemark, TypeHelper.StringToDateTime(bestTime, new DateTime(1970, 1, 1)), ipstr, (int)ShipMode.Express);
                    if (orderInfo != null)
                    {
                        oidList += orderInfo.Oid + ",";
                        //删除剩余的满赠赠品
                        if (storeCartInfo.RemainedOrderProductList.Count > 0)
                        {
                            List<OrderProductInfo> delOrderProductList = Carts.GetFullSendMinorOrderProductList(storeCartInfo.RemainedOrderProductList);
                            if (delOrderProductList.Count > 0)
                            {
                                Carts.DeleteOrderProductList(delOrderProductList);
                                pCount += Carts.SumOrderProductCount(storeCartInfo.RemainedOrderProductList) - delOrderProductList.Count;
                            }
                        }
                        //创建订单处理
                        OrderActions.CreateOrderAction(new OrderActionInfo()
                        {
                            Oid = orderInfo.Oid,
                            Uid = uid,
                            RealName = "本人",
                            ActionType = (int)OrderActionType.Submit,
                            ActionTime = DateTime.Now,
                            ActionDes = orderInfo.OrderState == (int)OrderState.WaitPaying ? "您提交了订单，等待您付款" : "您提交了订单，请等待系统确认"
                        });
                        result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(orderInfo, JsonTimeFormat()) + "}";
                    }
                    else
                    {
                        result = "{\"result\":false,\"data\":[{\"msg\":\"提交失败，请联系管理员！\"}]}";
                        SendData(result);
                    }
                }
                Carts.SetCartProductCountCookie(pCount);

            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 获取运费及商品总价
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="saId">配送地址id</param>
        /// <param name="selectedCartItemKeyList">选中的购物车项键列表  为商品类型_商品id 例如："0_99,0_109,1_87"类型(0代表项为购物车商品,1代表项为购物车套装,2代表项为购物车满赠,3代表项为购物车满减)</param>
        /// <param name="shipmode">配送方式</param>
        [WebMethod(Description = "获取运费及商品总价")]
        public void GetShipFreeAmount(int uid, int saId, string selectedCartItemKeyList, int shipmode)
        {
             string result = "{\"result\":false,\"data\":[{\"msg\":\"获取运费失败！\"}]}";
             try
             {
                 //验证用户
                 PartUserInfo puinfo = Users.GetPartUserById(uid);
                 if (puinfo == null)
                 {
                     result = "{\"result\":false,\"data\":[{\"msg\":\"用户信息错误！\"}]}";
                     SendData(result);
                 }
                 //验证配送地址是否为空
                 FullShipAddressInfo fullShipAddressInfo = ShipAddresses.GetFullShipAddressBySAId(saId, uid);
                    //购物车商品列表
                List<OrderProductInfo> orderProductList = Carts.GetCartProductList(uid);
                if (orderProductList.Count < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"购物车中没有商品！\"}]}";
                    SendData(result);
                }
                  //店铺购物车列表
                List<StoreCartInfo> storeCartList = Carts.TidyMallOrderProductList(StringHelper.SplitString(selectedCartItemKeyList), orderProductList);
                if (Carts.SumMallCartOrderProductCount(storeCartList) < 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"购物车中没有选中的商品！\"}]}";
                    SendData(result);
                }
                int provinceid = 0;
                int cityid = 0;
                if (fullShipAddressInfo != null)
                {
                    provinceid = fullShipAddressInfo.ProvinceId;
                    cityid = fullShipAddressInfo.CityId;
                }
                ShipFreeAmountModel model = new ShipFreeAmountModel();
                foreach (StoreCartInfo item in storeCartList)
                {
                    decimal productamount= Carts.SumOrderProductAmount(item.SelectedOrderProductList, uid);
                    model.ProductAmount += productamount;
                    model.ShipFree += Orders.GetShipFee(provinceid, cityid, item.SelectedOrderProductList, productamount, shipmode);
                }

                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
             }
             catch (Exception)
             {

             }
             SendData(result);
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="oid">订单id</param>
        [WebMethod(Description = "订单确认收货")]
        public void OrderReceive(int uid, int oid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"确认收货失败！\"}]}";
            try
            {
                //检查用户
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }

                //验证订单
                OrderInfo orderInfo = Orders.GetOrderByOid(oid);
                if (orderInfo == null || orderInfo.Uid != uid)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该订单不存在！\"}]}";
                    SendData(result);
                }

                //不是等待付款或者货到付款订单确认中状态时，不能取消订单
                if (orderInfo.OrderState != (int)OrderState.Sended || orderInfo.PayMode != 1)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该订单现在还不允许做确认收货操作！\"}]}";
                    SendData(result);
                }

                //确认收货
                Orders.ReceiveOrder(oid);
                //创建订单处理
                OrderActions.CreateOrderAction(new OrderActionInfo()
                {
                    Oid = oid,
                    Uid = uid,
                    RealName = "本人",
                    ActionType = (int)OrderActionType.Receive,
                    ActionTime = DateTime.Now,
                    ActionDes = "您确认了已收货"
                });
                result = "{\"result\":true,\"data\":[{\"msg\":\"订单确认收货成功！\"}]}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 获取支付插件列表
        /// </summary>
         [WebMethod(Description = "获取支付插件列表")]
        public void GetPayPluginList(int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取支付插件列表失败！\"}]}";
            try
            {
                PayPluginListModel model = new PayPluginListModel();
                model.pluginlist = Plugins.GetPayPluginList();
                var user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    model.UserAmount = 0;
                }
                else
                {
                    model.UserAmount = user.UserAmount;
                }
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
         /// 余额支付
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="uid"></param>
        /// <param name="paypassword"></param>
         [WebMethod(Description = "余额支付")]
         public void CreditPayOrder(string oidList, int uid, string psw)
         {
             string result = "{\"result\":false,\"data\":[{\"msg\":\"余额支付失败！\"}]}";
             try
             {
                 //检查用户
                 PartUserInfo user = Users.GetPartUserById(uid);
                 if (user == null)
                 {
                     result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                     SendData(result);
                 }
                 if (string.IsNullOrWhiteSpace(oidList))
                 {
                     result = "{\"result\":false,\"data\":[{\"msg\":\"订单信息错误！\"}]}";
                     SendData(result);
                 }
                 //支付密码
                 UserPayPassword paypasswordinfo = Users.GetUserPayPasswordByUid(uid);
                 if (paypasswordinfo == null)
                 {
                     result = "{\"result\":false,\"data\":[{\"msg\":\"您还没有设置支付密码，请在个人中心中设置支付密码！\"}]}";
                     SendData(result);
                 }
                 if (string.IsNullOrWhiteSpace(psw))
                 {
                     result = "{\"result\":false,\"data\":[{\"msg\":\"请输入支付密码！\"}]}";
                     SendData(result);
                 }
                 //判断密码是否正确
                 if (Users.CreateUserPassword(psw, paypasswordinfo.Salt) != paypasswordinfo.Password)
                 {
                     result = "{\"result\":false,\"data\":[{\"msg\":\"支付密码错误！\"}]}";
                     SendData(result);
                 }

                 //验证订单
                 List<OrderInfo> orderList = new List<OrderInfo>();
                 decimal allPayMoney = 0;
                 string osns = "";
                 foreach (string oid in StringHelper.SplitString(oidList))
                 {
                     //订单信息
                     OrderInfo orderInfo = Orders.GetOrderByOid(TypeHelper.StringToInt(oid));
                     if (orderInfo == null || orderInfo.Uid != uid || orderInfo.OrderState != (int)OrderState.WaitPaying)
                     {
                         result = "{\"result\":false,\"data\":[{\"msg\":\"订单信息错误！\"}]}";
                         SendData(result);
                     }
                     orderList.Add(orderInfo);
                     allPayMoney += orderInfo.SurplusMoney;
                     osns += orderInfo.OSN + ",";
                 }
                 if (allPayMoney > user.UserAmount)
                 {
                     result = "{\"result\":false,\"data\":[{\"msg\":\"余额不足，无法支付！\"}]}";
                     SendData(result);
                 }
                 //余额支付记录
                 CreditLogInfo loginfo = new CreditLogInfo()
                 {
                     Uid = uid,
                     RankCredits = 0,
                     Action = (int)CreditAction.UserOrderCreditPay,
                     ActionCode = 0,
                     ActionDes = "您使用余额支付订单：" + allPayMoney + "元，订单号为：" + osns.TrimEnd(','),
                     Operator = uid,
                     FrozenAmount = 0,
                     UserAmount = -allPayMoney,
                     ActionTime = DateTime.Now
                 };
                 Credits.SendCredits(0, loginfo);
                 foreach (var order in orderList)
                 {
                     if (order.SurplusMoney > 0 && order.OrderState == (int)OrderState.WaitPaying)
                     {
                         PluginInfo plugininfo = Plugins.GetPayPluginBySystemName("creditpay");
                         Orders.PayOrder(order.Oid, OrderState.Confirming, "", DateTime.Now, 1, plugininfo.SystemName, plugininfo.FriendlyName);
                         OrderActions.CreateOrderAction(new OrderActionInfo()
                         {
                             Oid = order.Oid,
                             Uid = order.Uid,
                             RealName = "本人",
                             ActionType = (int)OrderActionType.Pay,
                             ActionTime = loginfo.ActionTime,
                             ActionDes = "你使用余额支付订单成功"
                         });

                         //队员抽水
                         Orders.OrderPayCommission(order);
                     }
                 }
                 result = "{\"result\":true,\"data\":[{\"msg\":\"余额支付成功！\"}]}";
             }
             catch (Exception)
             {
             }
             SendData(result);
         }

        #endregion

        #region 用户信息

        /// <summary>
        /// 我的收货地址
        /// </summary>
        /// <param name="uid">用户id</param>
        [WebMethod(Description = "我的收货地址")]
        public void MyShipAddressList(int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"添加/修改收货地址失败！\"}]}";
            try
            {
                //检查用户
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }

                ShipAddressListModel model = new ShipAddressListModel();
                model.ShipAddressList = ShipAddresses.GetFullShipAddressList(uid);
                model.ShipAddressCount = model.ShipAddressList.Count;
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {

            }
            SendData(result);

        }

        /// <summary>
        /// 收货地址详情
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="saId">地址id</param>
        [WebMethod(Description = "收货地址详情")]
        public void MyShipAddressDetail(int uid, int saId)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取收货地址详情失败！\"}]}";
            try
            {
                //检查用户
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }
                ShipAddressInfo addressInfo = ShipAddresses.GetShipAddressBySAId(saId, uid);
                if (addressInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"收货地址不存在！\"}]}";
                    SendData(result);
                }
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(addressInfo, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 添加/编辑收货地址
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="aid">地址id（小于1说明是新添加）</param>
        /// <param name="name">收货人姓名</param>
        /// <param name="regionId">地区Id</param>
        /// <param name="address">收货地址</param>
        /// <param name="mobile">手机号</param>
        /// <param name="phone">固话号</param>
        /// <param name="zipcode">邮编</param>
        /// <param name="email">邮箱</param>
        /// <param name="isDefault">是否为默认地址（1：表示默认）</param>
        [WebMethod(Description = "添加/编辑收货地址")]
        public void EditShipAddress(int uid, string aid, string name, int regionId, string address, string mobile, string phone, string zipcode, string email, string isDefault)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"添加/修改收货地址失败！\"}]}";
            try
            {
                //检查区域
                RegionInfo regionInfo = Regions.GetRegionById(regionId);
                if (regionInfo == null || regionInfo.Layer != 3)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择有效的区域！\"}]}";
                    SendData(result);
                }

                //检查收货人姓名
                if (string.IsNullOrWhiteSpace(name) || name.Length > 25)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"收货人姓名不能为空且最多只能输入25个字！\"}]}";
                    SendData(result);
                }

                //检查手机号
                if (string.IsNullOrWhiteSpace(mobile) || !ValidateHelper.IsMobile(mobile))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请正确填写手机号码！\"}]}";
                    SendData(result);
                }

                //检查固话
                if (!string.IsNullOrWhiteSpace(phone) && !ValidateHelper.IsPhone(phone))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"固定电话格式不正确！\"}]}";
                    SendData(result);
                }

                //检查邮箱
                if (!string.IsNullOrWhiteSpace(email) && !ValidateHelper.IsEmail(email))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"邮箱格式不正确！\"}]}";
                    SendData(result);
                }

                //检查邮编
                if (!string.IsNullOrWhiteSpace(email) && !ValidateHelper.IsZipCode(zipcode))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"邮编格式不正确！\"}]}";
                    SendData(result);
                }

                //检查详细地址
                if (string.IsNullOrWhiteSpace(address) || address.Length > 75)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"详细地址不能为空且最多只能输入75个字！\"}]}";
                    SendData(result);
                }

                //检查用户
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }

                int addressInfoId = 0;
                int.TryParse(aid, out addressInfoId);
                int isdefault = 0;
                int.TryParse(isDefault, out isdefault);

                ShipAddressInfo addressInfo = ShipAddresses.GetShipAddressBySAId(addressInfoId, uid);
                if (addressInfo == null)
                {
                    //检查配送地址数量是否达到系统所允许的最大值
                    int shipAddressCount = ShipAddresses.GetShipAddressCount(uid);
                    if (shipAddressCount >= baseWebController.WorkContext.MallConfig.MaxShipAddress)
                    {
                        result = "{\"result\":false,\"data\":[{\"msg\":\"收货地址的数量已经达到系统所允许的最大值！\"}]}";
                        SendData(result);
                    }
                    addressInfo = new ShipAddressInfo();
                }
                addressInfo.Uid = uid;
                addressInfo.RegionId = regionId;
                addressInfo.IsDefault = isdefault == 0 ? 0 : 1;
                addressInfo.Alias = WebHelper.HtmlEncode(name);
                addressInfo.Consignee = WebHelper.HtmlEncode(name);
                addressInfo.Mobile = mobile;
                addressInfo.Phone = phone;
                addressInfo.Email = email;
                addressInfo.ZipCode = zipcode;
                addressInfo.Address = WebHelper.HtmlEncode(address);
                if (addressInfo.SAId < 1)
                {
                    ShipAddresses.CreateShipAddress(addressInfo);
                    result = "{\"result\":true,\"data\":[{\"msg\":\"添加收货地址成功！\"}]}";
                }
                else
                {
                    ShipAddresses.UpdateShipAddress(addressInfo);
                    result = "{\"result\":true,\"data\":[{\"msg\":\"修改收货地址成功！\"}]}";
                }
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 删除收货地址
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="saId">地址id</param>
        [WebMethod(Description = "删除收货地址")]
        public void DeleteShipAddress(int uid, int saId)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"删除收货地址失败！\"}]}";
            try
            {
                //检查用户
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }

                //检查收货地址
                ShipAddressInfo addressInfo = ShipAddresses.GetShipAddressBySAId(saId, uid);
                if (addressInfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"收货地址不存在！\"}]}";
                    SendData(result);
                }

                //删除收货地址
                ShipAddresses.DeleteShipAddress(saId, uid);
                result = "{\"result\":true,\"data\":[{\"msg\":\"删除收货地址成功！\"}]}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 获取用户等级
        /// </summary>
        /// <param name="uid">用户id</param>
        [WebMethod(Description = "获取用户等级")]
        public void GetUserRank(int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取用户等级失败！\"}]}";
            try
            {
                //检查用户
                PartUserInfo user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }

                //检查用户等级
                UserRankInfo rankinfo = UserRanks.GetUserRankById(user.UserRid);
                if (rankinfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"等级信息错误！\"}]}";
                    SendData(result);
                }
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(rankinfo) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="uid">用户id</param>
        [WebMethod(Description = "获取用户信息")]
        public void GetUserById(int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取用户信息失败！\"}]}";
            try
            {
                //检查用户
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }
                UserInfoAppModel model = new UserInfoAppModel();
                model.UserName = user.UserName;
                model.RankTitle = UserRanks.GetUserRankByCredits(user.RankCredits).Title;
                model.NickName = user.NickName;
                model.RealName = user.RealName;
                model.Gender = user.Gender;
                model.Avatar = user.Avatar;
                model.IdCard = user.IdCard;
                model.BirthDay = user.Bday.ToString();
                model.RegionId = user.RegionId;
                model.Bio = user.Bio;
                model.Address = user.Address;
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 获取用户支付密码
        /// </summary>
        /// <param name="uid">用户id</param>
        [WebMethod(Description = "获取用户支付密码")]
        public void GetUserPayPassword(int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取用户支付密码失败！\"}]}";
            try
            {
                //检查用户
                UserInfo user = Users.GetUserById(uid);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在，请重新登录！\"}]}";
                    SendData(result);
                }
                var model = Users.GetUserPayPasswordByUid(uid);
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="nickName">昵称</param>
        /// <param name="realName">真实姓名</param>
        /// <param name="gender">性别：0未知，1男，2女</param>
        /// <param name="avatar">头像</param>
        /// <param name="idCard">身份证</param>
        /// <param name="birthDay">出生日期</param>
        /// <param name="regionId">地址区域</param>
        /// <param name="bio">个性签名</param>
        /// <param name="address">地址</param>
        [WebMethod(Description = "修改用户信息")]
        public void UserEdit(int uid, string nickName, string realName, string gender, string idCard, string birthDay, string regionId, string bio, string address)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"修改用户信息失败！\"}]}";
            try
            {
                PartUserInfo userinfo = Users.GetUserById(uid);
                if (userinfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                if (string.IsNullOrWhiteSpace(idCard))
                {
                    idCard = "";
                }
                if (string.IsNullOrWhiteSpace(realName))
                {
                    realName = "";
                }
                if (string.IsNullOrWhiteSpace(bio))
                {
                    bio = "";
                }
                if (string.IsNullOrWhiteSpace(address))
                {
                    address = "";
                }

                //验证昵称
                if (nickName.Length > 10 || nickName.Length < 2)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"昵称的长度不能大于10小于2！\"}]}";
                    SendData(result);
                }
                else if (FilterWords.IsContainWords(nickName))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"昵称中包含禁止单词！\"}]}";
                    SendData(result);
                }

                //验证真实姓名
                if (realName.Length > 5)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"真实姓名的长度不能大于5！\"}]}";
                    SendData(result);
                }

                //验证性别
                int genderid = TypeHelper.StringToInt(gender);
                if (genderid < 0 || genderid > 2)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的性别！\"}]}";
                    SendData(result);
                }

                //验证区域
                int regionid = TypeHelper.StringToInt(regionId);
                if (regionid > 0)
                {
                    RegionInfo regionInfo = Regions.GetRegionById(regionid);
                    if (regionInfo == null || regionInfo.Layer != 3)
                    {
                        result = "{\"result\":false,\"data\":[{\"msg\":\"请选择正确的区域地址！\"}]}";
                        SendData(result);
                    }
                }
                //验证详细地址
                if (address.Length > 75)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"详细地址的长度不能大于75！\"}]}";
                    SendData(result);
                }

                //验证简介
                if (bio.Length > 150)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"简介的长度不能大于150！\"}]}";
                    SendData(result);
                }

                string defaulttime = TypeHelper.StringToDateTime("1900-1-1").ToShortDateString();
                DateTime bday = TypeHelper.StringToDateTime(birthDay);
                Users.UpdateUser(uid, userinfo.UserName, WebHelper.HtmlEncode(nickName), userinfo.Avatar, genderid, WebHelper.HtmlEncode(realName), bday, idCard, regionid, WebHelper.HtmlEncode(address), WebHelper.HtmlEncode(bio));
                if (nickName.Length > 0 && userinfo.Avatar.Length > 0 && realName.Length > 0 && !bday.ToShortDateString().Equals(defaulttime) && idCard.Length > 0 && regionid > 0 && address.Length > 0)
                {
                    Credits.SendCompleteUserInfoCredits(ref userinfo, DateTime.Now);
                }
                result = "{\"result\":true,\"data\":[{\"msg\":\"更新用户信息成功！\"}]}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 更改用户头像 
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="avatar">用户头像</param>
        [WebMethod(Description = "更改用户头像")]
        public void UserAvatarEdit(int uid, string avatar)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"头像信息错误！\"}]}";
            try
            {
                PartUserInfo userinfo = Users.GetUserById(uid);
                if (userinfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                if (!SecureHelper.IsBase64String(avatar))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"头像信息错误11111！\"}]}";
                    SendData(result);
                }
                string filename = MallUtils.SaveUserAvatarByBase64(avatar);
                if (string.IsNullOrWhiteSpace(filename))
                {
                    SendData(result);
                }
                Users.UpdateUserAvatarByUid(uid, filename);
                result = "{\"result\":true,\"data\":[{\"msg\":\"更新用户信息成功！\"}]}";
            }

            catch
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 介绍人关系 
        /// </summary>
        /// <param name="uid">用户id</param>
        [WebMethod(Description = "介绍人关系")]
        public void GetIntroducers(int uid)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取介绍人关系失败！\"}]}";
            try
            {
                IntroduceShowModel model = new IntroduceShowModel();
                //推荐者
                IntroduceModel intrmodel = null;
                int introduceid = Users.GetMyIntroducer(uid);
                UserInfo user = Users.GetUserById(introduceid);
                if (user != null)
                {
                    intrmodel = new IntroduceModel();
                    intrmodel.NickName = user.NickName;
                    intrmodel.UserName = user.UserName;
                }
                model.introducer = intrmodel;
                model.introduceCount = 0;
                //推荐的会员列表
                List<IntroduceModel> intrmodellist = new List<IntroduceModel>();
                DataTable introducelist = Users.GetMyIntroducerList(uid);
                foreach (DataRow row in introducelist.Rows)
                {
                    IntroduceModel umodel = new IntroduceModel();
                    umodel.NickName = row["nickname"].ToString();
                    umodel.UserName = row["username"].ToString();
                    umodel.AddTime = TypeHelper.ObjectToDateTime(row["registertime"]).ToString("yyyy-MM-dd HH:mm:ss");
                    intrmodellist.Add(umodel);
                }
                model.introduceCount = intrmodellist.Count();
                model.MyIntroducers = intrmodellist;
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(model, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }
        /// <summary>
        /// 重新设置密码
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="confirmPwd">确认密码</param>
        [WebMethod(Description = "重新设置密码")]
        public void ReSetPassword(string userName, string password, string confirmPwd)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"重新设置密码失败！\"}]}";
            try
            {
                PartUserInfo user = Users.GetPartUserByName(userName);
                if (user == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"用户不存在！\"}]}";
                    SendData(result);
                }
                //验证
                if (string.IsNullOrWhiteSpace(password))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"密码不能为空！\"}]}";
                    SendData(result);
                }
                else if (password.Length < 4 || password.Length > 32)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"密码必须大于3且不大于32个字符！\"}]}";
                    SendData(result);
                }
                else if (password != confirmPwd)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"两次输入的密码不一样！\"}]}";
                    SendData(result);
                }
                //生成用户新密码
                string p = Users.CreateUserPassword(password, user.Salt);
                //设置用户新密码
                Users.UpdateUserPasswordByUid(user.Uid, p);
                result = "{\"result\":true,\"data\":[{\"msg\":\"重新设置密码成功！\"}]}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }

        /// <summary>
        /// 支付密码更新
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="password">支付密码</param>
        [WebMethod(Description = "支付密码更新")]
        public void UserPayPasswordEdit(int uid, string password, string confirmpwd)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"支付密码更新失败！\"}]}";
            try
            {
                PartUserInfo userinfo = Users.GetUserById(uid);
                if (userinfo == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该用户不存在！\"}]}";
                    SendData(result);
                }
                if (string.IsNullOrEmpty(password) || password.Length < 6)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"支付密码长度必须大于5小于32！\"}]}";
                    SendData(result);
                }
                if (!password.Equals(confirmpwd))
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"两次密码不一致！\"}]}";
                    SendData(result);
                }
                var upaypasswordinfo = Users.GetUserPayPasswordByUid(uid);
                //创建支付密码
                if (upaypasswordinfo == null)
                {
                    string salt = Randoms.CreateRandomValue(6);
                    string p = Users.CreateUserPassword(password, salt);
                    Users.CreatePayPassword(uid, p, salt);
                }
                //修改密码
                else
                {
                    string p = Users.CreateUserPassword(password, upaypasswordinfo.Salt);
                    Users.UpdatePayPasswordByUid(uid, p);
                }
                result = "{\"result\":true,\"data\":[{\"msg\":\"更新用户支付密码成功！\"}]}";
            }

            catch
            {
            }
            SendData(result);
        }
        #endregion

        #region 广告

        /// <summary>
        /// 根据广告位Id获取广告列表
        /// </summary>
        /// <param name="adPosId"></param>
        [WebMethod(Description = "根据广告位Id获取广告列表")]
        public void GetAdvertList(int adPosId)
        {
            string result = "{\"result\":false,\"data\":[{\"msg\":\"获取广告列表失败！\"}]}";
            try
            {
                var adPos = Adverts.GetAdvertPositionById(adPosId);
                if (adPos == null)
                {
                    result = "{\"result\":false,\"data\":[{\"msg\":\"该广告位不存在！\"}]}";
                }
                var list = Adverts.GetAdvertList(adPosId);
                result = "{\"result\":true,\"data\":" + JsonConvert.SerializeObject(list, JsonTimeFormat()) + "}";
            }
            catch (Exception)
            {
            }
            SendData(result);
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        private void SendData(string data)
        {
            Context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            Context.Response.Write(data);
            Context.Response.End();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        private void SendDataTest(string data)
        {
            Context.Response.ContentEncoding = System.Text.Encoding.Default;
            Context.Response.Write(data);
            Context.Response.End();
        }

        /// <summary>
        /// json时间转换
        /// </summary>
        /// <returns></returns>
        private IsoDateTimeConverter JsonTimeFormat()
        {
            var iso = new IsoDateTimeConverter();
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            return iso;
        }
        #endregion

    }
}
