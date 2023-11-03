var MinutesSecond = 60 * 1000;
var MonthSecond = 30 * 24 * 60 * 60 * 1000;
var Coker = {
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
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        Save: function (data) {
            return $.ajax({
                url: "/api/Website/Save",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
        },
        UpdatePassword: function (para) {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/User/UpdatePassword",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(para),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            }).done(function (result) {
                if (result.success) {
                    _c.sweet.success("密碼變更成功");
                    _dfr.resolve();
                } else { 
                    _c.sweet.error(result.message,"密碼請包含英文大小寫、數字及特殊符號且密碼長度達十二碼以上!!");
                    _dfr.resolve();
                }
            });
            return _dfr.promise();
        }
    },
    Company: {
        Save: function (data) {
            return $.ajax({
                url: "/api/Company/Save",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }
    },
    Recipient : {
        DeleteRecipients: function (id) {
            return $.ajax({
                url: "/api/Newsletter/DeleteRecipients/",
                type: "DELETE",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: id }),
            });
        },
        GetRecipientsTag: function () {
            return $.ajax({
                url: "/api/Newsletter/GetRecipientsTag/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }
    },
    Newsletter: {
        send: function (id) {
            return $.ajax({
                url: "/api/Newsletter/Send/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }
    },
    PowerManagement: {
        GetAll: function () {
            return $.ajax({
                url: "/api/PowerManagement/AllMenus/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
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
                showDropdowns:true,
                locale: {
                    format: 'YYYY/MM/DD HH:mm',
                    separator: " ~ ",
                    applyLabel: "　確認　",
                    cancelLabel: "　取消　",
                    daysOfWeek: ["日", "一", "二", "三", "四", "五", "六"],
                    monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"]
                }
            }, setting || {})
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
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/HtmlContent/GetOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetTypeList: function () {
            return $.ajax({
                url: "/api/HtmlContent/GetTypeList",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetAllComponent: function () {
            return $.ajax({
                url: "/api/HtmlContent/GetAllComponent",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetComponent: function (type) {
            return $.ajax({
                url: "/api/HtmlContent/GetComponent",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ type: type }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        getAllList: function () {
            return $.ajax({
                url: "/api/WebMenu/GetAllList/",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        createOrEdit: function (data) {
            return $.ajax({
                url: "/api/WebMenu/CreateOrEdit",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        getConten: function (id) {
            return $.ajax({
                url: "/api/WebMenu/GetConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        saveConten: function (data) {
            return $.ajax({
                url: "/api/WebMenu/saveConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        importConten: function (data) {
            return $.ajax({
                url: "/api/WebMenu/importConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        delete: function (id) {
            return $.ajax({
                url: "/api/WebMenu/Delete",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        updateLevelAndSerNo: function (list) {
            return $.ajax({
                url: "/api/WebMenu/updateLevelAndSerNo",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ list: list }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetPageTypeList: function () {
            return $.ajax({
                url: "/api/WebMenu/GetPageTypeList",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }, createOrEdit: function (data) {
            return $.ajax({
                url: "/api/ObjectType/CreateOrEdit",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }, delete: function (id) {
            return $.ajax({
                url: "/api/ObjectType/DeleteHtmlContent",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }, updateSerNo: function (list) {
            return $.ajax({
                url: "/api/ObjectType/UpdateSerNo",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ list: list }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }, getConten: function (id) {
            return $.ajax({
                url: "/api/ObjectType/GetConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }, GetNewsletterConten: function () {
            return $.ajax({
                url: "/api/ObjectType/GetNewsletterConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }, SaveConten: function (data) {
            return $.ajax({
                url: "/api/ObjectType/saveConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }
    },
    Articles : {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/Article/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        GetDataOne: function (Id) {
            return $.ajax({
                url: "/api/Article/GetDataOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        },
        Delete: function (Id) {
            return $.ajax({
                url: "/api/Article/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        },
        GetConten: function (data) {
            return $.ajax({
                url: "/api/Article/GetConten",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        SaveConten: function (data) {
            return $.ajax({
                url: "/api/Article/SaveConten",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        ImportConten: function (data) {
            return $.ajax({
                url: "/api/Article/ImportConten",
                type: "POST",
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
                    dataType: "json",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            Stock: function (data) {
                return $.ajax({
                    url: "/api/Product/StockAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            ProdTechCert: function (data) {
                return $.ajax({
                    url: "/api/Product/TechCertAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            ProdPrice: function (data) {
                return $.ajax({
                    url: "/api/Product/ProdPriceAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
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
                    processData: false,
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
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
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            ProdStock: function (id) {
                return $.ajax({
                    url: "/api/Product/GetStockDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            ProdTechCert: function (id) {
                return $.ajax({
                    url: "/api/Product/GetTechCertDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            ProdPrice: function (id) {
                return $.ajax({
                    url: "/api/Product/GetPriceDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PSId: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
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
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            Stock: function (id) {
                return $.ajax({
                    url: "/api/Product/StockDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            Price: function (id) {
                return $.ajax({
                    url: "/api/Product/PriceDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
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
                    dataType: "json",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            SaveConten: function (data) {
                return $.ajax({
                    url: "/api/Product/SaveConten",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            ImportConten: function (data) {
                return $.ajax({
                    url: "/api/Product/ImportConten",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            }
        }
    },
    Member: {
        Get: function (id) {
            return $.ajax({
                url: "/api/Member/GetAllData/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        Update: function (data) {
            return $.ajax({
                url: "/api/Member/Update",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetSelf: function () {
            return $.ajax({
                url: "/api/Member/GetSelfData/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
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
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/Tag/GetProductDataAll/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { PId: id },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                processData: false,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                processData: false,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        UploadYTLink: function (data) {
            return $.ajax({
                url: "/api/FileUpload/uploadYTLink",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        getFileList: function (type) {
            return $.ajax({
                url: "/api/FileUpload/getFileList",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ type: type }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        getImgFile: function (data) {
            return $.ajax({
                url: "/api/FileUpload/getImgFiles",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        fileSortChange: function (data) {
            return $.ajax({
                url: "/api/FileUpload/fileSortChange",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        Delete: function (data) {
            return $.ajax({
                url: "/api/FileUpload/DeleteFile",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        DeleteFileById: function (data) {
            return $.ajax({
                url: "/api/FileUpload/DeleteFileById",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        TypeDelect: function (id) {
            return $.ajax({
                url: "/api/Specification/TypeDelete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        SpecDelect: function (id) {
            return $.ajax({
                url: "/api/Specification/SpecDelete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetPickSpecList: function () {
            return $.ajax({
                url: "/api/Specification/GetPickSpecList/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
    },
    StoreSet: {
        GetValues: function (data) {
            return $.ajax({
                url: "/api/StoreSet/getValues/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        SaveValues: function (data) {
            return $.ajax({
                url: "/api/StoreSet/CreateOrUpdate",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }
    },
    Object: {
        merge: function (target, source) {
            // Iterate through `source` properties and if an `Object` set property to merge of `target` and `source` properties
            for (const key of Object.keys(source)) {
                if (source[key] instanceof Object) {
                    Object.assign(source[key], co.Object.merge(target[key], source[key]))
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
    Zipcode: {
        init: function (id) {
            $TWzipcode = $(id);

            $TWzipcode.twzipcode({
                'zipcodeIntoDistrict': true,
            });

            var $county, $district;

            $county = $TWzipcode.children('.county');
            $district = $TWzipcode.children('.district');

            $county.children('select').attr({
                id: "SelectCity",
                class: "city form-select",
                required: "required"
            });
            $county.append("<label class='px-4 required' for='SelectCity'>縣市</label>");
            var $county_first_option = $county.children('select').children('option').first();
            $county_first_option.text("請選擇縣市");
            $county_first_option.attr('disabled', 'disabled');

            $district.children('select').attr({
                id: "SelectTown",
                class: "town form-select",
                required: "required"
            });
            $district.append("<label class='required' for='SelectTown'>鄉鎮</label>");
            var $district_first_option = $district.children('select').children('option').first();
            $district_first_option.text("請選擇鄉鎮");
            $district_first_option.attr('disabled', 'disabled');
        },
        setData: function (obj) {
            const $addr = obj.el.find(".address");
            var address_split = obj.addr.split(" ");
            obj.el.twzipcode('set', {
                'county': address_split[0],
                'district': address_split[1],
            });
            $addr.val(address_split[2]);
        },
        getData: function ($e) {
            return $e.find(".county>select").val() + " " + $e.find(".district>select").val() + " " + $e.find(".address").val()
        }
    },
    Form: {
        insertData: function (obj) {
            for (const key in obj) {
                const $e = $(`[name="${key}"]`);
                if ($e.length > 0) {
                    switch ($e[0].tagName) {
                        case "INPUT":
                            switch ($e.attr("type").toLowerCase()) {
                                case "radio":
                                    $(`[name="${key}"][value="${obj[key]}"]`).prop("checked", true);
                                    break;
                                default:
                                    $e.val(obj[key]);
                                    break;
                            }
                            break;
                        case "DIV":
                            switch ($e.data("form-type")) {
                                case "zipcode":
                                    co.Zipcode.setData({
                                        el: $e,
                                        addr: obj[key]
                                    });
                                    break;
                            }
                            break;
                    }
                } else console.log(key);
            }
        },
        getJson: function (id) {
            let form = document.getElementById(id);
            let formFields = new FormData(form);
            let formDataObject = Object.fromEntries(formFields.entries());
            let exItems = $(`#${id}`).find(`div[name]`);
            exItems.each(function () {
                const $e = $(this);
                switch ($e.data("form-type")) {
                    case "zipcode":
                        formDataObject[$e.attr("name")] = co.Zipcode.getData($e);
                        break;
                }
            });
            return formDataObject;
        }
    }
}
var _c = Coker;
var co = Coker;
co.Cookie.EffectiveTime = co.Data.Time.DataRetentionTime;