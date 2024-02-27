Coker.extend({
    HtmlContent: {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/HtmlContent/AddUp",
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
        Get: function (id) {
            return $.ajax({
                url: "/api/HtmlContent/GetOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        Delete: function (id) {
            const myId = parseInt(id);
            return $.ajax({
                url: "/api/HtmlContent/Delete",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: myId }),
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        },
        GetTypeList: function () {
            return $.ajax({
                url: "/api/HtmlContent/GetTypeList",
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
        GetAllComponent: function () {
            return $.ajax({
                url: "/api/HtmlContent/GetAllComponent",
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
        GetComponent: function (type) {
            return $.ajax({
                url: "/api/HtmlContent/GetComponent",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ type: type }),
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }
    }
});