(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-modals", factory);
    else factory(Coker);

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        MemberPage.Modals = {
            init: function (loginData) {
                var state = MemberPage.State;

                state.loginData = loginData || null;
                state.resetEmailModal = $(MemberPage.Selectors.resetEmailModal).length > 0
                    ? new bootstrap.Modal($(MemberPage.Selectors.resetEmailModal))
                    : null;
                state.reOrderAlertModal = $(MemberPage.Selectors.reOrderAlertModal).length > 0
                    ? new bootstrap.Modal($(MemberPage.Selectors.reOrderAlertModal))
                    : null;

                state.resetEmailCaptchaInput = $(MemberPage.Selectors.resetEmailCaptchaInput);
                state.resetEmailCaptchaImage = $(MemberPage.Selectors.resetEmailCaptchaImage);
                state.resetEmailForm = $(MemberPage.Selectors.resetEmailForm);

                this.initResetPasswordUi();
                this.bindResetEmail();
                this.bindReOrderAlert();
            },

            initResetPasswordUi: function () {
                $("#ResetForm .reset_old_pass").removeClass("d-none");
                $("#ResetForm .reset_old_pass input").removeAttr("disabled");
                $("#ResetOldPassFeedBack").removeClass("d-none");
                $("#ResetModal .btn_resetforget").removeClass("d-none");
            },

            bindResetEmail: function () {
                var state = MemberPage.State;

                $(".btn_resetEmail").off("click.memberModals").on("click.memberModals", function (event) {
                    event.preventDefault();
                    if (state.resetEmailModal) state.resetEmailModal.show();
                });

                $("#ResetEmailModal .btn_resetforget").off("click.memberModals").on("click.memberModals", function () {
                    $("#ResetModal .btn_resetforget").trigger("click");
                });

                $("#ResetEmailModal .btn_refresh").off("click.memberModals").on("click.memberModals", function (event) {
                    event.preventDefault();
                    w.NewCaptcha(state.resetEmailCaptchaImage, state.resetEmailCaptchaInput);
                });

                state.resetEmailModalElement = document.getElementById("ResetEmailModal");

                if (state.resetEmailModalElement != null && !$(state.resetEmailModalElement).data("memberModalBound")) {
                    state.resetEmailModalElement.addEventListener("show.bs.modal", function () {
                        w.NewCaptcha(state.resetEmailCaptchaImage, state.resetEmailCaptchaInput);
                    });

                    state.resetEmailModalElement.addEventListener("hidden.bs.modal", function () {
                        w.FormClear(state.resetEmailForm, state.resetEmailCaptchaInput);
                    });

                    $(state.resetEmailModalElement).data("memberModalBound", true);
                }

                $("#ResetEmailForm input").off("keypress.memberModals").on("keypress.memberModals", function (event) {
                    if (event.which == 13) {
                        event.preventDefault();
                        $("#ResetEmailModal .btn_resetmail").trigger("click");
                    }
                });

                $(".btn_resetmail").off("click.memberModals").on("click.memberModals", function () {
                    MemberPage.Modals.submitResetEmail();
                });
            },

            bindReOrderAlert: function () {
                var state = MemberPage.State;

                state.reOrderAlertModalElement = document.getElementById("ReOrderAlertModal");

                if (state.reOrderAlertModalElement != null && !$(state.reOrderAlertModalElement).data("memberModalBound")) {
                    state.reOrderAlertModalElement.addEventListener("hidden.bs.modal", function () {
                        $(".btn_repay").removeClass("d-none");
                        $("#ReOrderAlertModal .orderlist ul li").remove();
                    });

                    $(state.reOrderAlertModalElement).data("memberModalBound", true);
                }

                $("#ReOrderAlertModal .btn_cancelrepay").off("click.memberModals").on("click.memberModals", function () {
                    if (state.reOrderAlertModal) state.reOrderAlertModal.hide();
                });
            },

            submitResetEmail: function () {
                var state = MemberPage.State;

                if (w.SiteFormCheck(state.resetEmailForm, state.resetEmailCaptchaInput)) {
                    w.CaptchaVerify(state.resetEmailCaptchaImage, state.resetEmailCaptchaInput, function () {
                        MemberPage.Modals.resetEmailAction();
                    });
                    return;
                }

                state.resetEmailCaptchaInput.addClass("is-invalid");
                state.resetEmailCaptchaInput.siblings("div").addClass("me-4 pe-2");
                w.NewCaptcha(state.resetEmailCaptchaImage, state.resetEmailCaptchaInput);
                state.resetEmailCaptchaInput.val("");

                C.sweet.warning("請注意", "請確實填寫資料", null, true);
            },

            resetEmailAction: function () {
                var state = MemberPage.State;
                var inputData = C.Form.getJson($(MemberPage.Selectors.resetEmailForm).attr("id"));

                if (inputData.email == state.oldEmail) {
                    C.sweet.info(local.InfoEmailSameNoChange, null);
                    if (state.resetEmailModal) state.resetEmailModal.hide();
                    return;
                }

                C.sweet.loading();

                C.User.EmailChange(inputData).done(function (result) {
                    if (result.success) {
                        C.sweet.success(local.ResultEmailChangeSuccess, function () {
                            if (C.api && typeof C.api.clearAuth === "function") C.api.clearAuth();
                            w.location.href = "/";
                        }, false);
                        return;
                    }

                    w.NewCaptcha(state.resetEmailCaptchaImage, state.resetEmailCaptchaInput);

                    switch (result.error) {
                        case local.PasswordIncorrect:
                            C.sweet.confirm(local.PasswordIncorrect, result.message, local.ForgotPassword + "?", local.Confirm, function () {
                                $("#ResetModal .btn_resetforget").trigger("click");
                            });
                            break;
                        case local.PasswordErrorThreeTimesTitle:
                            C.sweet.error(result.error, result.message, null, false);
                            break;
                        default:
                            C.sweet.error(result.error, result.message, null, false);
                            break;
                    }
                });
            }
        };
    }
})(window);
