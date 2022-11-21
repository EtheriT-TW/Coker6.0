var newpass, checkpass
var newshow = false, checkshow = false

function PageReady() {
    ManagementDataCollapse();
    OrderDetailsPosition();

    $(window).resize(function () {
        ManagementDataCollapse();
        OrderDetailsPosition();
    });

    getLatestOrder();

    TWZipCodeInit();

    $(".btn_newpass_lock ").on("click", function () {
        $(this).children("span").text($(this).text() == "visibility_off" ? "visibility" : "visibility_off");
        newshow = !newshow;
        $(this).siblings(".new_pass").attr("type", newshow ? "text" : "password");
    });

    $(".btn_checkpass_lock ").on("click", function () {
        $(this).children("span").text($(this).text() == "visibility_off" ? "visibility" : "visibility_off");
        checkshow = !checkshow;
        $(this).siblings(".check_pass").attr("type", checkshow ? "text" : "password");
    });
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