var $btn_tag_save, $tag
var tag_list = []
var $price_modal, priceModal, $techcert_body, techcertModal;
var tag_changedBySelectBox, tag_clearSelectionButton;
var tag_check_list = [], tag_text

$.fn.extend({
    TagListModalInit: function () {
        const $self = $(this);
        const $TagList = $("#TagList");
        const myModal = document.getElementById('TagModal');
        const $btnTagSave = $(myModal).find(".btn_tag_save");
        const tagModal = new bootstrap.Modal(myModal);
        $(this).data("tagOption", $self);
        function getTagListDataGridInstance() {
            var _dfr = $.Deferred();
            const check = function () {
                if ($TagList.hasClass("isReady")) {
                    clearTimeout(timer)
                    _dfr.resolve($TagList.dxDataGrid("instance"));
                } else timer = setTimeout(check, 100);
            }
            let timer = setTimeout(check, 100);
            return _dfr.promise();
        }
        $self.TagDataClear = function () {
            $self.data({
                tagList: [],
                tagCheckList:[],
                tagText:""
            });
            $self.val("");
            getTagListDataGridInstance().done(function (result) {
                result.clearSelection()
            });
        }
        $self.TagDataSet = function(datas) {
            var text = ""
            if (datas != null && datas.length > 0) {
                var temp_list = [];
                $self.data({
                    "tagList": [],
                    "tagCheckList": []
                });
                datas.forEach(function (data) {
                    var obj = {};
                    obj["Id"] = data.id;
                    obj["FK_TId"] = data.fK_TId;
                    obj["IsDeleted"] = false;
                    temp_list.push(data.fK_TId);
                    text = text == "" ? data.tag_Name : text + "、" + data.tag_Name;
                    $self.data("tagList").push(obj)
                    $self.data("tagCheckList").push(data.id);
                });
                getTagListDataGridInstance().done(function (result) {
                    result.selectRows(temp_list);
                });
            }
            $self.data("tagText", text);
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
                $self.data("tagList").forEach(function (item) {
                    if (!item.IsDeleted) {
                        temp_list.push(item.FK_TId);
                    }
                })
                getTagListDataGridInstance().done(function (result) {
                    result.selectRows(temp_list);
                });
            });
        }
        $btnTagSave.off("click").on("click", function () {
            const $self = $(myModal).data("target");
            if ($self.data("tagCheckList").length > 0) {
                $self.data("tagList").forEach(function (item) {
                    var index = $self.data("tagCheckList").indexOf(item.FK_TId);
                    if (index > -1) {
                        item.IsDeleted = false;
                    } else {
                        item.IsDeleted = true;
                    }
                })
                if ($self.data("tagCheckList").length > 0) {
                    
                    $self.data("tagCheckList").forEach(function (item) {
                        if (!!!$self.data("tagList").find((element) => element.FK_TId === item)) {
                            var obj = {};
                            obj["Id"] = 0;
                            obj["FK_TId"] = item;
                            obj["IsDeleted"] = false;
                            $self.data("tagList").push(obj);
                        }
                    })
                }
            } else {
                $self.data("tagList").forEach(function (item) {
                    item.IsDeleted = true;
                })
            }
            $self.val($self.data("tagText"));
            tagModal.hide();
        })
        return $self;
    }
});

function TagListModalInit() {
    TagListModalElementInit();

    tagModal = new bootstrap.Modal(document.getElementById('TagModal'))
    $("#TagModal").on("hidden.bs.modal", function () {
        var temp_list = [];
        tag_list.forEach(function (item) {
            if (!item.IsDeleted) {
                temp_list.push(item.FK_TId);
            }
        })
        getTagListDataGridInstance().selectRows(temp_list);
    })

    $btn_tag_save.on("click", function () {
        if (tag_check_list.length > 0) {
            tag_list.forEach(function (item) {
                var index = tag_check_list.indexOf(item.FK_TId)
                if (index > -1) {
                    item.IsDeleted = false;
                    tag_check_list.splice(index, 1)
                } else {
                    item.IsDeleted = true;
                }
            })
            if (tag_check_list.length > 0) {
                tag_check_list.forEach(function (item) {
                    var obj = {};
                    obj["Id"] = 0;
                    obj["FK_TId"] = item;
                    obj["IsDeleted"] = false;
                    tag_list.push(obj);
                })
            }
        } else {
            tag_list.forEach(function (item) {
                item.IsDeleted = true;
            })
        }
        $tag.val(tag_text);
        tagModal.hide();
    })
}

function TagListModalElementInit() {
    $tag = $("#InputTag");
    $btn_tag_save = $(".btn_tag_save");
}

function getTagListDataGridInstance() {
    return $("#TagList").dxDataGrid("instance");
}

function TagList_ClearBtnInit(e) {
    tag_clearSelectionButton = e.component;
}

function TagList_ClearBtnClick() {
    if (tag_list.length > 0) {
        tag_list.forEach(function (item) {
            item.IsDeleted = true;
        })
    }
    getTagListDataGridInstance().clearSelection();
}

function TagList_SelectChange(selectedItems) {
    const data = selectedItems.selectedRowsData;
    const $self = $("#TagModal").data("target");
    tag_check_list = [];
    if (data.length > 0) {
        tag_text = data.map((value) => `${value.Title}`).join("、");

        data.forEach(function (item) {
            tag_check_list.push(item.Id);
        })
    } else {
        tag_text = "無";
    }
    tag_changedBySelectBox = false;
    tag_clearSelectionButton.option('disabled', !data.length);
    if (!!$self) {
        $self.data({
            tagCheckList: tag_check_list,
            tagText: tag_text
        });
    }
}

function TagDataClear() {
    tag_list = [];
    tag_check_list = [];
    tag_text = "";
    $tag.val("")
    getTagListDataGridInstance().clearSelection();
}

function TagDataSet(datas) {
    var text = ""
    if (datas != null && datas.length > 0) {
        var temp_list = [];
        datas.forEach(function (data) {
            var obj = {};
            obj["Id"] = data.id;
            obj["FK_TId"] = data.fK_TId;
            obj["IsDeleted"] = false;
            temp_list.push(data.fK_TId);
            text = text == "" ? data.tag_Name : text + "、" + data.tag_Name;
            tag_list.push(obj)
        })
        getTagListDataGridInstance().selectRows(temp_list);
    }
    $tag.val(text == "" ? "無" : text);
}
function TagInitSet(datas) {
    tag_check_list = [];
    if (datas.length > 0) {
        tag_text = datas.map((value) => `${value.title}`).join("、");
        datas.forEach(function (item) {
            tag_check_list.push(item.id)
        })
    } else {
        tag_text = "無";
    }

    tag_changedBySelectBox = false;
    tag_clearSelectionButton.option('disabled', !datas.length);
    $btn_tag_save.trigger("click");
}

function tagContentReady(e) {
    $(e.element).addClass("isReady");
}