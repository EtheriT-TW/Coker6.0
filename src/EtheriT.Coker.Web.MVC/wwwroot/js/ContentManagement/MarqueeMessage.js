var $space, $title, $link, $target
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
        }
    };

    $CardBody = $("#CreatePost > .card-body")
    $space = $CardBody.children(".select_placement").children("div").children("select");
    $title = $CardBody.children(".input_text").children("textarea");
    $link = $CardBody.children(".input_link").children(".input_link");
    $target = $CardBody.children(".input_link").children(".checkbox_link");

    const forms = $('#PostForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.draft_or_publish("直接發布", Draft, Publish);
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()

    $input_number = $CardBody.children(".input_text").children("div").children(".input_number");
    $title.on('keyup', function () {
        $input_number.text($title.val().length);
    });

    $link.on('keyup', function () {
        if ($link.val().length > 0) {
            $target.attr('checked', 'checked');
        } else {
            $target.removeAttr("checked");
        }
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

function Draft() {
    co.Marquees.Add({
        WebsiteId: 1,
        title: $title.val(),
        disp_opt: false,
        ser_no: 1,
        link: "https://" + $link.val(),
        target: $target.is(":checked"),
        StartTime: picker.getStartDate().format(),
        EndTime: picker.getEndDate().format()
    }).done(function () {
        Coker.sweet.success("已存為草稿", null, true);
        setTimeout(function () { location.reload() }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", "儲存草稿發生未知錯誤", null, true);
    });
}

function Publish() {
    co.Marquees.Add({
        WebsiteId: 1,
        title: $title.val(),
        disp_opt: true,
        ser_no: 1,
        link: "https://" + $link.val(),
        target: $target.is(":checked"),
        StartTime: picker.getStartDate().format(),
        EndTime: picker.getEndDate().format()
    }).done(function () {
        Coker.sweet.success("已成功發布", null, true);
        setTimeout(function () { location.reload() }, 1000);
    }).fail(function () {
        Coker.sweet.error("錯誤", "發布發生未知錯誤", null, true);
    });
}