var MinutesSecond = 60 * 1000;
var MonthSecond = 30 * 24 * 60 * 60 * 1000;
var Coker = {
    Data: {
        DefauleUrl: "/Dashboard/index",
        Header: {
            Authorization: 'Bearer ' + $.cookie("token"),
            Secret: $.cookie("secret")
        },
        Time: {
            DataRetentionTime: 30 * MinutesSecond,
            DataRetentionLongTime: 3 * MonthSecond,
            ReCheckTime: 20 * MinutesSecond
        },
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
            expDate.setTime(expDate.getTime() + co.Cookie.EffectiveTime);
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
            if (location.pathname != "/") co.Cookie.Add("lastViewPage", location.pathname);
            typeof (PageReady) === "function" && PageReady();
        }
    },
    WebSite: {
        exchange: function (id) {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/Website/Exchange",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: id }),
                dataType: "json"
            }).done(function (result) {
                if (result.success) {
                    co.Cookie.EffectiveTime = co.Data.Time.DataRetentionLongTime;
                    co.Cookie.Add("LastWebSite", result.message);
                    co.Cookie.EffectiveTime = co.Data.Time.D;
                }
                _dfr.resolve(result);
            });
            return _dfr.promise();
        },
        getPrivacyAndTerms: function () {
            return $.ajax({
                url: "/api/Website/GetPrivacyAndTerms/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
            });
        }
    },
    User: {
        Login: function (para) {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/User/Login",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(para),
                dataType: "json"
            }).done(function (result) {
                co.Cookie.AddAll({
                    token: result.token,
                    secret: result.secret,
                    endDateTime: (new Date(result.endDateTime)).getTime()
                });
                _dfr.resolve(result);
            });
            return _dfr.promise();
        },
        Logout: function () {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/User/Logout",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            }).done(function (result) {
                if (result.success) {
                    _c.Cookie.DelAll();
                    location.href = "/";
                    _dfr.resolve();
                }
            });
            return _dfr.promise();
        },
        Check: function () {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/User/Chech",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            }).done(function (result) {
                co.Cookie.AddAll({
                    token: result.token,
                    secret: result.secret,
                    endDateTime: (new Date(result.endDateTime)).getTime()
                });
                co.Cookie.EffectiveTime = co.Data.Time.DataRetentionLongTime;
                co.Cookie.Add("LastWebSite", co.Cookie.Get("LastWebSite"));
                co.Cookie.EffectiveTime = co.Data.Time.D;
                _dfr.resolve(result);
            }).fail(function () {
                _c.Cookie.DelAll();
                _dfr.resolve();
            });
            return _dfr.promise();
        }
    },
    sweet: {
        config: {
            timeout: 1500
        },
        success: function (text, action, autoclose) {
            var closetime = false;
            if (autoclose) { closetime = Coker.sweet.config.timeout }

            Swal.fire({
                icon: 'success',
                html: text,
                showConfirmButton: !autoclose,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                timer: closetime
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        error: function (title, text, action, autoclose) {
            var closetime = false;
            if (autoclose) { closetime = Coker.sweet.config.timeout }

            Swal.fire({
                icon: 'error',
                title: title,
                html: text,
                showConfirmButton: !autoclose,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                reverseButtons: true,
                timer: closetime
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        confirm: function (title, text, confirmtexet, cancanceltext, action) {
            Swal.fire({
                icon: 'info',
                title: title,
                html: text,
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: confirmtexet,
                cancelButtonText: cancanceltext,
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        TitleHilight: function (string, title) {
            return string.replace("{0}", `<span class='ConfirmKeyWord'>${title}</span>`);
        }
    },
    Picker: {
        Init: function ($picker, setting) {
            const target = co.Object.merge({
                timePicker: true,
                timePicker24Hour: true,
                autoUpdateInput: true,
                locale: {
                    format: 'YYYY/MM/DD HH:mm',
                    separator: " ~ ",
                    applyLabel: "　確認　",
                    cancelLabel: "　取消　",
                    daysOfWeek: ["日", "一", "二", "三", "四", "五", "六"],
                    monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"]
                }
            }, setting || {})
            console.log(target);
            $picker.daterangepicker(target);

            $picker.on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
            });
        },
    },
    HtmlContent: {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/HtmlContent/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/HtmlContent/GetOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        Delete: function (id) {
            const myId = parseInt(id);
            return $.ajax({
                url: "/api/HtmlContent/Delete",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: myId }),
            });
        },
        GetTypeList: function () {
            return $.ajax({
                url: "/api/HtmlContent/GetTypeList",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        GetAllComponent: function () {
            return $.ajax({
                url: "/api/HtmlContent/GetAllComponent",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        GetComponent: function (type) {
            return $.ajax({
                url: "/api/HtmlContent/GetComponent",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ type: type }),
                dataType: "json"
            });
        }
    },
    WebMesnus: {
        getAll: function () {
            return $.ajax({
                url: "/api/WebMenu/GetAll",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        getAllList: function () {
            return $.ajax({
                url: "/api/WebMenu/GetAllList/",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        createOrEdit: function (data) {
            return $.ajax({
                url: "/api/WebMenu/CreateOrEdit",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        getConten: function (id) {
            return $.ajax({
                url: "/api/WebMenu/GetConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json"
            });
        },
        saveConten: function (data) {
            return $.ajax({
                url: "/api/WebMenu/saveConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        importConten: function (data) {
            return $.ajax({
                url: "/api/WebMenu/importConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        delete: function (id) {
            return $.ajax({
                url: "/api/WebMenu/Delete",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json"
            });
        },
        updateLevelAndSerNo: function (list) {
            return $.ajax({
                url: "/api/WebMenu/updateLevelAndSerNo",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ list: list }),
                dataType: "json"
            });
        },
        GetPageTypeList: function () {
            return $.ajax({
                url: "/api/WebMenu/GetPageTypeList",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
    },
    ObjectType: {
        GetAll: function () {
            return $.ajax({
                url: "/api/ObjectType/GetAll",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        }, createOrEdit: function (data) {
            return $.ajax({
                url: "/api/ObjectType/CreateOrEdit",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }, delete: function (id) {
            return $.ajax({
                url: "/api/ObjectType/DeleteHtmlContent",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json"
            });
        }, updateSerNo: function (list) {
            return $.ajax({
                url: "/api/ObjectType/UpdateSerNo",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ list: list }),
                dataType: "json"
            });
        }, getConten: function (id) {
            return $.ajax({
                url: "/api/ObjectType/GetConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json"
            });
        }, SaveConten: function (data) {
            return $.ajax({
                url: "/api/ObjectType/saveConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    },
    Product: {
        AddUp: {
            Product: function (data) {
                return $.ajax({
                    url: "/api/Product/ProductAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            Stock: function (data) {
                return $.ajax({
                    url: "/api/Product/StockAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            ProdTechCert: function (data) {
                return $.ajax({
                    url: "/api/Product/TechCertAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            ProdPrice: function (data) {
                return $.ajax({
                    url: "/api/Product/ProdPriceAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            Import: function (formData) {
                return $.ajax({
                    url: '/api/Product/ProdReplace',
                    type: 'POST',
                    data: formData,
                    headers: _c.Data.Header,
                    contentType: false,
                    crossDomain: true,
                    dataType: 'json',
                    mimeType: "multipart/form-data",
                    processData: false
                });
            }
        },
        Get: {
            ProdOne: function (id) {
                return $.ajax({
                    url: "/api/Product/GetProdDataOne/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { id: id },
                });
            },
            ProdStock: function (id) {
                return $.ajax({
                    url: "/api/Product/GetStockDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id },
                });
            },
            ProdTechCert: function (id) {
                return $.ajax({
                    url: "/api/Product/GetTechCertDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id },
                });
            },
            ProdPrice: function (id) {
                return $.ajax({
                    url: "/api/Product/GetPriceDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PSId: id },
                });
            },
        },
        Delete: {
            Prod: function (id) {
                return $.ajax({
                    url: "/api/Product/ProdDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                });
            },
            Stock: function (id) {
                return $.ajax({
                    url: "/api/Product/StockDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                });
            },
            Price: function (id) {
                return $.ajax({
                    url: "/api/Product/PriceDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                });
            }
        },
        Content: {
            GetConten: function (data) {
                return $.ajax({
                    url: "/api/Product/GetConten",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            SaveConten: function (data) {
                return $.ajax({
                    url: "/api/Product/SaveConten",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            ImportConten: function (data) {
                return $.ajax({
                    url: "/api/Product/ImportConten",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            }
        }
    },
    Tag: {
        AddDelect: function (data) {
            return $.ajax({
                url: "/api/Tag/TagAssociateAddDelect",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/Tag/GetProductDataAll/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { PId: id },
            });
        }
    },
    File: {
        Upload: function (formData) {
            return $.ajax({
                url: '/api/FileUpload/uploadFiles',
                type: 'POST',
                data: formData,
                headers: _c.Data.Header,
                contentType: false,
                crossDomain: true,
                dataType: 'json',
                mimeType: "multipart/form-data",
                processData: false
            });
        },
        Upload360: function (formData) {
            return $.ajax({
                url: '/api/FileUpload/upload360Files',
                type: 'POST',
                data: formData,
                headers: _c.Data.Header,
                contentType: false,
                crossDomain: true,
                dataType: 'json',
                mimeType: "multipart/form-data",
                processData: false
            });
        },
        UploadYTLink: function (data) {
            return $.ajax({
                url: "/api/FileUpload/uploadYTLink",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        getFileList: function (type) {
            return $.ajax({
                url: "/api/FileUpload/getFileList",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ type: type }),
                dataType: "json"
            });
        },
        getImgFile: function (data) {
            return $.ajax({
                url: "/api/FileUpload/getImgFiles",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        fileSortChange: function (data) {
            return $.ajax({
                url: "/api/FileUpload/fileSortChange",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Delete: function (data) {
            return $.ajax({
                url: "/api/FileUpload/DeleteFile",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        DeleteFileById: function (data) {
            return $.ajax({
                url: "/api/FileUpload/DeleteFileById",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        UploadImageInit: function (elementId, label_text) {
            const upload = new FileUploadWithPreview.FileUploadWithPreview(elementId, Option = {
                accept: "image/*",
                multiple: true,
                text: {
                    browse: "　瀏 覽　",
                    chooseFile: "選擇圖片...",
                    label: "圖片上傳",
                },
            });
            return upload;
        },
        Upload360Init: function (elementId, label_text) {
            const upload = new FileUploadWithPreview.FileUploadWithPreview(elementId, Option = {
                accept: "image/*",
                multiple: true,
                text: {
                    browse: "　瀏 覽　",
                    chooseFile: "選擇多張圖片...",
                    label: "360圖片上傳(檔名請按編號排序 ex: image-1.jpg、image-2.jpg...)",
                    selectedCount: "張圖片已選擇",
                },
            });
            return upload;
        },
        UploadVideoInit: function (elementId, label_text) {
            const upload = new FileUploadWithPreview.FileUploadWithPreview(elementId, Option = {
                accept: "video/*",
                multiple: true,
                text: {
                    browse: "　瀏 覽　",
                    chooseFile: "選擇檔案...",
                    label: "影片上傳",
                },
            });
            return upload;
        },
        UploadVideoPreviewInit: function (elementId, label_text) {
            const upload = new FileUploadWithPreview.FileUploadWithPreview(elementId, Option = {
                accept: "video/*",
                text: {
                    browse: "　瀏 覽　",
                    chooseFile: "選擇檔案...",
                    label: "影片上傳",
                },
            });
            return upload;
        }
    },
    Spec: {
        SpecAddUp: function (data) {
            return $.ajax({
                url: "/api/Specification/SpecAddUp_Data",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        TypeDelect: function (id) {
            return $.ajax({
                url: "/api/Specification/TypeDelete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
            });
        },
        SpecDelect: function (id) {
            return $.ajax({
                url: "/api/Specification/SpecDelete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
            });
        },
        GetPickSpecList: function () {
            return $.ajax({
                url: "/api/Specification/GetPickSpecList/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
            });
        },
    },
    Object: {
        merge: function (target, source) {
            console.log("in");
            // Iterate through `source` properties and if an `Object` set property to merge of `target` and `source` properties
            for (const key of Object.keys(source)) {
                if (source[key] instanceof Object) {
                    console.log(source[key]);
                    Object.assign(source[key], co.Object.merge(target[key], source[key]))
                }
            }

            // Join `target` and modified `source`
            Object.assign(target || {}, source)
            return target
        }
    }
}
var _c = Coker;
var co = Coker;
co.Cookie.EffectiveTime = co.Data.Time.DataRetentionTime;