using System;
using System.Web;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.Models;
using System.Collections.Generic;

namespace BrnMall.Web.Controllers
{
    /// <summary>
    /// 账号控制器类
    /// </summary>
    public partial class AccountController : BaseWebController
    {
        /// <summary>
        /// 商城登录
        /// </summary>
        public ActionResult Login()
        {
            string returnUrl = WebHelper.GetQueryString("returnUrl");
            if (returnUrl.Length == 0)
                returnUrl = "/";

            if (WorkContext.MallConfig.LoginType == "")
                return PromptView(returnUrl, "商城目前已经关闭登录功能!");
            if (WorkContext.Uid > 0)
                return PromptView(returnUrl, "您已经登录，无须重复登录!");
            if (WorkContext.MallConfig.LoginFailTimes != 0 && LoginFailLogs.GetLoginFailTimesByIp(WorkContext.IP) >= WorkContext.MallConfig.LoginFailTimes)
                return PromptView(returnUrl, "您已经输入错误" + WorkContext.MallConfig.LoginFailTimes + "次密码，请15分钟后再登录!");

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
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "用户名不能为空！", "}");
            }
            else if ((!SecureHelper.IsSafeSqlString(accountName, false)))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "用户名不存在！", "}");
            }

            //验证密码
            if (string.IsNullOrWhiteSpace(password))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码不能为空！", "}");
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

                //将购物车中商品数量写入cookie
                List<OrderProductInfo> orderProductList = Carts.GetCartProductList(partUserInfo.Uid, WorkContext.Sid);
                int pCount = Carts.SumOrderProductCount(orderProductList);
                Carts.SetCartProductCountCookie(pCount);

                //将用户信息写入cookie中
                MallUtils.SetUserCookie(partUserInfo, (WorkContext.MallConfig.IsRemember == 1 && isRemember == 1) ? 15 : -1);
                return AjaxResult("success", "登录成功");
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        public ActionResult Register()
        {
            string returnUrl = WebHelper.GetQueryString("returnUrl");
            if (returnUrl.Length == 0)
                returnUrl = "/";

            if (WorkContext.MallConfig.RegType.Length == 0)
                return PromptView(returnUrl, "商城目前已经关闭注册功能！");
            if (WorkContext.Uid > 0)
                return PromptView(returnUrl, "你已经是本商城的注册用户，无需再注册！");
            if (WorkContext.MallConfig.RegTimeSpan > 0)
            {
                DateTime registerTime = Users.GetRegisterTimeByRegisterIP(WorkContext.IP);
                if ((DateTime.Now - registerTime).Minutes <= WorkContext.MallConfig.RegTimeSpan)
                    return PromptView(returnUrl, "你注册太频繁，请间隔一定时间后再注册！");
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
            string introduceName = WebHelper.GetFormString("introduceName");
            string verifyCode = WebHelper.GetFormString("verifyCode");
            string phoneVerifyCode = WebHelper.GetFormString("phoneVerifyCode");
            StringBuilder errorList = new StringBuilder("[");
            #region 验证

            //账号验证
            if (!ValidateHelper.IsMobile(accountName))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "账户名必须为11位的手机号码！", "}");
            }

            //推荐者验证
            if (!string.IsNullOrEmpty(introduceName) && !Users.IsSurperMember(introduceName))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "leaderName", "该推荐者不存在！", "}");
            }

            //密码验证
            if (string.IsNullOrWhiteSpace(password))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码不能为空！", "}");
            }
            else if (password.Length < 4 || password.Length > 32)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "密码必须大于3且不大于32个字符！", "}");
            }
            else if (password != confirmPwd)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "password", "两次输入的密码不一样！", "}");
            }

            //手机短信验证
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
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "verifyCode", "验证码不能为空！", "}");
                }
                else if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "verifyCode", "验证码不正确！", "}");
                }
            }

            //当以上验证都通过时
            UserInfo userInfo = null;
            if (errorList.Length == 1)
            {
                if (Users.IsExistUserName(accountName))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "该用户名已经存在！", "}");
                }
                else if (Users.IsExistMobile(accountName))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "该手机号已经存在！", "}");
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

                userInfo.Gender = 0;
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
                    return AjaxResult("exception", "创建用户失败,请联系管理员");

                //添加用户推荐者
                if (!string.IsNullOrEmpty(introduceName))
                {
                    int introduceId = Users.GetUidByUserName(introduceName);
                    if (introduceId > 0)
                    {
                        Users.CreateUserIntroduceId(userInfo.Uid, introduceId);
                        //给新用户升级为8折会员
                        UserRankInfo rank =null;
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

                //发送注册欢迎信息
                if (WorkContext.MallConfig.IsWebcomeMsg == 1)
                {
                    if (userInfo.Email.Length > 0)
                        Emails.SendWebcomeEmail(userInfo.Email);
                    if (userInfo.Mobile.Length > 0)
                        SMSes.SendWebcomeSMS(userInfo.Mobile);
                }

                //同步上下午
                WorkContext.Uid = userInfo.Uid;
                WorkContext.UserName = userInfo.UserName;
                WorkContext.UserEmail = userInfo.Email;
                WorkContext.UserMobile = userInfo.Mobile;
                WorkContext.NickName = userInfo.NickName;

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
                Sessions.RemoverSession(WorkContext.Sid);
                OnlineUsers.DeleteOnlineUserBySid(WorkContext.Sid);
            }
            //将购物车中商品数量写入cookie
            List<OrderProductInfo> orderProductList = Carts.GetCartProductList(-1, WorkContext.Sid);
            int pCount = Carts.SumOrderProductCount(orderProductList);
            Carts.SetCartProductCountCookie(pCount);
            return Redirect("/");
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
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "账户名不能为空！", "}");
            }
            else if ((!SecureHelper.IsSafeSqlString(accountName)))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "账户名不存在！", "}");
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
            if (ModelState.IsValid)
            {
                partUserInfo = Users.GetPartUserByName(accountName);
                if (partUserInfo == null)
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "accountName", "用户名不存在！", "}");
                }

            }

            if (errorList.Length == 1)
            {
                return AjaxResult("success", Url.Action("selectfindpwdtype", new RouteValueDictionary { { "uid", partUserInfo.Uid } }));
            }
            else
            {
                return AjaxResult("error", errorList.Remove(errorList.Length - 1, 1).Append("]").ToString(), true);
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

            //检查手机码
            if (string.IsNullOrWhiteSpace(mobileCode))
            {
                return AjaxResult("emptymobilecode", "手机验证码不能为空！");
            }
            else if (Sessions.GetValueString(WorkContext.Sid, "ucsvMoibleCode") != mobileCode)
            {
                return AjaxResult("wrongmobilecode", "手机验证码不正确！");
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
                return PromptView("用户不存在");
            //判断验证时间是否过时
            if (DateTime.Now.AddMinutes(-30) > time)
                return PromptView("此链接已经失效，请重新验证");

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
                    if (uid <1 && uid!=WorkContext.Uid)
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
                    Sessions.SetItem(WorkContext.Sid, "ucsuMobile", phoneNumber);
                    Sessions.SetItem(WorkContext.Sid, "ucsvMoibleCode", moibleCode);
                    return Json("succ");
                }
                return Json("failed");
            }
            catch (Exception)
            {
                return Json("failed");
            }
            finally {
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
