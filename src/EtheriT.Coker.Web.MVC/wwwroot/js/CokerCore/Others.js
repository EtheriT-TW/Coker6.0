Coker.extend({
    Recipient: {
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
        }, UpdateJson: function (data) {
            return $.ajax({
                url: "/api/Newsletter/UpdateJson",
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
        }, SaveConten: function (data) {
            return $.ajax({
                url: "/api/Newsletter/SaveConten",
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
    Picker: {
        Init: function ($picker, setting) {
            const today = moment();
            const target = co.Object.merge({
                timePicker: true,
                timePicker24Hour: true,
                autoUpdateInput: true,
                showDropdowns: true,
                startDate:today.Date,
                endDate:today.Date,
                locale: {
                    format: 'YYYY/MM/DD HH:mm',
                    separator: " ~ ",
                    applyLabel: "　確認　",
                    cancelLabel: "　取消　",
                    daysOfWeek: ["日", "一", "二", "三", "四", "五", "六"],
                    monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"]
                }
            }, setting || {})
            const s = $picker.daterangepicker(target);
            $picker.data("picker",s);
            $picker.on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
            });
        },
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
    Zipcode: {
        init: function (id) {
            const reandomStr = co.String.generateRandomString(5);
            $TWzipcode = $(id);

            $TWzipcode.twzipcode({
                'zipcodeIntoDistrict': true,
            });

            var $county, $district;

            $address = $TWzipcode.find('.address');
            $county = $TWzipcode.children('.county');
            $district = $TWzipcode.children('.district');

            $county.children('select').attr({
                id: `SelectCity_${reandomStr}`,
                class: "city form-select"
            }).prop("required", $address.prop("required"));
            $county.append(`<label class='px-4 required' for='SelectCity_${reandomStr}'>縣市</label>`);
            var $county_first_option = $county.children('select').children('option').first();
            $county_first_option.text("請選擇縣市");
            $county_first_option.attr('disabled', 'disabled');

            $district.children('select').attr({
                id: `SelectTown_${reandomStr}`,
                class: "town form-select"
            }).prop("required", $address.prop("required"));
            $district.append(`<label class='required' for='SelectTown_${reandomStr}'>鄉鎮</label>`);
            var $district_first_option = $district.children('select').children('option').first();
            $district_first_option.text("請選擇鄉鎮");
            $district_first_option.attr('disabled', 'disabled');
        },
        setData: function (obj) {
            const $addr = obj.el.find(".address");
            if (co.String.isNullOrEmpty(obj.addr)) {
                obj.el.twzipcode('reset');
                obj.el.find(".address").val("");
            } else {
                var address_split = obj.addr.split(" ");
                obj.el.twzipcode('set', {
                    'county': address_split[0],
                    'district': address_split[1],
                });
                $addr.val(address_split[2]);
            }
        },
        getData: function ($e) {
            return $e.find(".county>select").val() + " " + $e.find(".district>select").val() + " " + $e.find(".address").val()
        }
    }, Grapes: {
        setEditor: (editor,html,css) => {
            editor.DomComponents.clear(); // Clear components
            editor.CssComposer.clear();  // Clear styles
            editor.UndoManager.clear(); // Clear undo history
            editor.setStyle(css);
            editor.setComponents(html);
        },
        setFile: function (editor,id,type) {
            co.File.getFileList({ id: id, type: type }).done(function (result) {
                if (result.success) {
                    var images = [];
                    $(result.files).each(function (index) {
                        images.push({
                            src: this.path,
                            name: `${this.name}`,
                            guid: this.guid
                        });
                    });
                    editor.settings.asset = result.files;
                    editor.AssetManager.add(images);
                }
            });
        }
    }, Date: {
        GetDateTimeStr: function (str) {
            const datetime = new Date(str);
            const dateStr = `${String(datetime.getFullYear()).padStart(4, "0")}-${String(datetime.getMonth() + 1).padStart(2, "0")}-${String(datetime.getDate()).padStart(2, "0")
                }T${String(datetime.getHours()).padStart(2, "0")
                }:${String(datetime.getMinutes()).padStart(2, "0")
                }:${String(datetime.getSeconds()).padStart(2, "0")}`; 
            return dateStr;
        }
    }
});

var _c = Coker;
var co = Coker;
co.Cookie.EffectiveTime = co.Data.Time.DataRetentionTime;