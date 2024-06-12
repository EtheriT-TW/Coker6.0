const Coker ={
    extend:function(obj) {
       for (var i in obj) {
          if (!Coker.hasOwnProperty(i)) {
             Coker[i] = obj[i];
          }
       }
    },
    Data: {
        DefauleUrl: "/Welcome/index",
        Header: {
            Authorization: 'Bearer ' + $.cookie("token"),
            Secret: $.cookie("secret")
        },
        Time: {
            DataRetentionTime: 30 * MinutesSecond,
            DataRetentionLongTime: 3 * MonthSecond,
            ReCheckTime: 20 * MinutesSecond
        },
        Target: [
            { Id:1, Name: "另開新視窗", value: "_blank" },
            { Id:0, Name: "直接連結", value: "_self" }
        ],
        ReplaceAndSinge: function (str) {
            if (!!str) {
                var s = str.replace(/&amp;/g, "&");
                if (s.indexOf("&amp;") > 0) return _c.Data.ReplaceAndSinge(s);
                else return s
            } else return "";
        },
        HtmlDecode: function (str) {
            var ele = document.createElement('span');
            ele.innerHTML = _c.Data.ReplaceAndSinge(str);
            return ele.textContent || ele.innerText;
        },
        HtmlEncode: function (str) {
            var ele = document.createElement('span');
            ele.appendChild(document.createTextNode(str));
            return ele.innerHTML;
        }

    },
    Cookie: {
        EffectiveTime: 0,
        Add: function (key, value) {
            var expDate = new Date();
            expDate.setTime(expDate.getTime() + Coker.Cookie.EffectiveTime);
            $.cookie(key, value, { path: "/", expires: expDate });
        },
        AddAll: function (obj) {
            for (var key in obj) {
                if (typeof (key) != "object") _c.Cookie.Add(key, obj[key]);
            }
        },
        Del: function (key) {
            $.removeCookie(key, { path: "/" });
        },
        Get: function (key) {
            return $.cookie(key);
        },
        DelAll: function () {
            var cookies = $.cookie();
            for (var cookie in cookies) {
                if (cookie != "LastWebSite") $.removeCookie(cookie, { path: "/" });
            }
        }
    },
    Page: {
        Ready: function () {
            if (location.pathname != "/") Coker.Cookie.Add("lastViewPage", location.pathname);
            typeof (PageReady) === "function" && PageReady();
        }
    },
    i18: {
        getAll: function () {
            return $.ajax({
                url: "/api/i18/getLocal",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }
    },
    String: {
        generateRandomString: function (num) {
            const characters =
                'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
            let result1 = ' ';
            const charactersLength = characters.length;
            for (let i = 0; i < num; i++) {
                result1 +=
                    characters.charAt(Math.floor(Math.random() * charactersLength));
            }

            return result1;
        },
        isNullOrEmpty: function (str) {
            if (typeof (str) == "undefined" || str == null || str.trim() == "") return true;
            else return false;
        },
        getWeekNumber: function (i) {
            const characters = "一二三四五六日";
            return characters.charAt(i-1);
        }
    },
    Object: {
        merge: function (target, source) {
            // Iterate through `source` properties and if an `Object` set property to merge of `target` and `source` properties
            for (const key of Object.keys(source)) {
                if (source[key] instanceof Object && !Array.isArray(source[key])) {
                    Object.assign(source[key], Coker.Object.merge(target[key], source[key]))
                }
            }

            // Join `target` and modified `source`
            Object.assign(target || {}, source)
            return target
        },
        arrayToObject: function (array) {
            let obj = {};
            for (let i = 0; i < array.length; i++) {
                obj[array[i].key] = array[i].value;
            }
            return obj;
        },
        objectToArray: function (obj) {
            let array = [];
            for (const key of Object.keys(obj)) {
                array.push({
                    key: key,
                    value: obj[key]
                });
            }
            return array;
        }
    },
    Array: {
        Search: function (array, obj, rejectID) {
            var index = -1
            var i = 0;
            if (Array.isArray(array)) {
                array.forEach(function (element) {
                    var m = true;
                    if (element.ID != rejectID || typeof (rejectID) == "undefined") {
                        for (var key in obj) {
                            if (element[key] != obj[key]) {
                                m = false;
                                break;
                            }
                        }
                        if (m) {
                            index = i;
                            return;
                        }
                    }
                    i++;
                });
            }
            return index;
        },
        Delete: function (array, obj) {
            const index = _c.Array.Search(array, obj);
            if(index >-1) array.splice(index, 1);
        }
    }
}