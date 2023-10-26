var new_pass_show = false, check_pass_show = false, isMailLock = false, BasicInfoFilled = false, LoginMailFilled = false, PassIsCheck = true
var BasicInfoForm, LoginMailForm
var $btn_mail_lock, $btn_newpass_lock, $btn_checkpass_lock, $newpass, $passcheck, $NewPassFeedBack
var $member_number, $name, $sex, $status, $level, $email_basic, $cellphone, $telphone_area, $telphone, $telphone_ext, $address_city, $address_town, $address, $email_login, $newpass, $passcheck
var member_list

function PageReady() {
    ManagementDataCollapse();

    OrderDetailsPosition();

    co.Zipcode.init("#TWzipcode");

    ElementInit();

    $(window).resize(function () {
        ManagementDataCollapse();
        OrderDetailsPosition();
    });

    $(".order_data").each(function () {
        var $self_status = $(this).children("div").children(".status");
        ($self_status.text() == "出貨中") && $self_status.addClass("bg_fluorescent");
    });

    if ($email_login.val() != "") {
        $(".btn_mail_lock > span").text("lock");
        $email_login.attr("disabled", "disabled");
        isMailLock = true;
    }

    $(".btn_save").on("click", DataSave);

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回會員管理", "資料將不被保存", "確定", "取消", function () {
            history.back();
        });
    })

    $email_login.keyup(function () {
        LoginMailFilled = FormCheck(LoginMailForm);
    });
    $newpass.keyup(function () {
        PassIsCheck = PassCheck();
    });
    $passcheck.keyup(function () {
        PassIsCheck = PassCheck();
    });

    $btn_mail_lock.on("click", function (event) {
        event.preventDefault();
        event.stopPropagation();
        mail_lock($(this));
    });
    $btn_newpass_lock.on("click", function (event) {
        event.preventDefault();
        event.stopPropagation();
        new_pass_show = PassDisplay($(this), new_pass_show)
    });
    $btn_checkpass_lock.on("click", function (event) {
        event.preventDefault();
        event.stopPropagation();
        check_pass_show = PassDisplay($(this), check_pass_show)
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    BasicInfoForm = $("#MemberData > form")
    LoginMailForm = $("#LoginData form")
    $btn_mail_lock = $(".btn_mail_lock")
    $btn_newpass_lock = $(".btn_newpass_lock")
    $btn_checkpass_lock = $(".btn_checkpass_lock")
    $NewPassFeedBack = $("#NewPassFeedBack");
    $CheckPassFeedBack = $("#CheckPassFeedBack");

    $member_number = $(".member_number")
    $name = $("#InputName");
    $sex = $("input[name=RadioGender]");
    $status = $("select[name='MemberStatus']");
    $level = $("select[name='MemberLevel']");
    $email_basic = $("#InputMailBasic");
    $cellphone = $("#InputCellPhone");
    $telphone_area = $("#InputTelPhoneArea");
    $telphone = $("#InputTelPhone");
    $telphone_ext = $("#InputTelPhoneExt");
    $address_city = $("select[name='county']");
    $address_town = $("select[name='district']");
    $address = $("#InputAddress");
    $email_login = $("#InputMailLogin")
    $newpass = $("#InputPasswordNew");
    $passcheck = $("#InputPasswordCheck");
}

function FormDataClear() {
    keyId = 0;
    $name.val("");
    $sex.each(function () {
        if ($(this).val() == 3) {
            $(this).prop("checked", true);
        }
    })
    $status.val("");
    $level.val("");
    $email_basic.val("");
    $cellphone.val("");
    $telphone_area.val("");
    $telphone.val("");
    $telphone_ext.val("");
    $address_city.val("");
    $address_town.val("");
    $address.val("");
    $email_login.val("");
    $newpass.val("");
    $passcheck.val("");
}

function contentReady(e) {
    member_list = e;
    HashDataEdit();
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
            FormDataClear();
            var hash = window.location.hash.replace("#", "");
            co.Member.Get(parseInt(hash)).done(function (result) {
                if (result != null) {
                    MoveToContent();
                    keyId = parseInt(hash);
                    FormDataSet(result)
                } else {
                    window.location.hash = ""
                }
            })
        }
    } else {
        BackToList();
    }
}

function editButtonClicked(e) {
    keyId = e.row.key;
    window.location.hash = keyId;
    HashDataEdit();
}

function FormDataSet(result) {
    $member_number.text(result.id)
    $name.val(result.name);
    $sex.each(function () {
        if ($(this).val() == result.sex) {
            $(this).prop("checked", true);
        }
    })
    $status.val(result.status);
    $level.val(result.level)
    $email_basic.val(result.email);
    $cellphone.val(result.cellPhone);
    if (result.telPhone != null) {
        var telphone_split = result.telPhone.split("-");
        $telphone_area.val(telphone_split[0]);
        if (telphone_split.length > 1) {
            var telphone_split2 = telphone_split[1].split("#");
            $telphone.val(telphone_split2[0]);
            if (telphone_split2.length > 1) $telphone_ext.val(telphone_split2[1]);
        }
    }
    if (result.address != null) {
        co.Zipcode.setData({
            el: $TWzipcode,
            addr: result.address
        });
    }
    $email_login.val(result.email);
}

