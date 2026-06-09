Coker.extend({
    Marketing: {
        AddUp: function (data) {
            return _c.request.post("/api/Marketing/AddUp", data);
        },

        Get: function (id) {
            return _c.request.get("/api/Marketing/GetOne", { id: id });
        },

        Delete: function (id) {
            return _c.request.delete("/api/Marketing/Delete", { id: id });
        },

        GetOptions: function () {
            return _c.request.get("/api/Marketing/GetOptions");
        }
    }
});