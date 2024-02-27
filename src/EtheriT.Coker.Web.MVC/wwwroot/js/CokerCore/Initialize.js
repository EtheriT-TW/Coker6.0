const MinutesSecond = 60 * 1000;
const MonthSecond = 30 * 24 * 60 * 60 * 1000;
const getTarget = (options) => {
    return {
        store: DevExpress.data.AspNet.createStore({
            key: "ID",
            loadUrl: '/api/Newsletter/GetTargetLookup'
        }),
        filter: null
    };
};