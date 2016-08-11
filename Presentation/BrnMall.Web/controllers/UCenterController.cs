using System;
using System.Text;
using System.Data;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using BrnMall.Web.Models;
using System.IO;
using System.Net;

namespace BrnMall.Web.Controllers
{
    /// <summary>
    /// 用户中心控制器类
    /// </summary>
    public partial class UCenterController : BaseWebController
    {
        #region 用户信息

        /// <summary>
        /// 用户信息
        /// </summary>
        public ActionResult UserInfo()
        {
            UserInfoModel model = new UserInfoModel();

            model.UserInfo = Users.GetUserById(WorkContext.Uid);
            model.UserRankInfo = WorkContext.UserRankInfo;

            RegionInfo regionInfo = Regions.GetRegionById(model.UserInfo.RegionId);
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

            //设定上传图片参数
            string allowImgType = string.Empty;
            string[] imgTypeList = StringHelper.SplitString(BMAConfig.MallConfig.UploadImgType, ",");
            foreach (string imgType in imgTypeList)
            {
                allowImgType += string.Format("*{0};", imgType.ToLower());
            }
            string[] sizeList = StringHelper.SplitString(WorkContext.MallConfig.UserAvatarThumbSize);
            ViewData["size"] = sizeList[sizeList.Length / 2];
            ViewData["allowImgType"] = allowImgType;
            ViewData["maxImgSize"] = BMAConfig.MallConfig.UploadImgSize;
            return View(model);
        }

