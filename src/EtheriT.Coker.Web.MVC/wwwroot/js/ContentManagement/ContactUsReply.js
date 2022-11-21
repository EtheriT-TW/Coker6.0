
function PageReady() {
    const forms = $('#ReplyForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Reply();
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        if ($("#Status").val() == "Processed") {
            history.back();
        } else {
            Coker.sweet.confirm("返回上一頁", "資料將不被保存", "確定", "取消", function () {
                // 存草稿
                history.back();
            });
        }
    })

}

function Reply() {
    Coker.sweet.confirm("直接回覆", "回覆後不可取消", "確定", "取消", function () {
        Coker.sweet.success("已成功回覆", null, true);
        $("#Status").val("Processed");
        $("#InputReply").attr("disabled", "disabled");
    });
    $(".btn_done").attr("disabled", "disabled");
}