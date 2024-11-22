Coker.extend({
    UserHabits: {
        AddUpUserGroup: function (data) {
            return $.ajax({
                url: "/api/UserHabits/AddUpUserGroup",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        DeleteUserGroup: function (id) {
            return $.ajax({
                url: `/api/UserHabits/DeleteUserGroup?id=${id}`,
                type: "DELETE",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header
            });
        },
        GetUserGroupOne: function (id) {
            return $.ajax({
                url: "/api/UserHabits/GetUserGroupOne",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: {id:id},
                dataType: "json"
            });
        }
    }
});