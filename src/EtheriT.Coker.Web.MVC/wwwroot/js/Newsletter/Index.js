var $title, $title_text, $describe, $describe_text, $sort, $sort_input, $sort_checkbox, $picker, $nodeDate, $permanent, $disp_op, $pop_visible, $removedFromShelves;
var startDate, endDate, keyId;
var article_list, initTag_list;
var setPage, initPageData, AllPageData;
var devCom = {};
let $form;

function PageReady() {
    // 啟動
    const editor = grapesInit({
        save: function (html, css) {
            var _dfr = $.Deferred();
            const e = $(html);
            const contenText = function (c) {
                $(c).find("br").replaceWith("\n");
                return $(c).text();
            }
            const data = {
                Id: $("#gjs").data("id"),
                Title: $(html).find("#headerTitle").text(),
                Conten1Title: $(html).find("#Conten1Title").text(),
                Conten1Conten: contenText($(html).find("#Conten1Conten")),
                Conten2MainTitle: contenText($(html).find("#support .mainTitle")),
                Conten2Title: contenText($(html).find("#support .title")),
                Conten2Conten: contenText($(html).find("#support .text-left"))
            };
            console.log(data);
            co.Articles.SaveConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        import: function (html, css) {
            var _dfr = $.Deferred();
            co.Articles.ImportConten({
                Id: $("#gjs").data("id"),
                SaveHtml: html,
                SaveCss: css
            }).done(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        getComponer: function () {
            var _dfr = $.Deferred();
            co.HtmlContent.GetAllComponent().done(function (result) {
                if (result.success) _dfr.resolve(result.list);
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        }
    });

    //設定html資料
    setPage = function (id) {
        $("body").addClass("grapesEdit");
        co.Articles.GetConten({ Id: id }).done(function (result) {
            if (result.success) {
                if (result.conten.saveHtml.trim() == "") {
                    result.conten.saveHtml = initPageData.html;
                    result.conten.saveCss = initPageData.css;
                }
                var html = co.Data.HtmlDecode(result.conten.saveHtml);
                co.Grapes.setEditor(editor, html, result.conten.saveCss);
                if (!!result.title) $("#TopLine .title").text(result.title);
            } else {
                co.sweet.error(result.error);
            }
        });
    }
    $(".image_upload").each(function () {
        ImageUploadModalInit($(this));
    });
    ElementInit();
    TagListModalInit();

    co.Recipient.GetRecipientsTag().done(function (result) {
        initTag_list = [];
        if (result != null) initTag_list.push(result);
    });

    co.ObjectType.GetNewsletterAllConten().done(function (result) {
        if (result.success) {
            AllPageData = result.conten
            if (AllPageData.length > 1) initPageData = AllPageData[1];
        }
    });

    $form = $("#ArticletForm").CokerMultiStepForm();

    const forms = $('#ArticletForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                //if (!form.checkValidity()) {
                //    event.preventDefault()
                //    event.stopPropagation()
                //} else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將儲存", "儲存後將顯示於安排的位置", "儲存", "取消", function () {
                        AddUp("已成功儲存", "儲存發生未知錯誤");
                    });
                //}
                form.classList.add('was-validated')
                WasValidated();
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回電子報列表", "資料將不被保存", "確定", "取消", function () {
            article_list.component.refresh();
            BackToList();
        });
    })

    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });

    $(".btn_to_canvas").on("click", function (event) {
        event.preventDefault()
        Swal.fire({
            icon: 'info',
            title: "前往電子報編輯頁",
            html: "是否保存資料?",
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#888888',
            denyButtonColor: '#d33',
            confirmButtonText: "　是　",
            denyButtonText: "　否　",
            cancelButtonText: "　取消　",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                AddUp("資料已儲存", "儲存發生未知錯誤", "canvas");
            } else if (result.isDenied) {
                var hash = window.location.hash.replace("#", "") + "-1";
                window.location.hash = hash;
                MoveToCanvas();
            }
        })
    });

    $title_text.on('keyup', function () {
        var $self = $(this);
        $title.children("div").children(".count").text($self.val().length)
    });

    $describe_text.on('keyup', function () {
        var $self = $(this);
        $describe.children("div").children(".count").text($self.val().length)
    });

    $sort_checkbox.on("click", function () {
        var $self = $(this);
        if ($self.is(":checked")) {
            $sort_input.removeAttr("disabled");
        } else {
            $sort_input.val('');
            $sort_input.attr("disabled", "disabled");
        }
    })

    co.Picker.Init($picker);
    co.Picker.Init($nodeDate, { singleDatePicker: true, timePicker: false, locale: { format: 'YYYY/MM/DD' } });
    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/MM/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/MM/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
    });
    $permanent.on("click", function () {
        if ($permanent.is(":checked")) {
            $picker.val('');
            $picker.attr("disabled", "disabled");
            startDate = null;
            endDate = null;
        } else {
            $picker.removeAttr("disabled");
        }
    })

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $title = $(".title");
    $title_text = $title.children("textarea");
    $describe = $(".describe");
    $describe_text = $describe.children("textarea");
    $sort = $(".sort");
    $sort_input = $sort.children("input");
    $sort_checkbox = $sort.children(".checkbox").children("input");
    $picker = $("#InputDate");
    $nodeDate = $("#NodeDate");
    $permanent = $("#PermanentCheck");
    $disp_op = $("#PageVisible");
    $pop_visible = $("#PopularVisible");
    $removedFromShelves = $("#RemovedFromShelves");
}

