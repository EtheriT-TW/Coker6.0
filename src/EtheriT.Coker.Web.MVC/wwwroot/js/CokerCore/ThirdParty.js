Coker.extend({
    ThirdParty: {
        PayRefund: function (payment, ohid, refund) {
            return $.ajax({
                url: "/api/ThirdParty/PayRefund/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { payment: payment, ohid: ohid, refund: refund },
            });
        },
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
            CheckPaymentStatus: function (ohid) {
                return $.ajax({
                    url: "/api/ThirdParty/LinePayCheckPaymentStatus/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { ohid: ohid },
                });
            }
        },
        PChomePay: {
            CheckStatus: function (ohid) {
                return $.ajax({
                    url: "/api/ThirdParty/PChomePayCheckPaymentStatus/",
                    type: "GET",
                    contentType: 'application/json; charset=utf-8',
                    headers: _c.Data.Header,
                    data: { ohid: ohid },
                });
            },
        }
    }
});