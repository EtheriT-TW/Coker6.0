
let bulkQtyBox;
function OnUpdating(e) {
    var data = e.changes.map(e => ({
        Id: e.key,
        StockQuantity: e.data.StockQuantity
    }));
    co.sweet.confirm("確認更新庫存數量？", "更新後庫存數量後將會變更售完狀態。", "確認", "取消", function () {
        co.Stock.BatchSet(data).then(function (res) {
            if (res.success) {
                DevExpress.ui.notify("已更新", "success", 2000);
                e.component.refresh();
            } else {
                DevExpress.ui.notify("更新失敗，請稍後再試", "error", 2000);
            }
        });
    });
}
function onToolbarPreparing(e) {
    const items = e.toolbarOptions.items;
    const revertIndex = items.findIndex(i => i.name === "revertButton");
    const insertIndex = revertIndex >= 0 ? revertIndex + 1 : items.length;

    items.splice(insertIndex, 0, {
        location: "after",
        widget: "dxButton",
        options: {
            icon: "refresh",
            hint: "重新整理",
            stylingMode: "text", // 只顯示 icon
            onClick: function () {
                const grid = e.component;
                if (grid.hasEditData && grid.hasEditData()) {
                    co.sweet.confirm("資料未儲存", "尚有未儲存的變更，是否放棄並重新整理？", "確認", "取消", function () {
                        grid.cancelEditData();
                        grid.refresh();
                    });
                } else {
                    grid.refresh();
                }
            }
        }
    });

    items.push({
        location: "before",
        widget: "dxNumberBox",
        options: {
            placeholder: "批次可銷售量",
            showSpinButtons: true,
            min: 1,
            inputAttr: { 'aria-label': '批次可銷售量' },
            onInitialized: (args) => { bulkQtyBox = args.component; },
            width: 140
        }
    });

    items.push({
        location: "before",
        widget: "dxButton",
        options: {
            icon: "check",
            hint: "將本頁可銷售量套用為輸入數字",
            text: "批次設定",
            onClick: () => applyBulkToCurrentPage(e.component)
        }
    });
    function applyBulkToCurrentPage(grid) {
        const val = Number(bulkQtyBox?.option("value"));
        if (!val || val <= 0) {
            DevExpress.ui.notify("請輸入大於 0 的數字", "error", 2000);
            return;
        }

        const rows = grid.getVisibleRows().filter(r => r.rowType === "data");
        if (rows.length === 0) {
            DevExpress.ui.notify("本頁沒有可套用的資料", "warning", 2000);
            return;
        }
        co.sweet.confirm("批次調整", `將把本頁 ${rows.length} 筆「可銷售量」改為 ${val}，是否繼續？`, "確認", "取消", function () {
            grid.beginUpdate();
            try {
                rows.forEach(r => {
                    const rowIndex = grid.getRowIndexByKey(r.key);
                    grid.cellValue(rowIndex, "StockQuantity", val);
                });
            } finally {
                grid.endUpdate();
            }

            DevExpress.ui.notify(
                `已套用至本頁 ${rows.length} 筆，請按「儲存」完成變更`,
                "success",
                2000
            );
        });
    }
}