function contentReady(e) {
    article_list = e;
    HashDataEdit();
}
function NewsletterReady(e) {
    $(e.element).addClass("formItem list").data({ component: e.component });
    devCom.header = e.component;
}

function hashChange(e) {
    if (!!e) {
        HashDataEdit();
        e.preventDefault();
    } else {
        console.log("HashChange錯誤")
    }
}

function HashDataEdit() {
    if (window.location.hash != "") {
        if (window.currentHash != window.location.hash) {
            var hash = window.location.hash.replace("#", "");
            if (parseInt(hash) == 0) {
                window.location.hash = 0;
                keyId = 0;
                FormDataClear();
                MoveToContent();
            } else {
                co.Articles.GetDataOne(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        keyId = parseInt(hash);
                        if (hash.indexOf("-") > 0) {
                            MoveToCanvas();
                        } else {
                            MoveToContent();
                            FormDataSet(result);
                        }
                    } else {
                        window.location.hash = ""
                        keyId = "";
                    }
                })
            }
        }
    } else {
        BackToList();
    }
}

function editButtonClicked(e) {
    MoveToContent();
    keyId = e.row.key;
    window.location.hash = keyId;
}

function FormDataClear() {
    TagDataClear();
    $(".image_upload").each(function () {
        ImageUploadModalClear($(this));
    });
    keyId = 0;
    $title_text.val("");
    $title.children("div").children(".count").text(0);
    $describe_text.val("");
    $describe.children("div").children(".count").text(0);
    $sort_input.val("");
    $sort_input.attr("disabled", "disabled");
    $sort_checkbox.prop("checked", false);
    $permanent.prop("checked", true);
    TagInitSet(initTag_list);
    $disp_op.prop("checked", false);
    $pop_visible.prop("checked", false);
    $removedFromShelves.prop("checked", false);
    $form.find(".part").remove();
    $form.set(initJson());
}

function FormDataSet(result) {
    FormDataClear();
    keyId = result.id;
    
    $describe_text.val(result.description);
    $describe.children("div").children(".count").text(result.description.length);
    $nodeDate.val(result.nodeDate);
    startDate = result.startTime;
    endDate = result.endTime;
    $disp_op.prop("checked", result.visible);
    $pop_visible.prop("checked", result.popularVisible);
    $removedFromShelves.prop("checked", !result.removedFromShelves);

    if (result.serNO != 500) {
        $sort_input.val(result.serNO);
        $sort_input.removeAttr("disabled");
        $sort_checkbox.prop("checked", true);
    }

    if (result.permanent) {
        $picker.val('');
        $picker.attr("disabled", "disabled");
        $permanent.prop("checked", true);
    } else {
        startDate != null && $picker.data('daterangepicker').setStartDate(startDate);
        endDate != null && $picker.data('daterangepicker').setEndDate(endDate);
        $permanent.prop("checked", false);
    }

    if (result.nodeDate != null) {
        $nodeDate.data('daterangepicker').setStartDate(result.nodeDate);
        $nodeDate.data('daterangepicker').setEndDate(result.nodeDate);
    }
    TagDataSet(result.tagDatas);
    //載入並設定資料
    $(".image_upload").each(function () {
        ImageUploadModalClear($(this));
    });
    //const json = initJson();
    const json = result.dataJson || {};
    json.title = result.title;
    $title.children("div").children(".count").text(json.title.length);
    $form.set(json);
}

function paletteButtonClicked(e) {
    keyId = e.row.key;
    window.location.hash = keyId + "-1";
}
function sendButtonClicked(e) {
    const Id = e.row.key;
    co.sweet.confirm("是否確認發送電子報", "電子報將會寄送給所有收件人", "寄出", "取消", function () {
        co.Newsletter.send(Id).done((result) => {
            if (result.success) co.sweet.success("寄送成功");
            else co.sweet.error("傳送失敗", result.error);
        });

    });
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Articles.Delete(e.row.key).done(function (result) {
            if (result.success) {
                e.component.refresh();
            }
        });
    });
}

