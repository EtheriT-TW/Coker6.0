(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        util: {
            device: {
                isMobileDevice: function () {
                    var mobileDevices = ["Android", "webOS", "iPhone", "iPad", "iPod", "BlackBerry", "Windows Phone"];
                    for (var i = 0; i < mobileDevices.length; i++) {
                        if (navigator.userAgent.match(mobileDevices[i])) return true;
                    }
                    return false;
                },
                getDeviceType: function () {
                    var ua = navigator.userAgent;

                    // 偵測是否為平板 (iPad 或 Android 且不含 Mobile 字樣通常是大螢幕平板)
                    var isTablet = /(ipad|tablet|(android(?!.*mobile))|(windows(?!.*phone)(.*touch))|kindle|playbook|silk)/i.test(ua);

                    // 偵測是否為手機
                    var isMobile = /Mobile|iP(hone|od)|Android|BlackBerry|IEMobile|Kindle|NetFront|Silk-Accelerated|(hpw|web)OS|Fennec|Minimo|Opera M(obi|ini)|Blazer|Dolfin|Dolphin|Skyfire|Zune/i.test(ua);

                    // 偵測特殊環境 (對你抓 LINE Bug 很有幫助)
                    var isLine = /Line/i.test(ua);

                    if (isTablet) return "平板" + (isLine ? " (LINE)" : "");
                    if (isMobile) return "手機" + (isLine ? " (LINE)" : "");
                    return "電腦";
                },
                getNetworkStatus: function () {
                    if (navigator.onLine === false) return "❌ 斷線";
                    var conn = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
                    if (!conn || !conn.effectiveType) return null;
                    var statusMap = { '4g': '🟢 暢通', '3g': '🟡 延遲', '2g': '🟠 壅塞', 'slow-2g': '🔴 極慢' };
                    return statusMap[conn.effectiveType.toLowerCase()] || null;
                },
                getBrowserInfo: function () {
                    var ua = navigator.userAgent, match;
                    if ((match = ua.match(/Line\/([\d.]+)/i))) return "LINE v" + match[1];
                    if ((match = ua.match(/Edg\/([\d.]+)/i))) return "Edge v" + match[1];
                    if ((match = ua.match(/Chrome\/([\d.]+)/i))) return "Chrome v" + match[1];
                    if ((match = ua.match(/Firefox\/([\d.]+)/i))) return "Firefox v" + match[1];
                    if ((match = ua.match(/Safari\/([\d.]+)/i)) && !/Chrome/i.test(ua)) {
                        var v = ua.match(/Version\/([\d.]+)/i);
                        return "Safari v" + (v ? v[1] : match[1]);
                    }
                    return "未知軟體";
                },
                getLoadTime: function () {
    var nav = window.performance.getEntriesByType("navigation")[0];
    if (!nav) return null;

    var total = (nav.loadEventEnd / 1000).toFixed(2);
    var dom = (nav.domContentLoadedEventEnd / 1000).toFixed(2);
    var ttfb = (nav.responseStart / 1000).toFixed(2); // 伺服器反應時間

    if (nav.loadEventEnd <= 0) return "⌛ 載入中...";

    // 格式：總計s [Server等待s | DOM建構s]
    return total + "s [伺服器:" + ttfb + "s | 結構:" + dom + "s]";
},
                initDebugMode: function () {
                    var self = this;
                    var _update = function () {
                        if (document.body.id !== 'debug-env') return;

                        var data = {
                            "寬度": window.innerWidth + "px",
                            "DPR": window.devicePixelRatio,
                            "基準": window.getComputedStyle(document.documentElement).fontSize,
                            "類型": self.getDeviceType(),
                            "軟體": self.getBrowserInfo(),
                            "耗時": self.getLoadTime(),
                            "模式": window.matchMedia('(prefers-color-scheme: dark)').matches ? "深色" : "淺色"
                        };

                        var net = self.getNetworkStatus();
                        if (net) data["網路"] = net;

                        var infoString = Object.keys(data).map(function (k) { return k + ": " + data[k]; }).join("\n");
                        document.body.setAttribute('data-debug-content', "📊 環境診斷\n" + infoString);
                    };

                    var _check = function () {
                        if (window.location.hash === '#debug-env') {
                            document.body.id = 'debug-env';
                            _update();
                        } else if (document.body.id === 'debug-env') {
                            document.body.removeAttribute('id');
                            document.body.removeAttribute('data-debug-content');
                        }
                    };

                    window.addEventListener('hashchange', _check);
                    window.addEventListener('resize', _update);
                    _check();
                }
            }
        }
    });

    // Legacy: Coker.isMobileDevice()
    Coker.isMobileDevice = Coker.isMobileDevice || function () {
        return Coker.util.device.isMobileDevice();
    };
    // 初始化 Debug 模式
    Coker.util.device.initDebugMode();
})(window);