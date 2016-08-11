//using System;
using System.Web.Mvc;
using System.Web.Routing;
using BrnMall.Core;
using BrnMall.Services;
using System;

namespace BrnMall.Web.Framework
{
    /// <summary>
    /// 移动前台基础控制器类
    /// </summary>
    public class BaseMobileController : Controller
    {
        //工作上下文
        public MobileWorkContext WorkContext = new MobileWorkContext();

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            this.ValidateRequest = false;
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {

        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //不能应用在子方法上
            if (filterContext.IsChildAction)
                return;

            WorkContext.IsHttpAjax = WebHelper.IsAjax();
            WorkContext.IP = WebHelper.GetIP();
            WorkContext.RegionInfo = Regions.GetRegionByIP(WorkContext.IP);
            WorkContext.RegionId = WorkContext.RegionInfo.RegionId;
            WorkContext.Url = WebHelper.GetUrl();
            WorkContext.UrlReferrer = WebHelper.GetUrlReferrer();

            //获得用户唯一标示符sid
            WorkContext.Sid = MallUtils.GetSidCookie();
            if (WorkContext.Sid.Length == 0)
            {
                //生成sid
                WorkContext.Sid = Sessions.GenerateSid();
                //将sid保存到cookie中
                MallUtils.SetSidCookie(WorkContext.Sid);
            }

            //获得用户唯一微信标识
            WorkContext.WeiXinUnionid = MallUtils.GetWeiXinUnionidCookie();
            if (string.IsNullOrWhiteSpace(WorkContext.WeiXinUnionid))
            {
                WorkContext.WeiXinUnionid = WeiXins.GetMyWeiXinUnionid();
                ////测试
                //WorkContext.WeiXinUnionid = "12333333";
                if (!string.IsNullOrWhiteSpace(WorkContext.WeiXinUnionid))
                {
                    MallUtils.SetWeiXinUnionidCookie(WorkContext.WeiXinUnionid);
                }
            }
            PartUserInfo partUserInfo;

            //获取当前微信绑定的用户
            partUserInfo = Users.GetPartUserInfoByWeixinUnid(WorkContext.WeiXinUnionid);
            if (partUserInfo != null)//微信号已绑定账号
            {
                //发放登录积分
                Credits.SendLoginCredits(ref partUserInfo, DateTime.Now);
            }
            else if (MallUtils.GetLoginTypeCookie() == "1")//微信快捷登录
            {
                partUserInfo = Users.GetPartUserByName(WorkContext.WeiXinUnionid);
                if (partUserInfo != null)
                {
                    //发放登录积分
                    Credits.SendLoginCredits(ref partUserInfo, DateTime.Now);
                    WorkContext.LoginType = 1;
                }
            }
            else //当用户为游客时
            {
                //创建游客
                partUserInfo = Users.CreatePartGuest();
            }
           
            //若用户为禁用等级而解禁时间已到，则解禁
            if (UserRanks.IsBanUserRank(partUserInfo.UserRid) && partUserInfo.LiftBanTime <= DateTime.Now)
            {
                UserRankInfo userRankInfo = UserRanks.GetUserRankByCredits(partUserInfo.RankCredits);
                Users.UpdateUserRankByUid(partUserInfo.Uid, userRankInfo.UserRid);
                partUserInfo.UserRid = userRankInfo.UserRid;
            }

            WorkContext.PartUserInfo = partUserInfo;

            WorkContext.Uid = partUserInfo.Uid;
            WorkContext.UserName = partUserInfo.UserName;
            WorkContext.UserEmail = partUserInfo.Email;
            WorkContext.UserMobile = partUserInfo.Mobile;
            WorkContext.Password = partUserInfo.Password;
            WorkContext.NickName = partUserInfo.NickName;
            WorkContext.Avatar = partUserInfo.Avatar;
            WorkContext.UserAmount = partUserInfo.UserAmount;
            WorkContext.FrozenAmount = partUserInfo.FrozenAmount;
            WorkContext.RankCreditName = Credits.RankCreditName;
            WorkContext.RankCreditCount = partUserInfo.RankCredits;

            WorkContext.UserRid = partUserInfo.UserRid;
            WorkContext.UserRankInfo = UserRanks.GetUserRankById(partUserInfo.UserRid);
            WorkContext.UserRTitle = WorkContext.UserRankInfo.Title;
            //设置用户商城管理员组
            WorkContext.MallAGid = partUserInfo.MallAGid;
            WorkContext.MallAdminGroupInfo = MallAdminGroups.GetMallAdminGroupById(partUserInfo.MallAGid);
            WorkContext.MallAGTitle = WorkContext.MallAdminGroupInfo.Title;

            //设置当前控制器类名
            WorkContext.Controller = RouteData.Values["controller"].ToString().ToLower();
            //设置当前动作方法名
            WorkContext.Action = RouteData.Values["action"].ToString().ToLower();
            WorkContext.PageKey = string.Format("/{0}/{1}", WorkContext.Controller, WorkContext.Action);

            WorkContext.ImageCDN = WorkContext.MallConfig.ImageCDN;
            WorkContext.CSSCDN = WorkContext.MallConfig.CSSCDN;
            WorkContext.ScriptCDN = WorkContext.MallConfig.ScriptCDN;

            //在线总人数
            WorkContext.OnlineUserCount = OnlineUsers.GetOnlineUserCount();
            //在线游客数
            WorkContext.OnlineGuestCount = OnlineUsers.GetOnlineGuestCount();
            //在线会员数
            WorkContext.OnlineMemberCount = WorkContext.OnlineUserCount - WorkContext.OnlineGuestCount;
            //搜索词
            WorkContext.SearchWord = string.Empty;
            //购物车中商品数量
            WorkContext.CartProductCount = Carts.GetCartProductCountCookie();

            //商城已经关闭
            if (WorkContext.MallConfig.IsClosed == 1 && WorkContext.MallAGid == 1 && WorkContext.PageKey != Url.Action("login", "account") && WorkContext.PageKey != Url.Action("logout", "account"))
            {
                filterContext.Result = PromptView(WorkContext.MallConfig.CloseReason);
                return;
            }

            //当前时间为禁止访问时间
            if (ValidateHelper.BetweenPeriod(WorkContext.MallConfig.BanAccessTime) && WorkContext.MallAGid == 1 && WorkContext.PageKey != Url.Action("login", "account") && WorkContext.PageKey != Url.Action("logout", "account"))
            {
                filterContext.Result = PromptView("当前时间不能访问本商城");
                return;
            }

            //当用户ip在被禁止的ip列表时
            if (ValidateHelper.InIPList(WorkContext.IP, WorkContext.MallConfig.BanAccessIP))
            {
                filterContext.Result = PromptView("您的IP被禁止访问本商城");
                return;
            }

            //当用户ip不在允许的ip列表时
            if (!string.IsNullOrEmpty(WorkContext.MallConfig.AllowAccessIP) && !ValidateHelper.InIPList(WorkContext.IP, WorkContext.MallConfig.AllowAccessIP))
            {
                filterContext.Result = PromptView("您的IP被禁止访问本商城");
                return;
            }

            //当用户IP被禁止时
            if (BannedIPs.CheckIP(WorkContext.IP))
            {
                filterContext.Result = PromptView("您的IP被禁止访问本商城");
                return;
            }

            //当用户等级是禁止访问等级时
            if (WorkContext.UserRid == 1)
            {
                filterContext.Result = PromptView("您的账号当前被锁定,不能访问");
                return;
            }

            //判断目前访问人数是否达到允许的最大人数
            if (WorkContext.OnlineUserCount > WorkContext.MallConfig.MaxOnlineCount && WorkContext.MallAGid == 1 && (WorkContext.Controller != "account" && (WorkContext.Action != "login" || WorkContext.Action != "logout")))
            {
                filterContext.Result = PromptView("商城人数达到访问上限, 请稍等一会再访问！");
                return;
            }

            //当用户为会员时,更新用户的在线时间
            if (WorkContext.Uid > 0)
                Users.UpdateUserOnlineTime(WorkContext.Uid);

            //更新在线用户
            Asyn.UpdateOnlineUser(WorkContext.Uid, WorkContext.Sid, WorkContext.NickName, WorkContext.IP, WorkContext.RegionId);
            //更新PV统计
            Asyn.UpdatePVStat(WorkContext.StoreId, WorkContext.Uid, WorkContext.RegionId, WebHelper.GetBrowserType(), WebHelper.GetOSType());
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            //错误log记录
            Exception ex = filterContext.Exception;
            MallUtils.WriteLogFile(ex.Message + "  详细：" + ex.StackTrace);

            //异常还未处理，导向友好错误界面
            if (!filterContext.ExceptionHandled)
            {
                if (WorkContext.IsHttpAjax)
                    filterContext.Result = AjaxResult("error", "系统错误,请联系管理员");
                else
                    filterContext.Result = View("error");

                //告诉系统异常已处理！！如果没有这个步骤，系统还是会按照正常的异常处理流程走
                filterContext.ExceptionHandled = true;
            }
        }

