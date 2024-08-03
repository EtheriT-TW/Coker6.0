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
                    dataType: "json",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            Stock: function (data) {
                return $.ajax({
                    url: "/api/Product/StockAddUp",
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
            ProdTechCert: function (data) {
                return $.ajax({
                    url: "/api/Product/TechCertAddUp",
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
            ProdPrice: function (data) {
                return $.ajax({
                    url: "/api/Product/ProdPriceAddUp",
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
                    processData: false,
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
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
                    data: { id: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            ProdStock: function (id) {
                return $.ajax({
                    url: "/api/Product/GetStockDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            ProdTechCert: function (id) {
                return $.ajax({
                    url: "/api/Product/GetTechCertDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PId: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            ProdPrice: function (id) {
                return $.ajax({
                    url: "/api/Product/GetPriceDataAll/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { PSId: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
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
                    data: { Id: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            Stock: function (id) {
                return $.ajax({
                    url: "/api/Product/StockDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            Price: function (id) {
                return $.ajax({
                    url: "/api/Product/PriceDelete/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { Id: id },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
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
                    dataType: "json",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("requestverificationtoken",
                            $('input:hidden[name="AntiforgeryFieldname"]').val());
                    }
                });
            },
            SaveConten: function (data) {
                return $.ajax({
                    url: "/api/Product/SaveConten",
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
            ImportConten: function (data) {
                return $.ajax({
                    url: "/api/Product/ImportConten",
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
        },
        ThirdParty: {
            save: function (data) {
                return $.ajax({
                    url: "/api/ThirdParty/SaveThirdParty",
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
    }
});