function Update(success_text, error_text) {
    var sex = 3
    $sex.each(function () {
        if ($(this).is(":checked")) {
            sex = $(this).val();
        }
    })
    co.Member.Update({
        Id: keyId,
        Name: $name.val(),
        Sex: sex,
        Status: $status.val(),
        Level: $level.val(),
        Email: $email_basic.val(),
        CellPhone: $cellphone.val(),
        TelPhone: $telphone_area.val() == "" ? "" : $telphone_area.val() + "-" + $telphone.val() + ($telphone_ext.val() ==""?"":"#" + $telphone_ext.val()),
        Address: $address_city.val() + " " + $address_town.val() + " " + $address.val()
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () {
            BackToList();
            member_list.component.refresh();
        }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function DataSave() {
    BasicInfoFilled = FormCheck(BasicInfoForm);
    LoginMailFilled = FormCheck(LoginMailForm);
    PassIsCheck = PassCheck();
    if (BasicInfoFilled && LoginMailFilled && PassIsCheck) {
        Coker.sweet.confirm("即將儲存", "確認儲存會員資料?", "儲存", "取消", function () {
            Update("資料儲存成功", "資料儲存發生未知錯誤");
        });
    } else {
        Coker.sweet.error("資料填寫有誤", "請確認填寫資料是否正確", null, false);
    }
}

function FormCheck(Forms) {
    var Check = false;
    Array.from(Forms).forEach(form => {
        if (form.checkValidity()) {
            Check = true;
        }
        form.classList.add('was-validated')
    })
    return Check;
}

function PassCheck() {
    $btn_newpass_lock.addClass("pe-4");
    $btn_checkpass_lock.addClass("pe-4");
    var hasNum = /\d+/, hasLetter = /[a-zA-Z]+/, hasSpesym = /[^\a-\z\A-\Z0-9]/g;
    $newpass.addClass("is-invalid");
    $passcheck.addClass("is-invalid");
    if ($newpass.val().length >= 6) {
        if (hasNum.test($newpass.val()) && hasLetter.test($newpass.val()) && !(hasSpesym.test($newpass.val()))) {
            $newpass.removeClass("is-invalid");
            $newpass.addClass("is-valid");
            if ($passcheck.val() == $newpass.val()) {
                $passcheck.removeClass("is-invalid");
                $passcheck.addClass("is-valid");
                return true;
            } else {
                $CheckPassFeedBack.text("密碼不相符");
            }
        } else {
            $NewPassFeedBack.text("密碼格式有誤");
            $CheckPassFeedBack.text("密碼格式有誤");
        }
    } else {
        if ($newpass.val().length == 0 && $passcheck.val().length == 0) {
            $newpass.removeClass("is-invalid");
            $passcheck.removeClass("is-invalid");
            $newpass.removeClass("is-valid");
            $passcheck.removeClass("is-valid");
            $btn_newpass_lock.removeClass("pe-4");
            $btn_checkpass_lock.removeClass("pe-4");
            return true;
        } else {
            $NewPassFeedBack.text("請輸入6個以上的字元");
            $CheckPassFeedBack.text("密碼格式有誤");
        }
    }
    return false;
}

function mail_lock($self) {
    var $self_span = $self.children("span")
    LoginMailFilled = FormCheck(LoginMailForm);
    if (!isMailLock && $email_login.val() != "" && LoginMailFilled) {
        $self_span.text("lock")
        $email_login.attr("disabled", "disabled");
        isMailLock = true;
        $btn_mail_lock.removeClass("pe-4");
    } else {
        $self_span.text("lock_open")
        $email_login.removeAttr("disabled");
        isMailLock = false;
        $btn_mail_lock.addClass("pe-4");
    }
}

function PassDisplay($self, display) {
    var $self_span = $self.children("span")
    if (display) {
        $self_span.text("visibility_off");
        $self.siblings("input").attr("type", "password");
        display = false;
    } else {
        $self_span.text("visibility");
        $self.siblings("input").attr("type", "text");
        display = true;
    }
    return display;
}

function MoveToContent() {
    $(".btn_back").addClass("d-flex");
    $(".btn_save").removeClass("d-none");
    $("#MemberList").addClass("d-none");
    $("#MemberContent").removeClass("d-none");
}

function BackToList() {
    $(".btn_back").removeClass("d-flex");
    $(".btn_save").addClass("d-none");
    $("#MemberList").removeClass("d-none");
    $("#MemberContent").addClass("d-none");
    $("#MemberForm").removeClass("was-validated");
    window.location.hash = ""
}

function ManagementDataCollapse() {
    $this_body = $("body > .wrapper > .content-area > .content-wrapper");
    $OrderDetails = $("#MainBlock");
    $ManagementData = $("#SideBlock");

    if ($this_body.width() >= 1024) {
        $("#Btn_Side_Collapse").addClass("d-none");
        $OrderDetails.removeClass("col-12");
        $ManagementData.addClass("col-3");
        $ManagementData.removeClass("offcanvas offcanvas-end visible");
        $ManagementData.css('visibility', '');
    } else {
        $("#Btn_Side_Collapse").removeClass("d-none");
        $OrderDetails.addClass("col-12");
        $ManagementData.addClass("offcanvas offcanvas-end visible");
        $ManagementData.removeClass("col-3");
    }
}

function OrderDetailsPosition() {
    if ($("#SideBlock").width() < 280) {
        $(".order_data").each(function () {
            var $btn_details = $(this).children("a");
            $btn_details.removeClass("position-absolute");
        });
    } else {
        $(".order_data").each(function () {
            var $btn_details = $(this).children("a");
            $btn_details.addClass("position-absolute");
        });
    }
}