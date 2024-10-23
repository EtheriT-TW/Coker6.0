Coker.extend({
    Remote: {
        GetRemoteCount: function (data) {
            return $.ajax({
                url: "/api/Remote/GetRemoteCount",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    }
});