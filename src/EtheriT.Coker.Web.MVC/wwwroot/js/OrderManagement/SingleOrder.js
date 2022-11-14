function PageReady() {
    ManagementDataCollapse();
    $(window).resize(ManagementDataCollapse);
}

function ManagementDataCollapse() {
    $this_body = $("body > .wrapper > .content-area > .content-wrapper");
    $OrderDetails = $("#OrderDetails");
    $ManagementData = $("#ManagementData");

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