Coker.extend({
    Contact: {
        GetDataOne: function (id) {
            return $.ajax({
                url: "/api/Contact/GetDataOne",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: {id:id},
                dataType: "json"
            });
        },
        Replay: function (data) {
            return $.ajax({
                url: "/api/Contact/ReplyContact",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    }
});