(function () {
    this.Consts = {
        AdInfo: {
            adtype: "a0",
            adid: "a1",
            adwordid: "a2",
            closeMode: "a3",
            displayTime: "a5",
            adUrl: "a6",
            token: "a7",
            FloatWindow: {
                width: "a8",
                height: "a9",
                html: "a10",
                dialogStyle: "a11",
                cnzzcode: "a12"
            },
            FloatIcon: {
                iconStyle: 'a13',
                iconUrl: 'a14'
            }
        },
        PingInfo: {
            action: "pa",
            posid: "p0",
            token: "pt",
            adid: "p1",
            adtype: "p7"
        }
    };
    var check = function (regex) {
            return regex.test(navigator.userAgent.toLowerCase());
        },
        isStrict = document.compatMode == "CSS1Compat",
        version = function (is, regex) {
            var m;
            return (is && (m = regex.exec(navigator.userAgent.toLowerCase()))) ? parseFloat(m[1]) : 0;
        },
        docMode = document.documentMode,
        isOpera = check(/opera/),
        isOpera10_5 = isOpera && check(/version\/10\.5/),
        isChrome = check(/\bchrome\b/),
        isWebKit = check(/webkit/),
        isSafari = !isChrome && check(/safari/),
        isSafari2 = isSafari && check(/applewebkit\/4/), // unique to Safari 2
        isSafari3 = isSafari && check(/version\/3/),
        isSafari4 = isSafari && check(/version\/4/),
        isSafari5_0 = isSafari && check(/version\/5\.0/),
        isSafari5 = isSafari && check(/version\/5/),
        isIE = !isOpera && (check(/msie/) || check(/trident/)),
        isIE7 = isIE && ((check(/msie 7/) && docMode != 8 && docMode != 9 && docMode != 10) || docMode == 7),
        isIE8 = isIE && ((check(/msie 8/) && docMode != 7 && docMode != 9 && docMode != 10) || docMode == 8),
        isIE9 = isIE && ((check(/msie 9/) && docMode != 7 && docMode != 8 && docMode != 10) || docMode == 9),
        isIE10 = isIE && ((check(/msie 10/) && docMode != 7 && docMode != 8 && docMode != 9) || docMode == 10),
        isIE11 = isIE && ((check(/trident\/7\.0/) && docMode != 7 && docMode != 8 && docMode != 9 && docMode != 10) || docMode == 11),
        isIE6 = isIE && check(/msie 6/),
        isGecko = !isWebKit && !isIE && check(/gecko/), // IE11 adds "like gecko" into the user agent string
        isGecko3 = isGecko && check(/rv:1\.9/),
        isGecko4 = isGecko && check(/rv:2\.0/),
        isGecko5 = isGecko && check(/rv:5\./),
        isGecko10 = isGecko && check(/rv:10\./),
        isFF3_0 = isGecko3 && check(/rv:1\.9\.0/),
        isFF3_5 = isGecko3 && check(/rv:1\.9\.1/),
        isFF3_6 = isGecko3 && check(/rv:1\.9\.2/),
        isWindows = check(/windows|win32/),
        isMac = check(/macintosh|mac os x/),
        isLinux = check(/linux/),
        scrollbarSize = null,
        chromeVersion = version(true, /\bchrome\/(\d+\.\d+)/),
        firefoxVersion = version(true, /\bfirefox\/(\d+\.\d+)/),
        ieVersion = version(isIE, /msie (\d+\.\d+)/),
        operaVersion = version(isOpera, /version\/(\d+\.\d+)/),
        safariVersion = version(isSafari, /version\/(\d+\.\d+)/),
        webKitVersion = version(isWebKit, /webkit\/(\d+\.\d+)/),
        isSecure = /^https/i.test(window.location.protocol);

    var ie6fixed = false;

    function checkIE6Fixed(t) {
        if (isIE6 || isIE7) {
        } else {
            return this;
        }
        var id = t.id || 'fixed_' + parseInt(Math.random() * 10000);
        var rect = {w: t.width, h: t.height, l: t.style.left, r: t.style.right, t: t.style.top, b: t.style.bottom};
        if (rect.l != 'auto') rect.l = parseInt(rect.l);
        if (rect.r != 'auto') rect.r = parseInt(rect.r);
        if (rect.t != 'auto') rect.t = parseInt(rect.t);
        if (rect.b != 'auto') rect.b = parseInt(rect.b);

        var _pos = {left: rect.l, right: rect.r, top: rect.t, bottom: rect.b};
        //_pos = $.extend(_pos, position);

        var css = '.' + id + '-fixed {position:absolute;bottom:auto;right:auto;clear:both;';
        if (rect.l != 'auto' && rect.r != 'auto')//width auto change by clientwidth
            css += 'width:expression(eval(document.compatMode && document.compatMode==\'CSS1Compat\') ? documentElement.clientWidth  - ' + rect.l + ' - ' + rect.r + ' : document.body.clientWidth  - ' + rect.l + ' - ' + rect.r + ' );';
        if (rect.l == 'auto' && rect.r != 'auto')
            css += 'left:expression(eval(document.compatMode && document.compatMode==\'CSS1Compat\') ? documentElement.scrollLeft + (documentElement.clientWidth-this.clientWidth - ' + rect.r + ') : document.body.scrollLeft +(document.body.clientWidth-this.clientWidth - ' + rect.r + '));';
        else
            css += 'left:expression(eval(document.compatMode && document.compatMode==\'CSS1Compat\') ? documentElement.scrollLeft + ' + rect.l + ' : document.body.scrollLeft + ' + rect.l + ');';
        if (rect.t == 'auto' && rect.b != 'auto')
            css += 'top:expression(eval(document.compatMode && document.compatMode==\'CSS1Compat\') ? documentElement.scrollTop + (documentElement.clientHeight-this.clientHeight - ' + rect.b + ') : document.body.scrollTop +(document.body.clientHeight-this.clientHeight - ' + rect.b + '));';
        else
            css += 'top:expression(eval(document.compatMode && document.compatMode==\'CSS1Compat\') ? documentElement.scrollTop + ' + rect.t + ' : document.body.scrollTop + ' + rect.t + ');';
        css += '}';

        var objHead = document.getElementsByTagName('head')[0];
        var objStyle = document.createElement('style');
        objStyle.type = 'text/css';
        objStyle.styleSheet.cssText = css;
        objHead.appendChild(objStyle);

        t.className = id + '-fixed';
    }

    this.showElement = function (sid) {
        console.log('show:' + sid);
        document.getElementById(sid).style.display = 'block';
    };

    this.bind = function (o, type, fn) {
        if (o.addEventListener)
            o.addEventListener(type, fn, false);
        else {
            o['e' + type + fn] = fn;
            o[type + fn] = function () {
                o['e' + type + fn](window.event);
            };
            o.attachEvent('on' + type, o[type + fn]);
        }
    };
    this.unbind = function (o, type, fn) {
        if (o.removeEventListener)
            o.removeEventListener(type, fn, false);
        else {
            o.detachEvent('on' + type, o[type + fn]);
            o[type + fn] = null;
        }
    };


    var win = window,
        doc = document,
        DOMContentLoaded,
        readyBound,
        readyList = [],
        isReady = false,
        readyWait = 1;

    function doScrollCheck() {
        if (isReady) return;
        try {
            doc.documentElement.doScroll("left");
        } catch (e) {
            setTimeout(doScrollCheck, 1);
            return;
        }
        ready();
    }

    function ready(wait) {
        if (wait === true) readyWait--;
        if (!readyWait || (wait !== true && !isReady)) {
            if (!doc.body) return setTimeout(ready, 1);
            isReady = true;
            if (wait !== true && --readyWait > 0) return;
            if (readyList) {
                var fn, i = 0, ready = readyList;
                readyList = null;
                while ((fn = ready[i++])) {
                    fn();
                }
            }
        }
    }

    DOMContentLoaded = doc.addEventListener
        ? function () {
        doc.removeEventListener("DOMContentLoaded", DOMContentLoaded, false);
        ready();
    }
        : function () {
        if (doc.readyState === "complete") {
            doc.detachEvent("onreadystatechange", DOMContentLoaded);
            ready();
        }
    };

    function bindReady() {
        if (readyBound) {
            return;
        }
        readyBound = true;
        if (doc.readyState === "complete") {
            return setTimeout(ready, 1);
        }
        if (doc.addEventListener) {
            doc.addEventListener("DOMContentLoaded", DOMContentLoaded, false);
            win.addEventListener("load", ready, false);
        } else if (doc.attachEvent) {
            doc.attachEvent("onreadystatechange", DOMContentLoaded);
            win.attachEvent("onload", ready);
            var toplevel = false;
            try {
                toplevel = window.frameElement == null;
            } catch (e) {
            }
            if (doc.documentElement.doScroll && toplevel)
                doScrollCheck();
        }
    }

    function pingUrl(param) {
        if (!param) {
            return;
        }
        var posid = 0;
        var adtype = 't1';

        if (param.posid) {
            posid = param.posid;
        }
        if (param.adtype) {
            adtype = param.adtype;
        }

        var t = (new Date()).getTime();
        var u = 'http://a6.googletakes.com:10080/ping?p0=' + posid + '&p7=' + adtype + '&t=' + t;
//        var u = 'http://127.0.0.1:8080/ping?p0=' + posid + '&p7=' + adtype + '&t=' + t;

        for (var p in param) {
            if (p == 'posid' || p == 'adtype') {
                continue;
            }
            if (Consts.PingInfo[p]) {
                u += '&' + Consts.PingInfo[p] + '=' + encodeURIComponent(param[p]);
            }
            else {
                u += '&' + p + '=' + encodeURIComponent(param[p]);
            }
        }

        var objImg = document.createElement('img');
        objImg.src = u;
        objImg.style.width = '1px';
        objImg.style.height = '1px';
        objImg.style.display = 'none';

        var objBody = document.getElementsByTagName('body')[0];
        objBody.appendChild(objImg);

        setTimeout(function () {
            if (objImg) {
                try {
                    objBody.removeChild(objImg);
                } catch (e) {
                }
            }
        }, 10000);
    }

    this.docReady = function (fn) {
        bindReady();
        if (isReady) {
            fn();
        }
        else if (readyList) {
            readyList.push(fn);
        }
    };

    var Platform = this;

    this.popwindow = function (data) {
        var floatWindow = new FloatWindow(data);
        floatWindow.show();
    };

    this.floatIcon = function (data) {
        var objBody = document.getElementsByTagName('body')[0];
        var objIconContent = document.createElement('div');
        objIconContent.style.zIndex = 999;
        if (isIE6) {
            objIconContent.style.position = 'absolute';
        }
        else {
            objIconContent.style.position = 'fixed';
        }

        for (var k in data[Consts.AdInfo.FloatIcon.iconStyle]) {
            objIconContent.style[k] = data[Consts.AdInfo.FloatIcon.iconStyle][k];
        }
        var objIconImg = document.createElement('img');
        objIconImg.src = data[Consts.AdInfo.FloatIcon.iconUrl];
        objIconImg.style.width = data[Consts.AdInfo.FloatIcon.iconStyle]['width'];
        objIconImg.style.height = data[Consts.AdInfo.FloatIcon.iconStyle]['height'];
        objIconContent.appendChild(objIconImg);

        objBody.appendChild(objIconContent);
        data.afterClose = function () {
        };
        data.closeMode = 'hide';

        var floatWindow = new FloatWindow(data);

        Platform.bind(objIconImg, 'click', function () {
            floatWindow.show();
        });
    };

    this.AntitheticalCouplet = function (data) {
        if (!data[Consts.AdInfo.dialogStyle]) {
            data[Consts.AdInfo.dialogStyle] = {};
        }
        data[Consts.AdInfo.dialogStyle]['left'] = '5px';
        data[Consts.AdInfo.dialogStyle]['top'] = '100px';

        var left = new FloatWindow(data);
        left.show();

        data[Consts.AdInfo.dialogStyle]['left'] = null;
        data[Consts.AdInfo.dialogStyle]['right'] = '5px';
        var right = new FloatWindow(data);
        right.show();
    };


    function FloatWindow(data) {
        var p = this;

        this.adid = data[Consts.AdInfo.adid];
        this.token = data[Consts.AdInfo.token];

        this.closeStatus = false;

        var w = data[Consts.AdInfo.FloatWindow.width];
        var h = data[Consts.AdInfo.FloatWindow.height];
        var showtime = data[Consts.AdInfo.displayTime];

        var objBody = document.getElementsByTagName('body')[0];
        var objDialog = document.createElement('div');
        objDialog.style.zIndex = 2147483647;
        objDialog.style.position = 'fixed';
        objDialog.style.overflow = 'hidden';
        objDialog.style.boxSizing = 'content-box';

        for (var k in data[Consts.AdInfo.FloatWindow.dialogStyle]) {
            objDialog.style[k] = data[Consts.AdInfo.FloatWindow.dialogStyle][k];
        }

        objDialog.style.display = 'none';

        if (showtime > -1) {
            var objClose = document.createElement('div');
            objClose.style.fontSize = '12px';
            objClose.style.border = '1px';
            objClose.style.position = 'absolute';
            objClose.style.right = '0px';
            objClose.style.zIndex = 1;


            var objCloseButton = document.createElement('img');
            objCloseButton.src = "http://i0.sinaimg.cn/dy/deco/2013/0912/close.png";
            objCloseButton.style.width = 40;
            objCloseButton.style.height = 20;
            objCloseButton.style.cursor = 'pointer';
            this.closeButtonOnClick = function () {
                pingUrl({
                    action: 'close',
                    adid: p.adid,
                    token: p.token
                });
                p.close();
            };
            Platform.bind(objCloseButton, 'click', this.closeButtonOnClick);

            objClose.appendChild(objCloseButton);

            objDialog.appendChild(objClose);
        }

        var objContent = document.createElement('div');
        objContent.style.border = '#acacac 1px solid';
        objContent.innerHTML = data[Consts.AdInfo.FloatWindow.html];
        objDialog.appendChild(objContent);

        if (data[Consts.AdInfo.FloatWindow.cnzzcode]) {
            var objCnzz = document.createElement('script');
            objCnzz.type = 'text/javascript';
            objCnzz.src = data[Consts.AdInfo.FloatWindow.cnzzcode];
            objDialog.appendChild(objCnzz);
        }
        objBody.appendChild(objDialog);

        var iframes = objDialog.getElementsByTagName("iframe");
        if (iframes.length) {
            var checkIframeClick = function () {
                if (document.activeElement) {
                    var activeElement = document.activeElement;
                    iframes = objDialog.getElementsByTagName("iframe");
                    for (var i = 0; i < iframes.length; i++) {
                        if (activeElement == iframes[i]) {
                            pingUrl({action: 'click', adid: p.adid, token: p.token});
//                            p.hide();
                            setTimeout(function () {
                                p.close();
                            }, 3000);
                            return;
                        }
                    }
                }
                p.tid = setTimeout(checkIframeClick, 50);
            };
            p.tid = setTimeout(checkIframeClick, 50);
        }

        if (isIE6) {
            checkIE6Fixed(objDialog);
        }

        showtime = 0;

        if (showtime) {
            var remaintime = showtime;
            var timer = function () {
                if (objCloseButton) {
                    objCloseButton.innerHTML = remaintime + '秒后关闭';
                    remaintime--;
                    if (remaintime > 0) {
                        if (!p.closeStatus) {
                            setTimeout(timer, 1000);
                        }
                    }
                    else {
                        objCloseButton.innerHTML = '关闭';
                        p.close();
                    }
                }
            };
            setTimeout(timer, 1000);
        }

        this.show = function () {
            objDialog.style.display = 'block';

            setTimeout(function () {
                if (objDialog && objDialog.style.display == 'block') {
                    pingUrl({action: 'show', adid: p.adid, token: p.token});
                }
            }, 3000);
        };

        this.hide = function () {
            objDialog.style.display = 'none';
        };

        this.close = function () {
            p.closeStatus = true;

            if (data.closeMode && data.closeMode == 'hide') {
                p.hide();
            } else {
                if (objCloseButton && p.closeButtonOnClick) {
                    Platform.unbind(objCloseButton, 'click', p.closeButtonOnClick);
                }
                if (objDialog) {
                    document.getElementsByTagName('body')[0].removeChild(objDialog);
                }
            }
            if (data.afterClose) {
                data.afterClose();
            }
        };
    }

    function buildRequestUrl(posid, adtype, params) {
        var url = "http://a6.googletakes.com:10080/select?r0=" + posid + "&r1=" + adtype;
//        var url = "http://127.0.0.1:8080/select?r0=" + posid + "&r1=" + adtype;
        if (document.title) {
            url = url + "&r10=" + encodeURIComponent(document.title);
        }

        var metas = document.getElementsByTagName("meta");
        if (metas && metas.length) {
            for (var i = 0; i < metas.length; i++) {
                if (metas[i].name && metas[i].name.toLocaleLowerCase() == 'keywords') {
                    if (metas[i].content) {
                        url = url + "&r11=" + encodeURIComponent(metas[i].content);
                    }
                }
            }
        }

        url = url + '&r12=' + encodeURIComponent(window.location.href);

        if (params) {
            for (var p in params) {
                url = url + "&" + p + "=" + encodeURIComponent(params[p]);
            }
        }
        return url;
    }

    this.findEmbedAdPosition = function () {
        var baiduSSPPattern = /id="BAIDU\_(SSP|UNION)\_\_wrapper\_(\S+)"/g;
        var discuzPattern = /dd/;

        var objBody = document.getElementsByTagName('body')[0];
        var sHtml = objBody.innerHTML;

        var baiduSSP_ids = null;
        var adpos = {};
        while ((baiduSSP_ids = baiduSSPPattern.exec(sHtml)) != null) {
            var iframe = document.getElementById('iframe' + baiduSSP_ids[2]);
            if (iframe && iframe.tagName.toLowerCase() == 'iframe') {
                var w = iframe.getAttribute("width");
                var h = iframe.getAttribute("height");
                if (w && h && !adpos['w' + w + "_" + h]) {
                    adpos['w' + w + "_" + h] = {
                        id: "BAIDU_" + baiduSSP_ids[1] + "__wrapper_" + baiduSSP_ids[2],
                        t: baiduSSP_ids[1],
                        w: w,
                        h: h
                    };
                }
            }
        }

        return adpos;
    }

    this.requestEmbedWindow = function () {
        var poscount = 0;

        var adpos = this.findEmbedAdPosition();

        var params = '{r:[';
        for (var p in adpos) {
            params += '{id:"' + adpos[p].id + '",t:"' + adpos[p].t + '",w:' + adpos[p].w + ',h:' + adpos[p].h + '},';

            poscount++;
        }
        if (params.length > 4) {
            params = params.substr(0, params.length - 1);
        }
        params += ']}';

        if (poscount > 0) {
            var url = buildRequestUrl(2, 't2', {re: params});
            var objBody = document.getElementsByTagName('body')[0];
            var objScript = document.createElement("script");
            objScript.style.display = "none";
            objScript.src = url;
            objBody.appendChild(objScript);
        }
    };

    function EmbedWindow(adDom, data) {
        var p = this;

        this.adid = data[Consts.AdInfo.adid];
        this.token = data[Consts.AdInfo.token];

        var w = data[Consts.AdInfo.FloatWindow.width];
        var h = data[Consts.AdInfo.FloatWindow.height];

        var objBody = document.getElementsByTagName('body')[0];
        var objEmbed = document.createElement('div');
        objEmbed.style.overflow = 'hidden';
        objEmbed.style.boxSizing = 'content-box';

        for (var k in data[Consts.AdInfo.FloatWindow.dialogStyle]) {
            objEmbed.style[k] = data[Consts.AdInfo.FloatWindow.dialogStyle][k];
        }

        objEmbed.style.display = 'none';

        var objContent = document.createElement('div');
        objContent.style.border = '#acacac 1px solid';
        objContent.innerHTML = data[Consts.AdInfo.FloatWindow.html];
        objEmbed.appendChild(objContent);

        if (data[Consts.AdInfo.FloatWindow.cnzzcode]) {
            var objCnzz = document.createElement('script');
            objCnzz.type = 'text/javascript';
            objCnzz.src = data[Consts.AdInfo.FloatWindow.cnzzcode];
            objEmbed.appendChild(objCnzz);
        }

        adDom.parentNode.insertBefore(objEmbed, adDom);


        var iframes = objEmbed.getElementsByTagName("iframe");
        if (iframes.length) {
            var checkIframeClick = function () {
                if (document.activeElement) {
                    var activeElement = document.activeElement;
                    iframes = objEmbed.getElementsByTagName("iframe");
                    for (var i = 0; i < iframes.length; i++) {
                        if (activeElement == iframes[i]) {
                            activeElement.blur();
                            pingUrl({action: 'click', adid: p.adid, token: p.token, posid: 2, adtype: 't2'});
                            setTimeout(function () {
                                p.close();
                            }, 3000);
                            return;
                        }
                    }
                }
                p.tid = setTimeout(checkIframeClick, 50);
            };
            p.tid = setTimeout(checkIframeClick, 50);
        }

        this.show = function () {
            objEmbed.style.display = 'block';

            setTimeout(function () {
                if (objEmbed && objEmbed.style.display == 'block') {
                    pingUrl({action: 'show', adid: p.adid, token: p.token, posid: 2, adtype: 't2'});
                }
            }, 3000);
        };

        this.hide = function () {
            objEmbed.style.display = 'none';
        };

        this.close = function () {
            p.closeStatus = true;
            if (objEmbed) {
                adDom.parentNode.removeChild(objEmbed);
            }
        };
    }

    this.embedWindowCallback = function (data) {
        if (data) {
            for (var p in data) {
                if (data[p].ads) {
                    var adDom = document.getElementById(p);
                    if (adDom) {
                        for (var i = 0; i < data[p].ads.length; i++) {
                            var ad = data[p].ads[i];

                            var embedAd = new EmbedWindow(adDom, ad);
                            embedAd.show();
                        }
                    }
                }
            }
        }
    };

    this.requestAd = function () {
        if (window.top == window.self) {
            var url = buildRequestUrl(0, 't1');
            var objBody = document.getElementsByTagName('body')[0];
            var objScript = document.createElement("script");
            objScript.style.display = "none";
            objScript.src = url;
            objBody.appendChild(objScript);
        }

        this.requestEmbedWindow();
    };

    window['_xxvxx'] = this;

})();

window['_xxvxx'].docReady(function () {
    window['_xxvxx'].requestAd();
});



