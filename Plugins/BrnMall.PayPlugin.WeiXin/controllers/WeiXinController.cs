using System;
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.Web.Framework;
using System.Web.Mvc;
using BrnMall.Core;
using BrnMall.PayPlugin.WeiXin.codes.weixinpay;
using BrnMall.PayPlugin.WeiXin.codes;
using BrnMall.PayPlugin.WeiXin.business;
using BrnMall.Services;
using BrnMall.PayPlugin.WeiXin.models;
using ThoughtWorks.QRCode.Codec;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace BrnMall.Web.Controllers
{
    public class WeiXinController : BaseWebController
    {
        public static string wxJsApiParam { get; set; } //H5调起JS API参数

        /// <summary>
        /// 微信商城支付
        /// </summary>
        /// <returns></returns>
        public ActionResult Pay()
        {
            //订单id列表
            string oidList = WebHelper.GetQueryString("oidList");
            PayShowModel model = new PayShowModel();
            decimal allPayMoney = 0M;
            string onlinePayOidList = "";
            List<OrderInfo> orderList = new List<OrderInfo>();
            foreach (string oid in StringHelper.SplitString(oidList))
            {
                //订单信息
                OrderInfo orderInfo = Orders.GetOrderByOid(TypeHelper.StringToInt(oid));
                if (orderInfo != null && orderInfo.PayMode == 1  && orderInfo.OrderState == (int)OrderState.WaitPaying)
                {
                    orderList.Add(orderInfo);
                    model.PaySystemName = orderInfo.PaySystemName;
                    model.AddTime = orderInfo.AddTime;
                    allPayMoney += orderInfo.SurplusMoney;
                    onlinePayOidList += orderInfo.Oid + "A";
                }
                else
                {
                    return Redirect("/mob/");
                }
            }
            if (onlinePayOidList.Length < 2)
            {
                return Redirect("/mob/");
            }

            string randoms = Randoms.CreateRandomValue(10, false).ToUpper();
            model.OidList = onlinePayOidList + randoms; ;
            model.OrderList = orderList;
            model.AllSurplusMoney = (int)(allPayMoney * 100);

            //JSAPI支付预处理
            try
            {
                //若传递了相关参数，则调统一下单接口，获得后续相关接口的入口参数
                JsApiPay jsApiPay = new JsApiPay();
                if (string.IsNullOrWhiteSpace(WebHelper.GetQueryString("code")))
                {
                    jsApiPay.GetCode();
                }
                Log.Debug(this.GetType().ToString(), "code为：" + WebHelper.GetQueryString("code"));
                //调用【网页授权获取用户信息】接口获取用户的openid和access_token
                jsApiPay.GetOpenidAndAccessTokenFromCode(WebHelper.GetQueryString("code"));
                jsApiPay.total_fee = model.AllSurplusMoney;//totalfee是以分为单位

                //统一下单
                WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResult(model);
                //获取H5调起JS API参数(预付订单)      
                WxPayData jsApiParam = new WxPayData();
                jsApiParam.SetValue("appId", unifiedOrderResult.GetValue("appid"));
                jsApiParam.SetValue("timeStamp", WxPayApi.GenerateTimeStamp());
                jsApiParam.SetValue("nonceStr", WxPayApi.GenerateNonceStr());
                jsApiParam.SetValue("package", "prepay_id=" + unifiedOrderResult.GetValue("prepay_id"));
                jsApiParam.SetValue("signType", "MD5");
                jsApiParam.SetValue("paySign", jsApiParam.MakeSign());
                ViewData["appId"] = jsApiParam.GetValue("appId");
                ViewData["timeStamp"] = jsApiParam.GetValue("timeStamp");
                ViewData["nonceStr"] = jsApiParam.GetValue("nonceStr");
                ViewData["package"] = jsApiParam.GetValue("package");
                ViewData["signType"] = jsApiParam.GetValue("signType");
                ViewData["paySign"] = jsApiParam.GetValue("paySign");
                Log.Debug(this.GetType().ToString(), "获取H5调起JS API参数(预付订单)为：" + jsApiParam.ToXml());
            }
            catch (Exception ex)
            {
                Log.Error(this.GetType().ToString(), "支付失败，原因：" + ex.Message);
                return Content("支付失败，请刷新再支付...");
            }
            return View("~/plugins/BrnMall.PayPlugin.WeiXin/views/weixin/pay.cshtml", model);
        }

        /// <summary>
        /// 扫码支付
        /// </summary>
        /// <returns></returns>
        public ActionResult NativePay()
        {
            //订单id列表
            string oidList = WebHelper.GetQueryString("oidList");
            string paySystemName = "";
            decimal allPayMoney = 0M;
            string onlinePayOidList = "";
            List<OrderInfo> orderList = new List<OrderInfo>();
            foreach (string oid in StringHelper.SplitString(oidList))
            {
                //订单信息
                OrderInfo orderInfo = Orders.GetOrderByOid(TypeHelper.StringToInt(oid));
                if (orderInfo != null && orderInfo.PayMode == 1 && orderInfo.Uid == WorkContext.Uid && orderInfo.OrderState == (int)OrderState.WaitPaying)
                {
                    orderList.Add(orderInfo);
                    paySystemName = orderInfo.PaySystemName;
                    allPayMoney += orderInfo.SurplusMoney;
                    onlinePayOidList += orderInfo.Oid + "A";
                }
                else
                {
                    return Redirect("/");
                }
            }

            if (onlinePayOidList.Length < 2)
            {
                return Redirect("/");
            }

            PayShowModel model = new PayShowModel();
            string randoms = Randoms.CreateRandomValue(10, false).ToUpper();
            model.OidList = onlinePayOidList + randoms; ;
            model.OrderList = orderList;
            model.AllSurplusMoney = (int)(allPayMoney * 100);

            try
            {
                //生成扫码支付模式一url
                NativePay nativePay = new NativePay();
                string url = nativePay.GetPayUrl(model);

                model.ImgUrl = HttpUtility.UrlEncode(url);
                return View("~/plugins/BrnMall.PayPlugin.WeiXin/views/weixin/nativepay.cshtml", model);
            }
            catch (Exception ex)
            {

                return Content("支付失败，原因：" + ex.Message);
            }
        }

        /// <summary>
        /// 支付异步（扫码+微商城+ios）返回通知
        /// </summary>
        /// <returns></returns>
        public ActionResult PayResult()
        {
            try
            {
                Notify notify = new Notify();
                WxPayData notifyData = notify.GetNotifyData();
                WxPayData res = new WxPayData();
                //检查支付结果中transaction_id是否存在
                if (!notifyData.IsSet("transaction_id"))
                {
                    //不存在，出log
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg", "支付结果中微信订单号不存在");
                    Log.Error(this.GetType().ToString(), "The Pay result is error : " + res.ToXml());
                    return Content("fail");
                }

                //查询微信订单，判断订单真实性
                string transaction_id = notifyData.GetValue("transaction_id").ToString();
                string trade_type = notifyData.GetValue("trade_type").ToString().Trim().ToUpper();
                res.SetValue("transaction_id", transaction_id);
                var payresult = PayResultFuc(res, trade_type);
                if (payresult == false)
                {
                    return Content("fail");
                }
                return Content("success");
            }
            catch (Exception ex)
            {
                Log.Error(this.GetType().ToString(), "交易出错 : " + ex.Message);
                return Content("交易出错，请联系商城管理员...");
            }
           
        }

        /// <summary>
        /// 安卓微信支付的回调
        /// </summary>
        /// <returns></returns>
        public ActionResult AndroidPayResult()
        {
            try
            {
                Notify notify = new Notify();
                WxPayData notifyData = notify.GetNotifyData();
                WxPayData res = new WxPayData();
                //检查支付结果中transaction_id是否存在
                if (!notifyData.IsSet("transaction_id"))
                {
                    //不存在，出log
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg", "支付结果中微信订单号不存在");
                    Log.Error(this.GetType().ToString(), "The Pay result is error : " + res.ToXml());
                    return Content("fail");
                }

                //查询微信订单，判断订单真实性
                string transaction_id = notifyData.GetValue("transaction_id").ToString();
                string trade_type = "ANDROID";
                res.SetValue("transaction_id", transaction_id);
                var payresult = PayResultFuc(res, trade_type);
                if (payresult == false)
                {
                    return Content("fail");
                }
                return Content("success");

            }
            catch (Exception ex)
            {
                Log.Error(this.GetType().ToString(), "交易出错 : " + ex.Message);
                return Content("交易出错，请联系商城管理员...");
            }
        }

        /// <summary>
        /// 回调共通方法（安卓、ios+商城+微商城）
        /// </summary>
        /// <param name="res"></param>
        /// <param name="trade_type"></param>
        /// <returns></returns>
        private bool PayResultFuc(WxPayData res, string trade_type)
        {
            string transaction_id = res.GetValue("transaction_id").ToString();

            WxPayData oderres = WxPayApi.OrderQuery(res, trade_type);
            //若订单查询失败，则立即返回结果给微信支付后台
            if (oderres.GetValue("return_code").ToString() != "SUCCESS" || oderres.GetValue("result_code").ToString() != "SUCCESS")
            {
                //log
                WxPayData failres = new WxPayData();
                failres.SetValue("return_code", "FAIL");
                failres.SetValue("return_msg", "订单查询失败");

                Log.Error(this.GetType().ToString(), "Order query failure : " + failres.ToXml());
                return false;
            }
            //确认支付真正在成功，处理商城订单
            //订单号
            string out_trade_no = oderres.GetValue("out_trade_no").ToString();
            //交易金额
            decimal tradeMoney = TypeHelper.StringToDecimal(oderres.GetValue("total_fee").ToString()) / 100;
            List<OrderInfo> orderList = new List<OrderInfo>();
            foreach (string oid in StringHelper.SplitString(StringHelper.SubString(out_trade_no, out_trade_no.Length - 10), "A"))
            {
                OrderInfo orderInfo = Orders.GetOrderByOid(TypeHelper.StringToInt(oid));
                if (orderInfo != null)
                {
                    orderList.Add(orderInfo);
                }
            }
            decimal allSurplusMoney = 0M;
            foreach (OrderInfo orderInfo in orderList)
            {
                allSurplusMoney += orderInfo.SurplusMoney;
            }

            if (orderList.Count > 0 && allSurplusMoney <= tradeMoney)
            {
                foreach (OrderInfo orderInfo in orderList)
                {
                    if (orderInfo.SurplusMoney > 0 && orderInfo.OrderState == (int)OrderState.WaitPaying)
                    {
                        PluginInfo plugininfo = Plugins.GetPayPluginBySystemName("weixin");
                        Orders.PayOrder(orderInfo.Oid, OrderState.Confirming, transaction_id, DateTime.Now, 1, plugininfo.SystemName, plugininfo.FriendlyName);
                        OrderActions.CreateOrderAction(new OrderActionInfo()
                        {
                            Oid = orderInfo.Oid,
                            Uid = orderInfo.Uid,
                            RealName = "本人",
                            ActionType = (int)OrderActionType.Pay,
                            ActionTime = TypeHelper.ObjectToDateTime(oderres.GetValue("time_end")),
                            ActionDes = "你使用微信支付订单成功，微信支付订单号为:" + transaction_id
                        });

                        //队员抽水
                        Orders.OrderPayCommission(orderInfo);

                    }

                }
            }
            //log
            WxPayData reslog = new WxPayData();
            reslog.SetValue("return_code", "SUCCESS");
            reslog.SetValue("return_msg", "OK");
            Log.Info(this.GetType().ToString(), "order query success : " + reslog.ToXml());
            return true;
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ImageResult DrawNativeImg(string url)
        {
            //初始化二维码生成工具
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            qrCodeEncoder.QRCodeVersion = 0;
            qrCodeEncoder.QRCodeScale = 4;

            string urldecode = HttpUtility.UrlDecode(url);
            //将字符串生成二维码图片
            Bitmap image = qrCodeEncoder.Encode(urldecode, Encoding.UTF8);

            //保存为PNG到内存流  
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            //输出二维码图片
            Response.BinaryWrite(ms.GetBuffer());
            //输出验证图片
            return new ImageResult(ms.ToArray(), "image/png");
        }

        /// <summary>
        /// 判断订单是否已经支付成功（0：未成功，1：成功）
        /// </summary>
        /// <param name="oidLst">订单ids</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult IsPayedOrder(string oidLst)
        {
            //订单id列表
            foreach (string oid in StringHelper.SplitString(StringHelper.SubString(oidLst, oidLst.Length - 10),"A"))
            {
                OrderInfo orderInfo = Orders.GetOrderByOid(TypeHelper.StringToInt(oid));
                if (orderInfo != null)
                {
                    if (orderInfo.OrderState <= (int)OrderState.WaitPaying)
                    {
                        return Json(0, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(1, JsonRequestBehavior.AllowGet);
        }
    }
}