function AddUp(success_text, error_text, place) {
    $(".image_upload").each(function () {
        const selt = this;
        const $input = $(selt).find(".img_input_frame");
        if ($input.data("delectList") != null) {
            co.File.DeleteFileById({
                Sid: keyId,
                Type: 6,
                Fid: $input.data("delectList")
            }).done(function (result) {
                if (result.success) $(selt).data("path", "");
            });
        }
    });
    co.Articles.AddUp({
        Id: keyId,
        Title: $title_text.val(),
        Description: $describe_text.val(),
        Visible: $disp_op.is(":checked"),
        SerNO: $sort_checkbox.is(":checked") ? $sort_input.val() : 500,
        PopularVisible: $pop_visible.is(":checked"),
        TagSelected: tag_list,
        permanent: $permanent.is(":checked"),
        StartTime: startDate,
        EndTime: endDate,
        NodeDate: $nodeDate.val(),
        RemovedFromShelves: !$removedFromShelves.is(":checked")
    }).done(function (result) {
        if (result.success) {
            var list = [];
            keyId = result.message;
            $(".image_upload").each(function () {
                const self = this;
                if ($(self).find(".img_input").data("file") != null && $(self).find(".img_input").data("file").File != null && $(self).find(".img_input").data("file").id == 0) {
                    const _dfr = $.Deferred();
                    const formData = new FormData();
                    formData.append("files", $(self).find(".img_input").data("file").File);
                    formData.append("type", 6);
                    formData.append("sid", result.message);
                    formData.append("serno", 500);
                    co.File.Upload(formData).done(function (response) {
                        if (response.success) {
                            if (response.files.length > 0) {
                                $(self).data("path", response.files[0])
                            }
                            _dfr.resolve();
                        } else {
                            _dfr.reject();
                            Coker.sweet.error("錯誤", error_text, null, true);
                        }
                    });
                    list.push(_dfr);
                }
            });
            $.when.apply(null, list).done(function () {
                const data = $form.getJson();
                data.Id = keyId;
                co.Newsletter.UpdateJson(data).done(function (result) {
                    if (result.success) {
                        setNewsletter(data).done(function (result1, result2) {
                            Coker.sweet.success(success_text, null, true);
                            if (place == "canvas") {
                                if (keyId == 0) window.location.hash = `${result.message}-1`;
                                else window.location.hash += "-1";
                            } else {
                                article_list.component.refresh();
                                BackToList();
                            }
                        });
                    } else Coker.sweet.error("錯誤", error_text, null, true);
                });
                
            });
            
        } else {
            Coker.sweet.error("錯誤", error_text, null, true);
        }
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}
function setNewsletter(data) {
    var list = [];
    let web = $(co.Data.HtmlDecode(AllPageData[1].html));
    let email = $("<div>").append(co.Data.HtmlDecode(AllPageData[0].html));
    $(web).setWebHtml(data);
    $(email).setEmailHtml(data);

    AllPageData[1].id = keyId;
    AllPageData[0].id = keyId;
    AllPageData[0].css = AllPageData[0].css.replace("background-color:#2959b4;", `background-color: ${data.BGColor};`)
    AllPageData[1].css = AllPageData[1].css.replace("background-color:#2959b4;", `background-color: ${data.BGColor};`)
    AllPageData[1].saveHtml = co.Data.HtmlEncode(`<body>${$("<div>").append(web).html()}</body>`);
    AllPageData[1].saveCss = AllPageData[1].css;
    AllPageData[0].html = co.Data.HtmlEncode(`<body>${$(email).html()}</body>`);
    list.push(co.Articles.ImportConten(AllPageData[1]));
    list.push(co.Newsletter.SaveConten(AllPageData[0]));
    return $.when.apply(null, list)
}


function MoveToContent() {
    UnValidated();
    /*if (keyId == 0) {
        $(".btn_to_canvas").addClass("text-dark");
        $(".btn_to_canvas").attr('disabled', '');
    } else {
        $(".btn_to_canvas").removeClass("text-dark");
        $(".btn_to_canvas").removeAttr('disabled');
    }*/
    $("#ArticleList").addClass("d-none");
    $("#ArticleContent").removeClass("d-none");
    $("#ArticleCanvas").addClass("d-none");
    $("body").removeClass("grapesEdit");
    $form.initPage();
}

function MoveToCanvas() {
    UnValidated();
    $("#gjs").data("id", keyId);
    setPage(keyId);
    $("#TopLine > div > a").removeClass("d-none");
    $("#ArticleList").addClass("d-none");
    $("#ArticleContent").addClass("d-none");
    $("#ArticleCanvas").removeClass("d-none");
}

function BackToList() {
    $("#TopLine > div > a").addClass("d-none");
    $("#ArticleList").removeClass("d-none");
    $("#ArticleContent").addClass("d-none");
    $("#ArticleCanvas").addClass("d-none");
    $("body").removeClass("grapesEdit");
    $("#TopLine .title").text("電子報管理");
    window.location.hash = ""
}

function WasValidated() {
    $sort.children(".checkbox").addClass("pe-4");
}

function UnValidated() {
    $("#ArticletForm").removeClass("was-validated");
    $sort.children(".checkbox").removeClass("pe-4");
}
$.fn.extend({
    setWebHtml: function (data) {
        const $self = $(this);
        $self.find("#logoImage").attr({ src: data.Logo.path })
        $self.find("#No").text(`No.${data.No}`);
        $self.find("#headerTitle").html(`${data.Title.replace(/\n/g, "<br />")}`);
        //主選單
        const $mainManu = $self.find("#navbarNav>ul");
        if (!!!$mainManu.data("templ")) $mainManu.data("templ", $mainManu.html()).empty();
        $(data.mainManu).each(function () {
            const $item = $($mainManu.data("templ"));
            $item.find("a").setLink(this);
            $item.appendTo($mainManu);
        });
        //conten1
        $self.find("#bannerImage").attr({ src: data.conten1.image.path });
        $self.find("#Conten1Title").html(`${data.conten1.Title.replace(/\n/g, "<br />")}`);
        $self.find("#Conten1Conten").html(`${data.conten1.Conten.replace(/\n/g, "<br />")}`);
        $self.find("#Conten1More").setLink(data.conten1.More);
        //news
        if (data.news.Visible) {
            $self.find("#NewsImage").attr({ src: data.news.image.path });
            const $newsList = $self.find("#NewsList");
            if (!!!$newsList.data("templ")) $newsList.data("templ", $newsList.html()).empty();
            $(data.news.List).each(function () {
                const $item = $($newsList.data("templ"));
                $item.find("a").setLink(this);
                $item.appendTo($newsList);
            });
        } else $self.find("#News").parents(".news").remove();
        //active
        if (data.active.Visible) {
            const $active = $self.find("#active");
            $active.find(".title").html(`${data.active.Title.replace(/\n/g, "<br />")}`);
            const $activeList = $active.find(".items");
            if (!!!$activeList.data("templ")) $activeList.data("templ", $activeList.html()).empty();
            $(data.active.List).each(function () {
                const $item = $($activeList.data("templ"));
                $item.find("a").setLink(this);
                $item.appendTo($activeList);
            });
        } else $self.find("#active").parents(".featured").remove();
        //conten2
        if (data.conten2.Visible) {
            const $conten2 = $self.find("#support");
            $conten2.find(".icon").attr({ src: data.conten2.icon.path });
            $conten2.find(".image").attr({ src: data.conten2.image.path });
            $conten2.find(".mainTitle").html(`${data.conten2.mainTitle.replace(/\n/g, "<br />")}`);
            $conten2.find(".title").html(`${data.conten2.Title.replace(/\n/g, "<br />")}`);
            $conten2.find(".text-left").html(`${data.conten2.Conten.replace(/\n/g, "<br />")}`);
        } else $self.find("#support").remove();
        //conten3
        if (data.conten3.Visible) {
            const $conten3 = $self.find("#Resource");
            $conten3.find(".mainTitle").html(`${data.conten3.mainTitle.replace(/\n/g, "<br />")}`);
            $conten3.find(".title").html(`${data.conten3.Title.replace(/\n/g, "<br />")}`);
            const $conten3List = $conten3.find(".items");
            if (!!!$conten3List.data("templ")) $conten3List.data("templ", $conten3List.html()).empty();
            $(data.conten3.List).each(function () {
                const $item = $($conten3List.data("templ"));
                $item.setLink(this);
                $item.appendTo($conten3List);
            });
        } else $self.find("#Resource").remove();
        //footer
        const $footer = $self.find("#footer"); 
        $footer.find(".conten").html(`${data.footer.Conten.replace(/\n/g, "<br />")}`);
        $footer.find(".lineLink").setLink(data.footer.List[0]);
        $footer.find(".homeLink").setLink(data.footer.List[1]);
    },
    setEmailHtml: function (data) {
        const $self = $(this);
        $self.find("#logoImage").attr({ src: data.LogoCompress.path });
        $self.find("#NO").text(`No.${data.No}`);
        $self.find("#headerTitle").html(`${data.Title.replace(/\n/g, "<br />")}`);
        //主選單
        const $mainManu = $self.find("#navbarNav table>tbody>tr");
        if (!!!$mainManu.data("templ")) $mainManu.data("templ", $mainManu.html()).empty();
        $(data.mainManu).each(function () {
            const $item = $($mainManu.data("templ"));
            $item.find("a").setLink(this);
            $item.appendTo($mainManu);
        });
        //conten1
        $self.find("#bannerImage").attr({ src: data.conten1.imageCompress.path });
        $self.find("#Conten1Title").html(`${data.conten1.Title.replace(/\n/g, "<br />")}`);
        $self.find("#Conten1Conten").html(`${data.conten1.Conten.replace(/\n/g, "<br />")}`);
        $self.find("#Conten1More>a").setLink(data.conten1.More);
        //news
        if (data.news.Visible) {
            $self.find("#NewsImage").attr({ src: data.news.image.path });
            const $newsList = $self.find("#NewsList");
            if (!!!$newsList.data("templ")) $newsList.data("templ", $newsList.html()).empty();
            $(data.news.List).each(function (index, element) {
                if (index >= 4) return;
                const $item = $($newsList.data("templ"));
                $item.find("a").setLink(element);
                $item.appendTo($newsList);
            });
        } else $self.find("#News").remove();
        //active
        if (data.active.Visible) {
            const $active = $self.find("#active");
            $active.find(".title").html(`${data.active.Title.replace(/\n/g, "<br />")}`);
            const $activeList = $active.find(".items");
            if (!!!$activeList.data("templ")) $activeList.data("templ", $activeList.html()).empty();
            $(data.active.List).each(function (index, element) {
                if (index >= 2) return;
                const $item = $($activeList.data("templ"));
                $item.find("a").setLink(element);
                $item.appendTo($activeList);
            });
        } else $self.find("#active").remove();
        //conten2
        if (data.conten2.Visible) {
            const $conten2 = $self.find("#subport");
            $conten2.find(".icon").attr({ src: data.conten2.iconCompress.path });
            $conten2.find(".image").attr({ src: data.conten2.imageCompress.path });
            $conten2.find(".mainTitle").html(`${data.conten2.mainTitle.replace(/\n/g, "<br />")}`);
            $conten2.find(".title").html(`${data.conten2.Title.replace(/\n/g, "<br />")}`);
        } else $self.find("#subport").remove();
        //conten3
        if (data.conten3.Visible) {
            const $conten3 = $self.find("#Resource");
            const $conten3List = $conten3.find(".items table>tbody");
            $conten3.find(".image").attr({ src: data.conten3.image.path });
            if (!!!$conten3List.data("templ")) $conten3List.data("templ", $conten3List.html()).empty();
            $(data.conten3.List).each(function (index, element) {
                if (index >= 3) return;
                const $item = $($conten3List.data("templ"));
                $item.setLink(element);
                $item.appendTo($conten3List);
            });
            $self.find("#linkMore").setLink({ alert: "連結至：完整資訊", target: true, link: `/eplus/newsletter/article/${keyId}` });
        } else $self.find("#Resource").remove();
        //footer
        const $footer = $self.find("#footer");
        $footer.find(".conten").html(`${data.footer.Conten.replace(/\n/g, "<br />")}`);
        $footer.find(".lineLink").setLink(data.footer.List[0]);
        $footer.find(".homeLink").setLink(data.footer.List[1]);
    },
    setLink: function (data) {
        const $link = $(this);
        if ($link.length == 0) return $link;
        if ((!!data.title && !!data.local) || (!!data.title && !!data.conten) || (!!!data.title)) {
            if ($link[0].tagName == "A") {
                $link.attr({ target: data.target ? "_blank" : "_self", title: data.alert, href: data.link });
            } else {
                $link.find("a").attr({ target: data.target ? "_blank" : "_self", title: data.alert, href: data.link });
            }
            if (!!data.title)
                $link.find(".card-title").html(`${data.title.replace(/\n/g, "<br />")}`);
            if (!!data.date) {
                const date = new Date(data.date);
                $link.find(".date").text(`${date.getFullYear() - 1911}/${date.getMonth() + 1}/${date.getDate()}(${co.String.getWeekNumber(date.getDay())})`);
            }
            if (!!data.local) {
                $link.find(".location").html(`${data.local.replace(/\n/g, "<br />")}`);
            }
            if (!!data.conten) {
                $link.find(".conten").html(`${data.conten.replace(/\n/g, "<br />")}`);
            }
            if (!!data.memo) {
                $link.find(".memo").html(`${data.memo.replace(/\n/g, "<br />")}`);
            }
        }else $link.attr({ target: data.target ? "_blank" : "_self", title: data.alert, href: data.link }).text(data.title);
    }
});
function initJson() {
    return {
        "id":0,
        "mainManu": [
            {
                "title": "電子報",
                "link": "/eplus/newsletter",
                "target": false,
                "alert": "連結至:電子報"
            },
            {
                "title": "資源手冊",
                "link": "/eplus/manual",
                "target": false,
                "alert": "連結至:資源手冊"
            },
            {
                "title": "活動資訊",
                "link": "/eplus/news",
                "target": false,
                "alert": "連結至:活動資訊"
            },
            {
                "title": "關於我們",
                "link": "/eplus/about",
                "target": false,
                "alert": "連結至:活動資訊"
            }
        ],
        "logo": {
            "id": 3084,
            "path": "/upload/eplus/htmlConten/9175370f-e129-44ad-8463-b901e1d071f8.png",
            "name": "logo.png"
        },
        "logoCompress": {
            "id": 3197,
            "path": "/upload/eplus/htmlConten/e4e11a29-9e5d-4bca-bed5-f8151e642653.png",
            "name": "155.png"
        },
        "no": 139,
        "bgColor":"#2959b4",
        "title": "高軟二期招商說明會臺中場今登場\n園管局邀請企業搶進亞灣2.0投資機會",
        "conten1": {
            "image": {
                "id": 3111,
                "path": "/upload/eplus/htmlConten/583bb0b6-357a-4458-be8e-2e0f50bc6f42.jpg",
                "name": "0846f10a-7b02-426c-aca4-49c8b5763d24.jpg"
            },
            "imageCompress": {
                "id": 3187,
                "path": "/upload/eplus/htmlConten/9319FCBC-D861-4A98-8D9C-20E7CF5B785E.jpg",
                "name": "583bb0b6-357a-4458-be8e-2e0f50bc6f42.jpg"
            },
            "title": "招商說明會 臺中場",
            "conten": "為擴大南部智慧科技產業群聚，經濟部產業園區管理局(以下簡稱園管局)啟動高雄軟體園區第二園區設置計畫(簡稱高軟二期)，今(23)日於臺中軟體園區M202會議舉辦招商說明會，向企業代表說明高軟二期投資環境、工程進度、招商對象、優惠方案以及園管局優質服務，現場線上共有近百位企業參與踴躍交流，園管局邀請企業搶進亞灣2.0投資機會。\r\n\r\n\r\n園管局表示，在5GAIoT發展推動上，除既有高軟一期已形成之產業聚落外，亞灣2.0的政策更將5G AIoT的應用擴展至半導體、石化永續、智慧港灣、智慧影視產業等特色產業，並擴大商業模式驗證，讓企業在亞灣設立研訓總部並輸出新南向國家；於該基礎下，園管局啟動高軟二期開發計畫，不論是旗艦辦公室需求業者，或一般中小企業與新創企業等，皆歡迎進駐高軟二期，園管局也將陪伴企業成長茁壯，希望業者都能到南部拓展據點，一起打拼奮鬥。\r\n\r\n\r\n高軟二期位於高軟一期北側，建物導入5G AIoT建築設計理念，透過多元智能科技設備及結合5G AIoT實證場域應用，提供廠商多元智慧化便利服務，並將取得銀級以上綠建築標章，園區生活機能優異，且周邊交通便捷，將是未來企業進駐首選將是高雄亞灣區亮點新指標，招商重點以資訊軟體、數位內容、智慧應用等產業為主，預估可提高產值33億元，可望創造2500個就業機會。園管局提供進駐園區的企業相關輔導資源，包括法人及學界的資源，無論在技術研發、市場拓銷、營運法規甚至是資源補助的申請等，都有直接對應的服務，讓公司創新進程上，可以有相當大幅度的快速成長。相信在中央地方政策推動下，佈局大南方，除了產業群聚效應，更能強化企業競爭優勢，讓產業飛躍升級，說明會廠商分享部分，所邀請『惇智科技股份有限公司』林家弘協理即特別分享園區所提供之輔導資源，對於企業轉型升級以及新知取得或應用上，具有相當實質幫助與效益。\r\n\r\n\r\n高軟二期聚焦 數位內容、資訊軟體、智慧應用、電子電信研發等5G、AIoT應用、研發及測試等知識密集型產業進駐，歡迎有意投資進駐之投資人踴躍提出申請。",
            "more": {
                "title": "了解更多",
                "link": "https://www.bip.gov.tw/info.aspx?pageid=5aa966ecd020763c&cid=c96e4e6e457d4971",
                "target": 1,
                "alert": "連結至: 招商說明會臺中場(另開新視窗)"
            }
        },
        "news": {
            "visible": true,
            "image": {
                "id": 3111,
                "path": "/upload/eplus/htmlConten/ca505174-c28e-44d4-870b-efcf68a3915e.png",
                "name": "newspic.png"
            },
            "list": [
                {
                    "title": "數位轉型智慧零售服務",
                    "link": "https://aceschool.iii.org.tw/CaseDetail.aspx?Case_Sqno=81",
                    "target": true,
                    "alert": "連結至: 數位轉型智慧零售服務(另開新視窗)"
                },
                {
                    "title": "人腦加電腦，蕈菇長得快又好",
                    "link": "https://aceschool.iii.org.tw/CaseDetail.aspx?Case_Sqno=83",
                    "target": true,
                    "alert": "連結至: 人腦加電腦，蕈菇長得快又好(另開新視窗)"
                },
                {
                    "title": "AI毫米波感測技術 前進日本提升照護領域醫療完善率",
                    "link": "https://aceschool.iii.org.tw/CaseDetail.aspx?Case_Sqno=85",
                    "target": true,
                    "alert": "連結至: AI毫米波感測技術 前進日本提升照護領域醫療完善率(另開新視窗)"
                },
                {
                    "title": "回到過去-打造國家文化記憶個案",
                    "link": "https://aceschool.iii.org.tw/CaseDetail.aspx?Case_Sqno=90",
                    "target": true,
                    "alert": "連結至: 回到過去-打造國家文化記憶個案(另開新視窗)"
                }
            ]
        },
        "active": {
            "visible": true,
            "title": "11月份\n精選活動",
            "list": [
                {
                    "title": "臺中軟體園區 2023成果發表會",
                    "date": "112/11/10",
                    "local": "台中",
                    "link": "https://forms.gle/eL7euAc4SPH3dTtB9",
                    "target": true,
                    "alert": "連結至: 臺中軟體園區 2023成果發表會(另開新視窗)"
                },
                {
                    "title": "【跨域創新定根基 低碳永續創新局】科技產業園區跨域創生暨低碳轉型成果分享交流會 ",
                    "date": "112/11/22",
                    "local": "高雄",
                    "link": "https://reurl.cc/9R0o3V",
                    "target": true,
                    "alert": "連結至:AI發展趨勢下的ChatGPT應用暨企業數位轉型政府補助資源說明(另開新視窗)"
                }
            ]
        },
        "conten2": {
            "visible": true,
            "mainTitle": "亮點輔導",
            "image": {
                "id": 3112,
                "path": "/upload/eplus/htmlConten/71ab4149-1877-4064-a159-464d3d471ada.png",
                "name": "螢幕擷取畫面 2023-10-26 183826.png"
            },
            "imageCompress": {
                "id": 3193,
                "path": "/upload/eplus/htmlConten/8b2b4074-15f5-4c8d-872c-990267b4c9da.jpg",
                "name": "71ab4149-1877-4064-a159-464d3d471ada.jpg"
            },
            "icon": {
                "id": 2922,
                "path": "/upload/eplus/htmlConten/bb0065aa-dddd-4571-9c81-04475f9928c2.png",
                "name": "bEpaper_切版用只剩一個版本.png"
            },
            "iconCompress": {
                "id": 3189,
                "path": "/upload/eplus/htmlConten/1A6E9238-41AC-4FB0-BB88-2262AF83A037.png",
                "name": "bb0065aa-dddd-4571-9c81-04475f9928c2.png"
            },
            "title": "虛實動畫影片提升企業專業製造新形象",
            "conten": "靖鎰企業從事生產汽車零件已有30年經驗，歷經多年累積，技術工藝已十分精密，並獲得多間大型車廠的技術經驗肯定。然而於公司行銷推廣，因傳產行業投入資源有限，尚停留於篇幅較長、畫質較模糊階段，於現今行銷推廣上較不易吸引人注意。經過輔導團隊溝通，靖鎰將企業形象宣傳方面提升，導入虛實動畫技術，製作全新企業形象影片做為首要對外數位門面。主軸以台灣精工車用零件的製造商為首，將靖鎰企業專業、精密、高品質製造的形象為展現重點，故拍攝手法以工廠內部實景為主，紀錄工廠內產品製作、精密生產過程，並輔以3D動畫特效，說明冷鍛造的特殊技術、高品質製造特點，對外擴散靖鎰企業本質的專業形象。"
        },
        "conten3": {
            "visible": true,
            "mainTitle": "政府資源",
            "title": "區內服務-協助廠商申請政府資源",
            "image": {
                "id": 3196,
                "path": "/upload/eplus/htmlConten/BEC709AE-02ED-4741-AC26-045AA8028153.png",
                "name": "政府資源.png"
            },
            "list": [
                {
                    "title": "經濟部協助中小企業低碳化智慧化轉型發展與納管工廠及特定工廠基礎設施優化專案貸款（低碳智慧納管貸款）",
                    "memo": "於114年10月31日以前受理申請，惟本貸款利息補貼預算用罄時或已屆申請期限即停止受理貸款申請。",
                    "conten": "經濟部為執行疫後強化經濟與社會韌性及全民共享經濟成果特別條例，經濟部推動產業及中小企業升級轉型辦法，辦理資金協助，推動中小企業朝低碳化、智慧化， 與納管工廠及特定工廠符合環境保護及公共安全相關要求邁向合法化經營之目的。",
                    "link": "https://0800056476.sme.gov.tw/plus/index.php",
                    "target": true,
                    "alert": "連結至:低碳智慧納管貸款(另開新視窗)"
                },
                {
                    "title": "商業服務業智慧減碳補助 ",
                    "memo": "受理期間自即日起至112年11月30日或經費用罄之日止",
                    "conten": "業服務業智慧減碳補助分成「單一」及「整合」服務應用二類，「單一服務應用類」補助上限為30萬元，整合服務應用類補助上限為150萬元。商業司進一步說明，如果店家應用智慧工具較單純，可以申請單一服務類，例如導入「數位行銷應用」，以APP或數位社群強化與消費者的互動，取代原有的紙本傳單，從節省的廣告紙張減少碳排放，至於須整合多項智慧工具的業者，可申請整合服務應用類補助。",
                    "link": "https://serv.gcis.nat.gov.tw/BIGS/",
                    "target": true,
                    "alert": "連結至:低碳智慧納管貸款(另開新視窗)"
                },
                {
                    "title": "低碳及智慧化升級轉型補助作業(中小型製造業(經常僱用員工數9人以下))",
                    "memo": " 自公告日起至 112 年 12 月 20 日 \n 或補助經費用罄之日",
                    "conten": "為依「疫後強化經濟與社會韌性及全民共享經濟成果特別條例」第 3 條第 5 款及\n「經濟部推動產業及中小企業升級轉型辦法」(以下簡稱本辦法) 第 5 條規定，補助個別製造業(經常僱用員工數 9 人以下)加速導入相關技術、設備及管理機制，朝向低碳化及智慧化升級轉型，提升我國產業競爭力及經濟韌性，特訂定本須知，以執行補助作業。",
                    "link": "https://www.sbir.org.tw/",
                    "target": true,
                    "alert": "連結至:低碳及智慧化升級轉型補助作業(另開新視窗)"
                },
                {
                    "title": "低碳及智慧化升級轉型補助作業(中小型製造業(經常僱用員工數 10 人以上) ",
                    "memo": " 自公告日起至 112 年 11月 30 日  \n 或補助經費用罄之日",
                    "conten": "為依「疫後強化經濟與社會韌性及全民共享經濟成果特別條例」第 3 條第 5款及「經濟部推動產業及中小企業升級轉型辦法」(下稱本辦法)第 5 條規定，補助個別製造業(經常僱用員工數 10 人以上)加速導入低碳化、智慧化相關技術、設備及管理機制，朝向低碳化及智慧化升級轉型，進而提升我國產業競爭力及經濟韌性，特訂定本須知，以執行補助作業。",
                    "link": "https://www.citd.moeaidb.gov.tw/CITDWeb/Web/Detail.aspx?p=9a437de9-66da-4a22-9a1a-067085181112",
                    "target": true,
                    "alert": "連結至:低碳及智慧化升級轉型補助作業中小型製造業(另開新視窗)"
                },
                {
                    "title": "經濟部中小及新創企業署小型企業創新研發計畫(SBIR)",
                    "memo": "採隨到隨受理方式",
                    "conten": "本計畫之推動，旨在帶動國內中小企業創新研發活動，協助其知識布局培育研發人才，並期望運用研發成果扶植產業體系，促進臺灣經濟發展。 ",
                    "link": "https://www.sbir.org.tw/",
                    "target": true,
                    "alert": "連結至:低碳及智慧化升級轉型補助作業(另開新視窗)"
                }
            ]
        },
        "footer": {
            "list": [
                {
                    "link": "https://line.me/R/ti/p/@998dalcj",
                    "alert": "連結至:科技產業園區產學供需平臺(另開新視窗)",
                    "target": true,
                    "className": "fa-brands fa-line"
                }, {
                    "link": "/eplus/",
                    "alert": "連結至: E++產業服務平台",
                    "target": false,
                    "className": "fas fa-house"
                }
            ],
            "conten": "經濟部產業園區管理局 版權所有 © 2023 BIP ALL Rights Reserved"
        }
    }
}