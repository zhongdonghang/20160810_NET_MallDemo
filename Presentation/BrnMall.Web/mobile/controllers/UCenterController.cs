using System;
using System.Text;
using System.Data;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.Mobile.Models;
using System.Web;
using System.Net;
using System.IO;

namespace BrnMall.Web.Mobile.Controllers
{
    /// <summary>
    /// 用户中心控制器类
    /// </summary>
    public partial class UCenterController : BaseMobileController
    {
        /// <summary>
        /// 首页
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 账户
        /// </summary>
        public ActionResult Account()
        {
            return View();
        }

        #region 用户中心
        /// <summary>
        /// 编辑用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditUser()
        {
            UserInfo user = Users.GetUserById(WorkContext.Uid);
            UserInfoModel model = new UserInfoModel();
            model.UserName = user.UserName;
            model.RankTitle = UserRanks.GetUserRankByCredits(user.RankCredits).Title;
            model.NickName = user.NickName;
            model.RealName = user.RealName;
            model.Gender = user.Gender;
            model.Avatar = user.Avatar;
            model.IdCard = user.IdCard;
            model.BirthDay = user.Bday;
            model.RegionId = user.RegionId;
            model.Bio = user.Bio;
            model.Address = user.Address;
            RegionInfo regionInfo = Regions.GetRegionById(user.RegionId);
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
            return View(model);
        }

        /// <summary>
        /// 编辑用户信息提交
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditUser(UserInfoModel model)
        {
            //验证昵称
            if (string.IsNullOrWhiteSpace(model.NickName) || model.NickName.Length > 10 || model.NickName.Length <2)
            {
                ModelState.AddModelError("NickName", "昵称的长度不能大于10小于2");
            }
            else if (FilterWords.IsContainWords(model.NickName))
            {
                ModelState.AddModelError("NickName", "昵称中包含禁止单词");
            }

            //验证真实姓名
            if (!string.IsNullOrWhiteSpace(model.RealName) && model.RealName.Length > 5)
            {
                ModelState.AddModelError("RealName", "真实姓名的长度不能大于5");
            }

            //验证性别
            if (model.Gender < 0 || model.Gender > 2)
            {
                ModelState.AddModelError("Gender", "请选择正确的性别");
            }

            //验证区域
            if (model.RegionId > 0)
            {
                RegionInfo regionInfo = Regions.GetRegionById(model.RegionId);
                if (regionInfo == null || regionInfo.Layer != 3)
                {
                    ModelState.AddModelError("RegionId", "请选择正确的地址");
                }
            }
            //验证详细地址
            if (!string.IsNullOrWhiteSpace(model.Address) && model.Address.Length > 75)
            {
                ModelState.AddModelError("Address", "详细地址的长度不能大于75");
            }

            //验证简介
            if (!string.IsNullOrWhiteSpace(model.Bio) && model.Bio.Length > 150)
            {
                ModelState.AddModelError("Bio", "简介的长度不能大于150");
            }
            //上传图片
            model.Avatar=Upload("uploaduseravatar");
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(model.Avatar) && !string.IsNullOrWhiteSpace(WorkContext.Avatar))
                {
                    model.Avatar = WorkContext.Avatar;
                }
                if (string.IsNullOrWhiteSpace(model.IdCard))
                {
                    model.IdCard = "";
                }
                if (string.IsNullOrWhiteSpace(model.RealName))
                {
                    model.RealName = "";
                }
                if (string.IsNullOrWhiteSpace(model.Bio))
                {
                    model.Bio = "";
                }
                if (string.IsNullOrWhiteSpace(model.Address))
                {
                    model.Address = "";
                }
                if (model.RegionId < 1)
                {
                    model.RegionId = 0;
                }
                string defaulttime = TypeHelper.StringToDateTime("1900-1-1").ToShortDateString();
                Users.UpdateUser(WorkContext.Uid, model.UserName, WebHelper.HtmlEncode(model.NickName), WebHelper.HtmlEncode(model.Avatar), model.Gender, WebHelper.HtmlEncode(model.RealName), model.BirthDay, model.IdCard, model.RegionId, WebHelper.HtmlEncode(model.Address), WebHelper.HtmlEncode(model.Bio));
                if (model.UserName.Length > 0 && model.NickName.Length > 0 && model.Avatar.Length > 0 && model.RealName.Length > 0 && !model.BirthDay.ToShortDateString().Equals(defaulttime) && model.IdCard.Length > 0 && model.RegionId > 0 && model.Address.Length > 0)
                {
                    Credits.SendCompleteUserInfoCredits(ref WorkContext.PartUserInfo, DateTime.Now);
                }

