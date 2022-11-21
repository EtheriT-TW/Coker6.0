var $space, $title, $link, $target, $disp_opt
var Dataid
var picker

function PageReady() {
    co.Marquees = {
        Add: function (data) {
            return $.ajax({
                url: "/api/Marquee/Add",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/Marquee/Get/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        Update: function (data) {
            return $.ajax({
                url: "/api/Marquee/Update",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    };

    var url = location.href;

    if (url.indexOf('?id=') != -1) {
        Dataid = (url.split('?id='))[1];
    }

    $CardBody = $("#CreatePost > .card-body")
    $space = $CardBody.children(".select_placement").children("div").children("select");
    $disp_opt = $CardBody.children(".select_placement").children("button").children("span");
    $title = $CardBody.children(".input_text").children("textarea");
    $link = $CardBody.children(".input_link").children(".input_link");
    $target = $CardBody.children(".input_link").children(".checkbox_link").children("input");

    if (Dataid > 0) {
        co.Marquees.Get(Dataid).done(function (result) {
            $title.text(result.title);
            $disp_opt.text(result.disp_opt ? "visibility" : "visibility_off");
            $link.val(result.link);
        });
    }

    const forms = $('#PostForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.confirm("即將發布", "發布後將直接顯示於安排的位置", "發布", "取消", function () {
                        if (Dataid > 0) {
                            Update($disp_opt.text() == "visibility" ? true : false, "已成功發布", "發布發生未知錯誤");
                        } else {
                            Add($disp_opt.text() == "visibility" ? true : false, "已成功發布", "發布發生未知錯誤");
                        }
                    });
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()

    $(".btn_save").on("click", function () {
        $disp_opt.text("visibility_off");
        if (Dataid > 0) {
            Update(false, "已存為草稿", "儲存草稿發生未知錯誤");
        } else {
            Add(false, "已存為草稿", "儲存草稿發生未知錯誤");
        }
    })

    $disp_opt.on("click", function () {
        if ($disp_opt.text() == "visibility") {
            $disp_opt.text("visibility_off");
        } else if ($disp_opt.text() == "visibility_off") {
            $disp_opt.text("visibility");
        }
    })

    $input_number = $CardBody.children(".input_text").children("div").children(".input_number");
    $title.on('keyup', function () {
        $input_number.text($title.val().length);
    });

    picker = new Lightpick({
        field: document.getElementById('Datepicker'),
        singleDate: false,
        selectForward: true,
        minDate: moment(),
        format: 'YYYY/MM/DD',
        separator: ' ~ ',
        onSelect: function (start, end) {
            var str = '';
            str += start ? start.format('YYYY MMMM Do') + ' to ' : '';
            str += end ? end.format('YYYY MMMM Do') : '　...　';
        }
    });
}

function Add(display, success_text, error_text) {
    co.Marquees.Add({
        WebsiteId: 1,
        title: $title.val(),
        disp_opt: display,
        ser_no: 1,
        link: $link.val(),
        target: $target.is(":checked"),
        StartTime: picker.getStartDate().format(),
        EndTime: picker.getEndDate().format()
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () { history.back() }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}

function Update(display, success_text, error_text) {
    co.Marquees.Update({
        id: Dataid,
        WebsiteId: 1,
        title: $title.val(),
        disp_opt: display,
        ser_no: 1,
        link: $link.val(),
        target: $target.is(":checked"),
        StartTime: picker.getStartDate().format(),
        EndTime: picker.getEndDate().format()
    }).done(function () {
        Coker.sweet.success(success_text, null, true);
        setTimeout(function () { history.back() }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", error_text, null, true);
    });
}