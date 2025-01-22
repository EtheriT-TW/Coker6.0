let flowSizesType = "day";
var PageReady = function () {
    $("#loadDataByYear").on("click", loadDataByYear);
    $("#loadDataByMonth").on("click", loadDataByMonth);
    $("#loadDataByDay").on("click", loadDataByDay);
    function loadDataByYear() {
        var grid = $('#changeList').dxDataGrid('instance');
        $(".btn").removeClass("btn-primary");
        $(this).addClass("btn-primary");
        flowSizesType = "year";
        grid.refresh();  // 重新加載 DataGrid
    }
    function loadDataByMonth() {
        var grid = $('#changeList').dxDataGrid('instance');
        $(".btn").removeClass("btn-primary");
        $(this).addClass("btn-primary");
        flowSizesType = "month";
        grid.refresh();  // 重新加載 DataGrid
    }
    function loadDataByDay() {
        var grid = $('#changeList').dxDataGrid('instance');
        $(".btn").removeClass("btn-primary");
        $(this).addClass("btn-primary");
        flowSizesType = "day";
        grid.refresh();  // 重新加載 DataGrid
    }
}
var GetFlowSizesType = function(){
    return flowSizesType;
}