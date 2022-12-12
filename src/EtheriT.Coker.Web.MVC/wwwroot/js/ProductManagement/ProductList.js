var $btn_display, $name, $name_count, $introduction, $introduction_count, $illustrate, $illustrate_count, $marks, $tag, $price, $number, $min_number, $date, $picker, $permanent
var startDate, endDate, keyId, disp_opt = true
var product_list

function PageReady() {
    co.Product = {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/Product/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/Product/GetOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        Delete: function (id) {
            return $.ajax({
                url: "/api/Product/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        }
    };

    ElementInit();

    $picker = $("#InputDate");

    co.Picker.Init($picker);

    $picker.on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('YYYY/M/DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY/M/DD HH:mm'));
        startDate = picker.startDate.format("");
        endDate = picker.endDate.format("");
    });

    const forms = $('#ProductForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
                        AddUp(disp_opt, "已成功發布", "發布發生未知錯誤");
                    });
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        Coker.sweet.confirm("返回商品列表", "資料將不被保存", "確定", "取消", function () {
            history.back();
        });
    })
    $(".btn_add").on("click", function () {
        FormDataClear();
        window.location.hash = 0;
        HashDataEdit();
    });
    $(".btn_save").on("click", function () {
        disp_opt = false;
        $btn_display.children("span").text("visibility_off");
        AddUp(false, "已存為草稿", "儲存草稿發生未知錯誤");
    });
    $(".btn_input_pic").on("click", function (event) {
        event.preventDefault();
        $(".input_pic").click();
    })
    $btn_display.on("click", function () {
        if (disp_opt) {
            $btn_display.children("span").text("visibility_off");
            disp_opt = !disp_opt;
        } else {
            $btn_display.children("span").text("visibility");
            disp_opt = !disp_opt;
        }
    })
    $(".btn_expand").on("click", function () {
        var $self = $(this);
        console.log($self)
        if ($self.children("span").text() == "expand_more") {
            $self.children("span").text("expand_less")
        } else {
            $self.children("span").text("expand_more")
        }
    })

    $name.on('keyup', function () {
        $name_count.text($name.val().length);
    });
    $introduction.on('keyup', function () {
        $introduction_count.text($introduction.val().length);
    });
    $illustrate.on('keyup', function () {
        $illustrate_count.text($illustrate.val().length);
    });
    $("input[type='number']").on("input", function () {
        $(this).val($(this).val() < 0 ? 0 : $(this).val())
    });

    $permanent.on("click", function () {
        if ($permanent.is(":checked")) {
            $date.val('');
            $date.attr("disabled", "disabled");
            startDate = null;
            endDate = null;
        } else {
            $date.removeAttr("disabled");
        }
    })

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
}

function ElementInit() {
    $btn_display = $("#Btn_Display");
    $name = $("#InputName");
    $name_count = $("#ProductForm > .name .name_count");
    $introduction = $("#InputIntroduction");
    $introduction_count = $("#ProductForm > .introduction .introduction_count");
    $illustrate = $("#InputIllustrate");
    $illustrate_count = $("#ProductForm > .illustrate .illustrate_count");
    $marks = $("#InputMarks");
    $tag = $("#InputTag");
    $price = $("#InputPrice");
    $number = $("#InputNumber");
    $min_number = $("#InputMinNumber");
    $date = $("#InputDate");
    $permanent = $("#PermanentCheck");
}

function FormDataClear() {
    keyId = 0;
    $btn_display.children("span").text("visibility");
    disp_opt = true;
    $name.val("");
    $name_count.text(0);
    $introduction.val("");
    $introduction_count.text(0);
    $illustrate.val("");
    $illustrate_count.text(0);
    $marks.val("");
    $tag.val("");
    $price.val("");
    $number.val("");
    $min_number.val("");
    $permanent.prop("checked", false);
    $date.val("");
    $date.removeAttr("disabled");
}

function contentReady(e) {
    product_list = e;
    HashDataEdit();
}

function hashChange(e) {
    if (!!e) {
        HashDataEdit();
        e.preventDefault();
    } else {
        console.log("HashChange錯誤")
    }
}

function HashDataEdit() {
    if (window.location.hash != "") {
        if (window.currentHash != window.location.hash) {
            var hash = window.location.hash.replace("#", "");
            if (parseInt(hash) == 0) {
                FormDataClear();
                MoveToContent();
            } else {
                co.Product.Get(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        console.log(result)
                        FormDataSet(result);
                        MoveToContent();
                    } else {
                        window.location.hash = ""
                    }
                })
            }
        }
    } else {
        BackToList();
    }
}

function editButtonClicked(e) {
    MoveToContent();
    keyId = e.row.key;
    window.location.hash = keyId
}

function FormDataSet(result) {
    FormDataClear();
    startTime = result.startTime;
    endTime = result.endTime;
    keyId = result.id;
    disp_opt = result.disp_Opt;
    $btn_display.children("span").text(result.disp_Opt ? "visibility" : "visibility_off");

    $name.val(result.title);
    $name_count.text($name.val().length);
    $introduction.val(result.introduction);
    $introduction_count.text($introduction.val().length);
    $illustrate.val(result.description);
    $illustrate_count.text($illustrate.val().length);

    $marks.val("");
    $tag.val("");
    $price.val(result.price);
    $number.val(result.stock);
    $min_number.val(result.min_Qty);
    $date = $("#InputDate");
    if (result.permanent) {
        $date.val('');
        $date.attr("disabled", "disabled");
        $permanent.prop("checked", true);
    } else {
        startTime != null && $picker.data('daterangepicker').setStartDate(startTime);
        endTime != null && $picker.data('daterangepicker').setEndDate(endTime);
    }
}

function deleteButtonClicked(e) {
    Coker.sweet.confirm("刪除資料", "刪除後不可返回", "確定刪除", "取消", function () {
        co.Product.Delete(e.row.key);
        e.component.refresh();
    });
}

function AddUp(display, success_text, error_text) {
    co.Product.AddUp({
        Id: keyId,
        FK_WebsiteId: $.cookie('WebSiteId'),
        Title: $name.val(),
        Disp_Opt: display,
        Ser_No: 500,
        Introduction: $introduction.val(),
        Description: $illustrate.val(),
        Price: $price.val(),
        Discount: 0,
        StartTime: startDate,
        EndTime: endDate,
        permanent: $permanent.is(":checked"),
        FK_S1id: 1,
        FK_S2id: 1,
        Stock: $number.val(),
        Min_Qty: $min_number.val(),
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () {
            BackToList();
            product_list.component.refresh();
        }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function MoveToContent() {
    $("#ProductForm").removeClass("was-validated");
    $("#ProductList").addClass("d-none");
    $("#ProductContent").removeClass("d-none");
}

function BackToList() {
    $("#ProductList").removeClass("d-none");
    $("#ProductContent").addClass("d-none");
    window.location.hash = ""
}