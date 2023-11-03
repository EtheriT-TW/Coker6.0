function PageReady() {
    const data = [
        {
            Id: 1,
            Name: "總管理者",
            menubar: [{
                Id: 1,
                Name: "易碩網際科技"
            }, {
                Id: 2,
                Name: "黃小姐"
            }, {
                Id: 3,
                Name: "章小姐"
            }]
        },
        {
            Id: 2,
            Name: "網站維護者",
            menubar: [{
                Id: 4,
                Name: "張小姐"
            }]
        },
        {
            Id: 0,
            Name: "無群組",
            menubar: [{
                Id: 5,
                Name: "黃先生"
            }]
        }
    ];
    const html = $("#MemberData").html();
    const roleHtml = $("#RoleData").html();
    const powerHtml = $("#PowerData").html();
    const init = function () {
        $(data).each(function (index, element) {
            if (element.Id != 0) addRole(element);
            else setNoGroup(element);
        });
        $(".powerctrl .role-item").first().trigger("click");
        loadMenus();
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
        $(data.menubar).each((i,e) => {
            addItem("#roleMember", e);
        });
        console.log($self.data())
    }
    const setNoGroup = function (e) {
        $(e.menubar).each(function () {
            addItem("#noGroupMember", this)
        })
    }
    const addItem = function (id, element) {
        const $item = $(html).data(element);
        $item.find(".title").text(element.Name);
        $item.appendTo(id);
    }
    const addRole = function (element) {
        const $item = $(roleHtml).data(element);
        $item.find(".title").text(element.Name);
        $item.on("click", setMember);
        $item.appendTo("#RoleList");
    }
    init();
    $(".powerctrl .member-item").on("click", function () {
        if ($(this).hasClass("checked")) $(this).removeClass("checked");
        else $(this).addClass("checked");
    });

    $("#right-btn").on("click", function () {
        $("#roleMember .member-item").each(function () {
            if ($(this).hasClass("checked")) {
                $(this).appendTo("#noGroupMember");
                $(this).removeClass("checked");
            }
        });
    });

    $("#left-btn").on("click", function () {
        $("#noGroupMember .member-item.checked").each(function () {
            if ($(this).hasClass("checked")) {
                $(this).appendTo("#roleMember");
                $(this).removeClass("checked");
            }
        });
    });

    $(".fa-trash-alt").on("click", function () {
        const m = $(this).closest(".item");
        const s = $(m).hasClass("member-item") ? `是否確認刪除使用者<span class="ConfirmDanger">${$(m).text()}</span>` : `是否確認刪除角色<span class="rad">${$(m).text()}</span>`;
        co.sweet.confirm("確認刪除?", s, "確認", "取消", function () {
            $(m).remove();
        });
    });

}