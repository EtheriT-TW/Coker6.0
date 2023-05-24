var $btn_techcert_save, $techcert
var techcert_list = []
var $price_modal, priceModal, $techcert_body, techcertModal;
var techcert_changedBySelectBox, techcert_clearSelectionButton;
var techcert_check_list = [], techcert_text

function TechCertListModalInit() {
    TechCertListModalElementInit();

    techcertModal = new bootstrap.Modal(document.getElementById('TechCertModal'))
    $("#TechCertModal").on("hidden.bs.modal", function () {
        var temp_list = [];
        techcert_list.forEach(function (item) {
            if (!item.IsDeleted) {
                temp_list.push(item.FK_TCId);
            }
        })
        getTechCertListDataGridInstance().selectRows(temp_list);
    })

    $btn_techcert_save.on("click", function () {
        if (techcert_check_list.length > 0) {
            techcert_list.forEach(function (item) {
                var index = techcert_check_list.indexOf(item.FK_TCId)
                if (index > -1) {
                    item.IsDeleted = false;
                    techcert_check_list.splice(index, 1)
                } else {
                    item.IsDeleted = true;
                }
            })
            if (techcert_check_list.length > 0) {
                techcert_check_list.forEach(function (item) {
                    var obj = {};
                    obj["Id"] = 0;
                    obj["FK_TCId"] = item;
                    obj["IsDeleted"] = false;
                    techcert_list.push(obj);
                })
            }
        } else {
            techcert_list.forEach(function (item) {
                item.IsDeleted = true;
            })
        }
        $techcert.val(techcert_text);
        techcertModal.hide();
    })
}

function TechCertListModalElementInit() {
    $techcert = $("#InputTechCert");
    $btn_techcert_save = $(".btn_techcert_save");
}

function getTechCertListDataGridInstance() {
    return $("#TechCertList").dxDataGrid("instance");
}

function TechCertList_ClearBtnInit(e) {
    techcert_clearSelectionButton = e.component;
}

function TechCertList_ClearBtnClick() {
    if (techcert_list.length > 0) {
        techcert_list.forEach(function (item) {
            item.IsDeleted = true;
        })
    }
    getTechCertListDataGridInstance().clearSelection();
}

function TechCertList_SelectChange(selectedItems) {
    var data = selectedItems.selectedRowsData;

    techcert_check_list = [];
    if (data.length > 0) {
        techcert_text = data.map((value) => `${value.Title}`).join("、");

        data.forEach(function (item) {
            techcert_check_list.push(item.Id)
        })
    } else {
        techcert_text = "無";
    }

    techcert_changedBySelectBox = false;
    techcert_clearSelectionButton.option('disabled', !data.length);
}

function TechCertDataClear() {
    techcert_list = [];
    techcert_check_list = [];
    techcert_text = "";
    $techcert.val("")
    getTechCertListDataGridInstance().clearSelection();
}

function TechCertDataSet(datas) {
    var text = ""
    if (datas != null && datas.length > 0) {
        var temp_list = [];
        datas.forEach(function (data) {
            var obj = {};
            obj["Id"] = data.id;
            obj["FK_TCId"] = data.fK_TCId;
            obj["IsDeleted"] = false;
            temp_list.push(data.fK_TCId);
            text = text == "" ? data.techCert_Name : text + "、" + data.techCert_Name;
            techcert_list.push(obj)
        })
        getTechCertListDataGridInstance().selectRows(temp_list);
    }
    $techcert.val(text == "" ? "無" : text);
}