Coker.extend({
    Contact: {
        GetDataOne: function (id) {
            return $.ajax({
                url: "/api/Contact/GetDataOne",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: {id:id},
                dataType: "json",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("requestverificationtoken",
                        $('input:hidden[name="AntiforgeryFieldname"]').val());
                }
            });
        }
    }
});