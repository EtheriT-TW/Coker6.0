(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        dom: {
            zipcode: {
                init: function (id) {
                    var reandomStr = Coker.util.string.generateRandomString(5);
                    var $TWzipcode = $(id);

                    $TWzipcode.twzipcode({
                        zipcodeIntoDistrict: true
                    });

                    var $address = $TWzipcode.find(".address");
                    var $county = $TWzipcode.children(".county");
                    var $district = $TWzipcode.children(".district");

                    $county.children("select").attr({
                        id: "SelectCity_" + reandomStr,
                        class: "city form-select"
                    }).prop("required", $address.prop("required"));
                    $county.append("<label class='px-4 required' for='SelectCity_" + reandomStr + "'>縣市</label>");
                    var $county_first_option = $county.children("select").children("option").first();
                    $county_first_option.text("請選擇縣市");
                    $county_first_option.attr("disabled", "disabled");

                    $district.children("select").attr({
                        id: "SelectTown_" + reandomStr,
                        class: "town form-select"
                    }).prop("required", $address.prop("required"));
                    $district.append("<label class='required' for='SelectTown_" + reandomStr + "'>鄉鎮</label>");
                    var $district_first_option = $district.children("select").children("option").first();
                    $district_first_option.text("請選擇鄉鎮");
                    $district_first_option.attr("disabled", "disabled");
                },

                setData: function (obj) {
                    var $addr = obj.el.find(".address");

                    if (Coker.util.string.isNullOrEmpty(obj.addr)) {
                        obj.el.twzipcode("reset");
                        obj.el.find(".address").val("");
                    } else {
                        var address_split = String(obj.addr).split(" ");
                        obj.el.twzipcode("set", {
                            county: address_split[0],
                            district: address_split[1]
                        });
                        $addr.val(address_split[2]);
                    }
                },

                getData: function ($e) {
                    return $e.find(".county>select").val() + " " +
                        $e.find(".district>select").val() + " " +
                        $e.find(".address").val();
                }
            }
        }
    });

    // Legacy: Coker.Zipcode.*
    Coker.Zipcode = Coker.Zipcode || {};
    Coker.Zipcode.init = Coker.Zipcode.init || Coker.dom.zipcode.init;
    Coker.Zipcode.setData = Coker.Zipcode.setData || Coker.dom.zipcode.setData;
    Coker.Zipcode.getData = Coker.Zipcode.getData || Coker.dom.zipcode.getData;

})(window);