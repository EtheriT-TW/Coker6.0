function PageReady() {
    $("#ImageUpload").ImageUploadModalClear();
    co.Zipcode.init("#TWzipcode");
    co.Member.GetSelf().done(function (result) {
        if (result != null) {
            co.Form.insertData(result,"#memberForm");
        }
    });
    $("#memberForm .submit").on("click", function () {
        co.Member.Update(co.Form.getJson("memberForm")).done(function (result) {
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