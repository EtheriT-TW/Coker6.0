Coker.extend({
    Company: {
        Save: function (data) {
            return $.ajax({
                url: "/api/Company/Save",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    }
});