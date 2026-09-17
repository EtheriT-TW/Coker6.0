var companyInfoIsEdit = false;

var companyInfoIsEmpty = false;
var companyLookupComplete = false;
var companyLookupTimer = null;
var companyLookupRequest = null;

function PageReady() {
    companyInfoIsEmpty = $("#CompnyData").data("company-empty") === true;
    co.File.getImgFile({ Sid: $("#WebsiteID").val(), Type: 11, Size: 1, }).done(function (files) {
        if (files.length > 0) {
            for (var i = files.length - 1; i > -1; i--) {
                ImageUploadModalDataInsert($("#IconImageUpload"), files[i].id, files[i].link, files[i].name)
            }
        }
    })

    co.File.getImgFile({ Sid: $("#WebsiteID").val(), Type: 12, Size: 1, }).done(function (files) {
        if (files.length > 0) {
            for (var i = files.length - 1; i > -1; i--) {
                ImageUploadModalDataInsert($("#LogoImageUpload"), files[i].id, files[i].link, files[i].name)
            }
        }
    })

    co.File.getImgFile({ Sid: $("#WebsiteID").val(), Type: 13, Size: 1, }).done(function (files) {
        if (files.length > 0) {
            for (var i = files.length - 1; i > -1; i--) {
                ImageUploadModalDataInsert($("#ShareImageUpload"), files[i].id, files[i].link, files[i].name)
            }
        }
    })

    $("#IconImageUpload").ImageUploadModalClear();
    $("#LogoImageUpload").ImageUploadModalClear();
    $("#ShareImageUpload").ImageUploadModalClear();
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

    $("#TaxID, #Name").on("input", CompanyIdentityChanged);

    $("#CompanyInfo > .form_btn > .btn_exit").on("click", CompanyInfoExit);
    $("#CompanyInfo > .form_btn > .btn_save").on("click", CompanyInfoSave);
    $("#WebsiteInfo > .form_btn > .btn_save").on("click", WebsiteInfoSave);

    let addr = $("#TWzipcode .address").val()
    co.Zipcode.init("#TWzipcode");
    co.Zipcode.setData({
        el: $("#TWzipcode"),
        addr: addr
    });

}

function CompanyInfoEdit() {
    const $this_form_input = $("#CompanyInfo > form > div input");
    const $this_form_select = $("#CompanyInfo > form > div select");
    const $this_form_btn = $("#CompanyInfo > .form_btn");
    if (companyInfoIsEdit) {
        if (companyInfoIsEmpty) {
            $("#TaxID, #Name").removeAttr("disabled");
            SetCompanyDetailFieldsEnabled(false);
            SetCompanyLookupFeedback("請輸入統編或公司／機構名稱，系統會先查詢既有資料。", "text-muted");
            $("#TaxID").trigger("focus");
        } else {
            $this_form_input.removeAttr("disabled");
            $this_form_select.removeAttr("disabled");
            $("#TaxID").attr("readonly", "readonly");
        }
        $this_form_btn.removeClass("d-none").addClass("d-flex");
    } else {
        $("#TaxID").removeAttr("readonly");
        $this_form_input.attr('disabled', 'disabled');
        $this_form_select.attr('disabled', 'disabled');
        $this_form_btn.removeClass("d-flex").addClass("d-none");
    }
}

function CompanyInfoExit() {
    if (companyInfoIsEmpty) ResetEmptyCompanyForm();
    companyInfoIsEdit = false;
    CompanyInfoEdit();
}

