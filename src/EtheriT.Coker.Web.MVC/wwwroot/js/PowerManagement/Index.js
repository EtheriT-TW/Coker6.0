function PageReady() {
    let nItem = null,RoleID=0;
    let data,setMenu=[],type;
    let html = $("#MemberData").html();
    let roleHtml = $("#RoleData").html();
    const powerHtml = $("#PowerData").html();
    let permission;
    const init = function () {
        co.PowerManagement.GetPermission().done(function(result){
            permission = result;
            const $e = $("<div>").append(roleHtml);
            const $m = $("<div>").append(html);
            if (!permission.CanRemove) {
                $e.find(".btn.fa-trash-alt").remove();
                $m.find(".btn.fa-trash-alt").remove();
                $(".delete").remove();
            } 
            if (!permission.CanUpdate) $e.find(".btn.fa-edit").remove();
            if (!permission.CanCreate) $(".addbtn").remove();
            roleHtml = $e.html();
            html = $m.html();
            $(data).each(function (index, element) {
                if (element.id != 0) addRole(element);
                else {
                    $("#noGroupMember").data(element);
                    setNoGroup();
                }
            });
            $(".powerctrl .role-item").first().trigger("click");
            loadMenus();
            co.Zipcode.init("#TWzipcode");
            co.Zipcode.init("#new-TWzipcode");
        });
    }
    const loadMenus = function () {
        co.PowerManagement.GetAll().done(function (result) {
            const $view = $("#offcanvas1").data("init", result.jobs);
            const $body = $view.find("#Permissions");
            $view.find(".siteName").text(result.title);
            insetMenu($body, result.jobs);
            if (permission.CanUpdate) {
                $("#offcanvas1 .selectedItem").off("change.save").on("change.save", function () {
                    const $self = $(this)
                    const data = {
                        name: $self.data("name"),
                        IsGranted: $(this).prop("checked")
                    };
                    if (co.Array.Search(setMenu, { name: data.name }) > -1) co.Array.Delete(setMenu, { name: data.name });
                    else setMenu.push(data);
                });
                $("#offcanvas1 .save").off("click").on("click", function () {
                    const obj = {
                        Items: setMenu
                    }
                    obj[type] = $(nItem).data("id");
                    co.PowerManagement.SavePermissions(obj).done(function (result) {
                        if (result.success) {
                            co.sweet.success("儲存成功");
                        } else {
                            co.sweet.error("儲存失敗", result.error);
                        }
                    });
                });
            } else {
                $("#offcanvas1 .selectedItem + label").addClass("pointer-event-none");
                $("#offcanvas1 .save").remove();
            } 
        });
    }
    const insetMenu = function ($body, jobs) {
        $(jobs).each((i, self) => {
            if (self.enable) {
                const $item = $(powerHtml);
                $item.find(".title").text(self.title);
                const newItem = $item.find("#new");
                $item.find("#new+label").attr({ for: `${self.pageName}_new` });
                newItem.attr({ id: `${self.pageName}_new`, name: `${self.pageName}_new` }).data({ "name": `${self.pageName}.Create`}).prop("checked", self.canCreate)

                const delItem = $item.find("#del");
                $item.find("#del+label").attr({ for: `${self.pageName}_del` });
                delItem.attr({ id: `${self.pageName}_del`, name: `${self.pageName}_del` }).data({ "name": `${self.pageName}.Delete` }).prop("checked", self.canRemove)

                const editItem = $item.find("#edit");
                $item.find("#edit+label").attr({ for: `${self.pageName}_edit` });
                editItem.attr({ id: `${self.pageName}_edit`, name: `${self.pageName}_edit` }).data({ "name": `${self.pageName}.Edit` }).prop("checked", self.canUpdate)

                const viewItem = $item.find("#view");
                $item.find("#view+label").attr({ for: `${self.pageName}_view` });
                viewItem.attr({ id: `${self.pageName}_view`, name: `${self.pageName}_view` }).data({ "name": `${self.pageName}.View` }).prop("checked", self.canVisble);
                viewItem.on("change", function () {
                    if (!$(this).prop("checked")) {
                        editItem.prop("checked") && editItem.prop("checked", false).trigger('change');
                        delItem.prop("checked") && delItem.prop("checked", false).trigger('change');
                        newItem.prop("checked") && newItem.prop("checked", false).trigger('change');
                    }
                });
                $item.appendTo($body);
                if (self.jobItemModels != null && self.jobItemModels.length > 0) insetMenu($item, self.jobItemModels);
            }
        });
    }
    const setMenuInitPermissions = function (data) {
        $(data).each(function () {
            setMenuItemPermissions(this);
            if (this.jobItemModels != null) {
                setMenuInitPermissions(this.jobItemModels);
            }
        });
    }
    const setMenuUserPermissions = function (data) {
        $(data).each(function () {
            const self = this;
            const items = self.name.split(".");
            switch (items[1]) {
                case "Edit":
                    $(`[name="${items[0]}_edit"]`).prop("checked", self.isGranted);
                    break;
                case "Delete":
                    $(`[name="${items[0]}_del"]`).prop("checked", self.isGranted);
                    break;
                case "Create":
                    $(`[name="${items[0]}_new"]`).prop("checked", self.isGranted);
                    break;
                case "View":
                    $(`[name="${items[0]}_view"]`).prop("checked", self.isGranted);
                    break;
            }
        });
    }
    const setMenuItemPermissions = function (self) {
        $(`[name="${self.pageName}_new"]`).prop("checked", self.canCreate);
        $(`[name="${self.pageName}_del"]`).prop("checked", self.canRemove);
        $(`[name="${self.pageName}_edit"]`).prop("checked", self.canUpdate);
        $(`[name="${self.pageName}_view"]`).prop("checked", self.canVisble);
    }
    const setMember = function () {
        const $self = $(this);
        const data = $self.data();
        $(".powerctrl .role-item").removeClass("roleChecked");
        $self.addClass("roleChecked");
        $("#roleMember").data(data);
        setRoleMember();
    }
    const setNoGroup = function () {
        var members = $("#noGroupMember").data("members");
        $("#noGroupMember").empty();
        $(members).each(function () {
            addItem("#noGroupMember", this)
        })
    }
    const setRoleMember = function () {
        var members = $("#roleMember").data("members");
        $("#roleMember").empty();
        $(members).each(function () {
            addItem("#roleMember", this)
        })
    }
    const addItem = function (id, element) {
        const $item = $(html).data(element);
        $item.find(".title").text(element.name);
        $item.on("click", function (event) {
            event.stopPropagation();
            if ($(this).hasClass("checked")) $(this).removeClass("checked");
            else $(this).addClass("checked");
        });
        $item.find(".fa-trash-alt").on("click", function (event) {
            event.stopPropagation();
            const m = $(this).closest(".item");
            const s = `是否確認刪除使用者<span class="ConfirmDanger">${$(m).text()}</span>`;
            const l = $(this).closest(".card-content");
            co.sweet.confirm("確認刪除?", s, "確認", "取消", function () {
                co.PowerManagement.RemoveMappingUserAndWebsite($(m).data("id")).done((result) => {
                    if (result.success) {
                        co.sweet.success("已刪除授權");
                        if ($(l).attr("id") == "noGroupMember") {
                            var members = $("#noGroupMember").data("members");
                            co.Array.Delete(members, $(m).data());
                            $("#noGroupMember").data("members", members);
                            setNoGroup();
                            $('.offcanvas').offcanvas('hide');
                        } else {
                            var members = $("#roleMember").data("members");
                            co.Array.Delete(members, $(m).data());
                            $("#roleMember").data("members", members);
                            setRoleMember();
                            $('.offcanvas').offcanvas('hide');
                        }
                    } else co.sweet.error(result.error);
                });
            });
        });
        $item.find(".fa-edit").on("click", function (event) {
            event.stopPropagation();
            const self = $(this).parents(".member-item").first();
            nItem = self;
            co.PowerManagement.GetUser(self.data("id")).done((result) => {
                if (result.success) {
                    var $form = $("#offcanvastopByMember form");
                    co.Form.insertData(result.data, $form);
                } else co.sweet.error("資料錯誤", result.error);
            });
        });
        $item.find(".fa-user-cog").on("click", function (event) {
            event.stopPropagation();
            const self = $(this).parents(".member-item").first();
            nItem = self;
            type = "FK_UserId";
        });
        $item.appendTo(id);
    }
    const addRole = function (element) {
        const $item = $(roleHtml).data(element);
        $item.find(".title").text(element.name);
        $item.on("click", setMember);
        $item.find(".fa-trash-alt").on("click", function (event) {
            event.stopPropagation();
            const m = $(this).closest(".item");
            const s = `是否確認刪除角色<span class="rad">${$(m).text()}</span>`;
            co.sweet.confirm("確認刪除?", s, "確認", "取消", function () {
                co.PowerManagement.DeleteRole($(m).data("id")).done((result) => {
                    if (result.success) {
                        co.sweet.success("已刪除角色");
                        $(m).remove();
                    } else co.sweet.error(result.error);
                });
            });
        });
        $item.find(".fa-edit").on("click", function (event) {
            event.stopPropagation();
            const m = $(this).closest(".item");
            nItem = m;
            co.Form.insertData($(m).data(), $("#offcanvastopByRoleEdit"));
        });
        $item.find(".fa-user-cog").on("click", function (event) {
            event.stopPropagation();
            const self = $(this).parents(".role-item").first();
            nItem = self;
            type = "FK_RoleId";
        });
        $item.appendTo("#RoleList");
    }
    co.PowerManagement.getAllUsers().done((result) => {
        if (result.success) {
            data = result.data;
            init();
        }
    });
    

    $("#right-btn").on("click", function () {
        const $items = $("#roleMember .member-item.checked");
        const $Role = $("#RoleList .role-item.roleChecked");
        if ($items.length > 0) {
            const removeitems = [];
            const moveItems = [];
            $items.each(function () {
                removeitems.push($(this).data("id"));
                moveItems.push(this);
            });
            co.PowerManagement.RemoveUserToRole({ Users: removeitems, RoleId: $Role.data("id") }).done((result) => {
                if (result.success) {
                    $(moveItems).each(function () {
                        co.Array.Delete($Role.data("members"), $(this).data());
                        $(this).appendTo("#noGroupMember");
                        $(this).removeClass("checked");
                    });
                }
            });
        } else co.sweet.error("請選擇要退出群組的使用者");
    });

    $("#left-btn").on("click", function () {
        const $items = $("#noGroupMember .member-item.checked");
        const $Role = $("#RoleList .role-item.roleChecked");
        if ($items.length <= 0) co.sweet.error("加入失敗","請選擇要加入角色的使用者");
        else if ($Role.length <= 0) co.sweet.error("加入失敗", "請選擇或新增使用者要加入的角色");
        else {
            const additems = [];
            const moveItems = [];
            $items.each(function () {
                additems.push($(this).data("id"));
                moveItems.push(this);
            });
            co.PowerManagement.AddUserToRole({ Users: additems, RoleId: $Role.data("id") }).done((result) => {
                if (result.success) {
                    $(moveItems).each(function () {
                        $Role.data("members").push($(this).data());
                        $(this).appendTo("#roleMember");
                        $(this).removeClass("checked");
                    });
                }
            });
        } 
    });
    $("#offcanvastopByMember .cancel").on("click", () => {
        $('.offcanvas').offcanvas('hide');
    });
    $("#offcanvastopByMember .delete").on("click", () => {
        $(nItem).find(".fa-trash-alt").trigger("click");
    });
    $("#offcanvastopByMember .submit").on("click", () => {
        $('.offcanvas').offcanvas('hide');
    });
    $("#offcanvastopByAddUser .submit").on("click", () => {
        const text = $(`#offcanvastopByAddUser [name="email"]`).val();
        if (text.trim()=="") co.sweet.error("請輸入要加入管理的使用者帳號或電子信箱");
        else {
            co.PowerManagement.MappingUserAndWebsite({
                emailOrAccount: text,
                RoleId: RoleID
            }).done((result) => {
                if (result.success) {
                    var obj = JSON.parse(result.message);
                    $('.offcanvas').offcanvas('hide');
                    if (RoleID == 0) {
                        var data = $("#noGroupMember").data();
                        data.members.unshift({ id: obj.Id, name: obj.Name });
                        $("#noGroupMember").data("members", data.members);
                        setNoGroup();
                        $("#noGroupMember .item:first-child").trigger("click");
                    } else {
                        var data = $("#roleMember").data();
                        data.members.unshift({ id: obj.Id, name: obj.Name });
                        $("#roleMember").data("members", data.members);
                        setRoleMember();
                        $("#roleMember .item:first-child").trigger("click");
                    }
                } else co.sweet.error("新增失敗",result.error);
            });
        }
    });
    co.Form.init("offcanvastopByNewMemberForm", (id) => {
        const data = co.Form.getJson(id);
        const $password = document.getElementById("password");
        const setPasswordError = (text) => {
            $password.setCustomValidity(text)
            $("#password ~ .invalid-feedback").text(text);
            $("#PasswordConfirm").val("");
        }
        if (data.password.length < 8 || password > 30) setPasswordError("密碼長度需在8~30個字元");
        else if (data.password != data.PasswordConfirm) setPasswordError("密碼與密碼驗證不相符");
        else {
            co.PowerManagement.AddUser(data).done(function (resule) {
                if (resule.success) {
                    $(`#offcanvastopByAddUser [name="email"]`).val(data.account);
                    $("#offcanvastopByAddUser .submit").trigger("click");
                } else {
                    if (resule.error.indexOf("密碼") > -1) {
                        $("#password").val("");
                        setPasswordError(resule.error);
                    }
                    co.sweet.error("帳號新增失敗", resule.error);
                }
            });
        }
    });
    $("#offcanvastopByRole .submit").on("click", () => {
        const text = $(`#offcanvastopByRole [name="name"]`).val();
        if (text.trim() == "") co.sweet.error("角色名稱不可為空");
        else {
            co.PowerManagement.AddRole({ Name: text }).done(function (result) {
                if (result.success) {
                    var obj = JSON.parse(result.message);
                    $('.offcanvas').offcanvas('hide');
                    addRole({ id: obj.Id, name: obj.Name, members: [] });
                    $("#RoleList .item:last-child").trigger("click");
                } else co.sweet.error("新增失敗", result.error);
            });
        }
    });
    $("#offcanvastopByRoleEdit .submit").on("click", () => {
        const text = $(`#offcanvastopByRoleEdit [name="name"]`).val();
        if (text.trim() == "") co.sweet.error("角色名稱不可為空");
        else {
            const data = co.Form.getJson("offcanvastopByRoleEditForm");
            co.PowerManagement.EditRole(data).done(function (result) {
                if (result.success) {
                    $('.offcanvas').offcanvas('hide');
                    $(nItem).find(".title").text(data.name);
                    $(nItem).data(data);
                    co.sweet.success("修改成功");
                } else co.sweet.error("修改失敗", result.error);
            });
        }
    });
    $("#addUserInRole").on("click", () => {
        if ($("#RoleList .roleChecked").length == 0) {
            co.sweet.error("請選擇欲讓使用者加入的角色");
            $('.offcanvas').offcanvas('hide');
        } else {
            $('#offcanvastopByAddUserSelect').offcanvas('show');
            RoleID = $("#RoleList .roleChecked").data("id");
        }
    });
    $("#offcanvas1").on("shown.bs.offcanvas", function () {
        setMenu.length = 0;
        setMenuInitPermissions($("#offcanvas1").data("init"));
        const obj = {};
        obj[type] = $(nItem).data("id");
        $("#offcanvas1 .data>.type").text(type.indexOf("Role") > 0 ? "角色" : "使用者");
        $("#offcanvas1 .data>.name").text($(nItem).data("name"));
        co.PowerManagement.GetPermissions(obj).done(function (result) {
            if (result.success) {
                setMenuUserPermissions(result.items)
            } else co.sweet.error("權限抓取失敗", result.error);
        });
    });
    $("#offcanvastopByNewMember").on("show.bs.offcanvas", function () {
        $("#offcanvastopByNewMember form").get(0).reset();
    });
   
    $("#addUser").on("click", () => {
        RoleID = 0;
    });
}