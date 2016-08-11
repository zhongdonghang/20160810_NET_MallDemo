using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.Mobile.Models;
using System.Collections.Generic;

namespace BrnMall.Web.Mobile.Controllers
{
    /// <summary>
    /// 账号控制器类
    /// </summary>
    public partial class AccountController : BaseMobileController
    {

        /// <summary>
        /// 登录
        /// </summary>
        public ActionResult Login()
        {
            string returnUrl = WebHelper.GetQueryString("returnUrl");
            if (returnUrl.Length == 0)
                returnUrl = Url.Action("index", "home");

            if (WorkContext.MallConfig.LoginType == "")
                return PromptView(returnUrl, "商城目前已经关闭登录功能！");
            if (WorkContext.Uid > 0)
                return PromptView(returnUrl, "您已经登录，无须重复登录！");
            if (WorkContext.MallConfig.LoginFailTimes != 0 && LoginFailLogs.GetLoginFailTimesByIp(WorkContext.IP) >= WorkContext.MallConfig.LoginFailTimes)
                return PromptView(returnUrl, "您已经输入错误" + WorkContext.MallConfig.LoginFailTimes + "次密码，请15分钟后再登录！");

            //get请求
            if (WebHelper.IsGet())
            {
                LoginModel model = new LoginModel();

                model.ReturnUrl = returnUrl;
                model.ShadowName = WorkContext.MallConfig.ShadowName;
                model.IsRemember = WorkContext.MallConfig.IsRemember == 1;
                model.IsVerifyCode = CommonHelper.IsInArray(WorkContext.PageKey, WorkContext.MallConfig.VerifyPages);
                model.OAuthPluginList = Plugins.GetOAuthPluginList();

                return View(model);
            }

            //ajax请求
            string accountName = WebHelper.GetFormString(WorkContext.MallConfig.ShadowName);
            string password = WebHelper.GetFormString("password");
            string verifyCode = WebHelper.GetFormString("verifyCode");
            int isRemember = WebHelper.GetFormInt("isRemember");

            StringBuilder errorList = new StringBuilder("[");
            //验证账户名
            if (string.IsNullOrWhiteSpace(accountName))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "账户名不能为空！", "}");
            }
            else if ((!SecureHelper.IsSafeSqlString(accountName, false)))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "账户名不存在！", "}");
            }

            //验证密码
            if (string.IsNullOrWhiteSpace(password))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码不能为空！", "}");
            }
            else if (password.Length < 4 || password.Length > 32)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码不正确！", "}");
            }

            //验证验证码
            if (CommonHelper.IsInArray(WorkContext.PageKey, WorkContext.MallConfig.VerifyPages))
            {
                if (string.IsNullOrWhiteSpace(verifyCode))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "verifyCode", "验证码不能为空！", "}");
                }
                else if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "verifyCode", "验证码不正确！", "}");
                }
            }

            //当以上验证全部通过时
            PartUserInfo partUserInfo = null;
            if (errorList.Length == 1)
            {
                partUserInfo = Users.GetPartUserByName(accountName);
                if (partUserInfo == null)
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "用户名不存在！", "}");
                }

                //判断密码是否正确
                if (partUserInfo != null && Users.CreateUserPassword(password, partUserInfo.Salt) != partUserInfo.Password)
                {
                    LoginFailLogs.AddLoginFailTimes(WorkContext.IP, DateTime.Now);//增加登录失败次数
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码不正确！", "}");
                }
            }
            if (errorList.Length > 1)//验证失败时
            {
                return AjaxResult("error", errorList.Remove(errorList.Length - 1, 1).Append("]").ToString(), true);
            }
            else//验证成功时
            {
                //当用户等级是禁止访问等级时
                if (partUserInfo.UserRid == 1)
                    return AjaxResult("lockuser", "您的账号当前被锁定,不能访问！");

                //删除登录失败日志
                LoginFailLogs.DeleteLoginFailLogByIP(WorkContext.IP);
                //更新用户最后访问
                Users.UpdateUserLastVisit(partUserInfo.Uid, DateTime.Now, WorkContext.IP, WorkContext.RegionId);
                //更新购物车中用户id
                Carts.UpdateCartUidBySid(partUserInfo.Uid, WorkContext.Sid);
                //将用户信息写入cookie中
                MallUtils.SetUserCookie(partUserInfo, (WorkContext.MallConfig.IsRemember == 1 && isRemember == 1) ? 30 : -1);

                //绑定微信信息
                BindWeixin(partUserInfo.Uid);

                return AjaxResult("success", "登录成功！");
            }
        }

        /// <summary>
        /// 快速登录
        /// </summary>
        /// <param name="type">1：微信授权登录</param>
        /// <returns></returns>
        public ActionResult FastLogin(int type = 1)
        {
            //微信快速登录
            if (type == 1)
            {
                var partUserInfo = Users.GetPartUserByName(WorkContext.WeiXinUnionid);
                //已登录过
                if (partUserInfo != null)
                {
                    //当用户等级是禁止访问等级时
                    if (partUserInfo.UserRid == 1)
                        return AjaxResult("lockuser", "您的账号当前被锁定,不能访问");

                    //删除登录失败日志
                    LoginFailLogs.DeleteLoginFailLogByIP(WorkContext.IP);
                    //更新用户最后访问
                    Users.UpdateUserLastVisit(partUserInfo.Uid, DateTime.Now, WorkContext.IP, WorkContext.RegionId);
                    //更新购物车中用户id
                    Carts.UpdateCartUidBySid(partUserInfo.Uid, WorkContext.Sid);
                    //将用户信息写入cookie中
                    MallUtils.SetUserCookie(partUserInfo, 30);
                    WorkContext.LoginType = 1;
                }
                //未登录过  注册
                else
                {
                    #region 绑定用户信息
                    ////测试
                    //WeiXinUserInfo wxuser = new WeiXinUserInfo()
                    //{
                    //    Unionid = "12333333",
                    //    Sex = 2,
                    //    HeadImgUrl = "http://wx.qlogo.cn/mmopen/aXUpZVUYfjw9nrasyw3bsgDA3tGJ99tX7XFzicY4AZogoWlrIIibQczubJn78rb1SE4TYLcbpWhZnrVOqt81X7ALdLQWtWtrTw/0",
                    //    NickName = "weili叶O_O",
                    //    OpenId = "2132435445565"
                    //};
                    WeiXinUserInfo wxuser = WeiXins.GetMyWeiXinUserInfo();
                    if (wxuser == null || string.IsNullOrWhiteSpace(wxuser.Unionid))
                    {
                        return AjaxResult("exception", "用户信息错误，请刷新再重新登录！");
                    }
                    UserInfo userInfo = new UserInfo();
                    userInfo.UserName = wxuser.Unionid;
                    userInfo.UserRid = UserRanks.GetLowestUserRank().UserRid;
                    userInfo.StoreId = 0;
                    userInfo.MallAGid = 1;//非管理员组
                    userInfo.NickName = wxuser.NickName;
                    userInfo.Avatar = MallUtils.SaveUserAvatarByImgUrl(wxuser.HeadImgUrl);
                    userInfo.UserAmount = 0;
                    userInfo.FrozenAmount = 0;
                    userInfo.RankCredits = 0;
                    userInfo.VerifyEmail = 0;
                    userInfo.VerifyMobile = 0;
                    userInfo.Salt = "";
                    userInfo.Password = "";
                    userInfo.LastVisitIP = WorkContext.IP;
                    userInfo.LastVisitRgId = WorkContext.RegionId;
                    userInfo.LastVisitTime = DateTime.Now;
                    userInfo.RegisterIP = WorkContext.IP;
                    userInfo.RegisterRgId = WorkContext.RegionId;
                    userInfo.RegisterTime = DateTime.Now;

                    userInfo.Gender = wxuser.Sex;
                    userInfo.RealName = "";
                    userInfo.Bday = new DateTime(1900, 1, 1);
                    userInfo.IdCard = "";
                    userInfo.RegionId = 0;
                    userInfo.Address = "";
                    userInfo.Bio = "";

                    #endregion

                    //创建用户
                    userInfo.Uid = Users.CreateUser(userInfo);

                    //添加用户失败
                    if (userInfo.Uid < 1)
                    {
                        return AjaxResult("exception", "用户信息错误,请联系管理员");
                    }

                    //更新购物车中用户id
                    Carts.UpdateCartUidBySid(userInfo.Uid, WorkContext.Sid);
                    //将用户信息写入cookie
                    MallUtils.SetUserCookie(userInfo, 0);

                    //同步上下文
                    WorkContext.Uid = userInfo.Uid;
                    WorkContext.UserName = userInfo.UserName;
                    WorkContext.UserEmail = userInfo.Email;
                    WorkContext.UserMobile = userInfo.Mobile;
                    WorkContext.NickName = userInfo.NickName;
                    WorkContext.LoginType = 1;
                }

                MallUtils.SettLoginTypeCookie("1");

            }
            return RedirectToAction("Index","Ucenter");

        }
            
        /// <summary>
        /// 注册
        /// </summary>
        public ActionResult Register()
        {
            string returnUrl = WebHelper.GetQueryString("returnUrl");
            if (returnUrl.Length == 0)
                returnUrl = Url.Action("index", "home");

            if (WorkContext.MallConfig.RegType.Length == 0)
                return PromptView(returnUrl, "商城目前已经关闭注册功能!");
            if (WorkContext.Uid > 0)
                return PromptView(returnUrl, "你已经是本商城的注册用户，无需再注册!");
            if (WorkContext.MallConfig.RegTimeSpan > 0)
            {
                DateTime registerTime = Users.GetRegisterTimeByRegisterIP(WorkContext.IP);
                if ((DateTime.Now - registerTime).Minutes <= WorkContext.MallConfig.RegTimeSpan)
                    return PromptView(returnUrl, "你注册太频繁，请间隔一定时间后再注册!");
            }

            //get请求
            if (WebHelper.IsGet())
            {
                RegisterModel model = new RegisterModel();

                model.ReturnUrl = returnUrl;
                model.ShadowName = WorkContext.MallConfig.ShadowName;
                model.IsVerifyCode = CommonHelper.IsInArray(WorkContext.PageKey, WorkContext.MallConfig.VerifyPages);

                return View(model);
            }

            //ajax请求
            string accountName = WebHelper.GetFormString(WorkContext.MallConfig.ShadowName).Trim().ToLower();
            string password = WebHelper.GetFormString("password");
            string confirmPwd = WebHelper.GetFormString("confirmPwd");
            string verifyCode = WebHelper.GetFormString("verifyCode");
            string phoneVerifyCode = WebHelper.GetFormString("phoneVerifyCode");
            string introduceName = WebHelper.GetFormString("introduceName");
            StringBuilder errorList = new StringBuilder("[");
            #region 验证

            //账号验证
            if (!ValidateHelper.IsMobile(accountName))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "账户名必须为11位的手机号码", "}");
            }

            //推荐者验证
            if (!string.IsNullOrEmpty(introduceName))
            {
                if (!ValidateHelper.IsMobile(introduceName))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "推荐者用户名必须为11位的手机号码", "}");
                }
                else if (accountName == introduceName)
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "leaderName", "推荐者不能是本人", "}");
                }
                else if (!Users.IsSurperMember(introduceName))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "leaderName", "该推荐者不存在", "}");
                }
            }

            //密码验证
            if (string.IsNullOrWhiteSpace(password))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码不能为空", "}");
            }
            else if (password.Length < 4 || password.Length > 32)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码必须大于3且不大于32个字符", "}");
            }
            else if (password != confirmPwd)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "两次输入的密码不一样", "}");
            }

            //手机验证码验证
            if (string.IsNullOrWhiteSpace(phoneVerifyCode) || phoneVerifyCode.Length != 6)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "phoneVerifyCode", "手机验证码错误！", "}");
            }
            else if (Sessions.GetValueString(WorkContext.Sid, "ucsvMoibleCode") != phoneVerifyCode)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "phoneVerifyCode", "手机验证码错误！", "}");
            }

            //验证码验证
            if (CommonHelper.IsInArray(WorkContext.PageKey, WorkContext.MallConfig.VerifyPages))
            {
                if (string.IsNullOrWhiteSpace(verifyCode))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "verifyCode", "验证码不能为空", "}");
                }
                else if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "verifyCode", "验证码不正确", "}");
                }
            }

            //其它验证
            int gender = WebHelper.GetFormInt("gender");
            if (gender < 0 || gender > 2)
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "gender", "请选择正确的性别", "}");

            string nickName = WebHelper.GetFormString("nickName");
            if (nickName.Length > 10)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "nickName", "昵称的长度不能大于10", "}");
            }
            else if (FilterWords.IsContainWords(nickName))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "nickName", "昵称中包含禁止单词", "}");
            }

            if (WebHelper.GetFormString("realName").Length > 5)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "realName", "真实姓名的长度不能大于5", "}");
            }

            string bday = WebHelper.GetFormString("bday");
            if (bday.Length == 0)
            {
                string bdayY = WebHelper.GetFormString("bdayY");
                string bdayM = WebHelper.GetFormString("bdayM");
                string bdayD = WebHelper.GetFormString("bdayD");
                bday = string.Format("{0}-{1}-{2}", bdayY, bdayM, bdayD);
            }
            if (bday.Length > 0 && bday != "--" && !ValidateHelper.IsDate(bday))
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "bday", "请选择正确的日期", "}");

            string idCard = WebHelper.GetFormString("idCard");
            if (idCard.Length > 0 && !ValidateHelper.IsIdCard(idCard))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "idCard", "请输入正确的身份证号", "}");
            }

            int regionId = WebHelper.GetFormInt("regionId");
            if (regionId > 0)
            {
                if (Regions.GetRegionById(regionId) == null)
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "regionId", "请选择正确的地址", "}");
                if (WebHelper.GetFormString("address").Length > 75)
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "address", "详细地址的长度不能大于75", "}");
                }
            }

            if (WebHelper.GetFormString("bio").Length > 150)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "bio", "简介的长度不能大于150", "}");
            }

            //当以上验证都通过时
            UserInfo userInfo = null;
            if (errorList.Length == 1)
            {

                if (Users.IsExistUserName(accountName))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "用户名已经存在", "}");
                }
                else if (Users.IsExistMobile(accountName))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "该手机号已经存在", "}");
                }
                else
                {
                    userInfo = new UserInfo();
                    userInfo.UserName = accountName;
                    userInfo.Email = string.Empty;
                    userInfo.Mobile = accountName;
                }

            }

            #endregion

            if (errorList.Length > 1)//验证失败
            {
                return AjaxResult("error", errorList.Remove(errorList.Length - 1, 1).Append("]").ToString(), true);
            }
            else//验证成功
            {
                #region 绑定用户信息

                userInfo.Salt = Randoms.CreateRandomValue(6);
                userInfo.Password = Users.CreateUserPassword(password, userInfo.Salt);
                userInfo.UserRid = UserRanks.GetLowestUserRank().UserRid;
                userInfo.StoreId = 0;
                userInfo.MallAGid = 1;//非管理员组
                if (nickName.Length > 0)
                    userInfo.NickName = WebHelper.HtmlEncode(nickName);
                else
                    userInfo.NickName = "jsy" + Randoms.CreateRandomValue(7);
                userInfo.Avatar = "";
                userInfo.UserAmount = 0;
                userInfo.FrozenAmount = 0;
                userInfo.RankCredits = 0;
                userInfo.VerifyEmail = 0;
                userInfo.VerifyMobile = 1;

                userInfo.LastVisitIP = WorkContext.IP;
                userInfo.LastVisitRgId = WorkContext.RegionId;
                userInfo.LastVisitTime = DateTime.Now;
                userInfo.RegisterIP = WorkContext.IP;
                userInfo.RegisterRgId = WorkContext.RegionId;
                userInfo.RegisterTime = DateTime.Now;

                userInfo.Gender = WebHelper.GetFormInt("gender");
                userInfo.RealName = WebHelper.HtmlEncode(WebHelper.GetFormString("realName"));
                userInfo.Bday = bday.Length > 0 ? TypeHelper.StringToDateTime(bday) : new DateTime(1900, 1, 1);
                userInfo.IdCard = WebHelper.GetFormString("idCard");
                userInfo.RegionId = WebHelper.GetFormInt("regionId");
                userInfo.Address = WebHelper.HtmlEncode(WebHelper.GetFormString("address"));
                userInfo.Bio = WebHelper.HtmlEncode(WebHelper.GetFormString("bio"));

                #endregion

                //创建用户
                userInfo.Uid = Users.CreateUser(userInfo);

                //添加用户失败
                if (userInfo.Uid < 1)
                {
                    return AjaxResult("exception", "创建用户失败,请联系管理员");
                }
                //添加用户推荐者
                if (!string.IsNullOrEmpty(introduceName))
                {
                    int introduceId = Users.GetUidByUserName(introduceName);
                    if (introduceId > 0)
                    {
                        Users.CreateUserIntroduceId(userInfo.Uid, introduceId);
                        //给新用户升级为8折会员
                        UserRankInfo rank = null;
                        List<UserRankInfo> ranklist = UserRanks.GetCustomerUserRankList();
                        foreach (var r in ranklist)
                        {
                            if (r.CreditsLower != 0)
                            {
                                rank = r;
                                break;

                            }
                        }
                        if (rank != null)
                        {
                            CreditLogInfo loginfo = new CreditLogInfo()
                            {
                                Uid = userInfo.Uid,
                                RankCredits = rank.CreditsLower,
                                Action = (int)CreditAction.Register,
                                ActionTime = DateTime.Now,
                                ActionCode = 0,
                                ActionDes = "有推荐者，升级为" + rank.Title,
                                Operator = 0
                            };
                            Credits.SendCredits(rank.UserRid, loginfo);
                        }
                    }
                }

                //发放注册积分
                Credits.SendRegisterCredits(ref userInfo, DateTime.Now);
                //更新购物车中用户id
                Carts.UpdateCartUidBySid(userInfo.Uid, WorkContext.Sid);
                //将用户信息写入cookie
                MallUtils.SetUserCookie(userInfo, 0);
                MallUtils.SettLoginTypeCookie("1");

                //同步上下文
                WorkContext.Uid = userInfo.Uid;
                WorkContext.UserName = userInfo.UserName;
                WorkContext.UserEmail = userInfo.Email;
                WorkContext.UserMobile = userInfo.Mobile;
                WorkContext.NickName = userInfo.NickName;

                //绑定微信信息
                BindWeixin(WorkContext.Uid);

                return AjaxResult("success", "注册成功");
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public ActionResult Logout()
        {
            if (WorkContext.Uid > 0)
            {
                WebHelper.DeleteCookie("jsy");
                WebHelper.DeleteCookie("jsysid");
                WebHelper.DeleteCookie("jsyweixinunid");
                WebHelper.DeleteCookie("jsylogintype");
                Sessions.RemoverSession(WorkContext.Sid);
                OnlineUsers.DeleteOnlineUserBySid(WorkContext.Sid);

            }
            return Redirect(Url.Action("Login", "Account"));
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        public ActionResult FindPwd()
        {
            //get请求
            if (WebHelper.IsGet())
            {
                FindPwdModel model = new FindPwdModel();

                model.ShadowName = WorkContext.MallConfig.ShadowName;
                model.IsVerifyCode = CommonHelper.IsInArray(WorkContext.PageKey, WorkContext.MallConfig.VerifyPages);

                return View(model);
            }

            //ajax请求
            string accountName = WebHelper.GetFormString(WorkContext.MallConfig.ShadowName);
            string verifyCode = WebHelper.GetFormString("verifyCode");

            StringBuilder errorList = new StringBuilder("[");
            //账号验证
            if (string.IsNullOrWhiteSpace(accountName))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "用户名不能为空！", "}");
            }
            else if (accountName.Length < 4 || accountName.Length > 50)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "用户名不存在！", "}");
            }
            else if ((!SecureHelper.IsSafeSqlString(accountName)))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "用户名不存在！", "}");
            }

            //验证码验证
            if (CommonHelper.IsInArray(WorkContext.PageKey, WorkContext.MallConfig.VerifyPages))
            {
                if (string.IsNullOrWhiteSpace(verifyCode))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "verifyCode", "验证码不能为空！", "}");
                }
                else if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "verifyCode", "验证码不正确！", "}");
                }
            }

            //当以上验证都通过时
            PartUserInfo partUserInfo = null;
            if (errorList.Length == 1)
            {
                if (ModelState.IsValid)
                {
                    partUserInfo = Users.GetPartUserByName(accountName);
                    if (partUserInfo == null)
                    {
                        errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "用户名不存在！", "}");
                    }
                }
            }
            //验证失败
            if (errorList.Length > 1)
            {
                return AjaxResult("error", errorList.Remove(errorList.Length - 1, 1).Append("]").ToString(), true);
            }
            else
            {
                return AjaxResult("success", Url.Action("selectfindpwdtype", new RouteValueDictionary { { "uid", partUserInfo.Uid } }));
            }
        }

        /// <summary>
        /// 选择找回密码方式
        /// </summary>
        /// <returns></returns>
        public ActionResult SelectFindPwdType()
        {
            int uid = WebHelper.GetQueryInt("uid");
            PartUserInfo partUserInfo = Users.GetPartUserById(uid);
            if (partUserInfo == null)
                return PromptView("用户不存在");

            SelectFindPwdTypeModel model = new SelectFindPwdTypeModel();
            model.PartUserInfo = partUserInfo;
            return View(model);
        }

        /// <summary>
        /// 验证找回密码手机
        /// </summary>
        public ActionResult VerifyFindPwdMobile()
        {
            int uid = WebHelper.GetQueryInt("uid");
            string mobileCode = WebHelper.GetFormString("mobileCode");

            PartUserInfo partUserInfo = Users.GetPartUserById(uid);
            if (partUserInfo == null)
            {
                return AjaxResult("nouser", "用户不存在！");
            }

            //手机验证码验证
            if (string.IsNullOrWhiteSpace(mobileCode))
            {
                return AjaxResult("emptymobilecode", "手机验证码不能为空！");
            }
            if (Sessions.GetValueString(WorkContext.Sid, "ucsvMoibleCode") != mobileCode)
            {
                return AjaxResult("errormobilecode", "手机验证码错误！");
            }

            string v = MallUtils.AESEncrypt(string.Format("{0},{1},{2}", partUserInfo.Uid, DateTime.Now, Randoms.CreateRandomValue(6)));
            string url = string.Format("http://{0}{1}", Request.Url.Authority, Url.Action("resetpwd", new RouteValueDictionary { { "v", v } }));
            return AjaxResult("success", url);
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        public ActionResult ResetPwd()
        {
            string v = WebHelper.GetQueryString("v");
            //解密字符串
            string realV = SecureHelper.AESDecrypt(v, WorkContext.MallConfig.SecretKey);

            //数组第一项为uid，第二项为验证时间,第三项为随机值
            string[] result = StringHelper.SplitString(realV);
            if (result.Length != 3)
                return HttpNotFound();

            int uid = TypeHelper.StringToInt(result[0]);
            DateTime time = TypeHelper.StringToDateTime(result[1]);

            PartUserInfo partUserInfo = Users.GetPartUserById(uid);
            if (partUserInfo == null)
            {
                return AjaxResult("error", "用户不存在！");
            }
            //判断验证时间是否过时
            if (DateTime.Now.AddMinutes(-30) > time)
            {
                return AjaxResult("error", "此链接已经失效，请重新验证！");
            }

            //get请求
            if (WebHelper.IsGet())
            {
                ResetPwdModel model = new ResetPwdModel();
                model.V = v;
                return View(model);
            }

            //ajax请求
            string password = WebHelper.GetFormString("password");
            string confirmPwd = WebHelper.GetFormString("confirmPwd");

            StringBuilder errorList = new StringBuilder("[");
            //验证
            if (string.IsNullOrWhiteSpace(password))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码不能为空", "}");
            }
            else if (password.Length < 4 || password.Length > 32)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码必须大于3且不大于32个字符", "}");
            }
            else if (password != confirmPwd)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "confirmPwd", "两次输入的密码不一样", "}");
            }

            if (errorList.Length == 1)
            {
                //生成用户新密码
                string p = Users.CreateUserPassword(password, partUserInfo.Salt);
                //设置用户新密码
                Users.UpdateUserPasswordByUid(uid, p);
                //清空当前用户信息
                WebHelper.DeleteCookie("jsy");
                Sessions.RemoverSession(WorkContext.Sid);
                OnlineUsers.DeleteOnlineUserBySid(WorkContext.Sid);

                return AjaxResult("success", Url.Action("login"));
            }
            else
            {
                return AjaxResult("error", errorList.Remove(errorList.Length - 1, 1).Append("]").ToString(), true);
            }
        }

        #region 微信绑定
        /// <summary>
        /// 我的微信
        /// </summary>
        /// <returns></returns>
        public ActionResult MyWeiXin()
        {
            //获取微信unionid
            //获得当前登录的微信账号
            var user = Users.GetPartUserInfoByWeixinUnid(WorkContext.WeiXinUnionid);
            return View(user);
        }

        /// <summary>
        /// 解除绑定账号
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult RemoveBindingAccount()
        {
            try
            {
                //获得当前登录的微信账号
                string unid = WorkContext.WeiXinUnionid;
                var user = Users.GetPartUserInfoByWeixinUnid(unid);
                Users.UpdateUserWxUnionIdsByUid(user.Uid, "");
                //退出登录
                if (WorkContext.Uid > 0)
                {
                    WebHelper.DeleteCookie("jsy");
                    Sessions.RemoverSession(WorkContext.Sid);
                    OnlineUsers.DeleteOnlineUserBySid(WorkContext.Sid);
                }
                return Json("succ", JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json("fail", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 绑定微信号至用户
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="unid"></param>
        /// <returns></returns>
        private void BindWeixin(int uid)
        {
            string unionid = WorkContext.WeiXinUnionid;
            PartUserInfo user = Users.GetPartUserById(uid);
            //账号已包含该微信
            if (user!=null && !user.WxUnionIds.Contains(unionid))
            {
                Users.UpdateUserWxUnionIdsByUid(uid, unionid);
            }
        }

        #endregion

        /// <summary>
        /// 获取手机验证码，前端通过Jquery post手机号，后台接受并判断号码是否正确，
        /// 正确就发送验证码；反之给出提醒
        /// </summary>
        /// <param name="phoneNumber">电话号码</param>
        /// <param name="type">验证类型 0:会员注册  1:找回密码验证 2：更改手机验证 3：更改密码验证 </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPhoneVerifyCode(string phoneNumber, int type = 0)
        {
            int uid = Users.GetUidByUserName(phoneNumber);
            StringBuilder body = null;
            switch (type)
            {
                case 0:

                    if (uid > 0)
                    {
                        return Json("该手机号已经被注册！");
                    }
                    body = new StringBuilder(BMAConfig.SMSConfig.SCVerifyBody);
                    break;
                case 1:
                    if (uid < 1)
                    {
                        return Json("该手机还未注册过！");
                    }
                    body = new StringBuilder(BMAConfig.SMSConfig.FindPwdBody);
                    break;
                case 2:
                    if (uid > 0)
                    {
                        return Json("该手机号已经被注册！");
                    }
                    body = new StringBuilder(BMAConfig.SMSConfig.SCUpdateBody);
                    break;
                case 3:
                    if (uid < 1 && uid != WorkContext.Uid)
                    {
                        return Json("手机号错误！");
                    }
                    body = new StringBuilder(BMAConfig.SMSConfig.SCUpdateBody);
                    break;
                default:
                    break;
            }
            //短信内容
            string moibleCode = Randoms.CreateRandomValue(6);
            body.Replace("{mallname}", BMAConfig.MallConfig.MallName);
            body.Replace("{code}", moibleCode);
            string content = body.ToString();
            bool result = false;
            try
            {
                result = SMSes.SendSCVerifySMS(phoneNumber, content);
                if (result)
                {
                    //将验证值保存在session中
                    Sessions.SetItem(WorkContext.Sid, "ucsvMoibleCode", moibleCode);
                    Sessions.SetItem(WorkContext.Sid, "ucsuMobile", phoneNumber);

                    return Json("succ");
                }
                return Json("failed");
            }
            catch (Exception)
            {
                return Json("failed");
            }
            finally
            {
                //记录短信log
                SMSLogInfo loginfo = new SMSLogInfo()
                {
                    CodeUsed = false,
                    IP = WorkContext.IP,
                    IsSendSuccess = result,
                    SMSContent = content,
                    SendTime = DateTime.Now,
                    Phone = phoneNumber,
                    Type = 0,
                    Code = moibleCode
                };
                SMSes.CreateSMSLog(loginfo);
            }
        }
    }
}
