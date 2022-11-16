
function PageReady() {
    $(".btn_done").on('click', function () {
        Coker.sweet.draft_or_publish("直接發布", Draft, Publish);
    })

    $input_number = $("#CreatePost > .card-body > .input_text > div > .input_number");
    $InputText = $("#InputText");
    $InputText.on('keyup', function () {
        $input_number.text($InputText.val().length);
    });

    $input_link = $("#CreatePost > .card-body > .input_link > .input_link");
    $checkbox_link = $("#CreatePost > .card-body > .input_link > .checkbox_link");
    $input_link.on('keyup', function () {
        if ($input_link.val().length > 0) {
            $checkbox_link.attr('checked', 'checked');
        } else {
            $checkbox_link.removeAttr("checked");
        }
    });

    var picker = new Lightpick({
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
    Coker.sweet.success("已存成草稿", null, true);
}

function Publish() {
    Coker.sweet.success("已成功發布", null, true);
}