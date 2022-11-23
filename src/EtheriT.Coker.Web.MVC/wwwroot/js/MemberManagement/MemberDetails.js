var newshow = false, checkshow = false, maillock = false, BasicInfoFilled = false, LoginMailFilled = false, PassIsCheck = true
var BasicInfoForm, LoginMailForm
var $btn_mail_lock, $btn_newpass_lock, $btn_checkpass_lock, $InputMail, $InputNew, $InputCheck, $PassFeedBack

function PageReady() {
    ManagementDataCollapse();
    OrderDetailsPosition();

    $(window).resize(function () {
        ManagementDataCollapse();
        OrderDetailsPosition();
    });

    getLatestOrder();

    TWZipCodeInit();

    ElementInit();

    if ($("#InputLoginMail").val() != "") {
        $(".btn_mail_lock > span").text("lock");
        $("#InputLoginMail").attr("disabled", "disabled");
        maillock = true;
    }

    $(".btn_save").on("click", DataSave);

    $InputMail.keyup(function () {
        LoginMailFilled = FormCheck(LoginMailForm);
    });
    $InputNew.keyup(function () {
        PassIsCheck = PassCheck();
    });
    $InputCheck.keyup(function () {
        PassIsCheck = PassCheck();
    });

    $btn_mail_lock.on("click", function (event) {
        event.preventDefault();
        event.stopPropagation();
        MailLock($(this));
    });

    $btn_newpass_lock.on("click", function (event) {
        event.preventDefault();
        event.stopPropagation();
        newshow = PassDisplay($(this), newshow)
    });

    $btn_checkpass_lock.on("click", function (event) {
        event.preventDefault();
        event.stopPropagation();
        checkshow = PassDisplay($(this), checkshow)
    });
}

function ElementInit() {
    BasicInfoForm = $("#MemberData > form")
    LoginMailForm = $("#LoginData form")
    $btn_mail_lock = $(".btn_mail_lock")
    $btn_newpass_lock = $(".btn_newpass_lock")
    $btn_checkpass_lock = $(".btn_checkpass_lock")
    $InputMail = $("#InputLoginMail")
    $InputNew = $("#InputNewPassword");
    $InputCheck = $("#InputConfirmPassword");
    $PassFeedBack = $("#PassFeedBack");
}

function DataSave() {
    BasicInfoFilled = FormCheck(BasicInfoForm);
    LoginMailFilled = FormCheck(LoginMailForm);
    PassIsCheck = PassCheck();
    if (BasicInfoFilled && LoginMailFilled && PassIsCheck) {
        Coker.sweet.success("資料更新成功", null, true);
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
    $InputNew.addClass("is-invalid");
    $InputCheck.addClass("is-invalid");
    if ($InputNew.val().length >= 6) {
        if (hasNum.test($InputNew.val()) && hasLetter.test($InputNew.val()) && !(hasSpesym.test($InputNew.val()))) {
            $InputNew.removeClass("is-invalid");
            $InputNew.addClass("is-valid");
            if ($InputCheck.val() == $InputNew.val()) {
                $InputCheck.removeClass("is-invalid");
                $InputCheck.addClass("is-valid");
                return true;
            }
        } else {
            $PassFeedBack.text("密碼格式有誤");
        }
    } else {
        if ($InputNew.val().length == 0 && $InputCheck.val().length == 0) {
            $InputNew.removeClass("is-invalid");
            $InputCheck.removeClass("is-invalid");
            $InputNew.removeClass("is-valid");
            $InputCheck.removeClass("is-valid");
            $btn_newpass_lock.removeClass("pe-4");
            $btn_checkpass_lock.removeClass("pe-4");
            return true;
        } else {
            $PassFeedBack.text("請輸入6個以上的字元");
        }
    }
    return false;
}

function MailLock($self) {
    var $self_span = $self.children("span")
    LoginMailFilled = FormCheck(LoginMailForm);
    if (!maillock && $("#InputLoginMail").val() != "" && LoginMailFilled) {
        $self_span.text("lock")
        $("#InputLoginMail").attr("disabled", "disabled");
        maillock = true;
        $btn_mail_lock.removeClass("pe-4");
    } else {
        $self_span.text("lock_open")
        $("#InputLoginMail").removeAttr("disabled");
        maillock = false;
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

function getLatestOrder() {
    $(".order_data").each(function () {
        var $self_status = $(this).children("div").children(".status");
        ($self_status.text() == "出貨中") && $self_status.addClass("bg_fluorescent");
    });
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

function TWZipCodeInit() {
    $TWzipcode = $('#TWzipcode');

    $TWzipcode.twzipcode({
        'zipcodeIntoDistrict': true,
        'countySel': '高雄市',
        'districtSel': '前鎮區'
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
}
