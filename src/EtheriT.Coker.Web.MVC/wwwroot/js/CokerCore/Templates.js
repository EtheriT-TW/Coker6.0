Coker.extend({
    Templates: {
        getDefaultFooter: function () {
            return fetch('/api/Template/getDefaultFooter', {
                method: 'GET',
                headers: {
                    "Content-Type": "application/json; charset=utf-8",
                    ..._c.Data.Header
                }
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error("Network response was not ok");
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    return data.object;
                } else {
                    throw new Error(data.error);
                }
            })
        },
        saveDefaultFooter: function (data) {
            return fetch('/api/Template/saveDefaultFooter', {
                method: 'POST',
                headers: {
                    "Content-Type": "application/json; charset=utf-8",
                    ..._c.Data.Header
                },
                body: JSON.stringify(data)
            }).then(response => {
                if (!response.ok) {
                    throw new Error("Network response was not ok");
                }
                return response.json();
            })
        },
        importDefaultFooter: function (data) {
            return fetch('/api/Template/importDefaultFooter', {
                method: 'POST',
                headers: {
                    "Content-Type": "application/json; charset=utf-8",
                    ..._c.Data.Header
                },
                body: JSON.stringify(data)
            }).then(response => {
                if (!response.ok) {
                    throw new Error("Network response was not ok");
                }
                return response.json();
            })
        },
        getDefaultHeader: function () {
            return fetch('/api/Template/getDefaultHeader', {
                method: 'GET',
                headers: {
                    "Content-Type": "application/json; charset=utf-8",
                    ..._c.Data.Header
                }
            }).then(response => {
                if (!response.ok) {
                    throw new Error("Network response was not ok");
                }
                return response.json();
            })
        },
        saveDefaultHeader: function (data) {
            return fetch('/api/Template/saveDefaultHeader', {
                method: 'POST',
                headers: {
                    "Content-Type": "application/json; charset=utf-8",
                    ..._c.Data.Header
                },
                body: JSON.stringify(data)
            }).then(response => {
                if (!response.ok) {
                    throw new Error("Network response was not ok");
                }
                return response.json();
            })
        }
    }
});