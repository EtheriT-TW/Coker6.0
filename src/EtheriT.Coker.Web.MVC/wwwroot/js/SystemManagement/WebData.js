var companyInfoIsEdit = false;

function PageReady() {
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

    $("#PrivacyStatement > div > .form_btn > .btn_exit").on("click", PrivacyStatementExit);
    $("#PrivacyStatement > div > .form_btn > .btn_save").on("click", PrivacyStatementSave);

    $("#MembershipTerms > div > .form_btn > .btn_exit").on("click", MembershipTermsExit);
    $("#MembershipTerms > div > .form_btn > .btn_save").on("click", MembershipTermsSave);

    TWZipCodeInit();
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

function CompanyInfoSave() {
    companyInfoIsEdit = false;
    CompanyInfoEdit();
}

function PrivacyStatementExit() {
    Coker.sweet.confirm("直接離開", "確定不儲存資料直接離開？", "是，直接離開", "否，繼續編輯", function () {
        $("#PrivacyStatement > div > button").click();
    });
}

function PrivacyStatementSave() {
    Coker.sweet.success("資料儲存成功", null, true);
    $("#PrivacyStatement > div > button").click();
}

function MembershipTermsExit() {
    Coker.sweet.confirm("直接離開", "確定不儲存資料直接離開？", "是，直接離開", "否，繼續編輯", function () {
        $("#MembershipTerms > div > button").click();
    });
}

function MembershipTermsSave() {
    Coker.sweet.success("資料儲存成功", null, true);
    $("#MembershipTerms > div > button").click();
}

function TWZipCodeInit() {
    $TWzipcode = $('#TWzipcode');

    $TWzipcode.twzipcode({
        'zipcodeIntoDistrict': true,
        'countySel': '新北市',
        'districtSel': '鶯歌區'
    });

    var $county, $district;

    $county = $TWzipcode.children('.county');
    $district = $TWzipcode.children('.district');

    $county.children('select').attr({
        id: "SelectCity",
        class: "city form-select",
        disabled: "disabled"
    });
    $county.append("<label class='px-4 required' for='SelectCity'>縣市</label>");
    var $county_first_option = $county.children('select').children('option').first();
    $county_first_option.text("請選擇縣市");
    $county_first_option.attr('disabled', 'disabled');

    $district.children('select').attr({
        id: "SelectTown",
        class: "town form-select",
        disabled: "disabled"
    });
    $district.append("<label class='required' for='SelectTown'>鄉鎮</label>");
    var $district_first_option = $district.children('select').children('option').first();
    $district_first_option.text("請選擇鄉鎮");
    $district_first_option.attr('disabled', 'disabled');
}