using System;

namespace BrnMall.Core
{
    /// <summary>
    /// 店铺状态枚举
    /// </summary>
    public enum StoreState
    {
        /// <summary>
        /// 营业
        /// </summary>
        Open = 0,
        /// <summary>
        /// 关闭
        /// </summary>
        Close = 1,
        /// <summary>
        /// 审核中
        /// </summary>
        Applying = 2,
        /// <summary>
        /// 审核不通过
        /// </summary>
        ApplyFail = 3
    }
}
