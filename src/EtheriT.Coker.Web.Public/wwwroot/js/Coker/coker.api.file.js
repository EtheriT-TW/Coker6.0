(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        File: {
            DownloadEncryptedFile: function (fid) {
                return $.ajax({
                    url: "/api/File/DecryptFile",
                    type: "GET",
                    data: { fid: fid },

                    xhr: function () {
                        var xhr = new XMLHttpRequest();

                        xhr.onreadystatechange = function () {

                            if (xhr.readyState === 2) {
                                var ct = (xhr.getResponseHeader("Content-Type") || "").toLowerCase();

                                var isText =
                                    ct.includes("text/") ||
                                    ct.includes("application/json") ||
                                    ct.includes("application/problem+json");

                                xhr.responseType = isText ? "text" : "blob";
                            }
                        };

                        return xhr;
                    }
                });
            }
        }
    });

})(window);