        /// <summary>
        /// 获得路由中的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        protected string GetRouteString(string key, string defaultValue)
        {
            object value = RouteData.Values[key];
            if (value != null)
                return value.ToString();
            else
                return defaultValue;
        }

        /// <summary>
        /// 获得路由中的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        protected string GetRouteString(string key)
        {
            return GetRouteString(key, "");
        }

        /// <summary>
        /// 获得路由中的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        protected int GetRouteInt(string key, int defaultValue)
        {
            return TypeHelper.ObjectToInt(RouteData.Values[key], defaultValue);
        }

        /// <summary>
        /// 获得路由中的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        protected int GetRouteInt(string key)
        {
            return GetRouteInt(key, 0);
        }

        /// <summary>
        /// 提示信息视图
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <returns></returns>
        protected ViewResult PromptView(string message)
        {
            return View("prompt", new PromptModel(message));
        }

        /// <summary>
        /// 提示信息视图
        /// </summary>
        /// <param name="backUrl">返回地址</param>
        /// <param name="message">提示信息</param>
        /// <returns></returns>
        protected ViewResult PromptView(string backUrl, string message)
        {
            return View("prompt", new PromptModel(backUrl, message));
        }

        /// <summary>
        /// ajax请求结果
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="content">内容</param>
        /// <returns></returns>
        protected ActionResult AjaxResult(string state, string content)
        {
            return AjaxResult(state, content, false);
        }

        /// <summary>
        /// ajax请求结果
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="content">内容</param>
        /// <param name="isObject">是否为对象</param>
        /// <returns></returns>
        protected ActionResult AjaxResult(string state, string content, bool isObject)
        {
            return Content(string.Format("{0}\"state\":\"{1}\",\"content\":{2}{3}{4}{5}", "{", state, isObject ? "" : "\"", content, isObject ? "" : "\"", "}"));
        }
    }
}
