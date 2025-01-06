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
        CheckRefund: function (payment, transactionId) {
            return $.ajax({
                url: "/api/ThirdParty/CheckRefund/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { payment: payment, transactionId: transactionId },
            });
        },
        CheckPaymentStatus: function (ohid, thirdparty) {
            return $.ajax({
                url: "/api/ThirdParty/CheckPaymentStatus/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { ohid: ohid, thirdparty: thirdparty },
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
        },
    }
});