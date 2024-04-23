Coker.extend({
    Form: {
        insertData: function (obj, $self) {
            if (typeof ($self) == "undefined" || $self == null) $self = $("form").first();
            else if (typeof ($self) == "string") $self = $($self);
            const formTypeSet = (type, $e, value) => {
                switch (type) {
                    case "zipcode":
                        co.Zipcode.setData({
                            el: $e,
                            addr: value
                        });
                        break;
                    case "date_range":
                        if (!!!$e.data('daterangepicker')) _c.Picker.Init($e);
                        if (!!obj[$e.data("start")] || !!obj[$e.data("end")]) {
                            $e.data('daterangepicker').setStartDate(obj[$e.data("start")]);
                            $e.data('daterangepicker').setEndDate(obj[$e.data("end")]);
                        } else $e.val("");
                        break;
                    case "date":
                        if (!!!$e.data('daterangepicker'))
                            _c.Picker.Init($e, { singleDatePicker: true, timePicker: false, locale: { format: 'YYYY/MM/DD' } });
                        $e.data('daterangepicker').setStartDate(value||"");
                        break;
                    case "disabled":
                        $e.on("change", function () {
                            const checked = $(this).data("direct") == "reverse" ? !$(this).prop("checked") : $(this).prop("checked");
                            if (checked) $(`#${$(this).data("target")}`).attr("disabled", "disabled").val("");
                            else $(`#${$(this).data("target")}`).removeAttr("disabled")
                        });

                        if (!!$e.data("value")) {
                            let _v = $(`#${$e.data("target")}`).val();
                            if (typeof ($e.data("value")) == "number") _v = parseInt(_v);
                            else if (typeof ($e.data("value")) == "string") _v = _v.toString();
                            value = $e.data("direct") == "reverse" ? !($e.data("value") == _v) : $e.data("value") == _v;
                        }
                        $e.prop("checked", value);
                        $e.trigger("change");
                        break;
                    case "images":
                        if (!!!$e.data("init")) {
                            $e.ImageUploadModalClear();
                            $e.data("init",true)
                        }
                        co.File.getImgFile({ Sid: obj[$e.data("target")], Type: $e.data("image-type"), Size: $e.data("image-size") }).done(function (file) {
                            if (file.length > 0)
                                ImageUploadModalDataInsert($e, file[0].id, file[0].link, file[0].name)
                        });
                        break;
                    case "html":
                        $e.empty().html($("<div>").html(value).html());
                        break;
                    default:
                        $e.val(value);
                        break;
                }
            }
            for (const key in obj) {
                const $e = $self.find(`[name="${key}"]`);
                if ($e.length > 0) {
                    if (!!$e.data("form-type")) formTypeSet($e.data("form-type"), $e, obj[key])
                    else {
                        switch ($e[0].tagName) {
                            case "INPUT":
                                switch ($e.attr("type").toLowerCase()) {
                                    case "radio":
                                        $self.find(`[name="${key}"][value="${obj[key]}"]`).prop("checked", true);
                                        break;
                                    case "checkbox":
                                        $e.prop("checked", obj[key]);
                                        break;

                                    case "datetime-local":
                                        $e.val(co.Date.GetDateTimeStr(obj[key]));
                                        break;
                                    default:
                                        $e.val(obj[key]);
                                        break;
                                }
                                break;
                            default:
                                $e.val(obj[key]);
                                break;
                        }
                    }
                } else console.log(key);
            }
        },
        getJson: function (id) {
            let form = document.getElementById(id);
            let formFields = new FormData(form);
            let formDataObject = Object.fromEntries(Array.from(formFields.keys(), key => {
                const val = formFields.getAll(key)
                return [key, val.length > 1 ? val : val.pop()]
            }));
            let exItems = $(`#${id}`).find(`div[name]`);
            exItems.each(function () {
                const $e = $(this);
                switch ($e.data("form-type")) {
                    case "zipcode":
                        formDataObject[$e.attr("name")] = co.Zipcode.getData($e);
                        break;
                    case "tags":
                        formDataObject[$e.attr("name")] = $e.find(".InputTag").data("tagList")
                        break;
                }
            });
            return formDataObject;
        },
        init: function (id, fun) {
            const form = document.getElementById(id);
            form.addEventListener('submit', event => {
                $(form).find(".customValidity").get(0).setCustomValidity("");
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    fun && fun(id);
                }
                form.classList.add('was-validated');
            }, false)
        }, clear: function (id) {
            const form = document.getElementById(id);
            const $items = $(`[data-form-type]`);
            _c.Form.insertData(_c.Form.getJson(id), `#${id}`);
            $items.each(function (i, e) {
                const $e = $(e);
                switch ($e.data("form-type")) {
                    case "images":
                        if (!!!$e.data("init")) {
                            $e.ImageUploadModalClear();
                            $e.data("init", true)
                        } else $e.ImageUploadModalClear();
                        break;
                }
            });
            form.reset();
        }
    }
});