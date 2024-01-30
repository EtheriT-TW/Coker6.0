'use strict';
$.fn.extend({
    GetPageJson: function () {
        let obj = {};
        const $self = $(this);
        const $items = $self.find(".formItem");
        $items.each(function () {
            const $item = $(this);
            const $top = $item.parents(".formItem");
            if ($top.length == 0 || $top.is($self)) {
                const key = $item.attr("name") || $item.data("name") || $item.attr("id");
                if ($item.hasClass("list")) {
                    if (!!key && key.indexOf("devextreme") < 0) obj[key] = $item.data("component").option("dataSource");
                    else obj = $item.data("component").option("dataSource");
                } else if ($item.attr("type") == "checkbox") {
                    obj[key] = $item.prop("checked");
                } else if (key == "TagSelected") {
                    obj[key] = tag_list;
                } else if ($item.hasClass("image_upload")) {
                    obj[key] = $item.data("path");
                } else if ($item.data("type") == "object") {
                    obj[key] = $item.GetPageJson();
                } else {
                    if ($item.prop("disabled") && !!$item.data("default")) {
                        obj[key] = $item.data("default");
                    } else obj[key] = $item.val();
                }
            }
        });
        return obj;
    },
    CokerMultiStepForm: function (options) {
        const $self = this;
        const defaults = {
            activeIndex: 0,
            activeClass: "active",
            validate: {},
            hideBackButton: false,
            allowUnvalidatedStep: false,
            allowClickNavigation: false,
            btn: {
                next: $self.find(`.msf-nav-button[data-type="next"]`),
                back: $self.find(`.msf-nav-button[data-type="back"]`),
                submit: $self.find(`.msf-nav-button[data-type="submit"]`)
            },
            save: null
        };
        const settings = $.extend({}, defaults, options);
        const $header = $self.find(".msf-header .msf-step");
        const $conten = $self.find(".msf-content .msf-view");
        const changeView = function () {
            $header.removeClass(settings.activeClass).eq(settings.activeIndex).addClass(settings.activeClass);
            $conten.removeClass(settings.activeClass).eq(settings.activeIndex).addClass(settings.activeClass);
            if (settings.activeIndex == 0) {
                settings.btn.back.addClass("d-none");
                settings.btn.next.removeClass("d-none");
                settings.btn.submit.addClass("d-none");
            } else if (settings.activeIndex == $header.length - 1) {
                settings.btn.back.removeClass("d-none");
                settings.btn.next.addClass("d-none");
                settings.btn.submit.removeClass("d-none");
            } else {
                settings.btn.back.removeClass("d-none");
                settings.btn.next.removeClass("d-none");
                settings.btn.submit.addClass("d-none");
            }
        }
        settings.btn.next.on("click", function () {
            settings.activeIndex++;
            if (settings.activeIndex > $header.length - 1) settings.activeIndex = $header.length - 1;
            changeView();
        });
        settings.btn.back.on("click", function () {
            settings.activeIndex--;
            if (settings.activeIndex < 0) settings.activeIndex = 0;
            changeView();
        });
        changeView();
        $self.on("coker-msf:viewChanged", function (event, data) {
            var progress = Math.round((data.completedSteps / data.totalSteps) * 100);
            $(".progress-bar").css("width", progress + "%").attr('aria-valuenow', progress);
        });
        $self.initPage = function () {
            settings.activeIndex = 0;
            changeView();
        };
        $self.getJson = function () {
            const json = {};
            $conten.each(function () {
                const $self = $(this);
                switch ($self.data("key")) {
                    case "main":
                        co.Object.merge(json, $self.GetPageJson());
                        break;
                    default:
                        json[$self.data("key")] = $self.GetPageJson();
                        break;
                }
            });
            return json;
        };
        $self.set = function (json) {
            $conten.each(function () {
                const $self = $(this);
                switch ($self.data("key")) {
                    case "main":
                        $self.find(".title textarea").val(json.title);
                        $self.find(".no input").val(json.no);
                        $self.find(".BGColor input").val(json.bgColor); 
                        if (!!json.logo) {
                            json.logo.File = null;
                            ImageSetData($self.find(`.image_upload[data-name="Logo"]`).data("path", json.logo), json.logo);
                            $self.find(`.image_upload[data-name="Logo"] .img_input_frame`).data("delectList");
                        } else $self.find(`.image_upload[data-name="Logo"]`).data("path", "");
                        if (!!json.logoCompress) {
                            json.logoCompress.File = null;
                            ImageSetData($self.find(`.image_upload[data-name="LogoCompress"]`).data("path", json.logoCompress), json.logoCompress);
                            $self.find(`.image_upload[data-name="LogoCompress"] .img_input_frame`).data("delectList");
                        } else $self.find(`.image_upload[data-name="LogoCompress"]`).data("path", "");
                        break;
                    case "mainManu":
                        const timmer = setInterval(function () {
                            const component = $self.find(".dx-widget").data("component");
                            if (!!component) {
                                $self.find(".dx-widget").data("component").option("dataSource", json.mainManu);
                                clearInterval(timmer);
                            }
                        }, 100);
                        break;
                    default:
                        if (!!json[$self.data("key")]) {
                            var con = json[$self.data("key")]
                            if (!!con.conten)
                                $self.find(".conten textarea").text(con.conten);
                            if (!!con.more) {
                                $self.find(`[name="title"]`).val(con.more.title);
                                $self.find(`[name="link"]`).val(con.more.link);
                                $self.find(`[name="target"]`).prop("checked", con.more.target == 1);
                                $self.find(`[name="alert"]`).val(con.more.alert);
                            }
                            if (!!con.list && $self.find(".dx-widget").length > 0) {
                                const timmer = setInterval(function () {
                                    const component = $self.find(".dx-widget").data("component");
                                    if (!!component) {
                                        $self.find(".dx-widget").data("component").option("dataSource", con.list);
                                        clearInterval(timmer);
                                    }
                                }, 100);
                            }
                            if (!!con.mainTitle)
                                $self.find(".mainTitle textarea").text(con.mainTitle);
                            if (!!con.title)
                                $self.find(".title textarea").val(con.title);
                            if ($self.find(`[name="Visible"]`).length>0)
                                $self.find(`[name="Visible"]`).prop("checked", con.visible);
                            if (!!con.line) {
                                $self.find(`.line [name="link"]`).val(con.line.link);
                                $self.find(`.line [name="alert"]`).val(con.line.alert);
                            }
                            if (!!con.home) {
                                $self.find(`.home [name="link"]`).val(con.home.link);
                                $self.find(`.home [name="alert"]`).val(con.home.alert);
                            }
                            if (!!con.image) ImageSetData($self.find(`.image_upload[data-name="image"]`).data("path", con.image), con.image);
                            else $self.find(`.image_upload[data-name="image"]`).data("path", "");
                            if (!!con.imageCompress) ImageSetData($self.find(`.image_upload[data-name="imageCompress"]`).data("path", con.imageCompress), con.imageCompress);
                            else $self.find(`.image_upload[data-name="imageCompress"]`).data("path", "");

                            if (!!con.icon) ImageSetData($self.find(`.image_upload[data-name="icon"]`).data("path", con.icon), con.icon);
                            else $self.find(`.image_upload[data-name="icon"]`).data("path", "")
                            if (!!con.iconCompress) ImageSetData($self.find(`.image_upload[data-name="iconCompress"]`).data("path", con.iconCompress), con.iconCompress);
                            else $self.find(`.image_upload[data-name="iconCompress"]`).data("path", "")
                            $self.find(`.image_upload[data-name="icon"] .img_input_frame`).data("delectList");
                            $self.find(`.image_upload[data-name="iconCompress"] .img_input_frame`).data("delectList");
                        }
                        break;
                }
            });
        };
        return $self;
    }
});