let ContactList, keyId, batchStatusSelectBox;
const lockedContactStatuses = ["已完成", "作廢/忽略"];
function contentReady(e) {
    ContactList = e;
}
function isLockedContactStatus(status) {
    return lockedContactStatuses.includes(status);
}
function contactToolbarPreparing(e) {
    e.toolbarOptions.items.unshift(
        {
            location: "after",
            widget: "dxSelectBox",
            options: {
                width: 180,
                dataSource: getContactStatusOptions(),
                valueExpr: "id",
                displayExpr: "name",
                placeholder: "批次修改狀態",
                showClearButton: true,
                elementAttr: {
                    id: "BatchContactStatus"
                },
                onInitialized: function (args) {
                    batchStatusSelectBox = args.component;
                }
            }
        },
        {
            location: "after",
            widget: "dxButton",
            options: {
                text: "批次儲存",
                icon: "save",
                type: "default",
                stylingMode: "contained",
                onClick: batchSaveContactStatus
            }
        }
    );
}
function onRowPrepared(e) {
    if (e.rowType === "data") {
        const $row = $(e.rowElement);

        switch (e.data.Status) {
            case "未處理":
                $row.addClass("status-pending");
                break;
            case "處理中":
                $row.addClass("status-processing");
                break;
            case "已回覆":
                $row.addClass("status-replied");
                break;
            case "已完成":
                $row.addClass("status-closed");
                break;
            case "作廢/忽略":
                $row.addClass("status-ignored");
                break;
        }

        if (isLockedContactStatus(e.data.Status)) {
            $row.addClass("status-locked");
            $row.attr("title", "此資料已完成或作廢，不可再修改狀態");
        }
    }
}
function contactSelectionChanged(e) {
    const lockedRows = (e.selectedRowsData || [])
        .filter(function (row) {
            return isLockedContactStatus(row.Status);
        });

    if (lockedRows.length === 0) {
        return;
    }

    const lockedKeys = lockedRows.map(function (row) {
        return row.Id;
    });

    e.component.deselectRows(lockedKeys);

    Coker.sweet.error("已完成、作廢/忽略的資料不可再修改狀態");
}
function contactRowClick(e) {
    if (e.rowType !== "data") {
        return;
    }

    if (!e.data || e.key === null || e.key === undefined) {
        return;
    }

    const $target = $(e.event.target);

    // checkbox 欄位、編輯按鈕、連結、button 由原本元件自己處理，不要再觸發 row click 選取
    if ($target.closest(".dx-command-select, .dx-command-edit, .dx-link, .dx-button, button, a").length > 0) {
        return;
    }

    if (isLockedContactStatus(e.data.Status)) {
        Coker.sweet.error("已完成、作廢/忽略的資料不可再修改狀態");
        return;
    }

    const grid = e.component;
    const selectedKeys = grid.getSelectedRowKeys();
    const isSelected = selectedKeys.includes(e.key);

    if (isSelected) {
        grid.deselectRows([e.key]);
    } else {
        grid.selectRows([e.key], true);
    }
}
function editButtonClicked(e) {
    keyId = e.row.key;
    window.location.hash = keyId;
}
function PageReady() {
    const forms = $('#ReplyForm');
    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                var $this = $(event.submitter);
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Reply($this);
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()

    $(".btn_back").on("click", function () {
        location.hash = "";
        /*
        if ($("#Status").val() == "Processed") {
            history.back();
        } else {
            Coker.sweet.confirm("返回上一頁", "資料將不被保存", "確定", "取消", function () {
                // 存草稿
                history.back();
            });
        }*/
    })

    $("textarea#InputReply").on("input", function () {
        const length = $(this).val().length;
        $(".textarea-meta").text(`${length} / 2000`);
    });

    if ("onhashchange" in window) {
        window.onhashchange = hashChange;
    } else {
        setInterval(hashChange, 1000);
    }
    $(window).trigger('hashchange');
}

