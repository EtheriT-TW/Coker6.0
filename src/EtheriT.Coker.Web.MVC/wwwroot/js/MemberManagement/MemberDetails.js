function PageReady() {
    ManagementDataCollapse();
    $(window).resize(ManagementDataCollapse);

    TWZipCodeInit();
}

function ManagementDataCollapse() {
    $this_body = $("body > .wrapper > .content-area > .content-wrapper");
    $MainBlock = $("#MainBlock");
    $SideBlock = $("#SideBlock");

    if ($this_body.width() >= 1024) {
        $("#Btn_Side_Collapse").addClass("d-none");
        $MainBlock.removeClass("col-12");
        $SideBlock.addClass("col-3");
        $SideBlock.removeClass("offcanvas offcanvas-end visible");
        $SideBlock.css('visibility', '');
    } else {
        $("#Btn_Side_Collapse").removeClass("d-none");
        $MainBlock.addClass("col-12");
        $SideBlock.addClass("offcanvas offcanvas-end visible");
        $SideBlock.removeClass("col-3");
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