var $btn_prod_save, $prod, ProdList_dxData
var prod_list = [];
var prod_changedBySelectBox, prod_clearSelectionButton;
var prod_check_list = [], prod_text

$.fn.extend({
    prodListModalInit: function () {
        const $self = $(this);
        const $ProdList = $("#devProdListModalGrid");
        const myModal = document.getElementById('ProdModal');
        const $btnProdSave = $(myModal).find(".btn_prod_save");
        const prodModal = new bootstrap.Modal(myModal);
        $(this).data("prodOption", $self);
        function getprodListDataGridInstance() {
            var _dfr = $.Deferred();
            const check = function () {
                if ($ProdList.hasClass("isReady")) {
                    clearTimeout(timer)
                    _dfr.resolve($ProdList.dxDataGrid("instance"));
                } else timer = setTimeout(check, 100);
            }
            let timer = setTimeout(check, 100);
            return _dfr.promise();
        }
        $self.ProdDataClear = function () {
            $self.data({
                prodList: [],
                prodCheckList: [],
                prodText: ""
            });
            $self.removeClass("multiple");
            $self.val("");
            getprodListDataGridInstance().done(function (result) {
                result.clearSelection()
            });
        }
        $self.ProdDataSet = function (datas) {
            var text = ""
            if (datas != null && datas.length > 0) {
                var temp_list = [];
                $self.data({
                    "prodList": [],
                    "prodCheckList": []
                });
                datas.forEach(function (data) {
                    var obj = {};
                    obj["Id"] = data.id;
                    obj["FK_ProdId"] = data.fK_ProdId;
                    obj["IsDeleted"] = false;
                    temp_list.push(data.fK_ProdId);
                    text = text == "" ? data.prod_Name : text + "、" + data.prod_Name;
                    $self.data("prodList").push(obj)
                    $self.data("prodCheckList").push(data.id);
                });
                getprodListDataGridInstance().done(function (result) {
                    result.selectRows(temp_list);
                });
            }
            $self.data("prodText", text);
            $self.val(text == "" ? "無" : text);
        }
        $self.on("click", function () {
            $(myModal).data("target", $self);
        });
        if (!!!$(myModal).data("isSet")) {
            $(myModal).data("isSet", true)
            myModal.addEventListener("hidden.bs.modal", function () {
                const $self = $(myModal).data("target");
                var temp_list = [];
                $self.data("prodList").forEach(function (item) {
                    if (!item.IsDeleted) {
                        temp_list.push(item.fK_ProdId);
                    }
                })
                getprodListDataGridInstance().done(function (result) {
                    result.selectRows(temp_list);
                });
            });
        }
        $btnProdSave.off("click").on("click", function () {
            const $self = $(myModal).data("target");
            if ($self.data("prodCheckList").length > 0) {
                $self.data("prodList").forEach(function (item) {
                    var index = $self.data("prodCheckList").indexOf(item.fK_ProdId);
                    if (index > -1) {
                        item.IsDeleted = false;
                    } else {
                        item.IsDeleted = true;
                    }
                })
                if ($self.data("prodCheckList").length > 0) {
                    $self.data("prodCheckList").forEach(function (item) {
                        if (!!!$self.data("prodList").find((element) => element.fK_ProdId === item)) {
                            var obj = {};
                            obj["Id"] = 0;
                            obj["fK_ProdId"] = item;
                            obj["IsDeleted"] = false;
                            $self.data("prodList").push(obj);
                        }
                    })
                }
            } else {
                $self.data("prodList").forEach(function (item) {
                    item.IsDeleted = true;
                })
            }
            $self.val($self.data("prodText"));
            prodModal.hide();
        })
        return $self;
    }
});
function prodListModalInit() {
    prodListModalElementInit();

    prodModal = new bootstrap.Modal(document.getElementById('ProdModal'))
    $("#prodModal").on("hidden.bs.modal", function () {
        var temp_list = [];
        prod_list.forEach(function (item) {
            if (!item.IsDeleted) {
                temp_list.push(item.fK_ProdId);
            }
        })
        getprodListDataGridInstance().selectRows(temp_list);
    })
    console.log($btn_prod_save);
    $btn_prod_save.on("click", function () {
        console.log(prod_check_list);
        if (prod_check_list.length > 0) {
            var new_list = prod_check_list.slice();
            prod_list.forEach(function (item) {
                var index = new_list.indexOf(item.fK_ProdId)
                if (index > -1) {
                    item.IsDeleted = false;
                    new_list.splice(index, 1)
                } else {
                    item.IsDeleted = true;
                }
            })
            if (new_list.length > 0) {
                new_list.forEach(function (item) {
                    var obj = {};
                    obj["Id"] = 0;
                    obj["fK_ProdId"] = item;
                    obj["IsDeleted"] = false;
                    prod_list.push(obj);
                })
            }
        } else {
            prod_list.forEach(function (item) {
                item.IsDeleted = true;
            })
        }
        $prod.val(prod_text);
        $prod.val(prod_text);
        const textarea = $prod[0];
        if (textarea.scrollHeight > textarea.offsetHeight) {
            $prod.addClass("multiple");
        } else {
            $prod.removeClass("multiple");
        }
        prodModal.hide();
        prodContentRefresh();
    })
}
function prodListModalElementInit() {
    $prod = $("#InputProd");
    $btn_prod_save = $(".btn_prod_save");
    console.log("init");
}
function getprodListDataGridInstance() {
    return $("#devProdListModalGrid").dxDataGrid("instance");
}
function prodList_ClearBtnInit(e) {
    prod_clearSelectionButton = e.component;
}
function prodList_ClearBtnClick() {
    if (prod_list.length > 0) {
        prod_list.forEach(function (item) {
            item.IsDeleted = true;
        })
    }
    getprodListDataGridInstance().clearSelection();
}
function prodList_SelectChange(selectedItems) {
    const data = selectedItems.selectedRowsData;
    const $self = $("#prodModal").data("target");
    prod_check_list = [];
    if (data.length > 0) {
        prod_text = data.map((value) => `${value.Title}`).join("、");

        data.forEach(function (item) {
            prod_check_list.push(item.Id);
        })
    } else {
        prod_text = "無";
    }
    prod_changedBySelectBox = false;
    prod_clearSelectionButton.option('disabled', !data.length);
    if (!!$self) {
        $self.data({
            prodCheckList: prod_check_list,
            prodText: prod_text
        });
    }
}
function ProdDataClear() {
    prod_list = [];
    prod_check_list = [];
    prod_text = "";
    $prod.val("")
    getprodListDataGridInstance().clearSelection();
}
function ProdDataSet(datas) {
    var text = ""
    if (datas != null && datas.length > 0) {
        var temp_list = [];
        datas.forEach(function (data) {
            var obj = {};
            obj["Id"] = data.id;
            obj["fK_ProdId"] = data.fK_ProdId;
            obj["IsDeleted"] = false;
            temp_list.push(data.fK_ProdId);
            text = text == "" ? data.prod_Name : text + "、" + data.prod_Name;
            prod_list.push(obj)
        })
        getprodListDataGridInstance().selectRows(temp_list);
    }
    $prod.val(text == "" ? "無" : text);
    const textarea = $prod[0];
    setTimeout(function () {
        $prod.removeClass("multiple");
        if (textarea.scrollHeight > textarea.offsetHeight) {
            $prod.addClass("multiple");
        }
    }, 300);
}
function getSelectSort() {
    if (prod_list.length > 0) {
        var temp_list = prod_list.filter(e => e.IsDeleted == false).slice();
        var prod_list_str = temp_list.map(item => item.fK_ProdId).join(',');
        return prod_list_str;
    }
}
function ProdInitSet(datas) {
    prod_check_list = [];
    if (datas.length > 0) {
        prod_text = datas.map((value) => `${value.title}`).join("、");
        datas.forEach(function (item) {
            prod_check_list.push(item.id)
        })
    } else {
        prod_text = "無";
    }

    prod_changedBySelectBox = false;
    prod_clearSelectionButton.option('disabled', !datas.length);
    $btn_prod_save.trigger("click");
}
function prodContentReady(e) {
    $(e.element).addClass("isReady");
    ProdList_dxData = $("#devProdListModalGrid").dxDataGrid("instance");
}
function prodContentRefresh() {
    ProdList_dxData.refresh();
}