Coker.extend({
    ThirdParty: {
        Line: {
            Confirm: function (ohid) {
                return $.ajax({
                    url: "/api/ThirdParty/LinePayConfirm/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { ohid: ohid },
                });
            },
            PayVoid: function (ohid) {
                return $.ajax({
                    url: "/api/ThirdParty/LinePayVoid/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { ohid: ohid },
                });
            },
            PayRefund: function (ohid, refund) {
                return $.ajax({
                    url: "/api/ThirdParty/LinePayRefund/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { ohid: ohid, refund: refund },
                });
            },
            CheckPaymentStatus: function (ohid) {
                return $.ajax({
                    url: "/api/ThirdParty/LinePayCheckPaymentStatus/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { ohid: ohid },
                });
            }
        }
    }
});