                return RedirectToAction("Account");
            }
            else
            {
                RegionInfo regionInfo = Regions.GetRegionById(model.RegionId);
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
                return View(model);
            }
        }

        /// <summary>
        /// 推荐者关系
        /// </summary>
        /// <returns></returns>
        public ActionResult Introducers()
        {
            IntroduceShowModel model = new IntroduceShowModel();

            //推荐者
            IntroduceModel intrmodel = null;
            int introduceid=Users.GetMyIntroducer(WorkContext.Uid);
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
           DataTable introducelist = Users.GetMyIntroducerList(WorkContext.Uid);
           foreach (DataRow row in introducelist.Rows)
           {
               IntroduceModel umodel = new IntroduceModel();
               umodel.NickName = row["nickname"].ToString();
               umodel.UserName = row["username"].ToString();
               umodel.AddTime = TypeHelper.ObjectToDateTime(row["registertime"]).ToString("yyyy-MM-dd");
               intrmodellist.Add(umodel);
               model.introduceCount += 1;
           }
           model.MyIntroducers = intrmodellist;
            return View(model);
        }
        #endregion

        #region 安全中心

        /// <summary>
        /// 安全验证
        /// </summary>
        public ActionResult SafeVerify()
        {
            string action = WebHelper.GetQueryString("act").ToLower();
            string mode = WebHelper.GetQueryString("mode").ToLower();

            if (action.Length == 0 || !CommonHelper.IsInArray(action, new string[3] { "updatepassword", "updatepaypassword", "updatemobile" }) || (mode.Length > 0 && !CommonHelper.IsInArray(mode, new string[2] { "password", "mobile" })))
            {
                return PromptView("信息错误，请刷新重试！");
            }

            SafeVerifyModel model = new SafeVerifyModel();
            model.Action = action;

            if (mode.Length == 0)
            {
                if (WorkContext.PartUserInfo.VerifyMobile == 1)//通过手机验证
                {
                    model.Mode = "mobile";
                }
                else//通过密码验证
                {
                    model.Mode = "password";
                }
            }
            else
            {
                if (mode == "mobile" && WorkContext.PartUserInfo.VerifyMobile == 1)
                {
                    model.Mode = "mobile";
                }
                else
                {
                    model.Mode = "password";
                }
            }

            return View(model);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        public ActionResult VerifyPassword()
        {
            string action = WebHelper.GetQueryString("act").ToLower();
            string password = WebHelper.GetFormString("password");
            string verifyCode = WebHelper.GetFormString("verifyCode");

            if (action.Length == 0 || !CommonHelper.IsInArray(action, new string[3] { "updatepassword", "updatemobile", "updatepaypassword" }))
                return AjaxResult("noaction", "动作不存在，请刷新重试！");

            //检查验证码
            if (string.IsNullOrWhiteSpace(verifyCode))
            {
                return AjaxResult("verifycode", "验证码不能为空！");
            }
            if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
            {
                return AjaxResult("verifycode", "验证码不正确！");
            }

            //检查密码
            if (string.IsNullOrWhiteSpace(password))
            {
                return AjaxResult("password", "密码不能为空！");
            }
            if (Users.CreateUserPassword(password, WorkContext.PartUserInfo.Salt) != WorkContext.Password)
            {
                return AjaxResult("password", "密码不正确！");
            }

            string v = MallUtils.AESEncrypt(string.Format("{0},{1},{2},{3}", WorkContext.Uid, action, DateTime.Now, Randoms.CreateRandomValue(6)));
            string url = Url.Action("safeupdate", new RouteValueDictionary { { "v", v } });
            return AjaxResult("success", url);
        }

        /// <summary>
        /// 发送验证手机短信
        /// </summary>
        public ActionResult SendVerifyMobile()
        {
            if (WorkContext.PartUserInfo.VerifyMobile == 0)
                return AjaxResult("unverifymobile", "手机号没有通过验证,所以不能发送验证短信！");

            string moibleCode = Randoms.CreateRandomValue(6);
            //发送验证手机短信
            SMSes.SendSCVerifySMS(WorkContext.UserMobile, moibleCode);
            //将验证值保存在session中
            Sessions.SetItem(WorkContext.Sid, "ucsvMoibleCode", moibleCode);

            return AjaxResult("success", "短信已经发送,请查收！");
        }

        /// <summary>
        /// 验证手机
        /// </summary>
        public ActionResult VerifyMobile()
        {
            string action = WebHelper.GetQueryString("act").ToLower();
            string moibleCode = WebHelper.GetFormString("moibleCode");
            string verifyCode = WebHelper.GetFormString("verifyCode");

            if (action.Length == 0 || !CommonHelper.IsInArray(action, new string[3] { "updatepassword", "updatemobile", "updatepaypassword" }))
                return AjaxResult("noaction", "动作不存在，请刷新再试！");
            if (WorkContext.PartUserInfo.VerifyMobile == 0)
                return AjaxResult("unverifymobile", "手机号没有通过验证,所以不能进行验证！");

            //检查验证码
            if (string.IsNullOrWhiteSpace(verifyCode))
            {
                return AjaxResult("verifycode", "验证码不能为空！");
            }
            if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
            {
                return AjaxResult("verifycode", "验证码不正确！");
            }

            //检查手机码
            if (string.IsNullOrWhiteSpace(moibleCode))
            {
                return AjaxResult("moiblecode", "手机验证码不能为空！");
            }
            if (Sessions.GetValueString(WorkContext.Sid, "ucsvMoibleCode") != moibleCode)
            {
                return AjaxResult("moiblecode", "手机验证码不正确！");
            }

            string v = MallUtils.AESEncrypt(string.Format("{0},{1},{2},{3}", WorkContext.Uid, action, DateTime.Now, Randoms.CreateRandomValue(6)));
            string url = Url.Action("safeupdate", new RouteValueDictionary { { "v", v } });
            return AjaxResult("success", url);
        }

        /// <summary>
        /// 发送验证邮箱邮件
        /// </summary>
        public ActionResult SendVerifyEmail()
        {
            string action = WebHelper.GetQueryString("act").ToLower();
            string verifyCode = WebHelper.GetFormString("verifyCode");

            if (action.Length == 0 || !CommonHelper.IsInArray(action, new string[3] { "updatepassword", "updatemobile", "updateemail" }))
                return AjaxResult("noaction", "动作不存在");
            if (WorkContext.PartUserInfo.VerifyEmail == 0)
                return AjaxResult("unverifyemail", "邮箱没有通过验证,所以不能发送验证邮件");

            //检查验证码
            if (string.IsNullOrWhiteSpace(verifyCode))
            {
                return AjaxResult("verifycode", "验证码不能为空");
            }
            if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
            {
                return AjaxResult("verifycode", "验证码不正确");
            }

            string v = MallUtils.AESEncrypt(string.Format("{0},{1},{2},{3}", WorkContext.Uid, action, DateTime.Now, Randoms.CreateRandomValue(6)));
            string url = string.Format("http://{0}{1}", Request.Url.Authority, Url.Action("safeupdate", new RouteValueDictionary { { "v", v } }));
            //发送验证邮件
            Emails.SendSCVerifyEmail(WorkContext.UserEmail, WorkContext.UserName, url);
            return AjaxResult("success", "邮件已经发送,请前往你的邮箱进行验证");
        }

        /// <summary>
        /// 安全更新
        /// </summary>
        public ActionResult SafeUpdate()
        {
            string v = WebHelper.GetQueryString("v");
            //解密字符串
            string realV = SecureHelper.AESDecrypt(v, WorkContext.MallConfig.SecretKey);

            //数组第一项为uid，第二项为动作，第三项为验证时间,第四项为随机值
            string[] result = StringHelper.SplitString(realV);
            if (result.Length != 4)
                return HttpNotFound();

            int uid = TypeHelper.StringToInt(result[0]);
            string action = result[1];
            DateTime time = TypeHelper.StringToDateTime(result[2]);

            //判断当前用户是否为验证用户
            if (uid != WorkContext.Uid)
                return HttpNotFound();
            //判断验证时间是否过时
            if (DateTime.Now.AddMinutes(-30) > time)
                return PromptView("此链接已经失效，请重新验证！");

            SafeUpdateModel model = new SafeUpdateModel();
            model.Action = action;
            model.V = WebHelper.UrlEncode(v);

            return View(model);
        }

        /// <summary>
        /// 更新密码
        /// </summary>
        public ActionResult UpdatePassword()
        {
            string v = WebHelper.GetQueryString("v");
            //解密字符串
            string realV = SecureHelper.AESDecrypt(v, WorkContext.MallConfig.SecretKey);

            //数组第一项为uid，第二项为动作，第三项为验证时间,第四项为随机值
            string[] result = StringHelper.SplitString(realV);
            if (result.Length != 4)
                return AjaxResult("noauth", "您的权限不足！");

            int uid = TypeHelper.StringToInt(result[0]);
            string action = result[1];
            DateTime time = TypeHelper.StringToDateTime(result[2]);

            //判断当前用户是否为验证用户
            if (uid != WorkContext.Uid)
                return AjaxResult("noauth", "您的权限不足！");
            //判断验证时间是否过时
            if (DateTime.Now.AddMinutes(-30) > time)
                return AjaxResult("expired", "密钥已过期,请重新验证！");

            string password = WebHelper.GetFormString("password");
            string confirmPwd = WebHelper.GetFormString("confirmPwd");
            string verifyCode = WebHelper.GetFormString("verifyCode");

            //检查验证码
            if (string.IsNullOrWhiteSpace(verifyCode))
            {
                return AjaxResult("verifycode", "验证码不能为空！");
            }
            if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
            {
                return AjaxResult("verifycode", "验证码不正确！");
            }

            //检查密码
            if (string.IsNullOrWhiteSpace(password))
            {
                return AjaxResult("password", "密码不能为空！");
            }
            if (password.Length < 4 || password.Length > 32)
            {
                return AjaxResult("password", "密码必须大于3且不大于32个字符！");
            }
            if (password != confirmPwd)
            {
                return AjaxResult("confirmpwd", "两次密码不相同！");
            }

            string p = Users.CreateUserPassword(password, WorkContext.PartUserInfo.Salt);
            //设置新密码
            Users.UpdateUserPasswordByUid(WorkContext.Uid, p);
            //同步cookie中密码
            MallUtils.SetCookiePassword(p);

            string url = Url.Action("safesuccess", new RouteValueDictionary { { "act", "updatePassword" } });
            return AjaxResult("success", url);
        }


        /// <summary>
        /// 更新支付密码
        /// </summary>
        public ActionResult UpdatePayPassword()
        {
            string v = WebHelper.GetQueryString("v");
            //解密字符串
            string realV = SecureHelper.AESDecrypt(v, WorkContext.MallConfig.SecretKey);

            //数组第一项为uid，第二项为动作，第三项为验证时间,第四项为随机值
            string[] result = StringHelper.SplitString(realV);
            if (result.Length != 4)
                return AjaxResult("noauth", "您的权限不足！");

            int uid = TypeHelper.StringToInt(result[0]);
            string action = result[1];
            DateTime time = TypeHelper.StringToDateTime(result[2]);

            //判断当前用户是否为验证用户
            if (uid != WorkContext.Uid)
                return AjaxResult("noauth", "您的权限不足！");
            //判断验证时间是否过时
            if (DateTime.Now.AddMinutes(-30) > time)
                return AjaxResult("expired", "密钥已过期,请重新验证！");

            string password = WebHelper.GetFormString("password");
            string confirmPwd = WebHelper.GetFormString("confirmPwd");
            string verifyCode = WebHelper.GetFormString("verifyCode");

            //检查验证码
            if (string.IsNullOrWhiteSpace(verifyCode))
            {
                return AjaxResult("verifycode", "验证码不能为空！");
            }
            if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
            {
                return AjaxResult("verifycode", "验证码不正确！");
            }

            //检查密码
            if (string.IsNullOrWhiteSpace(password))
            {
                return AjaxResult("password", "支付密码不能为空！");
            }
            if (password.Length < 4 || password.Length > 32)
            {
                return AjaxResult("password", "支付密码必须大于3且不大于32个字符！");
            }
            if (password != confirmPwd)
            {
                return AjaxResult("confirmpwd", "两次支付密码不相同！");
            }

            UserPayPassword upaypasswordinfo = Users.GetUserPayPasswordByUid(uid);
            //创建支付密码
            if (upaypasswordinfo == null)
            {
                string salt = Randoms.CreateRandomValue(6);
                string p = Users.CreateUserPassword(password, salt);
                Users.CreatePayPassword(WorkContext.Uid, p, salt);
            }
            //修改密码
            else
            {
                string p = Users.CreateUserPassword(password, upaypasswordinfo.Salt);
                Users.UpdatePayPasswordByUid(WorkContext.Uid, p);
            }

            string url = Url.Action("safesuccess", new RouteValueDictionary { { "act", "updatepaypassword" } });
            return AjaxResult("success", url);
        }

        /// <summary>
        /// 发送更新手机确认短信
        /// </summary>
        public ActionResult SendUpdateMobile()
        {
            string v = WebHelper.GetQueryString("v");
            //解密字符串
            string realV = SecureHelper.AESDecrypt(v, WorkContext.MallConfig.SecretKey);

            //数组第一项为uid，第二项为动作，第三项为验证时间,第四项为随机值
            string[] result = StringHelper.SplitString(realV);
            if (result.Length != 4)
                return AjaxResult("noauth", "您的权限不足！");

            int uid = TypeHelper.StringToInt(result[0]);
            string action = result[1];
            DateTime time = TypeHelper.StringToDateTime(result[2]);

            //判断当前用户是否为验证用户
            if (uid != WorkContext.Uid)
                return AjaxResult("noauth", "您的权限不足！");
            //判断验证时间是否过时
            if (DateTime.Now.AddMinutes(-30) > time)
                return AjaxResult("expired", "密钥已过期,请重新验证！");

            string mobile = WebHelper.GetFormString("mobile");

            //检查手机号
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return AjaxResult("mobile", "手机号不能为空！");
            }
            if (!ValidateHelper.IsMobile(mobile))
            {
                return AjaxResult("mobile", "手机号格式不正确！");
            }
            int tempUid = Users.GetUidByMobile(mobile);
            if (tempUid > 0 && tempUid != WorkContext.Uid)
                return AjaxResult("mobile", "手机号已经存在！");

            string mobileCode = Randoms.CreateRandomValue(6);
            //发送短信
            SMSes.SendSCUpdateSMS(mobile, mobileCode);
            //将验证值保存在session中
            Sessions.SetItem(WorkContext.Sid, "ucsuMobile", mobile);
            Sessions.SetItem(WorkContext.Sid, "ucsuMobileCode", mobileCode);

            return AjaxResult("success", "短信已发送,请查收");
        }

        /// <summary>
        /// 更新手机号
        /// </summary>
        public ActionResult UpdateMobile()
        {
            string v = WebHelper.GetQueryString("v");
            //解密字符串
            string realV = SecureHelper.AESDecrypt(v, WorkContext.MallConfig.SecretKey);

            //数组第一项为uid，第二项为动作，第三项为验证时间,第四项为随机值
            string[] result = StringHelper.SplitString(realV);
            if (result.Length != 4)
                return AjaxResult("noauth", "您的权限不足！");

            int uid = TypeHelper.StringToInt(result[0]);
            string action = result[1];
            DateTime time = TypeHelper.StringToDateTime(result[2]);

            //判断当前用户是否为验证用户
            if (uid != WorkContext.Uid)
                return AjaxResult("noauth", "您的权限不足！");
            //判断验证时间是否过时
            if (DateTime.Now.AddMinutes(-30) > time)
                return AjaxResult("expired", "密钥已过期,请重新验证！");

            string mobile = WebHelper.GetFormString("mobile");
            string moibleCode = WebHelper.GetFormString("moibleCode");
            string verifyCode = WebHelper.GetFormString("verifyCode");

            //检查验证码
            if (string.IsNullOrWhiteSpace(verifyCode))
            {
                return AjaxResult("verifycode", "验证码不能为空！");
            }
            if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
            {
                return AjaxResult("verifycode", "验证码不正确！");
            }
            var sessions = Sessions.GetSession(WorkContext.Sid);
            string ucsuMobile = sessions["ucsuMobile"].ToString();
            string ucsvMoibleCode = sessions["ucsvMoibleCode"].ToString();
            //检查手机号
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return AjaxResult("mobile", "手机号不能为空！");
            }
            if (Sessions.GetValueString(WorkContext.Sid, "ucsuMobile") != mobile)
            {
                return AjaxResult("mobile", "与验证的手机号不一致！");
            }

            //检查手机码
            if (string.IsNullOrWhiteSpace(moibleCode))
            {
                return AjaxResult("moiblecode", "短信验证码不能为空！");
            }
            if (Sessions.GetValueString(WorkContext.Sid, "ucsuMobileCode") != moibleCode)
            {
                return AjaxResult("moiblecode", "手机验证码不正确！");
            }

            //更新手机号
            string oldusername=Users.GetPartUserById(WorkContext.Uid).UserName;
            Users.UpdateUserMobileByUid(WorkContext.Uid, mobile);
            //修改账号log
            string title="用户id："+WorkContext.Uid+"将用户名"+oldusername+"修改为"+mobile;
            EventLogs.CreateEventLog("updateUsername", title, Environment.MachineName, DateTime.Now);
            //发放验证手机积分
            Credits.SendVerifyMobileCredits(ref WorkContext.PartUserInfo, DateTime.Now);

            string url = Url.Action("safesuccess", new RouteValueDictionary { { "act", "updateMobile" } });
            return AjaxResult("success", url);
        }

        /// <summary>
        /// 发送更新邮箱确认邮件
        /// </summary>
        public ActionResult SendUpdateEmail()
        {
            string v = WebHelper.GetQueryString("v");
            //解密字符串
            string realV = SecureHelper.AESDecrypt(v, WorkContext.MallConfig.SecretKey);

            //数组第一项为uid，第二项为动作，第三项为验证时间,第四项为随机值
            string[] result = StringHelper.SplitString(realV);
            if (result.Length != 4)
                return AjaxResult("noauth", "您的权限不足");

            int uid = TypeHelper.StringToInt(result[0]);
            string action = result[1];
            DateTime time = TypeHelper.StringToDateTime(result[2]);

            //判断当前用户是否为验证用户
            if (uid != WorkContext.Uid)
                return AjaxResult("noauth", "您的权限不足");
            //判断验证时间是否过时
            if (DateTime.Now.AddMinutes(-30) > time)
                return AjaxResult("expired", "密钥已过期,请重新验证");

            string email = WebHelper.GetFormString("email");
            string verifyCode = WebHelper.GetFormString("verifyCode");

            //检查验证码
            if (string.IsNullOrWhiteSpace(verifyCode))
            {
                return AjaxResult("verifycode", "验证码不能为空");
            }
            if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
            {
                return AjaxResult("verifycode", "验证码不正确");
            }

            //检查邮箱
            if (string.IsNullOrWhiteSpace(email))
            {
                return AjaxResult("email", "邮箱不能为空");
            }
            if (!ValidateHelper.IsEmail(email))
            {
                return AjaxResult("email", "邮箱格式不正确");
            }
            if (!SecureHelper.IsSafeSqlString(email, false))
            {
                return AjaxResult("email", "邮箱已经存在");
            }
            int tempUid = Users.GetUidByEmail(email);
            if (tempUid > 0 && tempUid != WorkContext.Uid)
                return AjaxResult("email", "邮箱已经存在");


            string v2 = MallUtils.AESEncrypt(string.Format("{0},{1},{2},{3}", WorkContext.Uid, email, DateTime.Now, Randoms.CreateRandomValue(6)));
            string url = string.Format("http://{0}{1}", Request.Url.Authority, Url.Action("updateemail", new RouteValueDictionary { { "v", v2 } }));

            //发送验证邮件
            Emails.SendSCUpdateEmail(email, WorkContext.UserName, url);
            return AjaxResult("success", "邮件已经发送，请前往你的邮箱进行验证");
        }

        /// <summary>
        /// 更新邮箱
        /// </summary>
        public ActionResult UpdateEmail()
        {
            string v = WebHelper.GetQueryString("v");
            //解密字符串
            string realV = SecureHelper.AESDecrypt(v, WorkContext.MallConfig.SecretKey);

            //数组第一项为uid，第二项为邮箱名，第三项为验证时间,第四项为随机值
            string[] result = StringHelper.SplitString(realV);
            if (result.Length != 4)
                return HttpNotFound();

            int uid = TypeHelper.StringToInt(result[0]);
            string email = result[1];
            DateTime time = TypeHelper.StringToDateTime(result[2]);

            //判断当前用户是否为验证用户
            if (uid != WorkContext.Uid)
                return HttpNotFound();
            //判断验证时间是否过时
            if (DateTime.Now.AddMinutes(-30) > time)
                return PromptView("此链接已经失效，请重新验证");
            int tempUid = Users.GetUidByEmail(email);
            if (tempUid > 0 && tempUid != WorkContext.Uid)
                return PromptView("此链接已经失效，邮箱已经存在");

            //更新邮箱名
            Users.UpdateUserEmailByUid(WorkContext.Uid, email);
            //发放验证邮箱积分
            Credits.SendVerifyEmailCredits(ref WorkContext.PartUserInfo, DateTime.Now);

            return RedirectToAction("safesuccess", new RouteValueDictionary { { "act", "updateEmail" }, { "remark", email } });
        }

        /// <summary>
        /// 安全成功
        /// </summary>
        public ActionResult SafeSuccess()
        {
            string action = WebHelper.GetQueryString("act").ToLower();
            string remark = WebHelper.GetQueryString("remark");

            if (action.Length == 0 || !CommonHelper.IsInArray(action, new string[3] { "updatepassword", "updatemobile", "updatepaypassword" }))
                return HttpNotFound();

            SafeSuccessModel model = new SafeSuccessModel();
            model.Action = action;
            model.Remark = remark;

            return View(model);
        }

        #endregion

        #region 订单

        /// <summary>
        /// 订单列表
        /// </summary>
        public ActionResult OrderList()
        {
            int page = WebHelper.GetQueryInt("page");
            string startAddTime = WebHelper.GetQueryString("startAddTime");
            string endAddTime = WebHelper.GetQueryString("endAddTime");
            int orderState = WebHelper.GetQueryInt("orderState");
            string keyword = WebHelper.GetQueryString("keyword");
            PageModel pageModel = new PageModel(10, page, Orders.GetUserOrderCount(WorkContext.Uid, startAddTime, endAddTime, orderState,keyword.Trim()));
            
            DataTable orderList = Orders.GetUserOrderList(WorkContext.Uid, pageModel.PageSize, pageModel.PageNumber, startAddTime, endAddTime, orderState,keyword.Trim());
            StringBuilder oidList = new StringBuilder();
            foreach (DataRow row in orderList.Rows)
            {
                oidList.AppendFormat("{0},", row["oid"]);
            }
            if (oidList.Length > 0)
                oidList.Remove(oidList.Length - 1, 1);

            OrderListModel model = new OrderListModel()
            {
                PageModel = pageModel,
                OrderList = orderList,
                OrderProductList = Orders.GetOrderProductList(oidList.ToString()),
                StartAddTime = startAddTime,
                EndAddTime = endAddTime,
                OrderState = orderState,
                KeyWord=keyword
            };

            return View(model);
        }

        /// <summary>
        /// 订单列表
        /// </summary>
        public ActionResult AjaxOrderList()
        {
            int page = WebHelper.GetQueryInt("page");
            string startAddTime = WebHelper.GetQueryString("startAddTime");
            string endAddTime = WebHelper.GetQueryString("endAddTime");
            int orderState = WebHelper.GetQueryInt("orderState");
            string keyword = WebHelper.GetQueryString("keyword");

            PageModel pageModel = new PageModel(10, page, Orders.GetUserOrderCount(WorkContext.Uid, startAddTime, endAddTime, orderState,keyword));

            DataTable orderList = Orders.GetUserOrderList(WorkContext.Uid, pageModel.PageSize, pageModel.PageNumber, startAddTime, endAddTime, orderState,keyword);
            StringBuilder oidList = new StringBuilder();
            foreach (DataRow row in orderList.Rows)
            {
                oidList.AppendFormat("{0},", row["oid"]);
            }
            if (oidList.Length > 0)
                oidList.Remove(oidList.Length - 1, 1);

            AjaxOrderListModel model = new AjaxOrderListModel()
            {
                PageModel = pageModel,
                OrderList = CommonHelper.DataTableToList(orderList),
                OrderProductList = Orders.GetOrderProductList(oidList.ToString())
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 订单信息
        /// </summary>
        public ActionResult OrderInfo()
        {
            int oid = WebHelper.GetQueryInt("oid");
            OrderInfo orderInfo = Orders.GetOrderByOid(oid);
            if (orderInfo == null || orderInfo.Uid != WorkContext.Uid)
                return PromptView("订单不存在");

            OrderInfoModel model = new OrderInfoModel();
            model.OrderInfo = orderInfo;
            model.RegionInfo = Regions.GetRegionById(orderInfo.RegionId);
            model.OrderProductList = AdminOrders.GetOrderProductList(oid);

            return View(model);
        }

        /// <summary>
        /// 订单动作列表
        /// </summary>
        public ActionResult OrderActionList()
        {
            int oid = WebHelper.GetQueryInt("oid");
            OrderInfo orderInfo = Orders.GetOrderByOid(oid);
            if (orderInfo == null || orderInfo.Uid != WorkContext.Uid)
                return PromptView("订单不存在");

            OrderActionListModel model = new OrderActionListModel();
            model.OrderInfo = orderInfo;
            model.OrderActionList = OrderActions.GetOrderActionList(oid);
            return View(model);
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        public ActionResult CancelOrder()
        {
            int oid = WebHelper.GetFormInt("oid");
            int cancelReason = WebHelper.GetFormInt("cancelReason");

            OrderInfo orderInfo = Orders.GetOrderByOid(oid);
            if (orderInfo == null || orderInfo.Uid != WorkContext.Uid)
                return AjaxResult("noorder", "订单不存在");

            if (!(orderInfo.OrderState == (int)OrderState.WaitPaying || (orderInfo.OrderState == (int)OrderState.Confirming && orderInfo.PayMode == 0)))
                return AjaxResult("donotcancel", "订单当前不能取消");

            //取消订单
            Orders.CancelOrder(ref WorkContext.PartUserInfo, orderInfo, WorkContext.Uid, DateTime.Now);
            //创建订单处理
            OrderActions.CreateOrderAction(new OrderActionInfo()
            {
                Oid = oid,
                Uid = WorkContext.Uid,
                RealName = "本人",
                ActionType = (int)OrderActionType.Cancel,
                ActionTime = DateTime.Now,
                ActionDes = "您取消了订单"
            });
            return AjaxResult("success", oid.ToString());
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <returns></returns>
        public ActionResult ReceiveProduct()
        {
            int oid = WebHelper.GetFormInt("oid");

            OrderInfo orderInfo = Orders.GetOrderByOid(oid);
            if (orderInfo == null || orderInfo.Uid != WorkContext.Uid)
            {
                return AjaxResult("noorder", "订单不存在！");
            }

            if (orderInfo.OrderState != (int)OrderState.Sended || orderInfo.PayMode==0)
            {
                return AjaxResult("donotreceive", "该订单当前不能执行确认收货操作！");
            }

            //确认收货
            Orders.ReceiveOrder(oid);
            //创建订单处理
            OrderActions.CreateOrderAction(new OrderActionInfo()
            {
                Oid = oid,
                Uid = WorkContext.Uid,
                RealName = "本人",
                ActionType = (int)OrderActionType.Receive,
                ActionTime = DateTime.Now,
                ActionDes = "您确认了收货，订单已完成"
            });
            return AjaxResult("success", oid.ToString());
        }


        /// <summary>
        /// 商城余额支付
        /// </summary>
        /// <param name="oid">订单id</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CreditPay(string oidList, string password)
        {
            if (string.IsNullOrWhiteSpace(oidList))
            {
                return Json("订单信息错误！", JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                return Json("请输入支付密码！", JsonRequestBehavior.AllowGet);
            }
            UserPayPassword paypasswordinfo = Users.GetUserPayPasswordByUid(WorkContext.Uid);
            if (paypasswordinfo == null)
            {
                return Json("您还没有设置支付密码，请在个人中心—>账户安全中设置支付密码！", JsonRequestBehavior.AllowGet);
            }
            //判断密码是否正确
            if (Users.CreateUserPassword(password, paypasswordinfo.Salt) != paypasswordinfo.Password)
            {
                return Json("支付密码错误！", JsonRequestBehavior.AllowGet);
            }

            List<OrderInfo> orderList = new List<OrderInfo>();
            decimal allPayMoney = 0;
            string osns = "";
            foreach (string oid in StringHelper.SplitString(oidList))
            {
                //订单信息
                OrderInfo orderInfo = Orders.GetOrderByOid(TypeHelper.StringToInt(oid));
                if (orderInfo == null || orderInfo.Uid != WorkContext.Uid || orderInfo.OrderState != (int)OrderState.WaitPaying || orderInfo.PaySystemName != "creditpay")
                {
                    return Json("订单信息错误！", JsonRequestBehavior.AllowGet);
                }
                orderList.Add(orderInfo);
                allPayMoney += orderInfo.SurplusMoney;
                osns += orderInfo.OSN + ",";
            }
            if (allPayMoney > WorkContext.UserAmount)
            {
                return Json("余额不足，无法支付！", JsonRequestBehavior.AllowGet);
            }

            //余额支付记录
            CreditLogInfo loginfo = new CreditLogInfo()
            {
                Uid = WorkContext.Uid,
                RankCredits = 0,
                Action = (int)CreditAction.UserOrderCreditPay,
                ActionCode = 0,
                ActionDes = "您使用余额支付订单：" + allPayMoney + "元，订单号为：" + osns.TrimEnd(','),
                Operator = WorkContext.Uid,
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
            return Json("succ", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 商品收藏夹

        /// <summary>
        /// 收藏夹商品列表
        /// </summary>
        public ActionResult AjaxFavoriteProductList()
        {
            int page = WebHelper.GetQueryInt("page");//当前页数

            PageModel pageModel = new PageModel(5, page, FavoriteProducts.GetFavoriteProductCount(WorkContext.Uid));
            AjaxFavoriteProductListModel model = new AjaxFavoriteProductListModel()
            {
                ProductList = CommonHelper.DataTableToList(FavoriteProducts.GetFavoriteProductList(pageModel.PageSize, pageModel.PageNumber, WorkContext.Uid)),
                PageModel = pageModel
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加商品到收藏夹
        /// </summary>
        public ActionResult AddProductToFavorite()
        {
            //商品id
            int pid = WebHelper.GetQueryInt("pid");
            //商品信息
            PartProductInfo partProductInfo = Products.GetPartProductById(pid);
            if (partProductInfo == null)
                return AjaxResult("noproduct", "请选择商品");
            //店铺信息
            StoreInfo storeInfo = Stores.GetStoreById(partProductInfo.StoreId);
            if (storeInfo.State != (int)StoreState.Open)
                return AjaxResult("nostore", "店铺不存在");

            //当收藏夹中已经存在此商品时
            if (FavoriteProducts.IsExistFavoriteProduct(WorkContext.Uid, pid))
                return AjaxResult("exist", "商品已经存在");

            //收藏夹已满时
            if (WorkContext.MallConfig.FavoriteProductCount <= FavoriteProducts.GetFavoriteProductCount(WorkContext.Uid))
                return AjaxResult("full", "收藏夹已满");

            bool result = FavoriteProducts.AddProductToFavorite(WorkContext.Uid, pid, 0, DateTime.Now);

            if (result)//添加成功
                return AjaxResult("success", "收藏成功");
            else//添加失败
                return AjaxResult("error", "收藏失败");
        }

        /// <summary>
        /// 删除收藏夹中的商品
        /// </summary>
        public ActionResult DelFavoriteProduct()
        {
            int pid = WebHelper.GetQueryInt("pid");//商品id
            bool result = FavoriteProducts.DeleteFavoriteProductByUidAndPid(WorkContext.Uid, pid);
            if (result)//删除成功
                return AjaxResult("success", pid.ToString());
            else//删除失败
                return AjaxResult("error", "删除失败");
        }

        #endregion

        #region 店铺收藏夹

        /// <summary>
        /// 收藏夹店铺列表
        /// </summary>
        public ActionResult AjaxFavoriteStoreList()
        {
            int page = WebHelper.GetQueryInt("page");//当前页数

            PageModel pageModel = new PageModel(10, page, FavoriteStores.GetFavoriteStoreCount(WorkContext.Uid));
            AjaxFavoriteStoreListModel model = new AjaxFavoriteStoreListModel()
            {
                StoreList = CommonHelper.DataTableToList(FavoriteStores.GetFavoriteStoreList(pageModel.PageSize, pageModel.PageNumber, WorkContext.Uid)),
                PageModel = pageModel
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加店铺到收藏夹
        /// </summary>
        public ActionResult AddStoreToFavorite()
        {
            //店铺id
            int storeId = WebHelper.GetQueryInt("storeId");
            //店铺信息
            StoreInfo storeInfo = Stores.GetStoreById(storeId);
            if (storeInfo == null || storeInfo.State != (int)StoreState.Open)
                return AjaxResult("nostore", "店铺不存在");

            //当收藏夹中已经存在此店铺时
            if (FavoriteStores.IsExistFavoriteStore(WorkContext.Uid, storeId))
                return AjaxResult("exist", "店铺已经存在");

            //收藏夹已满时
            if (WorkContext.MallConfig.FavoriteStoreCount <= FavoriteStores.GetFavoriteStoreCount(WorkContext.Uid))
                return AjaxResult("full", "收藏夹已满");

            bool result = FavoriteStores.AddStoreToFavorite(WorkContext.Uid, storeId, DateTime.Now);

            if (result)//添加成功
                return AjaxResult("success", "收藏成功");
            else//添加失败
                return AjaxResult("error", "收藏失败");
        }

        /// <summary>
        /// 删除收藏夹中的店铺
        /// </summary>
        public ActionResult DelFavoriteStore()
        {
            int storeId = WebHelper.GetQueryInt("storeId");//店铺id
            bool result = FavoriteStores.DeleteFavoriteStoreByUidAndStoreId(WorkContext.Uid, storeId);
            if (result)//删除成功
                return AjaxResult("success", storeId.ToString());
            else//删除失败
                return AjaxResult("error", "删除失败");
        }

        #endregion

        #region 浏览商品

        /// <summary>
        /// 浏览商品列表
        /// </summary>
        public ActionResult AjaxBrowseProductList()
        {
            int page = WebHelper.GetQueryInt("page");//当前页数

            PageModel pageModel = new PageModel(10, page, BrowseHistories.GetUserBrowseProductCount(WorkContext.Uid));
            AjaxBrowseProductListModel model = new AjaxBrowseProductListModel()
            {
                ProductList = BrowseHistories.GetUserBrowseProductList(pageModel.PageSize, pageModel.PageNumber, WorkContext.Uid),
                PageModel = pageModel
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 配送地址

        /// <summary>
        /// 配送地址列表
        /// </summary>
        /// <returns></returns>
        public ActionResult AjaxShipAddressList()
        {
            List<FullShipAddressInfo> shipAddressList = ShipAddresses.GetFullShipAddressList(WorkContext.Uid);
            int shipAddressCount = shipAddressList.Count;

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"count\":");
            sb.AppendFormat("\"{0}\",\"list\":[", shipAddressCount);
            foreach (FullShipAddressInfo fullShipAddressInfo in shipAddressList)
            {
                sb.AppendFormat("{0}\"saId\":\"{1}\",\"user\":\"{2}&nbsp;&nbsp;&nbsp;{3}\",\"address\":\"{4}&nbsp;{5}&nbsp;{6}&nbsp;{7}\"{8},", "{", fullShipAddressInfo.SAId, fullShipAddressInfo.Consignee, fullShipAddressInfo.Mobile.Length > 0 ? fullShipAddressInfo.Mobile : fullShipAddressInfo.Phone, fullShipAddressInfo.ProvinceName, fullShipAddressInfo.CityName, fullShipAddressInfo.CountyName, fullShipAddressInfo.Address, "}");
            }
            if (shipAddressCount > 0)
                sb.Remove(sb.Length - 1, 1);
            sb.Append("]}");

            return AjaxResult("success", sb.ToString(), true);
        }

        /// <summary>
        /// 配送地址列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ShipAddressList()
        {
            ShipAddressListModel model = new ShipAddressListModel();

            model.ShipAddressList = ShipAddresses.GetFullShipAddressList(WorkContext.Uid);
            model.ShipAddressCount = model.ShipAddressList.Count;

            return View(model);
        }

        /// <summary>
        /// 添加配送地址
        /// </summary>
        public ActionResult AddShipAddress()
        {
            if (WebHelper.IsGet())
            {
                ShipAddressModel model = new ShipAddressModel();
                return View(model);
            }
            else
            {
                int regionId = WebHelper.GetFormInt("regionId");
                string consignee = WebHelper.GetFormString("consignee");
                string mobile = WebHelper.GetFormString("mobile");
                string phone = WebHelper.GetFormString("phone");
                string email = WebHelper.GetFormString("email");
                string zipcode = WebHelper.GetFormString("zipcode");
                string address = WebHelper.GetFormString("address");
                int isDefault = WebHelper.GetFormInt("isDefault");

                string verifyResult = VerifyShipAddress(regionId, consignee, mobile, phone, email, zipcode, address);

                if (verifyResult.Length == 0)
                {
                    //检查配送地址数量是否达到系统所允许的最大值
                    int shipAddressCount = ShipAddresses.GetShipAddressCount(WorkContext.Uid);
                    if (shipAddressCount >= WorkContext.MallConfig.MaxShipAddress)
                        return AjaxResult("full", "配送地址的数量已经达到系统所允许的最大值");

                    ShipAddressInfo shipAddressInfo = new ShipAddressInfo();
                    shipAddressInfo.Uid = WorkContext.Uid;
                    shipAddressInfo.RegionId = regionId;
                    shipAddressInfo.IsDefault = isDefault == 0 ? 0 : 1;
                    shipAddressInfo.Alias = WebHelper.HtmlEncode(consignee);
                    shipAddressInfo.Consignee = WebHelper.HtmlEncode(consignee);
                    shipAddressInfo.Mobile = mobile;
                    shipAddressInfo.Phone = phone;
                    shipAddressInfo.Email = email;
                    shipAddressInfo.ZipCode = zipcode;
                    shipAddressInfo.Address = WebHelper.HtmlEncode(address);
                    int saId = ShipAddresses.CreateShipAddress(shipAddressInfo);
                    return AjaxResult("success", saId.ToString());
                }
                else
                {
                    return AjaxResult("error", verifyResult, true);
                }
            }
        }

        /// <summary>
        /// 编辑配送地址
        /// </summary>
        public ActionResult EditShipAddress()
        {
            if (WebHelper.IsGet())
            {
                int saId = WebHelper.GetQueryInt("saId");
                FullShipAddressInfo fullShipAddressInfo = ShipAddresses.GetFullShipAddressBySAId(saId, WorkContext.Uid);
                if (fullShipAddressInfo == null)
                    return PromptView(Url.Action("shipaddresslist"), "地址不存在");

                ShipAddressModel model = new ShipAddressModel();
                model.Alias = fullShipAddressInfo.Alias;
                model.Consignee = fullShipAddressInfo.Consignee;
                model.Mobile = fullShipAddressInfo.Mobile;
                model.Phone = fullShipAddressInfo.Phone;
                model.Email = fullShipAddressInfo.Email;
                model.ZipCode = fullShipAddressInfo.ZipCode;
                model.ProvinceId = fullShipAddressInfo.ProvinceId;
                model.CityId = fullShipAddressInfo.CityId;
                model.RegionId = fullShipAddressInfo.RegionId;
                model.Address = fullShipAddressInfo.Address;
                model.IsDefault = fullShipAddressInfo.IsDefault;

                return View(model);
            }
            else
            {
                int saId = WebHelper.GetQueryInt("saId");
                int regionId = WebHelper.GetFormInt("regionId");
                string consignee = WebHelper.GetFormString("consignee");
                string mobile = WebHelper.GetFormString("mobile");
                string phone = WebHelper.GetFormString("phone");
                string email = WebHelper.GetFormString("email");
                string zipcode = WebHelper.GetFormString("zipcode");
                string address = WebHelper.GetFormString("address");
                int isDefault = WebHelper.GetFormInt("isDefault");

                string verifyResult = VerifyShipAddress(regionId, consignee, mobile, phone, email, zipcode, address);
                if (verifyResult.Length == 0)
                {
                    ShipAddressInfo shipAddressInfo = ShipAddresses.GetShipAddressBySAId(saId, WorkContext.Uid);
                    //检查地址
                    if (shipAddressInfo == null)
                        return AjaxResult("noexist", "配送地址不存在");

                    shipAddressInfo.Uid = WorkContext.Uid;
                    shipAddressInfo.RegionId = regionId;
                    shipAddressInfo.IsDefault = isDefault == 0 ? 0 : 1;
                    shipAddressInfo.Alias = WebHelper.HtmlEncode(consignee);
                    shipAddressInfo.Consignee = WebHelper.HtmlEncode(consignee);
                    shipAddressInfo.Mobile = mobile;
                    shipAddressInfo.Phone = phone;
                    shipAddressInfo.Email = email;
                    shipAddressInfo.ZipCode = zipcode;
                    shipAddressInfo.Address = WebHelper.HtmlEncode(address);
                    ShipAddresses.UpdateShipAddress(shipAddressInfo);
                    return AjaxResult("success", "编辑成功");
                }
                else
                {
                    return AjaxResult("error", verifyResult, true);
                }
            }
        }

        /// <summary>
        /// 删除配送地址
        /// </summary>
        public ActionResult DelShipAddress()
        {
            int saId = WebHelper.GetQueryInt("saId");
            bool result = ShipAddresses.DeleteShipAddress(saId, WorkContext.Uid);
            if (result)//删除成功
                return AjaxResult("success", saId.ToString());
            else//删除失败
                return AjaxResult("error", "删除失败");
        }

        /// <summary>
        /// 设置默认配送地址
        /// </summary>
        public ActionResult SetDefaultShipAddress()
        {
            int saId = WebHelper.GetQueryInt("saId");
            bool result = ShipAddresses.UpdateShipAddressIsDefault(saId, WorkContext.Uid, 1);
            if (result)//设置成功
                return AjaxResult("success", saId.ToString());
            else//设置失败
                return AjaxResult("error", "设置失败");
        }

        /// <summary>
        /// 验证配送地址
        /// </summary>
        private string VerifyShipAddress(int regionId,string consignee, string mobile, string phone, string email, string zipcode, string address)
        {
            StringBuilder errorList = new StringBuilder("[");

            //检查区域
            RegionInfo regionInfo = Regions.GetRegionById(regionId);
            if (regionInfo == null || regionInfo.Layer != 3)
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "regionId", "请选择有效的区域", "}");

            //检查收货人
            if (string.IsNullOrWhiteSpace(consignee))
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "consignee", "收货人不能为空", "}");
            else if (consignee.Length > 10)
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "consignee", "最多只能输入10个字", "}");

            //检查手机号和固话号
            if (string.IsNullOrWhiteSpace(mobile) && string.IsNullOrWhiteSpace(phone))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "mobile", "手机号和固话号必填一项", "}");
            }
            else
            {
                if (!ValidateHelper.IsMobile(mobile))
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "mobile", "手机号格式不正确", "}");
                if (!ValidateHelper.IsPhone(phone))
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "phone", "固话号格式不正确", "}");
            }

            //检查邮箱
            if (!ValidateHelper.IsEmail(email))
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "email", "邮箱格式不正确", "}");

            //检查邮编
            if (!ValidateHelper.IsZipCode(zipcode))
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "zipcode", "邮编格式不正确", "}");

            //检查详细地址
            if (string.IsNullOrWhiteSpace(address))
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "address", "详细地址不能为空", "}");
            else if (address.Length > 75)
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "address", "最多只能输入75个字", "}");

            if (errorList.Length > 1)
                return errorList.Remove(errorList.Length - 1, 1).Append("]").ToString();
            else
                return "";
        }

        #endregion

        #region 用户资产

        /// <summary>
        /// 我的资产
        /// </summary>
        public ActionResult PayCredit()
        {
            int page = WebHelper.GetQueryInt("page");

            PageModel pageModel = new PageModel(15, page, Credits.GetUserAmountLogCount(WorkContext.Uid));
            UserAmountLogModel model = new UserAmountLogModel()
            {
                PageModel = pageModel,
                PayCreditLogList = Credits.GetUserAmountLogList(WorkContext.Uid, pageModel.PageSize, pageModel.PageNumber)
            };

            return View(model);
        }

        /// <summary>
        /// 我的资产列表
        /// </summary>
        public ActionResult AjaxPayCredit()
         {
            int page = WebHelper.GetQueryInt("page");

            PageModel pageModel = new PageModel(15, page, Credits.GetUserAmountLogCount(WorkContext.Uid));
            UserAmountLogModel model = new UserAmountLogModel()
            {
                PageModel = pageModel,
                PayCreditLogList = Credits.GetUserAmountLogList(WorkContext.Uid, pageModel.PageSize, pageModel.PageNumber)
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 申请提现列表
        /// </summary>
        public ActionResult WithdrawlList()
        {
            int page = WebHelper.GetQueryInt("page");
            PageModel pageModel = new PageModel(15, page, Credits.GetWithdrawalLogCount(WorkContext.Uid, 0, 0));
            WithdrawalLogListModel model = new WithdrawalLogListModel()
            {
                PageModel = pageModel,
                WithdrawalLogList = Credits.GetWithdrawalLogList(WorkContext.Uid, 0, 0, pageModel.PageNumber, pageModel.PageSize)
            };
            return View(model);
        }
        /// <summary>
        /// 申请提现列表
        /// </summary>
        public ActionResult AjaxWithdrawlList()
        {
            int page = WebHelper.GetQueryInt("page");
            PageModel pageModel = new PageModel(15, page, Credits.GetWithdrawalLogCount(WorkContext.Uid, 0, 0));
            WithdrawalLogListModel model = new WithdrawalLogListModel()
            {
                PageModel = pageModel,
                WithdrawalLogList = Credits.GetWithdrawalLogList(WorkContext.Uid, 0, 0, pageModel.PageNumber, pageModel.PageSize)
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提现详情
        /// </summary>
        /// <returns></returns>
        public ActionResult WithdrawalDetail(int wid)
        {
            WithdrawalLogInfo info = Credits.GetWithdrawalLogById(wid);
            if (info == null || info.Uid != WorkContext.Uid)
            {
                return PromptView("该提现申请不存在！");
            }
            return View(info);
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        public ActionResult WithdrawlApply()
        {
            if (WorkContext.UserAmount < BMAConfig.CreditConfig.MinAmount)
            {
                return PromptView("您还没有满足申请提现的条件！");
            }
            var list = Credits.GetWithdrawalLogList(WorkContext.Uid, (int)WithdrawalState.applying, 0, 1, 1);
            if (list != null && list.Count > 0)
            {
                return PromptView("您已经有在审核中的提现申请了，当前不能申请提现！");
            }
            List<SelectListItem> itemList = new List<SelectListItem>();
            itemList.Add(new SelectListItem() { Text = "支付宝", Value = ((int)WithdrawalType.Alipay).ToString() });
            itemList.Add(new SelectListItem() { Text = "银行卡", Value = ((int)WithdrawalType.BankCard).ToString() });
            ViewData["WithdrawalType"] = itemList;
            WithdrawalLogModel model = new WithdrawalLogModel();
            return View(model);
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WithdrawlApply(WithdrawalLogModel model)
        {
            if (WorkContext.UserAmount < BMAConfig.CreditConfig.MinAmount)
            {
                return PromptView("您还没有满足申请提现的条件！");
            }
            var list = Credits.GetWithdrawalLogList(WorkContext.Uid, (int)WithdrawalState.applying, 0, 1, 1);
            if (list != null && list.Count > 0)
            {
                return PromptView("您已经有在审核中的提现申请了，当前不能申请提现！");
            }
            if (model.ApplyAmount < BMAConfig.CreditConfig.MinWithdrawal)
            {
                ModelState.AddModelError("ApplyAmount", "申请提现的金额不能低于" + BMAConfig.CreditConfig.MinWithdrawal.ToString() + "元");
            }
            if (model.ApplyAmount > WorkContext.UserAmount)
            {
                return PromptView("您剩余的提现额度不足！"); 
            }
            if (!ModelState.IsValid)
            {
                List<SelectListItem> itemList = new List<SelectListItem>();
                itemList.Add(new SelectListItem() { Text = "支付宝", Value = ((int)WithdrawalType.Alipay).ToString() });
                itemList.Add(new SelectListItem() { Text = "银行卡", Value = ((int)WithdrawalType.BankCard).ToString() });
                ViewData["WithdrawalType"] = itemList;
                return View(model);
            }
            WithdrawalLogInfo winfo = new WithdrawalLogInfo()
            {
                ApplyAmount = model.ApplyAmount,
                ApplyRemark = model.ApplyRemark,
                ApplyTime = DateTime.Now,
                PayAccount = model.PayAccount,
                PayType = model.PayType,
                Phone = model.Phone,
                State = (int)WithdrawalState.applying,
                OperatTime = DateTime.Now,
                Uid = WorkContext.Uid
            };
            Credits.CreateWithdrawalLog(winfo);
            return RedirectToAction("WithdrawlList");
        }

        #endregion

        #region 优惠劵

        /// <summary>
        /// 优惠劵列表
        /// </summary>
        public ActionResult CouponList()
        {
            int type = WebHelper.GetQueryInt("type");

            CouponListModel model = new CouponListModel()
            {
                ListType = type,
                CouponList = Coupons.GetCouponList(WorkContext.Uid, type)
            };

            return View(model);
        }

        #endregion

        #region  订单评价

        /// <summary>
        /// 评价订单
        /// </summary>
        public ActionResult ReviewOrder()
        {
            int oid = WebHelper.GetQueryInt("oid");

            OrderInfo orderInfo = Orders.GetOrderByOid(oid);
            if (orderInfo == null || orderInfo.Uid != WorkContext.Uid)
            {
                return PromptView("订单不存在");
            }
            if (orderInfo.OrderState != (int)OrderState.Received && orderInfo.OrderState != (int)OrderState.Complete)
            {
                return PromptView("订单当前不能评价");
            }
            if (orderInfo.IsReview == 1)
            {
                return PromptView("此订单已经评价");
            }

            ReviewOrderModel model = new ReviewOrderModel()
            {
                OrderInfo = orderInfo,
                OrderProductList = Orders.GetOrderProductList(oid),
                StoreReviewInfo = Stores.GetStoreReviewByOid(oid)
            };
            return View(model);
        }


        /// <summary>
        /// 评价订单
        /// </summary>
        public ActionResult ReviewOrderPost()
        {
            int oid = WebHelper.GetQueryInt("oid");//订单id
            int descriptionStar = WebHelper.GetFormInt("descriptionStar");//商品描述星星
            int serviceStar = WebHelper.GetFormInt("serviceStar");//商家服务星星
            int shipStar = WebHelper.GetFormInt("shipStar");//商家配送星星
            string stars = WebHelper.GetFormString("stars");//星星
            string messages = WebHelper.GetFormString("messages");//评价内容
            string opids = WebHelper.GetFormString("opids");//评价内容

            OrderInfo orderInfo = Orders.GetOrderByOid(oid);
            if (orderInfo == null || orderInfo.Uid != WorkContext.Uid)
            {
                return AjaxResult("erro", "订单信息错误！请刷新重试");
            }
            if (orderInfo.OrderState != (int)OrderState.Complete && orderInfo.OrderState != (int)OrderState.Received)
            {
                return AjaxResult("erro", "订单当前不能评价");
            }
            if (orderInfo.IsReview == 1)
            {
                return AjaxResult("erro", "此订单已经评价");
            }
            string[] opidss = opids.Split('#');
            string[] starss = stars.Split('#');
            string[] messagess = messages.Split('#');
            if (opidss.Length != starss.Length && starss.Length != messages.Length)
            {
                return AjaxResult("erro", "评价信息错误，请重新评价");
            }
            for (int i = 0; i < opidss.Length; i++)
            {
                int star = TypeHelper.StringToInt(starss[i]);
                if (star > 5 || star < 1)
                {
                    return AjaxResult("erro", "请选择正确的星星");
                }
                if (messagess[i].Length == 0)
                {
                    return AjaxResult("erro", "请填写评价内容");
                }
                if (messagess[i].Length > 100)
                {
                    return AjaxResult("erro", "评价内容最多输入100个字");
                }
                //禁止词
                string bannedWord = FilterWords.GetWord(messagess[i]);
                if (bannedWord != "")
                {
                    return AjaxResult("erro", "评价内容中不能包含违禁词");
                }

            }
            if (descriptionStar > 5 || descriptionStar < 1)
            {
                return AjaxResult("erro", "请选择正确的商品描述星星");
            }
            if (serviceStar > 5 || serviceStar < 1)
            {
                return AjaxResult("erro", "请选择正确的商家服务星星");
            }
            if (shipStar > 5 || shipStar < 1)
            {
                return AjaxResult("erro", "请选择正确的商家配送星星");
            }

            //评价商品
            for (int i = 0; i < opidss.Length; i++)
            {
                int opid = TypeHelper.StringToInt(opidss[i]);
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
                    return AjaxResult("erro", "商品不存在");
                }
                int payCredits = Credits.SendReviewProductCredits(ref WorkContext.PartUserInfo, orderProductInfo, DateTime.Now);
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
                    IP = WorkContext.IP
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
                Uid = WorkContext.Uid,
                ReviewTime = DateTime.Now,
                IP = WorkContext.IP
            };
            Stores.CreateStoreReview(storeReviewInfo);

            //订单已评价
            Orders.UpdateOrderIsReview(oid, 1);
            return AjaxResult("success", "订单评价成功");
        }

        #endregion

        protected sealed override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            //不允许游客访问
            if (WorkContext.Uid < 1)
            {
                if (WorkContext.IsHttpAjax)//如果为ajax请求
                {
                    filterContext.Result = Content("nologin");
                }
                else//如果为普通请求
                {
                    filterContext.Result = RedirectToAction("login", "account", new RouteValueDictionary { { "returnUrl", WorkContext.Url } });
                }
            }

        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private string Upload(string operation)
        {
            string result = "";
            if (operation == "uploaduseravatar" && Request.Files.Count > 0)//上传用户头像
            {
                HttpPostedFileBase file = Request.Files[0];
                 result = MallUtils.SaveUploadUserAvatar(file);
                 if (result.Length < 5)
                 {
                     result = "";
                 }
            }
            return result;
        }
    }
}