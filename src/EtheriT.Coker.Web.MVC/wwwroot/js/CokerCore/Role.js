Coker.extend({
    Role: {
        Delete: function (id) {
            return $.ajax({
                url: `/api/Member/RoleDelete?id=${id}`,
                type: "DELETE",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header
            });
        }
    }
});