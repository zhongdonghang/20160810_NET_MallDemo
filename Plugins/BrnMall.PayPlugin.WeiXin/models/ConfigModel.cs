using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BrnMall.PayPlugin.WeiXin.models
{
    public class ConfigModel
    { 
         /// <summary>
        /// 绑定支付的APPID
         /// </summary>
        [Required(ErrorMessage = "APPID不能为空！")]
        public string AppId { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [Required(ErrorMessage = "商户号不能为空！")]
        public string MchId { get; set; }

        /// <summary>
        /// 绑定支付的APPID
        /// </summary>
        [Required(ErrorMessage = "APPID不能为空！")]
        public string IosAppId { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [Required(ErrorMessage = "商户号不能为空！")]
        public string IosMchId { get; set; }

        /// <summary>
        /// 绑定支付的APPID
        /// </summary>
        [Required(ErrorMessage = "APPID不能为空！")]
        public string AndroidAppId { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [Required(ErrorMessage = "商户号不能为空！")]
        public string AndroidMchId { get; set; }


        /// <summary>
        /// 商户支付密钥
        /// </summary>
        [Required(ErrorMessage = "商户支付密钥不能为空！")]
        public string Key { get; set; }

        /// <summary>
        /// 公众帐号secert
        /// </summary>
        [Required(ErrorMessage = "公众帐号secert不能为空！")]
        public string AppSecret { get; set; }

    }
}
