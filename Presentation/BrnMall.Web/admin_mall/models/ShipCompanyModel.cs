using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;

namespace BrnMall.Web.MallAdmin.Models
{
    /// <summary>
    /// 配送公司列表模型类
    /// </summary>
    public class ShipCompanyListModel
    {
        public PageModel PageModel { get; set; }
        public List<ShipCompanyInfo> ShipCompanyList { get; set; }
    }

    /// <summary>
    /// 配送公司模型类
    /// </summary>
    public class ShipCompanyModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "名称不能为空")]
        [StringLength(15, ErrorMessage = "名称长度不能大于15")]
        public string CompanyName { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        [Required(ErrorMessage = "编号不能为空")]
        [StringLength(30, ErrorMessage = "编号长度不能大于30")]
        public string Code { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        [DisplayName("排序")]
        public int DisplayOrder { get; set; }
    }
}
