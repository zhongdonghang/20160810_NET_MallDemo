using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrnMall.Web.Framework;
using System.Web.Mvc;
using BrnMall.PayPlugin.WeiXin.models;
using BrnMall.PayPlugin.WeiXin.codes;

namespace BrnMall.Web.MallAdmin.Controllers
{
    public class AdminWeiXinController : BaseMallAdminController
    {
        /// <summary>
        /// 配置
        /// </summary>
        [HttpGet]
        [ChildActionOnly]
        public ActionResult Config()
        {
            ConfigModel model = new ConfigModel();

            PluginSetInfo pluginSetInfo = PluginUtils.GetPluginSet();
            model.AppId = pluginSetInfo.APPID;
            model.Key = pluginSetInfo.Key;
            model.IosAppId = pluginSetInfo.IOSAPPID;
            model.IosMchId = pluginSetInfo.IOSMCHID;
            model.AndroidAppId = pluginSetInfo.ANDROIDAPPID;
            model.AndroidMchId = pluginSetInfo.ANDROIDMCHID;
            model.MchId = pluginSetInfo.MCHID;
            model.AppSecret = pluginSetInfo.APPSECRET;

            return View("~/plugins/BrnMall.PayPlugin.WeiXin/views/adminweixin/config.cshtml", model);
        }

        /// <summary>
        /// 配置
        /// </summary>
        [HttpPost]
        public ActionResult Config(ConfigModel model)
        {
            if (ModelState.IsValid)
            {
                PluginSetInfo pluginSetInfo = new PluginSetInfo();
                pluginSetInfo.APPID = model.AppId.Trim();
                pluginSetInfo.Key = model.Key.Trim();
                pluginSetInfo.MCHID = model.MchId.Trim();
                pluginSetInfo.IOSAPPID = model.IosAppId.Trim();
                pluginSetInfo.IOSMCHID = model.IosMchId.Trim();
                pluginSetInfo.ANDROIDAPPID = model.AndroidAppId.Trim();
                pluginSetInfo.ANDROIDMCHID = model.AndroidMchId.Trim();
                pluginSetInfo.APPSECRET = model.AppSecret.Trim();
                PluginUtils.SavePluginSet(pluginSetInfo);

                AddMallAdminLog("修改微信插件配置信息");
                return PromptView(Url.Action("config", "plugin", new { configController = "AdminWeiXin", configAction = "Config" }), "插件配 置修改成功");
            }
            return PromptView(Url.Action("config", "plugin", new { configController = "AdminWeiXin", configAction = "Config" }), "信息有误，请重新填写");
        }
    }
}
