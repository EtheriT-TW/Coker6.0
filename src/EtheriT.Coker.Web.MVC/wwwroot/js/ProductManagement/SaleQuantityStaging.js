function OnUpdating(e) {
    var data = e.changes.map(e => ({
        Id: e.key,
        StockQuantity: e.data.StockQuantity
    }));
    co.Stock.BatchSet(data).then(function (res) {
        if (res.success) {
            DevExpress.ui.notify("已更新", "success", 2000);
            e.component.refresh();
        } else {
            DevExpress.ui.notify("更新失敗，請稍後再試", "error", 2000);
        }
    });
}