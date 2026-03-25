function PageReady() {
    var editor = grapesInit({
        save: function (html, css) {
            var _dfr = $.Deferred();
            co.Templates.saveDefaultFooter({
                Id: $("#gjs").data("id")||0,
                SaveHtml: html,
                SaveCss: css
            }).then(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        import: function (html, css) {
            var _dfr = $.Deferred();
            co.Templates.importDefaultFooter({
                Id: $("#gjs").data("id")||0,
                SaveHtml: html,
                SaveCss: css
            }).then(function (resutlt) {
                if (resutlt.success) _dfr.resolve();
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        },
        getComponer: function () {
            var _dfr = $.Deferred();
            co.HtmlContent.GetAllComponent().done(function (result) {
                if (result.success) _dfr.resolve(result.list);
                else co.sweet.error(resutlt.error);
            });
            return _dfr.promise();
        }
    });
    co.Templates.getDefaultFooter().then(result => {
        var html = co.Data.HtmlDecode(result.footerTemplateDto.html);
        co.Grapes.setEditor(editor, html, result.footerTemplateDto.css);
        co.Grapes.setFile(editor, result.footerTemplateDto.id, 5);
        $("#gjs").data("id", result.footerTemplateDto.id);
        $("body").addClass("grapesEdit");
    }).catch(error => {
        console.error("錯誤處理：", error);
    });
}