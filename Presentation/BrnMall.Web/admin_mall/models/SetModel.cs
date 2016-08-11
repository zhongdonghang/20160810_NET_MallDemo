using System;
using System.Web;
using System.Data;
using System.Drawing;
using System.Web.Mvc;
using System.Drawing.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using BrnMall.Core;

namespace BrnMall.Web.MallAdmin.Models
{
    /// <summary>
    /// 站点模型类
    /// </summary>
    public class SiteModel
    {
        public string MallName { get; set; }
        [AllowHtml]
        public string SiteUrl { get; set; }
        public string SiteTitle { get; set; }
        public string SEOKeyword { get; set; }
        public string SEODescription { get; set; }
        public string ICP { get; set; }
        [AllowHtml]
        public string Script { get; set; }
        [Range(0, 1, ErrorMessage = "请选择正确的选项")]
        public int IsLicensed { get; set; }
    }

    /// <summary>
    /// 账号模型类
    /// </summary>
    public class AccountModel : IValidatableObject
    {
        public int[] RegType { get; set; }

        public string ReservedName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "注册时间间隔不能为负数")]
        [DisplayName("注册时间间隔")]
        public int RegTimeSpan { get; set; }

        [Range(0, 1, ErrorMessage = "请选择正确的类型")]
        [DisplayName("是否发送欢迎信息")]
        public int IsWebcomeMsg { get; set; }

        [AllowHtml]
        public string WebcomeMsg { get; set; }

        public int[] LoginType { get; set; }

        public string ShadowName { get; set; }

        [Range(0, 1, ErrorMessage = "请选择正确的类型")]
        [DisplayName("是否记住密码")]
        public int IsRemember { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "登录失败次数不能小于0")]
        [DisplayName("登录失败次数")]
        public int LoginFailTimes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errorList = new List<ValidationResult>();

            if (IsWebcomeMsg == 1 && string.IsNullOrWhiteSpace(WebcomeMsg))
                errorList.Add(new ValidationResult("欢迎信息不能为空!", new string[] { "WebcomeMsg" }));

            return errorList;
        }
    }

    /// <summary>
    /// 上传模型类
    /// </summary>
    public class UploadModel
    {
        /// <summary>
        /// 上传服务器
        /// </summary>
        public string UploadServer { get; set; }

        [Required(ErrorMessage = "图片类型不能为空")]
        public string UploadImgType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "图片大小必须大于0")]
        [DisplayName("图片大小")]
        public int UploadImgSize { get; set; }

    }

    /// <summary>
    /// 性能模型类
    /// </summary>
    public class PerformanceModel
    {
        public string ImageCDN { get; set; }

        public string CSSCDN { get; set; }

        public string ScriptCDN { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "在线用户过期时间不能小于0")]
        [DisplayName("在线用户过期时间")]
        public int OnlineUserExpire { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "更新用户在线时间间隔不能小于0")]
        [DisplayName("更新用户在线时间间隔")]
        public int UpdateOnlineTimeSpan { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "最大在线用户不能小于0")]
        [DisplayName("最大在线人数")]
        public int MaxOnlineCount { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "在线人数缓存时间不能小于0")]
        [DisplayName("在线人数缓存时间")]
        public int OnlineCountExpire { get; set; }

        [Range(0, 1, ErrorMessage = "请选择正确的选项")]
        [DisplayName("是否统计浏览器")]
        public int IsStatBrowser { get; set; }

        [Range(0, 1, ErrorMessage = "请选择正确的选项")]
        [DisplayName("是否统计操作系统")]
        public int IsStatOS { get; set; }

        [Range(0, 1, ErrorMessage = "请选择正确的选项")]
        [DisplayName("是否统计区域")]
        public int IsStatRegion { get; set; }
    }

    /// <summary>
    /// 访问模型类
    /// </summary>
    public class AccessModel : IValidatableObject
    {
        [Range(0, 1, ErrorMessage = "请选择正确的选项")]
        public int IsClosed { get; set; }

        [AllowHtml]
        public string CloseReason { get; set; }

        public string BanAccessTime { get; set; }

        public string BanAccessIP { get; set; }

        public string AllowAccessIP { get; set; }

        public string AdminAllowAccessIP { get; set; }

        [StringLength(32, MinimumLength = 8, ErrorMessage = "密钥长度必须大于7且小于33")]
        [Required(ErrorMessage = "密钥不能为空")]
        public string SecretKey { get; set; }

        public string CookieDomain { get; set; }

        public string RandomLibrary { get; set; }

        public string[] VerifyPages { get; set; }

        public string IgnoreWords { get; set; }

        public string AllowEmailProvider { get; set; }

        public string BanEmailProvider { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errorList = new List<ValidationResult>();

            if (IsClosed == 1 && string.IsNullOrWhiteSpace(CloseReason))
                errorList.Add(new ValidationResult("请填写商城关闭原因!", new string[] { "CloseReason" }));

            if (CookieDomain != null && (!CookieDomain.Contains(".") || !WebHelper.GetHost().Contains(CookieDomain)))
                errorList.Add(new ValidationResult("cookie域不合法!", new string[] { "CookieDomain" }));

            //if (!string.IsNullOrWhiteSpace(BanAccessTime))
            //{
            //    string[] timeList = StringHelper.SplitString(BanAccessTime, "\r\n");
            //    foreach (string time in timeList)
            //    {
            //        string[] startTimeAndEndTime = StringHelper.SplitString(time, "-");
            //        if (startTimeAndEndTime.Length == 2)
            //        {
            //            foreach (string item in startTimeAndEndTime)
            //            {
            //                string[] hourAndMinute = StringHelper.SplitString(item, ":");
            //                if (hourAndMinute.Length == 2)
            //                {
            //                    if (!CheckTime(hourAndMinute[0], hourAndMinute[1]))
            //                    {
            //                        errorList.Add(new ValidationResult("时间格式不正确!", new string[] { "BanAccessTime" }));
            //                        break;
            //                    }
            //                }
            //                else
            //                {
            //                    errorList.Add(new ValidationResult("时间格式不正确!", new string[] { "BanAccessTime" }));
            //                    break;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            errorList.Add(new ValidationResult("时间格式不正确!", new string[] { "BanAccessTime" }));
            //            break;
            //        }
            //    }
            //}

            return errorList;
        }

        private bool CheckTime(string hour, string minute)
        {
            if (hour.Length == 1 || hour.Length == 2)
            {
                int hourInt = TypeHelper.StringToInt(hour.Trim('0'));
                if (hourInt < 0 || hourInt > 23)
                    return false;
            }
            else
            {
                return false;
            }

            if (minute.Length == 1 || minute.Length == 2)
            {
                int minuteInt = TypeHelper.StringToInt(minute.Trim('0'));
                if (minuteInt < 0 || minuteInt > 59)
                    return false;
            }
            else
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 邮箱模型类
    /// </summary>
    public class EmailModel
    {
        [Required(ErrorMessage = "主机不能为空")]
        public string Host { get; set; }

        [Required(ErrorMessage = "端口不能为空")]
        public int Port { get; set; }

        [Required(ErrorMessage = "发送邮箱不能为空")]
        public string From { get; set; }

        [Required(ErrorMessage = "发送邮箱昵称不能为空")]
        public string FromName { get; set; }

        [Required(ErrorMessage = "账号不能为空")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "内容不能为空")]
        public string FindPwdBody { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "内容不能为空")]
        public string SCVerifyBody { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "内容不能为空")]
        public string SCUpdateBody { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "内容不能为空")]
        public string WebcomeBody { get; set; }
    }

    /// <summary>
    /// 短信模型类
    /// </summary>
    public class SMSModel
    {
        [Required(ErrorMessage = "地址不能为空")]
        public string Url { get; set; }

        [Required(ErrorMessage = "账号不能为空")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; }

        [Required(ErrorMessage = "内容不能为空")]
        public string FindPwdBody { get; set; }

        [Required(ErrorMessage = "内容不能为空")]
        public string SCVerifyBody { get; set; }

        [Required(ErrorMessage = "内容不能为空")]
        public string SCUpdateBody { get; set; }

        [Required(ErrorMessage = "内容不能为空")]
        public string WebcomeBody { get; set; }
    }

    /// <summary>
    /// 积分模型类
    /// </summary>
    public class CreditModel
    {
        /// <summary>
        /// 抽水百分比
        /// </summary>
        [Required(ErrorMessage = "抽水百分比不能为空")]
        public decimal Point { get; set; }
        /// <summary>
        /// 提现最低条件额度
        /// </summary>
        [Required(ErrorMessage = "提现最低条件额度不能为空")]
        public decimal MinAmount { get; set; }

        /// <summary>
        /// 提现最低额度
        /// </summary>
        [Required(ErrorMessage = "提现最低额度不能为空")]
        public decimal MinWithdrawal { get; set; }

    
        /// <summary>
        /// 等级积分名称
        /// </summary>
        [Required(ErrorMessage = "等级积分名称不能为空")]
        public string RankCreditName { get; set; }
        /// <summary>
        /// 每天最大发放等级积分
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "每天最大发放等级积分不能小于0")]
        [DisplayName("每天最大发放等级积分")]
        public int DayMaxSendRankCredits { get; set; }
        /// <summary>
        /// 注册等级积分
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "注册等级积分不能小于0")]
        [DisplayName("注册等级积分")]
        public int RegisterRankCredits { get; set; }
        /// <summary>
        /// 每天登录等级积分
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "每天登录等级积分不能小于0")]
        [DisplayName("每天登录等级积分")]
        public int LoginRankCredits { get; set; }
        /// <summary>
        /// 验证邮箱等级积分
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "验证邮箱等级积分不能小于0")]
        [DisplayName("验证邮箱等级积分")]
        public int VerifyEmailRankCredits { get; set; }
        /// <summary>
        /// 验证手机等级积分
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "验证手机等级积分不能小于0")]
        [DisplayName("验证手机等级积分")]
        public int VerifyMobileRankCredits { get; set; }
        /// <summary>
        /// 完善用户信息等级积分
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "完善用户信息等级积分不能小于0")]
        [DisplayName("完善用户信息等级积分")]
        public int CompleteUserInfoRankCredits { get; set; }
        /// <summary>
        /// 完成订单等级积分(以订单金额的百分比计算)
        /// </summary>
        [Range(0, 100, ErrorMessage = "完成订单等级积分必须位于0和100之间")]
        [DisplayName("完成订单等级积分")]
        public int CompleteOrderRankCredits { get; set; }
        /// <summary>
        /// 评价商品等级积分
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "评价商品等级积分不能小于0")]
        [DisplayName("评价商品等级积分")]
        public int ReviewProductRankCredits { get; set; }
    }

    /// <summary>
    /// 商城模型类
    /// </summary>
    public class MallModel
    {
        /// <summary>
        /// 是否允许游客使用购物车
        /// </summary>
        public int IsGuestSC { get; set; }
        /// <summary>
        /// 游客购物车最大商品数量
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "数量必须大于0")]
        [DisplayName("游客购物车最大商品数量")]
        public int GuestSCCount { get; set; }
        /// <summary>
        /// 会员购物车最大商品数量
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "数量必须大于0")]
        [DisplayName("会员购物车最大商品数量")]
        public int MemberSCCount { get; set; }
        /// <summary>
        /// 订单编号格式
        /// </summary>
        [Required(ErrorMessage = "订单编号格式不能为空")]
        public string OSNFormat { get; set; }
        /// <summary>
        /// 浏览历史数量
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "数量不能小于0")]
        [DisplayName("浏览历史数量")]
        public int BroHisCount { get; set; }
        /// <summary>
        /// 最大配送地址
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "数量必须大于0")]
        [DisplayName("最大配送地址")]
        public int MaxShipAddress { get; set; }
        /// <summary>
        /// 商品收藏夹最大容量
        /// </summary>
        public int FavoriteProductCount { get; set; }
        /// <summary>
        /// 店铺收藏夹最大容量
        /// </summary>
        public int FavoriteStoreCount { get; set; }
      /// <summary>
        /// 订单逾期未收货则系统自动收货（天数）
        /// </summary>
        public int OrderAutoReceive { get; set; }
         /// <summary>
        /// 订单逾期未支付则系统自动取消(分钟)
        /// </summary>
        public int OrderAutoCancel { get; set; }
    }
}
