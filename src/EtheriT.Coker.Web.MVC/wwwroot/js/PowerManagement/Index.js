function PageReady() {
    let nItem = null,RoleID=0;
    let data = [
        {
            id: 1,
            name: "總管理者",
            members: [{
                id: 1,
                name: "易碩網際科技"
            }, {
                id: 2,
                name: "黃小姐"
            }, {
                id: 3,
                name: "章小姐"
            }]
        },
        {
            id: 2,
            name: "網站維護者",
            members: [{
                id: 4,
                name: "張小姐"
            }]
        },
        {
            id: 0,
            name: "無群組",
            members: [{
                id: 5,
                name: "黃先生"
            }]
        }
    ];
    const html = $("#MemberData").html();
    const roleHtml = $("#RoleData").html();
    const powerHtml = $("#PowerData").html();
    const init = function () {
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
    }
    const loadMenus = function () {
        co.PowerManagement.GetAll().done(function (result) {
            const $view = $("#offcanvas1");
            const $body = $view.find(".offcanvas-body");
            $view.find(".siteName").text(result.title);
            insetMenu($body,result.jobs)
        });
    }
    const insetMenu = function ($body, jobs) {
        $(jobs).each((i, self) => {
            if (self.enable) {
                const $item = $(powerHtml);
                $item.find(".title").text(self.title);
                $item.find("#new+label").attr({ for: `${self.pageName}_new` });
                $item.find("#new").attr({ id: `${self.pageName}_new`, name: `${self.pageName}_new` }).prop("checked", self.canCreate)

                $item.find("#del+label").attr({ for: `${self.pageName}_del` });
                $item.find("#del").attr({ id: `${self.pageName}_del`, name: `${self.pageName}_del` }).prop("checked", self.canRemove)

                $item.find("#edit+label").attr({ for: `${self.pageName}_edit` });
                $item.find("#edit").attr({ id: `${self.pageName}_edit`, name: `${self.pageName}_edit` }).prop("checked", self.canUpdate)

                $item.find("#view+label").attr({ for: `${self.pageName}_view` });
                $item.find("#view").attr({ id: `${self.pageName}_view`, name: `${self.pageName}_view` }).prop("checked", self.canVisble)
                $item.appendTo($body);
                if (self.jobItemModels != null && self.jobItemModels.length > 0) insetMenu($item, self.jobItemModels);
            }
        });
        
    }
    const setMember = function () {
        const $self = $(this);
        const data = $self.data();
        $(".powerctrl .role-item").removeClass("roleChecked");
        $self.addClass("roleChecked");
        $("#roleMember").empty();
        $(data.members).each((i,e) => {
            addItem("#roleMember", e);
        });
    }
    const setNoGroup = function () {
        var members = $("#noGroupMember").data("members");
        $("#noGroupMember").empty();
        $(members).each(function () {
            addItem("#noGroupMember", this)
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
            co.sweet.confirm("確認刪除?", s, "確認", "取消", function () {
                co.PowerManagement.RemoveMappingUserAndWebsite($(m).data("id")).done((result) => {
                    if (result.success) {
                        co.sweet.success("已刪除授權");
                        if (RoleID == 0) {
                            var members = $("#noGroupMember").data("members");
                            co.Array.Delete(members, $(m).data());
                            $("#noGroupMember").data("members", members);
                            setNoGroup();
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
                /*co.PowerManagement.RemoveMappingUserAndWebsite($(m).data("id")).done((result) => {
                    if (result.success) co.sweet.success("已刪除授權");
                    else co.sweet.error(result.error);
                });*/
                $(m).remove();
                $('.offcanvas').offcanvas('hide');
            });
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
        if ($items.length > 0) {
            $items.each(function () {
                $(this).appendTo("#noGroupMember");
                $(this).removeClass("checked");
            });
        } else co.sweet.error("請選擇要退出群組的使用者");
    });

    $("#left-btn").on("click", function () {
        const $items = $("#noGroupMember .member-item.checked");
        const $Role = $("#RoleList .role-item.roleChecked");
        if ($items.length <= 0) co.sweet.error("加入失敗","請選擇要加入角色的使用者");
        else if ($Role.length <= 0) co.sweet.error("加入失敗", "請選擇或新增使用者要加入的角色");
        else {
            $items.each(function () {
                $Role.data("members").push($(this).data());
                $(this).appendTo("#roleMember");
                $(this).removeClass("checked");
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
                        data.members.push({ id: obj.Id, name: obj.Name });
                        $("#noGroupMember").data("members", data.members);
                        setNoGroup();
                    }
                } else co.sweet.error("新增失敗",result.error);
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
                    addRole({ id: obj.Id, name: obj.Name, members:[]});
                } else co.sweet.error("新增失敗", result.error);
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
    $("#addUser").on("click", () => {
        RoleID = 0;
    });
}