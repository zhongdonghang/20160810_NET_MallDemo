﻿using System;
using System.Data;

using BrnMall.Core;
using BrnMall.Services;
using BrnMall.Web.Framework;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrnMall.Web.StoreAdmin.Models
{
    /// <summary>
    /// 商品评价列表模型类
    /// </summary>
    public class ProductReviewListModel
    {
        public PageModel PageModel { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public DataTable ProductReviewList { get; set; }
        public int Pid { get; set; }
        public string ProductName { get; set; }
        public string Message { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    /// <summary>
    /// 商品评价回复列表模型类
    /// </summary>
    public class ProductReviewReplyListModel
    {
        public DataTable ProductReviewReplyList { get; set; }
    }

    /// <summary>
    /// 回复商品咨询模型类
    /// </summary>
    [Bind(Exclude = "ProductReviewInfo")]
    public class ReplyProductReviewModel
    {
        public ProductReviewInfo ProductReviewInfo { get; set; }

        [Required(ErrorMessage = "回复内容不能为空")]
        [StringLength(100, ErrorMessage = "最多只能输入100个字")]
        public string ReplyMessage { get; set; }
    }
}