function CompanyIdentityChanged() {
    if (!companyInfoIsEmpty || !companyInfoIsEdit) return;

    const isTaxIdInput = this.id === "TaxID";
    const taxId = String($("#TaxID").val() || "").replace(/\D/g, "").substring(0, 10);
    $("#TaxID").val(taxId);
    companyLookupComplete = false;
    window.clearTimeout(companyLookupTimer);
    if (companyLookupRequest) companyLookupRequest.abort();
    ClearCompanyDetailFields(isTaxIdInput && taxId.length > 0);
    SetCompanyDetailFieldsEnabled(false);
    const companyName = String($("#Name").val() || "").trim();

    if (taxId.length > 0 && taxId.length !== 8 && taxId.length !== 10) {
        SetCompanyLookupFeedback(
            "請輸入完整的 8 碼或 10 碼編號；若沒有統編，請清空後改輸入公司／機構名稱。",
            "text-muted"
        );
        return;
    }

    const lookupName = taxId.length === 0 ? companyName : "";
    if (taxId.length === 0 && lookupName.length < 2) {
        SetCompanyLookupFeedback("請輸入統編，或至少 2 個字的公司／機構名稱。", "text-muted");
        return;
    }

    SetCompanyLookupFeedback("正在查詢公司資料…", "text-muted");
    companyLookupTimer = window.setTimeout(function () {
        LookupCompany(taxId, lookupName);
    }, 400);
}

function LookupCompany(taxId, companyName) {
    companyLookupRequest = $.ajax({
        url: "/api/Company/GetByIdentity",
        type: "GET",
        headers: _c.Data.Header,
        data: { taxId: taxId, name: companyName },
        dataType: "json"
    }).done(function (result) {
        const currentTaxId = String($("#TaxID").val() || "").replace(/\D/g, "");
        const currentName = String($("#Name").val() || "").trim();
        if (currentTaxId !== taxId || (taxId.length === 0 && currentName !== companyName)) return;

        const matchStatus = String(result.matchStatus || result.MatchStatus || "").toLowerCase();
        if (result.success) {
            companyLookupComplete = true;
            FillCompanyDetailFields(result.company);
            SetCompanyDetailFieldsEnabled(false);
            SetCompanyLookupFeedback("已帶入既有公司資料，確認後即可儲存。", "text-success");
        } else if (matchStatus === "notfound") {
            companyLookupComplete = true;
            SetCompanyDetailFieldsEnabled(true);
            SetCompanyLookupFeedback("查無既有資料，請繼續填寫並建立公司資訊。", "text-primary");
            if (companyName.length === 0) $("#Name").trigger("focus");
        } else {
            companyLookupComplete = false;
            SetCompanyDetailFieldsEnabled(false);
            SetCompanyLookupFeedback(
                result.error || result.Error || "無法確認公司資料，請改用其他識別資料。",
                "text-danger"
            );
        }
    }).fail(function (_, status) {
        if (status === "abort") return;
        companyLookupComplete = false;
        SetCompanyDetailFieldsEnabled(false);
        SetCompanyLookupFeedback("目前無法查詢公司資料，請稍後再試。", "text-danger");
    }).always(function () {
        companyLookupRequest = null;
    });
}

function FillCompanyDetailFields(company) {
    company = company || {};
    $("#TaxID").val(company.taxID || company.taxId || company.TaxID || "");
    $("#Name").val(company.name || company.Name || "");
    $("#Contact").val(company.contact || company.Contact || "");
    $("#Email").val(company.email || company.Email || "");
    const address = company.address || company.Address || "";
    $("#Address").val(address);
    co.Zipcode.setData({ el: $("#TWzipcode"), addr: address });
}

function ClearCompanyDetailFields(clearName) {
    if (clearName) $("#Name").val("");
    $("#Contact, #Email, #Address").val("");
    co.Zipcode.setData({ el: $("#TWzipcode"), addr: "" });
}

function SetCompanyDetailFieldsEnabled(enabled) {
    const $fields = $("#Contact, #Email, #Address, #TWzipcode select");
    if (enabled) $fields.removeAttr("disabled");
    else $fields.attr("disabled", "disabled");
}

function SetCompanyLookupFeedback(message, colorClass) {
    $("#CompanyLookupFeedback")
        .removeClass("d-none text-muted text-success text-primary text-danger")
        .addClass(colorClass)
        .text(message);
}

