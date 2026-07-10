(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    if (typeof Coker.defineModule === "function") Coker.defineModule("member-profile", factory);
    else factory(Coker);

    function factory(C) {
        var MemberPage = (w.MemberPage = w.MemberPage || {});

        MemberPage.Profile = {
            init: function () {
                this.bindEvents();
                this.load();
            },

            bindEvents: function () {
                $(".btn_logout").off("click.memberProfile").on("click.memberProfile", function () {
                    C.User.Logout().done(function (result) {
                        if (result.success) {
                            if (C.api && typeof C.api.clearAuth === "function") C.api.clearAuth();
                            C.sweet.success("登出成功");
                            setTimeout(function () {
                                w.location.href = "/";
                            }, 1000);
                        }
                    });
                });

                $(".btn_modifi").off("click.memberProfile").on("click.memberProfile", function () {
                    MemberPage.Profile.submit();
                });

                $(".btn_resetPassword").off("click.memberProfile").on("click.memberProfile", function () {
                    if (w.resetModal && typeof w.resetModal.show === "function") {
                        w.resetModal.show();
                    }
                });
            },

            load: function () {
                C.User.GetUser().done(function (result) {
                    if (!result.success) return;

                    var data = result.data;

                    if (data.telPhone != null && data.telPhone != "") {
                        var phoneParts = String(data.telPhone).split("-");
                        data.zone = phoneParts[0] || "";
                        data.telPhone = phoneParts[1] || "";
                        data.ext = phoneParts.length > 2 ? phoneParts[2] : "";
                    }

                    C.Form.insertData(data, MemberPage.Selectors.userDataForm);

                    MemberPage.State.oldEmail = data.email;

                    C.Zipcode.setData({
                        el: $(MemberPage.Selectors.twZipcode),
                        addr: data.address
                    });

                    $("#ResetForm").data("Email", data.email);

                    $("#Birthday")
                        .attr("max", MemberPage.State.dateNow)
                        .off("keydown.memberProfile")
                        .on("keydown.memberProfile", function (e) {
                            e.preventDefault();
                        });
                });
            },

            submit: function () {
                var data = C.Form.getJson($(MemberPage.Selectors.userDataForm).attr("id"));

                if (data.name == "") {
                    C.sweet.warning("請注意", "姓名不可為空", null);
                    return;
                }

                if ($("#Email").val() == "") {
                    C.sweet.warning("請注意", "電子郵件不可為空", null);
                    return;
                }

                if (data.zone == "" ^ data.telPhone == "") {
                    C.sweet.warning("請注意", "如要填入電話資訊，請確實填寫區碼與聯絡電話", null);
                    return;
                }

                if (data.county == "" ^ data.address == "") {
                    C.sweet.warning("請注意", "如要填入地址資訊，請確實填寫縣市、鄉鎮與地址", null);
                    return;
                }

                if (data.birthday == "") data.birthday = null;
                data.address = data.county + " " + data.district + " " + data.address;

                if (data.cellPhone != "" && (!$.isNumeric(data.cellPhone) || data.cellPhone.length != 10)) {
                    C.sweet.warning("請注意", "手機格式不正確，請重新輸入", null);
                    return;
                }

                if (data.telPhone != "") {
                    if ($.isNumeric(data.zone) && $.isNumeric(data.telPhone) && ((data.ext != "" && $.isNumeric(data.ext)) || data.ext == "")) {
                        data.telPhone = data.zone + "-" + data.telPhone + "-" + data.ext;
                    } else {
                        C.sweet.warning("請注意", "電話格式不正確，請重新輸入", null);
                        return;
                    }
                }

                C.User.UserEdit(data).done(function () {
                    C.sweet.success("資料修改完成！", null, true);
                });
            }
        };
    }
})(window);
