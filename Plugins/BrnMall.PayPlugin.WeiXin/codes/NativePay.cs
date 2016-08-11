using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.PayPlugin.WeiXin.codes.weixinpay;
using BrnMall.Web.Framework;
using BrnMall.PayPlugin.WeiXin.models;

namespace BrnMall.PayPlugin.WeiXin.codes
{
    public class NativePay
    {
        WebWorkContext workContext = new WebWorkContext();
        /**
        * 生成直接支付url，支付url有效期为2小时,模式二
        * @param productId 订单ids
        * @return 模式二URL
        */
        public string GetPayUrl(PayShowModel model)
        {
            Log.Info(this.GetType().ToString(), "Native pay mode 2 url is producing...");

            WxPayData data = new WxPayData();
            data.SetValue("body", "来自" + workContext.MallConfig.MallName + "的微信扫码支付订单");//商品描述
            data.SetValue("attach", "test");//附加数据
            data.SetValue("out_trade_no", model.OidList);
            data.SetValue("total_fee", model.AllSurplusMoney);//总金额
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));//交易起始时间
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));//交易结束时间
            data.SetValue("goods_tag", "jsyproduct");//商品标记
            data.SetValue("trade_type", "NATIVE");//交易类型
            data.SetValue("product_id", model.OidList);//商品ID

            WxPayData result = WxPayApi.UnifiedOrder(data);//调用统一下单接口
            string url = result.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接

            Log.Info(this.GetType().ToString(), "Get native pay mode 2 url : " + url);
            return url;
        }

        /**
        * 参数数组转换为url格式
        * @param map 参数名与参数值的映射表
        * @return URL字符串
        */
        private string ToUrlParams(SortedDictionary<string, object> map)
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in map)
            {
                buff += pair.Key + "=" + pair.Value + "&";
            }
            buff = buff.Trim('&');
            return buff;
        }
    }
}
