(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    function getCloseTime() {
        // New config first, fallback legacy
        var t = (Coker.config && Coker.config.timeout && Coker.config.timeout.time) ||
            (Coker.timeout && Coker.timeout.time);
        return typeof t === "number" ? t : 1500;
    }

    Coker.extend({
        ui: {
            sweet: {
                loading: function () {
                    Swal.fire({
                        title: local.Processing,
                        html: local.FormReceivedPleaseWait,
                        allowOutsideClick: false,
                        didOpen: function () { Swal.showLoading(); }
                    });
                },

                custom: function (icon, title, text, confirmtext, confirmaction, canceltext, canceltextaction) {
                    var closetime = false;
                    if (confirmtext == null && canceltext == null) {
                        closetime = getCloseTime();
                    }

                    Swal.fire({
                        icon: icon,
                        title: title,
                        text: text,
                        showConfirmButton: confirmtext == null ? false : true,
                        showCancelButton: canceltext == null ? false : true,
                        confirmButtonColor: "#3085d6",
                        cancelButtonColor: "#d33",
                        confirmButtonText: confirmtext,
                        cancelButtonText: canceltext,
                        timer: closetime || undefined
                    }).then(function (result) {
                        if (result.isConfirmed) {
                            (typeof confirmaction === "function") && confirmaction();
                        } else if (result.dismiss === Swal.DismissReason.cancel) {
                            (typeof canceltextaction === "function") && canceltextaction();
                        }
                    });
                },

                success: function (text, action, autoclose) {
                    var closetime = autoclose ? getCloseTime() : false;

                    Swal.fire({
                        icon: "success",
                        html: text,
                        showConfirmButton: !autoclose,
                        showCancelButton: false,
                        confirmButtonColor: "#3085d6",
                        confirmButtonText: local.Confirm,
                        timer: closetime || undefined,
                        allowOutsideClick: false
                    }).then(function (result) {
                        if (result.isConfirmed) {
                            (typeof action === "function") && action();
                        }
                    });
                },

                error: function (title, text, action, autoclose) {
                    var closetime = autoclose ? getCloseTime() : false;

                    Swal.fire({
                        icon: "error",
                        title: title,
                        html: text,
                        showConfirmButton: !autoclose,
                        showCancelButton: false,
                        confirmButtonColor: "#3085d6",
                        confirmButtonText: local.Confirm,
                        timer: closetime || undefined,
                        allowOutsideClick: false
                    }).then(function (result) {
                        if (result.isConfirmed) {
                            (typeof action === "function") && action();
                        }
                    });
                },

                confirm: function (title, text, confirmtexet, cancanceltext, action) {
                    Swal.fire({
                        icon: "info",
                        title: title,
                        html: text,
                        showCancelButton: cancanceltext === "" ? false : true,
                        confirmButtonColor: "#3085d6",
                        cancelButtonColor: "#d33",
                        confirmButtonText: confirmtexet,
                        cancelButtonText: cancanceltext,
                        allowOutsideClick: false
                    }).then(function (result) {
                        if (result.isConfirmed) {
                            (typeof action === "function") && action();
                        }
                    });
                },

                info: function (title, action) {
                    Swal.fire({
                        icon: "info",
                        title: title,
                        showConfirmButton: true,
                        showCancelButton: false,
                        confirmButtonColor: "#3085d6",
                        confirmButtonText: local.Confirm,
                        allowOutsideClick: false
                    }).then(function (result) {
                        if (result.isConfirmed) {
                            (typeof action === "function") && action();
                        }
                    });
                },

                warning: function (title, text, action) {
                    var executed = false;

                    function runActionOnce() {
                        if (executed) return;
                        executed = true;
                        (typeof action === "function") && action();
                    }

                    Swal.fire({
                        title: title,
                        text: text,
                        icon: "warning",
                        showCancelButton: false,
                        confirmButtonColor: "#3085d6",
                        confirmButtonText: local.Confirm,
                        allowOutsideClick: false
                    }).then(function () {
                        runActionOnce();
                    });

                    if (typeof action === "function") {
                        setTimeout(runActionOnce, 3000);
                    }
                }
            }
        }
    });

    // Legacy: Coker.sweet.*
    Coker.sweet = Coker.sweet || {};
    Coker.sweet.loading = Coker.sweet.loading || Coker.ui.sweet.loading;
    Coker.sweet.custom = Coker.sweet.custom || Coker.ui.sweet.custom;
    Coker.sweet.success = Coker.sweet.success || Coker.ui.sweet.success;
    Coker.sweet.error = Coker.sweet.error || Coker.ui.sweet.error;
    Coker.sweet.confirm = Coker.sweet.confirm || Coker.ui.sweet.confirm;
    Coker.sweet.info = Coker.sweet.info || Coker.ui.sweet.info;
    Coker.sweet.warning = Coker.sweet.warning || Coker.ui.sweet.warning;

})(window);