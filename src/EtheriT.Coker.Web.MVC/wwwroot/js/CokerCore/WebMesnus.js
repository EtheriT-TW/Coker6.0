Coker.extend({
    WebMesnus: {
        getAll: function () {
            return $.ajax({
                url: "/api/WebMenu/GetAll",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        getAllList: function () {
            return $.ajax({
                url: "/api/WebMenu/GetAllList/",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        createOrEdit: function (data) {
            return $.ajax({
                url: "/api/WebMenu/CreateOrEdit",
                type: "Post",
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
        getConten: function (id) {
            return $.ajax({
                url: "/api/WebMenu/GetConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        saveConten: function (data) {
            return $.ajax({
                url: "/api/WebMenu/saveConten",
                type: "Post",
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
        importConten: function (data) {
            return $.ajax({
                url: "/api/WebMenu/importConten",
                type: "Post",
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
        delete: function (id) {
            return $.ajax({
                url: "/api/WebMenu/Delete",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        updateLevelAndSerNo: function (list) {
            return $.ajax({
                url: "/api/WebMenu/updateLevelAndSerNo",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ list: list }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetPageTypeList: function () {
            return $.ajax({
                url: "/api/WebMenu/GetPageTypeList",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
    }
});