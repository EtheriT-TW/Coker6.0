(function ($) {
    $.fn.extend({
        setRolesData: function (o) {
            /******************************************************** 
             * o.type 設定什麼東西的權限
             * o.id 目標對象的ID
             * *****************************************************/
            const $self = $(this);
            const $temp = $("#chooice");
            const $Roles = $self.find("#chooiceRoles .list");
            $("#RolesDetailsModal .typeName").text(o.title);
            co.PowerManagement.GetPagePermission(o).done(function (result) {
                if (result.success) {
                    $Roles.empty();
                    $(result.object.roles).each(function (index, element) {
                        const $item = $($temp.html());
                        $item.find(".name").text(element.name);
                        $item.find("#chooiceInput + label").attr({ for: `Roles${element.id}` });
                        $item.find("#chooiceInput").attr({ id: `Roles${element.id}`, name: "Roles" }).prop("checked", element.isChecked).val(element.id);
                        $Roles.append($item);
                    });
                } else {
                    co.sweet.error("資料取得錯誤", "無法取得網站使用者，您可能不具有權限。");
                }
            });
            $self.find(".btn_save").off("click").on("click", function () {
                const json = co.Object.merge(o, co.Form.getJson("RolesDetailsForm"));
                json.PageId = json.id;
                if (typeof (json.Users) == "string") json.Users = [json.Users];
                if (typeof (json.Roles) == "string") json.Roles = [json.Roles];
                delete json.id;
                co.PowerManagement.SavePagePermission(json).done(function (result) {
                    if (result.success) {
                        co.sweet.success("成功", "已成功設定");
                        $self.modal("hide");
                    } else co.sweet.error("失敗", "資料儲存失敗!");
                });
            });
            return $self;
        }
    });
})(jQuery)