function PageReady() {
    $("#ImageUpload").ImageUploadModalClear();
    co.Zipcode.init("#TWzipcode");
    co.Member.GetSelf().done(function (result) {
        if (result != null) {
            result.ImageUpload = "/";
            co.Form.insertData(result,"#memberForm");
        }
    });
    $("#memberForm .submit").on("click", function () {
        var list = [];
        const data = co.Form.getJson("memberForm");
        list.push(co.Member.Update(data));
        if ($("#ImageUpload .img_input_frame").data("delectList") != null) {
            list.push(
                co.File.DeleteFileById({
                    Sid: data.id,
                    Type: 14,
                    Fid: $("#ImageUpload .img_input_frame").data("delectList")
                })
            );
        }
        if ($("#ImageUpload .img_input").data("file") != null && $("#ImageUpload .img_input").data("file").File != null && $("#ImageUpload .img_input").data("file").id == 0) {
            var formData = new FormData();
            formData.append("files", $("#ImageUpload .img_input").data("file").File);
            formData.append("type", 14);
            formData.append("sid", data.id);
            formData.append("serno", 500);
            list.push(co.File.Upload(formData));
        }
        $.when.apply(null, list).done(function () {
            const arg = arguments[0];
            const result = Array.isArray(arg) ? arg[0] : arg;
            if (result.success) co.sweet.success("儲存成功");
            else co.sweet.error("儲存失敗");
        });
    });
    $("#changPassword .submit").on("click", function () {
        const obj = co.Form.getJson("changPassword");
        if (obj.newPassword != obj.newPasswordChecked) {
            co.sweet.error("密碼與密碼驗證不相符");
        } else {
            co.User.UpdatePassword(obj).done(function () {
                $('#offcanvasTop').offcanvas('hide');
                //let myOffCanvas = new bootstrap.Offcanvas(document.getElementById("offcanvasTop"));
                //console.log(myOffCanvas);
                //myOffCanvas.hide();
            });
        }
        /*co.Member.Update(co.Form.getJson("memberForm")).done(function (result) {
            co.sweet.success("儲存成功");
        });*/
        return false;
    });
}