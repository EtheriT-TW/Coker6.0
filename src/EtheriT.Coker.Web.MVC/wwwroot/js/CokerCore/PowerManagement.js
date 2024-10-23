Coker.extend({
    PowerManagement: {
        GetAll: function () {
            return $.ajax({
                url: "/api/PowerManagement/AllMenus/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header
            });
        },
        GetPermission: function () {
            return $.ajax({
                url: "/api/PowerManagement/GetPermission/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header
            });
        },
        GetPagePermission: function (data) {
            return $.ajax({
                url: "/api/PowerManagement/GetPagePermission/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data)
            });
        },
        SavePagePermission: function (data) {
            return $.ajax({
                url: "/api/PowerManagement/SavePagePermission/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data)
            });
        },
        getAllUsers: function () {
            return $.ajax({
                url: "/api/PowerManagement/AllUsers/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header
            });
        },
        GetUser: (id) => {
            return $.ajax({
                url: "/api/PowerManagement/GetUser/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id })
            });
        },
        AddUser: function (data) {
            return $.ajax({
                url: "/api/PowerManagement/AddUser/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data)
            });
        },
        RemoveMappingUserAndWebsite: (id) => {
            return $.ajax({
                url: "/api/PowerManagement/RemoveMappingUserAndWebsite/",
                type: "DELETE",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: id }),
            });
        },
        MappingUserAndWebsite: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/MappingUserAndWebsite",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        AddRole: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/AddRole",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }, AddUserToRole: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/AddUserToRole",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }, RemoveUserToRole: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/RemoveUserToRole",
                type: "DELETE",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        EditRole: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/EditRole",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }, DeleteRole: (id) => {
            return $.ajax({
                url: "/api/PowerManagement/DeleteRole/",
                type: "DELETE",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: id }),
            });
        },
        GetPermissions: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/GetPermissions",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        SavePermissions: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/SavePermissions",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    }
});