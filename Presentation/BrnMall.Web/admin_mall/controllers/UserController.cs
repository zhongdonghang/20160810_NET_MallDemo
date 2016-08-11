using System;
using System.Web;
using System.Data;
using System.Web.Mvc;
using System.Collections.Generic;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.MallAdmin.Models;
using BrnMall.Web.MallAdmin.Models;

namespace BrnMall.Web.MallAdmin.Controllers
{
    /// <summary>
    /// 商城后台用户控制器类
    /// </summary>
    public partial class UserController : BaseMallAdminController
    {
        /// <summary>
        /// 用户列表
        /// </summary>
        public ActionResult List(string userName, string email, string mobile, int userRid = 0, int mallAGid = 0,
                                 int pageNumber = 1, int pageSize = 15)
        {
            string condition = AdminUsers.AdminGetUserListCondition(userName, email, mobile, userRid, mallAGid);
            string sort = AdminUsers.AdminGetUserListSort("", "");

            PageModel pageModel = new PageModel(pageSize, pageNumber, AdminUsers.AdminGetUserCount(condition));

            UserListModel model = new UserListModel()
            {
                UserList = AdminUsers.AdminGetUserList(pageModel.PageSize, pageModel.PageNumber, condition, sort),
                PageModel = pageModel,
                UserName = userName,
                Email = email,
                Mobile = mobile,
                UserRid = userRid,
                MallAGid = mallAGid
            };
            List<SelectListItem> userRankList = new List<SelectListItem>();
            userRankList.Add(new SelectListItem() { Text = "全部等级", Value = "0" });
            foreach (UserRankInfo info in AdminUserRanks.GetUserRankList())
            {
                userRankList.Add(new SelectListItem() { Text = info.Title, Value = info.UserRid.ToString() });
            }
            ViewData["userRankList"] = userRankList;

            List<SelectListItem> mallAdminGroupList = new List<SelectListItem>();
            mallAdminGroupList.Add(new SelectListItem() { Text = "全部组", Value = "0" });
            foreach (MallAdminGroupInfo info in MallAdminGroups.GetMallAdminGroupList())
            {
                mallAdminGroupList.Add(new SelectListItem() { Text = info.Title, Value = info.MallAGid.ToString() });
            }
            ViewData["mallAdminGroupList"] = mallAdminGroupList;

            MallUtils.SetAdminRefererCookie(string.Format("{0}?pageNumber={1}&pageSize={2}&userName={3}&email={4}&mobile={5}&userRid={6}&mallAGid={7}",
                                                          Url.Action("list"), pageModel.PageNumber, pageModel.PageSize,
                                                          userName, email, mobile, userRid, mallAGid));
            return View(model);
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        [HttpGet]
        public ActionResult Add()
        {
            UserModel model = new UserModel();
            Load(model.RegionId);
            return View(model);
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        [HttpPost]
        public ActionResult Add(UserModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "该密码不能为空");
            }

            if (AdminUsers.IsExistUserName(model.UserName))
            {
                ModelState.AddModelError("UserName", "该手机号已经存在");
            }
            //积分和等级不符合
            var userrank = AdminUserRanks.GetUserRankById(model.UserRid);
            if (model.RankCredits < userrank.CreditsLower)
            {
                ModelState.AddModelError("UserRid", "等级和等级积分不符合，请修改");
            }
            if (userrank.CreditsUpper != -1 && model.RankCredits >= userrank.CreditsUpper)
            {
                ModelState.AddModelError("UserRid", "等级和等级积分不符合，请修改");
            }
            if (ModelState.IsValid)
            {
                string salt = Users.GenerateUserSalt();
                string nickName;
                if (string.IsNullOrWhiteSpace(model.NickName))
                    nickName = "jsy" + Randoms.CreateRandomValue(7);
                else
                    nickName = model.NickName;

                UserInfo userInfo = new UserInfo()
                {
                    UserName = model.UserName,
                    Email = model.Email == null ? "" : model.Email,
                    Mobile = model.UserName,
                    Salt = salt,
                    Password = Users.CreateUserPassword(model.Password, salt),
                    UserRid = model.UserRid,
                    StoreId = 0,
                    MallAGid = model.MallAGid,
                    NickName = WebHelper.HtmlEncode(nickName),
                    Avatar = model.Avatar == null ? "" : WebHelper.HtmlEncode(model.Avatar),
                    UserAmount = model.UserAmount,
                    FrozenAmount=model.FrozenAmount,
                    RankCredits = model.RankCredits,
                    VerifyEmail = 1,
                    VerifyMobile = 1,
                    LiftBanTime = UserRanks.IsBanUserRank(model.UserRid) ? DateTime.Now.AddDays(WorkContext.UserRankInfo.LimitDays) : new DateTime(1900, 1, 1),
                    LastVisitTime = DateTime.Now,
                    LastVisitIP = WorkContext.IP,
                    LastVisitRgId = WorkContext.RegionId,
                    RegisterTime = DateTime.Now,
                    RegisterIP = WorkContext.IP,
                    RegisterRgId = WorkContext.RegionId,
                    Gender = model.Gender,
                    RealName = model.RealName == null ? "" : WebHelper.HtmlEncode(model.RealName),
                    Bday = model.Bday ?? new DateTime(1970, 1, 1),
                    IdCard = model.IdCard == null ? "" : model.IdCard,
                    RegionId = model.RegionId,
                    Address = model.Address == null ? "" : WebHelper.HtmlEncode(model.Address),
                    Bio = model.Bio == null ? "" : WebHelper.HtmlEncode(model.Bio)
                };

                AdminUsers.CreateUser(userInfo);
                AddMallAdminLog("添加用户", "添加用户,用户为:" + model.UserName);
                return PromptView("用户添加成功");
            }
            Load(model.RegionId);

            return View(model);
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        [HttpGet]
        public ActionResult Edit(int uid = -1)
        {
            UserInfo userInfo = AdminUsers.GetUserById(uid);
            if (userInfo == null)
                return PromptView("用户不存在");

            UserModel model = new UserModel();
            model.UserName = userInfo.UserName;
            model.Email = userInfo.Email;
            model.Mobile = userInfo.Mobile;
            model.UserRid = userInfo.UserRid;
            model.MallAGid = userInfo.MallAGid;
            model.NickName = userInfo.NickName;
            model.Avatar = userInfo.Avatar;
            model.UserAmount = userInfo.UserAmount;
            model.FrozenAmount = userInfo.FrozenAmount;
            model.RankCredits = userInfo.RankCredits;
            model.Gender = userInfo.Gender;
            model.RealName = userInfo.RealName;
            model.Bday = userInfo.Bday;
            model.IdCard = userInfo.IdCard;
            model.RegionId = userInfo.RegionId;
            model.Address = userInfo.Address;
            model.Bio = userInfo.Bio;
            
            Load(model.RegionId);
            ViewData["introduce"] = GetUserIntroduce(uid);
            return View(model);
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        [HttpPost]
        public ActionResult Edit(UserModel model, int uid = -1)
        {
            UserInfo userInfo = AdminUsers.GetUserById(uid);
            if (userInfo == null)
            {
                return PromptView("用户不存在");
            }

            int uid2 = AdminUsers.GetUidByUserName(model.UserName);
            if (uid2 > 0 && uid2 != uid)
            {
                ModelState.AddModelError("UserName", "该用户名已经存在");
            }
            //积分和等级不符合
            var userrank = AdminUserRanks.GetUserRankById(model.UserRid);
            if (userrank.System==0 && model.RankCredits < userrank.CreditsLower)
            {
                ModelState.AddModelError("UserRid", "等级和等级积分不符合，请修改");
            }
            if (userrank.System == 0 && userrank.CreditsUpper != -1 && model.RankCredits >= userrank.CreditsUpper)
            {
                ModelState.AddModelError("UserRid", "等级和等级积分不符合，请修改");
            }
            if (ModelState.IsValid)
            {
                string nickName;
                if (string.IsNullOrWhiteSpace(model.NickName))
                {
                    nickName = userInfo.NickName;
                }
                else
                {
                    nickName = model.NickName;
                }
                userInfo.UserName = model.UserName;
                userInfo.Email = model.Email == null ? "" : model.Email;
                userInfo.Mobile = model.UserName;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    userInfo.Password = Users.CreateUserPassword(model.Password, userInfo.Salt);
                }
                userInfo.UserRid = model.UserRid;
                userInfo.MallAGid = model.MallAGid;
                userInfo.NickName = WebHelper.HtmlEncode(nickName);
                userInfo.Avatar = model.Avatar == null ? "" : WebHelper.HtmlEncode(model.Avatar);
                userInfo.UserAmount = model.UserAmount;
                userInfo.FrozenAmount=model.FrozenAmount;
                userInfo.RankCredits = model.RankCredits;
                userInfo.LiftBanTime = UserRanks.IsBanUserRank(model.UserRid) ? DateTime.Now.AddDays(WorkContext.UserRankInfo.LimitDays) : new DateTime(1900, 1, 1);
                userInfo.Gender = model.Gender;
                userInfo.RealName = model.RealName == null ? "" : WebHelper.HtmlEncode(model.RealName);
                userInfo.Bday = model.Bday ?? new DateTime(1970, 1, 1);
                userInfo.IdCard = model.IdCard == null ? "" : model.IdCard;
                userInfo.RegionId = model.RegionId;
                userInfo.Address = model.Address == null ? "" : WebHelper.HtmlEncode(model.Address);
                userInfo.Bio = model.Bio == null ? "" : WebHelper.HtmlEncode(model.Bio);

                AdminUsers.UpdateUser(userInfo);
                AddMallAdminLog("修改用户", "修改用户,用户名为:" + userInfo.UserName);
                return PromptView("用户修改成功");
            }

            Load(model.RegionId);
            ViewData["introduce"] = GetUserIntroduce(uid);
            return View(model);
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        public ViewResult Del(int[] uidList)
        {
            AdminUsers.DeleteUserById(uidList);
            AddMallAdminLog("删除用户", "删除用户,用户ID为:" + CommonHelper.IntArrayToString(uidList));
            return PromptView("用户删除成功");
        }

        /// <summary>
        /// 会员充值列表
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userRid"></param>
        /// <param name="mallAGid"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult UserCashList(string userName, int userRid = 0, int mallAGid = 0,int pageNumber = 1, int pageSize = 20)
        {
            string condition = AdminUsers.AdminGetUserListCondition(userName, "", "", userRid, mallAGid);
            string sort = AdminUsers.AdminGetUserListSort("", "");

            PageModel pageModel = new PageModel(pageSize, pageNumber, AdminUsers.AdminGetUserCount(condition));

            UserListModel model = new UserListModel()
            {
                UserList = AdminUsers.AdminGetUserList(pageModel.PageSize, pageModel.PageNumber, condition, sort),
                PageModel = pageModel,
                UserName = userName,
                UserRid = userRid,
                MallAGid = mallAGid
            };
            List<SelectListItem> userRankList = new List<SelectListItem>();
            userRankList.Add(new SelectListItem() { Text = "全部等级", Value = "0" });
            foreach (UserRankInfo info in AdminUserRanks.GetUserRankList())
            {
                userRankList.Add(new SelectListItem() { Text = info.Title, Value = info.UserRid.ToString() });
            }
            ViewData["userRankList"] = userRankList;

            List<SelectListItem> mallAdminGroupList = new List<SelectListItem>();
            mallAdminGroupList.Add(new SelectListItem() { Text = "全部组", Value = "0" });
            foreach (MallAdminGroupInfo info in MallAdminGroups.GetMallAdminGroupList())
            {
                mallAdminGroupList.Add(new SelectListItem() { Text = info.Title, Value = info.MallAGid.ToString() });
            }
            ViewData["mallAdminGroupList"] = mallAdminGroupList;

            MallUtils.SetAdminRefererCookie(string.Format("{0}?pageNumber={1}&pageSize={2}&userName={3}&userRid={4}&mallAGid={5}",
                                                          Url.Action("list"), pageModel.PageNumber, pageModel.PageSize,
                                                          userName, userRid, mallAGid));
            return View(model);
        }

        /// <summary>
        /// 充值
        /// </summary>
        [HttpGet]
        public JsonResult LoadCash(int[] uidList,decimal amount)
        {
            if (uidList.Length <1)
            {
                 return Json("请选择用户", JsonRequestBehavior.AllowGet);
            }
            if (amount <= 0)
            {
                 return Json("充值数额必须大于0", JsonRequestBehavior.AllowGet);
            }
            List<int> userids=new List<int>();
            foreach (var uid in uidList)
            {
                var user = Users.GetPartUserById(uid);
                if (user == null)
                {
                    continue;
                }
                CreditLogInfo clog = new CreditLogInfo() {
                    Uid = uid,
                    RankCredits = 0,
                    Action = (int)CreditAction.UserLoadCash,
                    ActionCode =0,
                    ActionDes = "系统为您充值了："+amount+"元",
                    Operator = WorkContext.Uid,
                    FrozenAmount = 0,
                    UserAmount = amount,
                    ActionTime = DateTime.Now
                };
                Credits.SendCredits(0, clog);
                userids.Add(uid);
            }
            AddMallAdminLog("用户充值", "给用户充值成功,用户ID为:" + string.Join(",", userids.ToArray()));
           return Json("succ", JsonRequestBehavior.AllowGet);
        }


        private void Load(int regionId)
        {
            List<SelectListItem> userRankList = new List<SelectListItem>();
            userRankList.Add(new SelectListItem() { Text = "选择会员等级", Value = "0" });
            foreach (UserRankInfo info in AdminUserRanks.GetUserRankList())
            {
                userRankList.Add(new SelectListItem() { Text = info.Title, Value = info.UserRid.ToString() });
            }
            ViewData["userRankList"] = userRankList;


            List<SelectListItem> mallAdminGroupList = new List<SelectListItem>();
            mallAdminGroupList.Add(new SelectListItem() { Text = "选择管理员组", Value = "0" });
            foreach (MallAdminGroupInfo info in MallAdminGroups.GetMallAdminGroupList())
            {
                mallAdminGroupList.Add(new SelectListItem() { Text = info.Title, Value = info.MallAGid.ToString() });
            }
            ViewData["mallAdminGroupList"] = mallAdminGroupList;

            RegionInfo regionInfo = Regions.GetRegionById(regionId);
            if (regionInfo != null)
            {
                ViewData["provinceId"] = regionInfo.ProvinceId;
                ViewData["cityId"] = regionInfo.CityId;
                ViewData["countyId"] = regionInfo.RegionId;
            }
            else
            {
                ViewData["provinceId"] = -1;
                ViewData["cityId"] = -1;
                ViewData["countyId"] = -1;
            }

            ViewData["referer"] = MallUtils.GetMallAdminRefererCookie();
        }

        /// <summary>
        /// 获取推荐关系
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private IntroduceShowModel GetUserIntroduce(int uid)
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
                model.introduceCount += 1;
            }
            model.MyIntroducers = intrmodellist;
            return model;
        }
    }
}
