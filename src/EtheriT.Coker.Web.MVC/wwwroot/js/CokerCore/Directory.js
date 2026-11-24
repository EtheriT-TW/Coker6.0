Coker.extend({
    Directory: {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/Directory/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (Id) {
            return $.ajax({
                url: "/api/Directory/GetDataOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        },
        Delete: function (Id) {
            return $.ajax({
                url: "/api/Directory/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        },
        GetDirectoryFacetConfig: function (DirectoryId) {
            return $.ajax({
                url: "/api/Directory/GetDirectoryFacetConfig/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: DirectoryId },
            });
        },
        SaveDirectoryFacetConfig: function (data) {
            return $.ajax({
                url: "/api/Directory/SaveDirectoryFacetConfig",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        GetArticleTemplates: function () {
            return $.ajax({
                url: "/api/ArticleTemplate/GetList",
                type: "GET",
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        GetArticleTemplateChangeInfo: function (directoryId, templateKey, fromTemplateKey) {
            return $.ajax({
                url: "/api/ArticleTemplate/GetChangeInfo",
                type: "GET",
                headers: _c.Data.Header,
                data: {
                    directoryId: directoryId,
                    templateKey: templateKey || "",
                    fromTemplateKey: fromTemplateKey || ""
                },
                dataType: "json"
            });
        },
        // 防偽權杖要先取新的、且與送出使用同一組登入標頭（共用 ArticleImport.WithToken），否則會被判定為不同使用者。
        ApplyArticleTemplate: function (directoryId) {
            return _c.ArticleImport.WithToken(function (headers) {
                return $.ajax({
                    url: "/api/ArticleTemplate/Apply",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: headers,
                    data: JSON.stringify({ DirectoryId: directoryId }),
                    dataType: "json"
                });
            });
        }
    },
    // 目錄管理「匯入資料」（api/ArticleImport）。上傳用 FormData，其餘 JSON。
    ArticleImport: {
        // 取防偽權杖和送出 POST 共用同一組登入標頭，讓兩個請求使用同一個驗證方式。
        WithToken: function (send) {
            var headers = $.extend({}, _c.Data.Header);
            return $.ajax({
                url: "/api/ArticleImport/RequestToken",
                type: "GET",
                headers: headers,
                cache: false,
                dataType: "json"
            }).then(function (tokens) {
                headers[tokens.headerName] = tokens.token;
                return send(headers);
            });
        },
        GetDirectories: function () {
            return $.ajax({
                url: "/api/ArticleImport/GetDirectories",
                type: "GET",
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        ReadColumns: function (file) {
            var data = new FormData();
            data.append("file", file);
            return _c.ArticleImport.WithToken(function (headers) {
                return $.ajax({
                    url: "/api/ArticleImport/ReadColumns",
                    type: "POST",
                    headers: headers,
                    data: data,
                    processData: false,
                    contentType: false,
                    dataType: "json"
                });
            });
        },
        Start: function (directoryId, columns, file) {
            var data = new FormData();
            data.append("directoryId", directoryId);
            (columns || []).forEach(function (column) { data.append("columns", column); });
            data.append("file", file);
            return _c.ArticleImport.WithToken(function (headers) {
                return $.ajax({
                    url: "/api/ArticleImport/Start",
                    type: "POST",
                    headers: headers,
                    data: data,
                    processData: false,
                    contentType: false,
                    dataType: "json"
                });
            });
        },
        GetCurrent: function () {
            return $.ajax({
                url: "/api/ArticleImport/GetCurrent",
                type: "GET",
                headers: _c.Data.Header,
                cache: false
            });
        },
        GetBatch: function (batchId) {
            return $.ajax({
                url: "/api/ArticleImport/GetBatch",
                type: "GET",
                headers: _c.Data.Header,
                data: { batchId: batchId },
                cache: false,
                dataType: "json"
            });
        },
        GetRow: function (rowId) {
            return $.ajax({
                url: "/api/ArticleImport/GetRow",
                type: "GET",
                headers: _c.Data.Header,
                data: { rowId: rowId },
                cache: false,
                dataType: "json"
            });
        },
        UpdateRow: function (rowId, fields) {
            return _c.ArticleImport.WithToken(function (headers) {
                return $.ajax({
                    url: "/api/ArticleImport/UpdateRow",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: headers,
                    data: JSON.stringify({ RowId: rowId, Fields: fields }),
                    dataType: "json"
                });
            });
        },
        Confirm: function (batchId, excludedRowIds) {
            return _c.ArticleImport.WithToken(function (headers) {
                return $.ajax({
                    url: "/api/ArticleImport/Confirm",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: headers,
                    data: JSON.stringify({ BatchId: batchId, ExcludedRowIds: excludedRowIds || [] }),
                    dataType: "json"
                });
            });
        },
        Cancel: function (batchId) {
            return _c.ArticleImport.WithToken(function (headers) {
                return $.ajax({
                    url: "/api/ArticleImport/Cancel",
                    type: "POST",
                    contentType: 'application/json; charset=utf-8',
                    headers: headers,
                    data: JSON.stringify({ BatchId: batchId }),
                    dataType: "json"
                });
            });
        }
    }
});