function PageReady() {
    $(".powerctrl .member-item").on("click", function () {
        if ($(this).hasClass("checked")) $(this).removeClass("checked");
        else $(this).addClass("checked");
    });

    $(".powerctrl .role-item").on("click", function () {
        var clickedRole = $(this);

        $(".powerctrl .role-item").each(function () {
            if ($(this).is(clickedRole)) {
                $(this).addClass("roleChecked");
            } else {
                $(this).removeClass("roleChecked");
            }
        });
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
        var member = $(this).closest('.member-item , .role-item');
        member.remove();
    });

}