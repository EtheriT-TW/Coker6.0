function AnchorPointInit() {
    // 配置觀察選項（需要觀察哪些變動）
    const config = { childList: true, subtree: true };
    const $wrapper = $(`[data-gjs-type="wrapper"]`);
    if ($wrapper.length > 0 && typeof ($wrapper.data("observer")) == "undefined") {
        // 當觀察到變動時執行的回呼函數
        const callback = function (mutationsList, observer) {
            observer.disconnect();
            for (const mutation of mutationsList) {
                if (mutation.type === 'childList') {
                    //子節點被添加或移除了
                    $(".anchor_directory").removeClass("selectList");
                    AnchorPointInit();
                }
            }
            observer.observe($wrapper[0], config);
        };
        // 創建一個觀察器實例並將其與回呼函數關聯
        const observer = new MutationObserver(callback);

        // 開始觀察目標節點的指定變動
        observer.observe($wrapper[0], config);
        $wrapper.data("observer", observer);
    }
    $(".anchor_directory:not(.selectList)").each(function () {
        var $directory = $(this);
        $directory.children("ul").empty();
        $(".anchor_title").each(function () {
            var $self = $(this);
            var text = $self.text().indexOf('\n') > -1 ? $self.text().replace(/\n/g,"") : $self.text();
            $directory.children("ul").append(`<li class="fs-5"><a class="text-black text-decoration-none" href="#${$self.attr("id")}"><div class="p-2 px-4">${text}</div></a></li>`);
        });
        $directory.find('a[href^="#"]').on("click", function (event) {
            var id = $(this).attr("href");
            var target = $(id).offset().top - 50;
            if ($('nav').hasClass('position-fixed')) {
                target -= $('nav').outerHeight()-10;
            }
            $('html, body').animate({ scrollTop: target }, 0);
            event.preventDefault();
        });
        $directory.addClass("selectList");
    });
    $(window).off("resize.selectList").on("resize.selectList", (event) => {
        $(".selectList").each((i, e) => {
            const $self = $(e);
            if ($self.find("select").length == 0 || $self.find("select").children().length == 1) {
                const $list = $self.find("a");
                const $select = $("<select>");
                $self.find("select").remove();
                if ($self.find('a[href^="#"]').length > 0) {
                    $select.append($("<option>").val("").text("請選擇將前往的標籤"))
                } else {
                    const text = $self.data("option-text") || "請選擇前往的頁面";
                    $select.append($("<option>").val("").text(text));
                }
                $list.each((j, a) => {
                    $select.append(
                        $("<option>").data("trigger", a).val(a.href).text($(a).text())
                    );
                });
                $select.on("change", function () {
                    const $o = $select.children(":selected");
                    if (/^#/.test($o.val())) $o.data("trigger").trigger("click");
                    else location.href = $o.val();

                    const anchor = $o.val().split('#')[1];
                    const $targetElement = $('#' + anchor);
                    if ($('nav').hasClass('position-fixed')) {
                        $('html, body').animate({
                            scrollTop: $targetElement.offset().top - $('nav').outerHeight() - 10
                        });
                    }
                });
                $self.append($select);
            }
            if ($(window).width() > 576) {
                $self.children().first().removeClass("d-none");
                $self.children("select").addClass("d-none");
            } else {
                $self.children().first().addClass("d-none");
                $self.children("select").removeClass("d-none");
            }
        });
    });
    $(window).trigger("resize.selectList");
}