        /// <summary>
        /// 编辑用户信息
        /// </summary>
        public ActionResult EditUser()
        {
            string userName = WebHelper.GetFormString("userName");
            string nickName = WebHelper.GetFormString("nickName");
            string avatar = WebHelper.GetFormString("avatar");
            string realName = WebHelper.GetFormString("realName");
            int gender = WebHelper.GetFormInt("gender");
            string idCard = WebHelper.GetFormString("idCard");
            string bday = WebHelper.GetFormString("bday");
            int regionId = WebHelper.GetFormInt("regionId");
            string address = WebHelper.GetFormString("address");
            string bio = WebHelper.GetFormString("bio");

            StringBuilder errorList = new StringBuilder("[");
            //验证用户名
            if (WorkContext.UserName.Length == 0 && userName.Length > 0)
            {
                if (userName.Length < 4 || userName.Length > 10)
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "userName", "用户名必须大于3且不大于10个字符", "}");
                }
                else if (userName.Contains(" "))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "userName", "用户名中不允许包含空格", "}");
                }
                else if (userName.Contains(":"))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "userName", "用户名中不允许包含冒号", "}");
                }
                else if (userName.Contains("<"))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "userName", "用户名中不允许包含'<'符号", "}");
                }
                else if (userName.Contains(">"))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "userName", "用户名中不允许包含'>'符号", "}");
                }
                else if ((!SecureHelper.IsSafeSqlString(userName)))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "userName", "用户名已经存在", "}");
                }
                else if (CommonHelper.IsInArray(userName, WorkContext.MallConfig.ReservedName, "\n"))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "userName", "用户名已经存在", "}");
                }
                else if (FilterWords.IsContainWords(userName))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "userName", "用户名包含禁止单词", "}");
                }
                else if (Users.IsExistUserName(userName))
                {
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "userName", "用户名已经存在", "}");
                }
            }
            else
            {
                userName = WorkContext.UserName;
            }

            //验证昵称
            if (nickName.Length > 10)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "nickName", "昵称的长度不能大于10", "}");
            }
            else if (FilterWords.IsContainWords(nickName))
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "nickName", "昵称中包含禁止单词", "}");
            }

            //验证真实姓名
            if (realName.Length > 5)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "realName", "真实姓名的长度不能大于5", "}");
            }

            //验证性别
            if (gender < 0 || gender > 2)
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "gender", "请选择正确的性别", "}");


            //验证出生日期
            if (bday.Length == 0)
            {
                string bdayY = WebHelper.GetFormString("bdayY");
                string bdayM = WebHelper.GetFormString("bdayM");
                string bdayD = WebHelper.GetFormString("bdayD");
                bday = string.Format("{0}-{1}-{2}", bdayY, bdayM, bdayD);
            }
            if (bday.Length > 0 && bday != "--" && !ValidateHelper.IsDate(bday))
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "bday", "请选择正确的日期", "}");

            //验证区域
            if (regionId > 0)
            {
                RegionInfo regionInfo = Regions.GetRegionById(regionId);
                if (regionInfo == null || regionInfo.Layer != 3)
                    errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "regionId", "请选择正确的地址", "}");
            }

            //验证详细地址
            if (address.Length > 75)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "address", "详细地址的长度不能大于75", "}");
            }

            //验证简介
            if (bio.Length > 150)
            {
                errorList.AppendFormat("{0}\"key\":\"{1}\",\"msg\":\"{2}\"{3},", "{", "bio", "简介的长度不能大于150", "}");
            }

            if (errorList.Length == 1)
            {
                if (bday.Length == 0 || bday == "--")
                    bday = "1900-1-1";

                if (regionId < 1)
                    regionId = 0;

                Users.UpdateUser(WorkContext.Uid, userName, WebHelper.HtmlEncode(nickName), WebHelper.HtmlEncode(avatar), gender, WebHelper.HtmlEncode(realName), TypeHelper.StringToDateTime(bday), idCard, regionId, WebHelper.HtmlEncode(address), WebHelper.HtmlEncode(bio));
                if (userName.Length > 0 && nickName.Length > 0 && avatar.Length > 0 && realName.Length > 0 && bday != "1900-1-1" && idCard.Length > 0 && regionId > 0 && address.Length > 0)
                {
                    Credits.SendCompleteUserInfoCredits(ref WorkContext.PartUserInfo, DateTime.Now);
                }
                return AjaxResult("success", "信息更新成功");
            }
            else
            {
                return AjaxResult("error", errorList.Remove(errorList.Length - 1, 1).Append("]").ToString(), true);
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
            int introduceid = Users.GetMyIntroducer(WorkContext.Uid);
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
                umodel.AddTime = TypeHelper.ObjectToDateTime(row["registertime"]).ToString("yyyy-MM-dd HH:mm:ss");
                intrmodellist.Add(umodel);
                model.introduceCount += 1;
            }
            model.MyIntroducers = intrmodellist;
            return View(model);
        }

        #endregion

        #region 安全中心

        /// <summary>
        /// 账户安全信息
        /// </summary>
        public ActionResult SafeInfo()
        {
            return View(WorkContext.PartUserInfo);
        }

        /// <summary>
        /// 安全验证
        /// </summary>
        public ActionResult SafeVerify()
        {
            string action = WebHelper.GetQueryString("act").ToLower();
            string mode = WebHelper.GetQueryString("mode").ToLower();

            if (action.Length == 0 || !CommonHelper.IsInArray(action, new string[3] { "updatepassword", "updatepaypassword", "updatemobile" }) || (mode.Length > 0 && !CommonHelper.IsInArray(mode, new string[2] { "password", "mobile" })))
                return HttpNotFound();

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
                return AjaxResult("noaction", "动作不存在");

            //检查验证码
            if (string.IsNullOrWhiteSpace(verifyCode))
            {
                return AjaxResult("verifycode", "验证码不能为空");
            }
            if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
            {
                return AjaxResult("verifycode", "验证码不正确");
            }

            //检查密码
            if (string.IsNullOrWhiteSpace(password))
            {
                return AjaxResult("password", "密码不能为空");
            }
            if (Users.CreateUserPassword(password, WorkContext.PartUserInfo.Salt) != WorkContext.Password)
            {
                return AjaxResult("password", "密码不正确");
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
                return AjaxResult("unverifymobile", "手机号没有通过验证,所以不能发送验证短信");

            string moibleCode = Randoms.CreateRandomValue(6);
            //发送验证手机短信
            SMSes.SendSCVerifySMS(WorkContext.UserMobile, moibleCode);
            //将验证值保存在session中
            Sessions.SetItem(WorkContext.Sid, "ucsvMoibleCode", moibleCode);

            return AjaxResult("success", "短信已经发送,请查收");
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
                return AjaxResult("noaction", "动作不存在");

            //检查验证码
            if (string.IsNullOrWhiteSpace(verifyCode))
            {
                return AjaxResult("verifycode", "验证码不能为空");
            }
            if (verifyCode.ToLower() != Sessions.GetValueString(WorkContext.Sid, "verifyCode"))
            {
                return AjaxResult("verifycode", "验证码不正确");
            }

            //检查手机码
            if (string.IsNullOrWhiteSpace(moibleCode))
            {
                return AjaxResult("moiblecode", "手机验证码不能为空");
            }
            if (Sessions.GetValueString(WorkContext.Sid, "ucsvMoibleCode") != moibleCode)
            {
                return AjaxResult("moiblecode", "手机验证码不正确");
            }

            string v = MallUtils.AESEncrypt(string.Format("{0},{1},{2},{3}", WorkContext.Uid, action, DateTime.Now, Randoms.CreateRandomValue(6)));
            string url = Url.Action("safeupdate", new RouteValueDictionary { { "v", v } });
            return AjaxResult("success", url);
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
                return PromptView("此链接已经失效，请重新验证");

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

            string password = WebHelper.GetFormString("password");
            string confirmPwd = WebHelper.GetFormString("confirmPwd");
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

            //检查密码
            if (string.IsNullOrWhiteSpace(password))
            {
                return AjaxResult("password", "密码不能为空");
            }
            if (password.Length < 4 || password.Length > 32)
            {
                return AjaxResult("password", "密码必须大于3且不大于32个字符");
            }
            if (password != confirmPwd)
            {
                return AjaxResult("confirmpwd", "两次密码不相同");
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

            string password = WebHelper.GetFormString("password");
            string confirmPwd = WebHelper.GetFormString("confirmPwd");
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

            //检查密码
            if (string.IsNullOrWhiteSpace(password))
            {
                return AjaxResult("password", "支付密码不能为空");
            }
            if (password.Length < 4 || password.Length > 32)
            {
                return AjaxResult("password", "支付密码必须大于3且不大于32个字符");
            }
            if (password != confirmPwd)
            {
                return AjaxResult("confirmpwd", "两次密码不相同");
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

            string mobile = WebHelper.GetFormString("mobile");

            //检查手机号
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return AjaxResult("mobile", "手机号不能为空");
            }
            if (!ValidateHelper.IsMobile(mobile))
            {
                return AjaxResult("mobile", "手机号格式不正确");
            }
            int tempUid = Users.GetUidByMobile(mobile);
            if (tempUid > 0 && tempUid != WorkContext.Uid)
                return AjaxResult("mobile", "手机号已经存在");

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
            try
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

                string mobile = WebHelper.GetFormString("mobile");
                string moibleCode = WebHelper.GetFormString("moibleCode");
                string verifyCode = WebHelper.GetFormString("verifyCode");

                //session里的数据
                var sessions = Sessions.GetSession(WorkContext.Sid);
                if (sessions.Count < 3)
                {
                    return AjaxResult("mobile", "请获取验证码进行验证");
                }
                string ucsuverifyCode = sessions["verifyCode"].ToString();
                string ucsuMobile = sessions["ucsuMobile"].ToString();
                string ucsvMoibleCode = sessions["ucsvMoibleCode"].ToString();

                //检查验证码
                if (string.IsNullOrWhiteSpace(verifyCode))
                {
                    return AjaxResult("verifycode", "验证码不能为空");
                }
                if (verifyCode.ToLower() != ucsuverifyCode)
                {
                    return AjaxResult("verifycode", "验证码不正确");
                }

                //检查手机号
                if (string.IsNullOrWhiteSpace(mobile))
                {
                    return AjaxResult("mobile", "手机号不能为空");
                }
                if (Users.IsExistUserName(mobile))
                {
                    return AjaxResult("mobile", "该手机号已注册");
                }


                if (string.IsNullOrWhiteSpace(ucsuMobile))
                {
                    return AjaxResult("mobile", "请获取验证码进行验证");
                }
                if (ucsuMobile != mobile)
                {
                    return AjaxResult("mobile", "与已接收验证码的手机不一致");
                }

                //检查手机码
                if (string.IsNullOrWhiteSpace(moibleCode))
                {
                    return AjaxResult("moiblecode", "手机验证码不能为空");
                }
                if (ucsvMoibleCode != moibleCode)
                {
                    return AjaxResult("moiblecode", "手机验证码不正确");
                }

                //更新手机号+用户名
                Users.UpdateUserMobileByUid(WorkContext.Uid, mobile);

                string url = Url.Action("safesuccess", new RouteValueDictionary { { "act", "updateMobile" } });
                return AjaxResult("success", url);
            }
            catch (Exception)
            {
                return AjaxResult("mobile", "验证失败");
            }
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

            PageModel pageModel = new PageModel(7, page, Orders.GetUserOrderCount(WorkContext.Uid, startAddTime, endAddTime, orderState,keyword));

            DataTable orderList = Orders.GetUserOrderList(WorkContext.Uid, pageModel.PageSize, pageModel.PageNumber, startAddTime, endAddTime, orderState,keyword);
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

            List<SelectListItem> itemList = new List<SelectListItem>();
            itemList.Add(new SelectListItem() { Text = "全部", Value = "0" });
            itemList.Add(new SelectListItem() { Text = "等待付款", Value = ((int)OrderState.WaitPaying).ToString() });
            itemList.Add(new SelectListItem() { Text = "待确认", Value = ((int)OrderState.Confirming).ToString() });
            itemList.Add(new SelectListItem() { Text = "已确认", Value = ((int)OrderState.Confirmed).ToString() });
            itemList.Add(new SelectListItem() { Text = "已备货", Value = ((int)OrderState.PreProducting).ToString() });
            itemList.Add(new SelectListItem() { Text = "已发货", Value = ((int)OrderState.Sended).ToString() });
            itemList.Add(new SelectListItem() { Text = "已收货", Value = ((int)OrderState.Received).ToString() });
            itemList.Add(new SelectListItem() { Text = "已完成", Value = ((int)OrderState.Complete).ToString() });
            itemList.Add(new SelectListItem() { Text = "已取消", Value = ((int)OrderState.Cancelled).ToString() });
            ViewData["orderStateList"] = itemList;
            return View(model);
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
            model.OrderActionList = null;
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
        public ActionResult ReceiveProduct()
        {
            int oid = WebHelper.GetFormInt("oid");
            OrderInfo orderInfo = Orders.GetOrderByOid(oid);
            if (orderInfo == null || orderInfo.Uid != WorkContext.Uid)
            {
                return AjaxResult("noorder", "订单不存在");
            }

            if (orderInfo.OrderState != (int)OrderState.Sended || orderInfo.PayMode != 1)
            {
                return AjaxResult("donotreceive", "该订单现在还不允许做确认收货操作");
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
                ActionDes = "您确认了已收货"
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
        public ActionResult FavoriteProductList()
        {
            int page = WebHelper.GetQueryInt("page");//当前页数
            string storeName = WebHelper.GetQueryString("storeName").Trim();//店铺名称
            string productName = WebHelper.GetQueryString("productName").Trim();//商品名称

            if (!SecureHelper.IsSafeSqlString(storeName) || !SecureHelper.IsSafeSqlString(productName))
                return PromptView(WorkContext.UrlReferrer, "您搜索的内容不存在");

            PageModel pageModel = new PageModel(10, page, (storeName.Length > 0 || productName.Length > 0) ? FavoriteProducts.GetFavoriteProductCount(WorkContext.Uid, storeName, productName) : FavoriteProducts.GetFavoriteProductCount(WorkContext.Uid));

            FavoriteProductListModel model = new FavoriteProductListModel()
            {
                ProductList = (storeName.Length > 0 || productName.Length > 0) ? FavoriteProducts.GetFavoriteProductList(pageModel.PageSize, pageModel.PageNumber, WorkContext.Uid, storeName, productName) : FavoriteProducts.GetFavoriteProductList(pageModel.PageSize, pageModel.PageNumber, WorkContext.Uid),
                PageModel = pageModel,
                ProductName = productName
            };

            return View(model);
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
        public ActionResult FavoriteStoreList()
        {
            int page = WebHelper.GetQueryInt("page");//当前页数

            PageModel pageModel = new PageModel(10, page, FavoriteStores.GetFavoriteStoreCount(WorkContext.Uid));

            FavoriteStoreListModel model = new FavoriteStoreListModel()
            {
                StoreList = FavoriteStores.GetFavoriteStoreList(pageModel.PageSize, pageModel.PageNumber, WorkContext.Uid),
                PageModel = pageModel
            };

            return View(model);
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
        /// 配送地址信息
        /// </summary>
        public ActionResult ShipAddressInfo()
        {
            int saId = WebHelper.GetQueryInt("saId");
            FullShipAddressInfo fullShipAddressInfo = ShipAddresses.GetFullShipAddressBySAId(saId, WorkContext.Uid);
            //检查地址
            if (fullShipAddressInfo == null)
                return AjaxResult("noexist", "地址不存在");

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\"saId\":\"{1}\",\"uid\":\"{2}\",\"regionId\":\"{3}\",\"isDefault\":\"{4}\",\"alias\":\"{5}\",\"consignee\":\"{6}\",\"mobile\":\"{7}\",\"phone\":\"{8}\",\"email\":\"{9}\",\"zipCode\":\"{10}\",\"address\":\"{11}\",\"provinceId\":\"{12}\",\"provinceName\":\"{13}\",\"cityId\":\"{14}\",\"cityName\":\"{15}\",\"countyId\":\"{16}\",\"countyName\":\"{17}\"{18}", "{", fullShipAddressInfo.SAId, fullShipAddressInfo.Uid, fullShipAddressInfo.RegionId, fullShipAddressInfo.IsDefault, fullShipAddressInfo.Alias, fullShipAddressInfo.Consignee, fullShipAddressInfo.Mobile, fullShipAddressInfo.Phone, fullShipAddressInfo.Email, fullShipAddressInfo.ZipCode, fullShipAddressInfo.Address, fullShipAddressInfo.ProvinceId, fullShipAddressInfo.ProvinceName, fullShipAddressInfo.CityId, fullShipAddressInfo.CityName, fullShipAddressInfo.CountyId, fullShipAddressInfo.CountyName, "}");

            return AjaxResult("success", sb.ToString(), true);
        }

        /// <summary>
        /// 添加配送地址
        /// </summary>
        public ActionResult AddShipAddress()
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

        /// <summary>
        /// 编辑配送地址
        /// </summary>
        public ActionResult EditShipAddress()
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
        private string VerifyShipAddress(int regionId, string consignee, string mobile, string phone, string email, string zipcode, string address)
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

        #region 用户资金

        /// <summary>
        /// 资金日志
        /// </summary>
        public ActionResult PayCreditLogList()
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
        /// 提现记录
        /// </summary>
        /// <returns></returns>
        public ActionResult  WithdrawalLogList()
        {
            int page = WebHelper.GetQueryInt("page");

            PageModel pageModel = new PageModel(15, page, Credits.GetWithdrawalLogCount(WorkContext.Uid,0,0));
            WithdrawalLogListModel model = new WithdrawalLogListModel()
            {
                PageModel = pageModel,
                WithdrawalLogList = Credits.GetWithdrawalLogList(WorkContext.Uid, 0, 0, pageModel.PageNumber, pageModel.PageSize)
            };

            return View(model);
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        /// <returns></returns>
        public ActionResult WithdrawalApply()
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
        public ActionResult WithdrawalApply(WithdrawalLogModel model)
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
                ModelState.AddModelError("ApplyAmount", "申请提现的金额不能低于" + BMAConfig.CreditConfig.MinWithdrawal.ToString()+"元");
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
                OperatTime=DateTime.Now,
                Uid=WorkContext.Uid
            };
            Credits.CreateWithdrawalLog(winfo);
            return RedirectToAction("WithdrawalLogList");
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

        /// <summary>
        /// 激活优惠劵
        /// </summary>
        public ActionResult ActivateCoupon()
        {
            string activateKey1 = WebHelper.GetFormString("activateKey1");
            string activateKey2 = WebHelper.GetFormString("activateKey2");
            string activateKey3 = WebHelper.GetFormString("activateKey3");
            string activateKey4 = WebHelper.GetFormString("activateKey4");

            if (activateKey1.Length != 4 || activateKey2.Length != 4 || activateKey3.Length != 4 || activateKey4.Length != 4)
                return AjaxResult("errorcouponsn", "优惠劵编号不正确");

            //优惠劵编号
            string couponSN = activateKey1 + activateKey2 + activateKey3 + activateKey4;
            //优惠劵
            CouponInfo couponInfo = Coupons.GetCouponByCouponSN(couponSN);
            if (couponInfo == null)
                return AjaxResult("noexist", "优惠劵不存在");
            if (couponInfo.Uid > 0)
                return AjaxResult("used", "优惠劵已使用");
            //优惠劵类型
            CouponTypeInfo couponTypeInfo = Coupons.GetCouponTypeById(couponInfo.CouponTypeId);
            if (couponTypeInfo == null)
                return AjaxResult("nocoupontype", "优惠劵类型不存在");
            if (couponTypeInfo.UseExpireTime == 0 && couponTypeInfo.UseEndTime <= DateTime.Now)
                return AjaxResult("expired", "此优惠劵已过期");
            //店铺信息
            StoreInfo storeInfo = Stores.GetStoreById(couponTypeInfo.StoreId);
            if (storeInfo.State != (int)StoreState.Open)
                return AjaxResult("nostore", "店铺不存在");

            Coupons.ActivateCoupon(couponInfo.CouponId, WorkContext.Uid, DateTime.Now, WorkContext.IP);
            return AjaxResult("success", "优惠劵激活成功");
        }

        #endregion

        #region 商品咨询

        /// <summary>
        /// 商品咨询列表
        /// </summary>
        public ActionResult ProductConsultList()
        {
            int page = WebHelper.GetQueryInt("page");

            PageModel pageModel = new PageModel(10, page, ProductConsults.GetUserProductConsultCount(WorkContext.Uid));
            UserProductConsultListModel model = new UserProductConsultListModel()
            {
                PageModel = pageModel,
                ProductConsultList = ProductConsults.GetUserProductConsultList(WorkContext.Uid, pageModel.PageSize, pageModel.PageNumber)
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
            if (orderInfo.OrderState != (int)OrderState.Complete && orderInfo.OrderState != (int)OrderState.Received)
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
                return AjaxResult("erro", "订单信息错误，请刷新重试");
            }
            if (orderInfo.OrderState != (int)OrderState.Complete && orderInfo.OrderState != (int)OrderState.Received)
            {
                return AjaxResult("erro", "订单当前不能评价");
            }
            if (orderInfo.IsReview == 1)
            {
                return AjaxResult("erro", "该订单已经评价");
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
        /// <summary>
        /// 商品评价列表
        /// </summary>
        public ActionResult ProductReviewList()
        {
            int page = WebHelper.GetQueryInt("page", 1);

            PageModel pageModel = new PageModel(10, page, ProductReviews.GetUserProductReviewCount(WorkContext.Uid));
            UserProductReviewListModel model = new UserProductReviewListModel()
            {
                PageModel = pageModel,
                ProductReviewList = ProductReviews.GetUserProductReviewList(WorkContext.Uid, pageModel.PageSize, pageModel.PageNumber)
            };

            return View(model);
        }

        #endregion

        #region 申请开店
        /// <summary>
        /// 申请开店第一步
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult storeapplystep1()
        {
            storeApplyStep1Model model = new storeApplyStep1Model();

            //申请失败可再编辑修改???
            StoreInfo store = WorkContext.StoreInfo;
            if (store != null && store.StoreId > 1 && (store.State == (int)StoreState.ApplyFail || store.State == (int)StoreState.Applying))
            {
                model.StoreName = WorkContext.StoreInfo.Name;
                StoreKeeperInfo keeper = Stores.GetStoreKeeperById(WorkContext.StoreId);
                if (keeper != null)
                {
                    model.StoreKeeperName = keeper.Name;
                    model.IdCard = keeper.IdCard;
                    model.Type = keeper.Type;
                    model.Address = keeper.Address;
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult storeapplystep1(storeApplyStep1Model model)
        {
            //model验证不通过
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                StoreInfo store = WorkContext.StoreInfo;
                //没有店铺/有店铺但是修改了店铺名情况下，验证店铺名是否已存在
                if (store == null || (store != null && store.Name != model.StoreName) && Stores.GetStoreIdByName(model.StoreName) > 0)
                {
                    ModelState.AddModelError("StoreName", "名称已经存在");
                }

                //店铺信息添加或修改
                if (store == null)
                {
                    store = new StoreInfo()
                   {
                       State = (int)StoreState.Applying,
                       RegionId = 0,
                       StoreRid = AdminStoreRanks.GetLowestStoreRank().StoreRid,
                       StoreIid = 0,
                       Logo = "",
                       CreateTime = DateTime.Now,
                       Mobile = "",
                       Phone = "",
                       QQ = "",
                       WW = "",
                       DePoint = 10.00m,
                       SePoint = 10.00m,
                       ShPoint = 10.00m,
                       Honesties = 0,
                       StateEndTime = DateTime.Now.AddMonths(24),
                       Theme = "Default",
                       Banner = "",
                       Announcement = "",
                       Description = ""
                   };
                }
                store.Name = model.StoreName;

                //店长信息添加或修改
                StoreKeeperInfo storeKeeperInfo = Stores.GetStoreKeeperById(store.StoreId);
                if (storeKeeperInfo == null)
                {
                    storeKeeperInfo = new StoreKeeperInfo();
                }
                storeKeeperInfo.Type = model.Type;
                storeKeeperInfo.Name = model.StoreKeeperName;
                storeKeeperInfo.IdCard = model.IdCard;
                storeKeeperInfo.Address = model.Address;

                //新申请情况，添加店铺、店长信息提交
                if (store.StoreId == 0)
                {
                    int storeId = AdminStores.CreateStore(store, storeKeeperInfo);
                    //绑定店铺id到用户
                    var user = Users.GetPartUserById(WorkContext.Uid);
                    user.StoreId = storeId;
                    Users.UpdatePartUser(user);
                }
                else
                {
                    AdminStores.UpdateStore(store);
                    AdminStores.UpdateStoreKeeper(storeKeeperInfo);
                }

                WorkContext.StoreId = store.StoreId;
                WorkContext.StoreInfo = store;
                return RedirectToAction("storeapplystep2");
            }
            catch (Exception)
            {
            }
            return View(model);
        }

        /// <summary>
        /// 申请开店第二步
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult storeapplystep2()
        {
            storeApplyStep2Model model = new storeApplyStep2Model();
            StoreInfo storeInfo = WorkContext.StoreInfo;
            StoreKeeperInfo keeper = Stores.GetStoreKeeperById(WorkContext.StoreId);

            //如果信息错误，则跳回申请第一步
            if (storeInfo == null || keeper == null || storeInfo.StoreId != keeper.StoreId)
            {
                return RedirectToAction("storeapplystep1");
            }
            model.RegionId = storeInfo.RegionId;
            model.StoreIid = storeInfo.StoreIid;
            model.Logo = storeInfo.Logo;
            model.Mobile = storeInfo.Mobile;
            model.Phone = storeInfo.Phone;
            model.QQ = storeInfo.QQ;
            model.WW = storeInfo.WW;
            model.Theme = storeInfo.Theme;
            model.Banner = storeInfo.Banner;
            model.Announcement = storeInfo.Announcement;
            model.Description = storeInfo.Description;

            //设定上传图片参数
            string allowImgType = string.Empty;
            string[] imgTypeList = StringHelper.SplitString(BMAConfig.MallConfig.UploadImgType, ",");
            foreach (string imgType in imgTypeList)
            {
                allowImgType += string.Format("*{0};", imgType.ToLower());
            }
            string[] sizeList = StringHelper.SplitString(WorkContext.MallConfig.UserAvatarThumbSize);
            ViewData["size"] = sizeList[sizeList.Length / 2];
            ViewData["allowImgType"] = allowImgType;
            ViewData["maxImgSize"] = BMAConfig.MallConfig.UploadImgSize;
            ViewData["storeId"] = storeInfo.StoreId;
            List<SelectListItem> storeIndustryList = new List<SelectListItem>();
            storeIndustryList.Add(new SelectListItem() { Text = "全部行业", Value = "0" });
            foreach (StoreIndustryInfo storeIndustryInfo in AdminStoreIndustries.GetStoreIndustryList())
            {
                storeIndustryList.Add(new SelectListItem() { Text = storeIndustryInfo.Title, Value = storeIndustryInfo.StoreIid.ToString() });
            }
            ViewData["storeIndustryList"] = storeIndustryList;

            List<SelectListItem> storeStateList = new List<SelectListItem>();
            storeStateList.Add(new SelectListItem() { Text = "全部", Value = "-1" });
            storeStateList.Add(new SelectListItem() { Text = "营业", Value = ((int)StoreState.Open).ToString() });
            storeStateList.Add(new SelectListItem() { Text = "关闭", Value = ((int)StoreState.Close).ToString() });
            storeStateList.Add(new SelectListItem() { Text = "申请中", Value = ((int)StoreState.Applying).ToString() });
            ViewData["storeStateList"] = storeStateList;
            List<SelectListItem> themeList = new List<SelectListItem>();
            DirectoryInfo dir = new DirectoryInfo(IOHelper.GetMapPath("/themes"));
            foreach (DirectoryInfo themeDir in dir.GetDirectories())
            {
                themeList.Add(new SelectListItem() { Text = themeDir.Name, Value = themeDir.Name });
            }
            ViewData["themeList"] = themeList;

            RegionInfo regionInfo = Regions.GetRegionById(storeInfo.RegionId);
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

        [HttpPost]
        public ActionResult storeapplystep2(storeApplyStep2Model model)
        {
            StoreInfo storeInfo = AdminStores.GetStoreById(model.StoreIid);
            if (storeInfo == null)
            {
                return RedirectToAction("storeapplystep1");
            }
            if (ModelState.IsValid)
            {
                storeInfo.State = (int)StoreState.Applying;
                storeInfo.RegionId = model.RegionId;
                storeInfo.StoreIid = model.StoreIid;
                storeInfo.Logo = model.Logo == null ? "" : model.Logo;
                storeInfo.Mobile = model.Mobile;
                storeInfo.Phone = model.Phone == null ? "" : model.Phone;
                storeInfo.QQ = model.QQ == null ? "" : model.QQ;
                storeInfo.WW = model.WW == null ? "" : model.WW;
                storeInfo.Theme = model.Theme;
                storeInfo.Banner = model.Banner == null ? "" : model.Banner;
                storeInfo.Announcement = model.Announcement == null ? "" : model.Announcement;
                storeInfo.Description = model.Description == null ? "" : model.Description;

                AdminStores.UpdateStore(storeInfo);
                return PromptView("店铺申请已经提交，请耐心等待商城工作人员的审核。");
            }
            return RedirectToAction("storeapplystep1");

        }

        #endregion

        protected sealed override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            //不允许游客访问
            if (WorkContext.Uid < 1)
            {
                if (WorkContext.IsHttpAjax)//如果为ajax请求
                    filterContext.Result = Content("nologin");
                else//如果为普通请求
                    filterContext.Result = RedirectToAction("login", "account", new RouteValueDictionary { { "returnUrl", WorkContext.Url } });
            }
        }
    }
}
