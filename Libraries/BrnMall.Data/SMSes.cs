using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.Core;

namespace BrnMall.Data
{
    public partial class SMSes
    {
        public static void CreateSMSLog(SMSLogInfo logInfo)
        {
            BrnMall.Core.BMAData.RDBS.CreateSMSLog(logInfo);
        }
    }
}
