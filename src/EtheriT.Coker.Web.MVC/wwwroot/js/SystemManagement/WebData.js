var companyInfoIsEdit = false;

function PageReady() {
    // 啟動
    const editor = grapesInit({
        save: function (html, css) {
            var _dfr = $.Deferred();
            co.WebMesnus.saveConten({
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
            co.WebMesnus.importConten({
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
        co.WebMesnus.getConten(id).done(function (result) {
            if (result.success) {
                var html = co.Data.HtmlDecode(result.conten.saveHtml);
                editor.setComponents(html);
                editor.setStyle(result.conten.saveCss);
            } else {
                co.sweet.error(result.error);
            }
        });
    }

    co.WebSite.getPrivacyAndTerms().done(function (result) {
        if (result.success) {
            if (result.message.split(" ").length == 2) {
                $("#PrivacyStatement").data("id", result.message.split(" ")[0]);
                $("#MembershipTerms").data("id", result.message.split(" ")[1]);
            } else {
                $("#PrivacyStatement").data("id", 0);
                $("#MembershipTerms").data("id", 0);
            }
            $(".btn_privacy").on("click", function () {
                if ($("#PrivacyStatement").data("id") == 0) {
                    co.WebMesnus.createOrEdit({
                        Id: 0,
                        Title: "隱私權聲明",
                        RouterName: "footer_privacy",
                        PageType: 1,
                        Visible: false,
                        SerNo: 1000,
                        PopularVisible: false,
                        LanBar: false,
                        Icon: "empty"
                    }).done(function (result) {
                        if (result.success) {
                            $("#PrivacyStatement").data("id", result.message);
                            $("#TopLine > .title").text("隱私權聲明頁面編輯");
                            window.location.hash = "#privacy"
                            MoveToCanvas($("#PrivacyStatement").data("id"));
                        }
                    })
                } else {
                    $("#TopLine > .title").text("隱私權聲明頁面編輯");
                    window.location.hash = "#privacy"
                    MoveToCanvas($("#PrivacyStatement").data("id"));
                }
            });
            $(".btn_terms").on("click", function () {
                if ($("#MembershipTerms").data("id") == 0) {
                    co.WebMesnus.createOrEdit({
                        Id: 0,
                        Title: "會員條款說明",
                        RouterName: "terms",
                        PageType: 1,
                        Visible: false,
                        SerNo: 1000,
                        PopularVisible: false,
                        LanBar: false,
                        Icon: "empty"
                    }).done(function (result) {
                        console.log(result)
                        if (result.success) {
                            $("#MembershipTerms").data("id", result.message);
                            $("#TopLine > .title").text("會員條款說明頁面編輯");
                            window.location.hash = "#terms"
                            MoveToCanvas($("#MembershipTerms").data("id"));
                        }
                    })
                } else {
                    $("#TopLine > .title").text("會員條款說明頁面編輯");
                    window.location.hash = "#terms"
                    MoveToCanvas($("#MembershipTerms").data("id"));
                }
            });
        }
    })

    $("#IconImageUpload").ImageUploadModalClear();
    $(".btn_input_icon").on('click', function () {
        $(".input_icon").click();
    });

    $(".btn_input_logo").on('click', function () {
        $(".input_logo").click();
    });

    $(".btn_company_info_edit").on('click', function () {
        companyInfoIsEdit = !companyInfoIsEdit;
        CompanyInfoEdit();
    });

    $("#CompanyInfo > .form_btn > .btn_exit").on("click", CompanyInfoExit);
    $("#CompanyInfo > .form_btn > .btn_save").on("click", CompanyInfoSave);
    $("#WebsiteInfo > .form_btn > .btn_save").on("click", WebsiteInfoSave);

    let addr = $("#TWzipcode .address").val()
    co.Zipcode.init("#TWzipcode");
    co.Zipcode.setData({
        el: $("#TWzipcode"),
        addr: addr
    });

    HashDataEdit();
    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
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
            console.log(hash)
            switch (hash) {
                case "privacy":
                    if (typeof ($("#PrivacyStatement").data("id")) != "undefined" && $("#PrivacyStatement").data("id") > 0) MoveToCanvas($("#PrivacyStatement").data("id"));
                    else {
                        window.location.hash = ""
                        keyId = "";
                    }
                    break;
                case "terms":
                    if (typeof ($("#MembershipTerms").data("id")) != "undefined" && $("#MembershipTerms").data("id") > 0) MoveToCanvas($("#MembershipTerms").data("id"));
                    else {
                        window.location.hash = ""
                        keyId = "";
                    }
                    break;
                default:
                    window.location.hash = ""
                    keyId = "";
                    break;
            }
        }
    } else {
        BackToMain();
    }
}

function CompanyInfoEdit() {
    $this_form_input = $("#CompanyInfo > form > div input");
    $this_form_select = $("#CompanyInfo > form > div select");
    $this_form_btn = $("#CompanyInfo > .form_btn");
    if (companyInfoIsEdit) {
        $this_form_input.removeAttr("disabled");
        $this_form_select.removeAttr("disabled");
        $this_form_btn.addClass("d-flex");
    } else {
        $this_form_input.attr('disabled', 'disabled');
        $this_form_select.attr('disabled', 'disabled');
        $this_form_btn.removeClass("d-flex");
    }
}

function CompanyInfoExit() {
    companyInfoIsEdit = false;
    CompanyInfoEdit();
}

function CompanyInfoSave(event) {
    companyInfoIsEdit = false;
    const form = document.getElementById("CompnyData");
    if (!form.checkValidity()) {
        event.preventDefault()
        event.stopPropagation()
    } else {
        co.Company.Save(co.Form.getJson("CompnyData")).done(function (resut) {
            if (resut.success) co.sweet.success("儲存成功");
            else co.sweet.error(resut.error);
        });
        CompanyInfoEdit();
    }
    form.classList.add('was-validated');
}
function WebsiteInfoSave(event) {
    const form = document.getElementById("WebsiteData");
    if (!form.checkValidity()) {
        event.preventDefault()
        event.stopPropagation()
    } else {
        co.WebSite.Save(co.Form.getJson("WebsiteData")).done(function (resut) {
            if (resut.success) co.sweet.success("儲存成功");
            else co.sweet.error(resut.error);
        });
    }
    form.classList.add('was-validated');
}

function MoveToCanvas(id) {
    $("#gjs").data("id", id);
    setPage(id);
    $("html,body").animate({ scrollTop: 0 });
    $("#TopLine > a").removeClass("d-none");
    $("#WebDataMain").addClass("d-none");
    $("#WebDataCanvas").removeClass("d-none");
}

function BackToMain() {
    $("#TopLine > a").addClass("d-none");
    $("#TopLine > .title").text("網站資料");
    $("#WebDataMain").removeClass("d-none");
    $("#WebDataCanvas").addClass("d-none");
    window.location.hash = ""
}