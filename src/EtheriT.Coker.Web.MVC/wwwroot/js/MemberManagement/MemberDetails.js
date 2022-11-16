function PageReady() {
    ManagementDataCollapse();
    OrderDetailsPosition();

    $(window).resize(function () {
        ManagementDataCollapse();
        OrderDetailsPosition();
    });

    getLatestOrder();
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