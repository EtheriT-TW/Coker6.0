function PageReady() {
    ManagementDataCollapse();
    $(window).resize(ManagementDataCollapse);

}

function ManagementDataCollapse() {
    if ($("body").width() >= 992) {
        $("#Btn_Side_Collapse").addClass("d-none");
        $("#ManagementData").addClass("col-12 col-lg-3");
        $("#ManagementData").removeClass("offcanvas offcanvas-end visible");
    } else {
        $("#Btn_Side_Collapse").removeClass("d-none");
        $("#ManagementData").addClass("offcanvas offcanvas-end visible");
        $("#ManagementData").removeClass("col-12 col-lg-3 px-3");
    }
}