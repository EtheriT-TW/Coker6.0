Coker.extend({
    Product: {
        AddUp: {
            Product: function (data) {
                return $.ajax({
                    url: "/api/Product/ProductAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            Stock: function (data) {
                return $.ajax({
                    url: "/api/Product/StockAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            ProdTechCert: function (data) {
                return $.ajax({
                    url: "/api/Product/TechCertAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            ProdPrice: function (data) {
                return $.ajax({
                    url: "/api/Product/ProdPriceAddUp",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            Import: function (formData) {
                return $.ajax({
                    url: '/api/Product/ProdReplace',
                    type: 'POST',
                    data: formData,
                    headers: _c.Data.Header,
                    contentType: false,
                    crossDomain: true,
                    dataType: 'json',
                    mimeType: "multipart/form-data",
                    processData: false
                });
            }
        },
        Get: {
            ProdOne: function (id) {
                return $.ajax({
                    url: "/api/Product/GetProdDataOne/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { id: id }
                });
            },
            ProdStock: function (id) {
                return $.ajax({
                    url: "/api/Product/GetStockDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id }
                });
            },
            ProdTechCert: function (id) {
                return $.ajax({
                    url: "/api/Product/GetTechCertDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id }
                });
            },
            ProdPrice: function (id) {
                return $.ajax({
                    url: "/api/Product/GetPriceDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PSId: id }
                });
            },
        },
        Delete: {
            Prod: function (id) {
                return $.ajax({
                    url: "/api/Product/ProdDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id }
                });
            },
            Stock: function (id) {
                return $.ajax({
                    url: "/api/Product/StockDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id }
                });
            },
            Price: function (id) {
                return $.ajax({
                    url: "/api/Product/PriceDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id }
                });
            }
        },
        Content: {
            GetConten: function (data) {
                return $.ajax({
                    url: "/api/Product/GetConten",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            SaveConten: function (data) {
                return $.ajax({
                    url: "/api/Product/SaveConten",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            },
            ImportConten: function (data) {
                return $.ajax({
                    url: "/api/Product/ImportConten",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            }
        },
        ThirdParty: {
            save: function (data) {
                return $.ajax({
                    url: "/api/ThirdParty/SaveThirdParty",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: JSON.stringify(data),
                    dataType: "json"
                });
            }
        },
        Spec: {
            ListInit: function () {
                $(".btn_spec_add > button").on("click", function (e) {
                    e.preventDefault();
                    SpecAdd(null);
                })
                $(function () {
                    //var drap_sy, drap_ey, drap_itemh;
                    $("#Spec_Frame > ul").each(function (index, element) {
                        $(element).sortable({
                            items: "> .spec_list",
                            axis: "y",
                            cursor: "move",
                            dropOnEmpty: false,
                            start: function (event, ui) {
                                //drap_sy = ui.item.offset().top;
                                //drap_itemh = ui.item.height() * 1.5
                            },
                            stop: function (event, ui) {
                                var index_now = ui.item.index(".spec_list") + 1;
                                var $ser_no = ui.item.find(".ser_no");
                                if (index_now > $ser_no.val()) {
                                    $ser_no.val(index_now)
                                    SortChange($(".spec_list "), "bigger", ui.item.data("serno"), $ser_no.val())
                                    ui.item.data("serno", $ser_no.val())
                                } else if (index_now < $ser_no.val()) {
                                    $ser_no.val(index_now)
                                    SortChange($(".spec_list "), "smaller", $ser_no.val(), ui.item.data("serno"))
                                    ui.item.data("serno", $ser_no.val())
                                }
                            }
                        });
                    });
                });
            },
        }
    }
});