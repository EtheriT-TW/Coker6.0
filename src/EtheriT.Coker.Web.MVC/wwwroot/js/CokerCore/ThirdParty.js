Coker.extend({
    ThirdParty: {
        HandleThirdPartyPayment: function (data) {
            return $.ajax({
                url: "/api/ThirdParty/HandleThirdPartyPayment/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
            });
        },
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
        CreateLogistics: function (ohid) {
            return $.ajax({
                url: "/api/ThirdParty/HandleThirdPartyPayment/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
            });
        },
        HandleThirdPartyLogistics: function (data) {
            return $.ajax({
                url: "/api/ThirdParty/HandleThirdPartyLogistics/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json",
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