function hashChange(e) {
    if (!!e) {
        HashDataEdit();
        e.preventDefault();
    } else {
        console.log("HashChange錯誤")
    }
}
function getContactStatusOptions() {
    return $("#Status option")
        .map(function () {
            const value = $(this).val();
            const text = $(this).text();

            if (value === undefined || value === null || value === "") {
                return null;
            }

            const statusId = parseInt(value, 10);

            if (Number.isNaN(statusId)) {
                return null;
            }

            return {
                id: statusId,
                name: text
            };
        })
        .get()
        .filter(function (item) {
            return item !== null;
        });
}
function batchSaveContactStatus() {
    if (!ContactList || !ContactList.component) {
        Coker.sweet.error("列表尚未完成載入");
        return;
    }

    const grid = ContactList.component;
    const selectedRowsData = grid.getSelectedRowsData();
    const selectedKeys = grid.getSelectedRowKeys();

    if (!selectedKeys || selectedKeys.length === 0) {
        Coker.sweet.error("請先勾選要批次修改的資料");
        return;
    }

    const lockedRows = (selectedRowsData || []).filter(function (row) {
        return isLockedContactStatus(row.Status);
    });

    if (lockedRows.length > 0) {
        const lockedKeys = lockedRows.map(function (row) {
            return row.Id;
        });

        grid.deselectRows(lockedKeys);
        Coker.sweet.error("已完成、作廢/忽略的資料不可再修改狀態");
        return;
    }

    const status = batchStatusSelectBox ? batchStatusSelectBox.option("value") : null;
    const statusText = batchStatusSelectBox ? batchStatusSelectBox.option("text") : "";

    if (status === null || status === undefined || status === "") {
        Coker.sweet.error("請選擇要批次修改的狀態");
        return;
    }

    const data = {
        ids: selectedKeys,
        status: status
    };

    Coker.sweet.confirm(
        "批次修改處理狀態",
        `確定要將 ${selectedKeys.length} 筆資料修改為「${statusText}」？`,
        "確定",
        "取消",
        function () {
            Coker.sweet.loading();
            co.Contact.BatchUpdateStatus(data)
                .then(function (result) {
                    Coker.sweet.success(
                        result?.message || "狀態已批次儲存",
                        null,
                        true
                    );

                    grid.clearSelection();
                    grid.refresh();

                    if (batchStatusSelectBox) {
                        batchStatusSelectBox.reset();
                    }
                })
                .catch(function (error) {
                    Coker.sweet.error(error.message || "批次儲存失敗");
                });
        }
    );
}
function HashDataEdit() {
    if (window.location.hash != "") {
        if (window.currentHash != window.location.hash) {
            var hash = window.location.hash.replace("#", "");
            if (parseInt(hash) != 0) {
                co.Contact.GetDataOne(parseInt(hash)).done(function (result) {
                    if (result != null) {
                        keyId = parseInt(hash);
                        FormDataSet(result);
                    } else {
                        window.location.hash = ""
                        keyId = "";
                    }
                })
            }
        }
    } else {
        BackToList();
    }
}

function BackToList() {
    $(".page").removeClass("show");
    $("#ArticleList").addClass("show");
}

function buildTableWithDom(formObj) {
    const table = document.createElement('table');
    table.className = 'table';
    const tbody = document.createElement('tbody');

    Object.entries(formObj).forEach(([key, obj]) => {
        const title = obj?.title ?? "";
        const value = obj?.value ?? "";

        const tr = document.createElement('tr');
        const th = document.createElement('th');
        th.classList.add('title');
        th.textContent = title; // 安全：textContent 不會解析 HTML

        const td = document.createElement('td');

        if (value == null || value === '') {
            td.textContent = '\u00A0'; // non-break space
        } else if (Array.isArray(value)) {
            td.textContent = value.map(x => String(x)).join(', ');
        } else if (typeof value === 'object') {
            // 若是物件，顯示 pretty JSON（也安全）
            const pre = document.createElement('pre');
            pre.style.margin = '0';
            pre.textContent = JSON.stringify(value, null, 2);
            td.appendChild(pre);
        } else {
            // 字串／數字：保留換行顯示（用 textContent + <br> 需拆行）
            if (String(value).includes('\n')) {
                String(value).split(/\r?\n/).forEach((line, i) => {
                    if (i) td.appendChild(document.createElement('br'));
                    td.appendChild(document.createTextNode(line));
                });
            } else {
                td.textContent = String(value);
            }
        }

        tr.appendChild(th);
        tr.appendChild(td);
        tbody.appendChild(tr);
    });

    table.appendChild(tbody);
    return table; // 回傳 DOM 節點，使用者決定放哪裡
}

function FormDataSet(result) {
    if (result.object.fromDate != null) {
        const formObj = JSON.parse(result.object.fromDate);
        result.object.html = buildTableWithDom(formObj)
    }
    co.Form.insertData(result.object, "#ReplyForm");
    $(".page").removeClass("show");
    $("#Form").addClass("show");
    $("#Status").find(`option[value="${result.object.status}"]`).prop("selected", true);

    var status = result.object.status;
    var disableReply = [2, 3, 9].includes(status);
    var disableDone = [3, 9].includes(status);

    if (result.object.replyTime.startsWith("0001-01-01")) {
        $(".reply-zone .textarea-meta").text("0 / 2000");
    } else {
        var date = new Date(result.object.replyTime);
        var formatted = date.toLocaleString("zh-TW", {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
            hour12: true
        });
        $(".reply-zone .textarea-meta").text(formatted);
    }

    $("#InputReply, .reply-zone .btn_send_reply").prop("disabled", disableReply);
    $(".reply-zone .btn_send_reply").text(disableReply ? "已送出，無法修改" : "送出回覆");
    $("#Form .btn_done, #Status").prop("disabled", disableDone);
}

function Reply($btn) {
    Coker.sweet.confirm("直接回覆", "回覆後不可取消", "確定", "取消", function () {
        Coker.sweet.loading();
        const data = co.Form.getJson("ReplyForm");
        if ($btn.hasClass('btn_send_reply')) data.Status = 2;
        co.Contact.Replay(data).done(function (result) {
            if (result.success) {
                Coker.sweet.success("已成功回覆", null, true);
                location.hash = "";
                ContactList.component.refresh();
            } else {
                Coker.sweet.error(result.message);
            }
        }).fail(function (xhr, status, error) {
            Coker.sweet.error(error);
        });
    });
}