function ResetEmptyCompanyForm() {
    window.clearTimeout(companyLookupTimer);
    if (companyLookupRequest) companyLookupRequest.abort();
    companyLookupComplete = false;
    $("#CompanyID").val(0);
    $("#TaxID").val("");
    ClearCompanyDetailFields(true);
    $("#CompanyLookupFeedback").addClass("d-none").text("");
    document.getElementById("CompnyData").classList.remove("was-validated");
}

function CompanyInfoSave(event) {
    const form = document.getElementById("CompnyData");
    if (companyInfoIsEmpty && !companyLookupComplete) {
        event.preventDefault();
        event.stopPropagation();
        SetCompanyLookupFeedback("請先完成統編或公司／機構名稱查詢。", "text-danger");
        return;
    }
    if (!form.checkValidity()) {
        event.preventDefault()
        event.stopPropagation()
    } else {
        co.Company.Save(co.Form.getJson("CompnyData")).done(function (resut) {
            if (resut.success) {
                $("#CompanyID").val(resut.message || resut.Message || 0);
                companyInfoIsEmpty = false;
                companyInfoIsEdit = false;
                $("#CompanyLookupFeedback").addClass("d-none").text("");
                CompanyInfoEdit();
                co.sweet.success("儲存成功");
            }
            else co.sweet.error(resut.error);
        });
    }
    form.classList.add('was-validated');
}
function WebsiteInfoSave(event) {
    const form = document.getElementById("WebsiteData");
    if (!form.checkValidity()) {
        event.preventDefault()
        event.stopPropagation()
    } else {
        let check = form.checkValidity();
        if (!check) return;
        co.WebSite.Save(co.Form.getJson("WebsiteData")).done(function (resut) {
            if (resut.success) {
                var imageUploadList = [];
                imageUploadList.push(handleFileUpload("#IconImageUpload", 11));
                imageUploadList.push(handleFileUpload("#LogoImageUpload", 12));
                imageUploadList.push(handleFileUpload("#ShareImageUpload", 13));
                $.when.apply(null, imageUploadList).done(function (result, result2, result3) {
                    if (result.success && result2.success && result3.success) {
                        co.sweet.success("儲存成功");
                    } else {
                        if (!result.success) co.sweet.error(result.message);
                        else if (!result2.success) co.sweet.error(result2.message);
                        else if (!result3.success) co.sweet.error(result3.message);
                        else co.sweet.error("發生錯誤");
                    }
                }).catch(function (error) {
                    co.sweet.error(error.message || "發生錯誤");
                });
            }
            else co.sweet.error(resut.error);
        });
    }
    form.classList.add('was-validated');
}

function handleFileUpload(selector, type) {
    var deleteList = $(selector).find(".img_input_frame").data("delectList");
    var file = $(`${selector} .img_input_frame > .img_input`).data("file")?.File;

    if (typeof (deleteList) != "undefined" && deleteList != null) {
        return co.File.DeleteFileById({
            Sid: $("#WebsiteID").val(),
            Type: type,
            Fid: deleteList
        }).then(function (result) {
            if (typeof (file) != "undefined" && file != null) {
                var _dfr = $.Deferred();
                var formData = new FormData();
                formData.append("files", file);
                formData.append("type", type);
                formData.append("sid", $("#WebsiteID").val());
                formData.append("serno", 500);
                co.File.Upload(formData).then(function (result) {
                    return _dfr.resolve(result);
                });
                return _dfr.promise();
            }
            return Promise.resolve({ success: true });
        });
    } else {
        if (typeof (file) != "undefined" && file != null) {
            var _dfr = $.Deferred();
            var formData = new FormData();
            formData.append("files", file);
            formData.append("type", type);
            formData.append("sid", $("#WebsiteID").val());
            formData.append("serno", 500);
            co.File.Upload(formData).then(function (result) {
                return _dfr.resolve(result);
            });
            return _dfr.promise();
        }
        return Promise.resolve({ success: true });
    }
}
