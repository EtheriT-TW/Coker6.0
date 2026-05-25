Coker.extend({
    sweet: {
        config: {
            timeout: 1500
        },
        loading: function () {
            Swal.fire({
                title: "資料處理中，請稍後。",
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                },
                willClose: () => {
                }
            }).then((result) => {
            });
        },
        success: function (text, action, autoclose) {
            var closetime = false;
            if (autoclose) { closetime = Coker.sweet.config.timeout }

            Swal.fire({
                icon: 'success',
                html: text,
                showConfirmButton: !autoclose,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                timer: closetime
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        error: function (title, text, action, autoclose) {
            var closetime = false;
            if (autoclose) { closetime = Coker.sweet.config.timeout }

            Swal.fire({
                icon: 'error',
                title: title,
                html: text,
                showConfirmButton: !autoclose,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                reverseButtons: true,
                timer: closetime
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        confirm: function (title, text, confirmtexet, cancanceltext, action, fail) {
            Swal.fire({
                icon: 'info',
                title: title,
                html: text,
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: confirmtexet,
                cancelButtonText: cancanceltext,
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                } else {
                    typeof (fail) === "function" && fail();
                }
            })
        },
        confirmAsync: async function (title, text, confirmtexet, cancanceltext) {
            return new Promise((resolve) => {
                co.sweet.confirm(
                    title,
                    text,
                    confirmtexet,
                    cancanceltext,
                    function () { // 使用者按「確認」
                        resolve(true);
                    },
                    function () { // 使用者按「取消」（如果你原本有這個 callback，就接上）
                        resolve(false);
                    }
                );
            });
        },
        confirmSave: function (title, text, saveAction, skipAction, cancelAction) {
            Swal.fire({
                icon: 'info',
                title: title,
                html: text,
                showCancelButton: true,
                showDenyButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#888888',
                denyButtonColor: '#d33',
                confirmButtonText: "是",
                denyButtonText: "否",
                cancelButtonText: "取消",
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (saveAction) === "function" && saveAction();
                } else if (result.isDenied) {
                    typeof (skipAction) === "function" && skipAction();
                } else {
                    typeof (cancelAction) === "function" && cancelAction();
                }
            });
        },
        warn: function (title, text, action) {
            Swal.fire({
                icon: 'warning',
                title: title,
                html: text,
                showConfirmButton: true,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        TitleHilight: function (string, title) {
            return string.replace("{0}", `<span class='ConfirmKeyWord'>${title}</span>`);
        }
    }
});