Coker.extend({
    PowerManagement: {
        GetAll: function () {
            return $.ajax({
                url: "/api/PowerManagement/AllMenus/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetPermission: function () {
            return $.ajax({
                url: "/api/PowerManagement/GetPermission/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        getAllUsers: function () {
            return $.ajax({
                url: "/api/PowerManagement/AllUsers/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetUser: (id) => {
            return $.ajax({
                url: "/api/PowerManagement/GetUser/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        AddUser: function (data) {
            return $.ajax({
                url: "/api/PowerManagement/AddUser/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        AddRole: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/AddRole",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }, AddUserToRole: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/AddUserToRole",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }, RemoveUserToRole: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/RemoveUserToRole",
                type: "DELETE",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        EditRole: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/EditRole",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
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
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        SavePermissions: (data) => {
            return $.ajax({
                url: "/api/PowerManagement/SavePermissions",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }
    }
});