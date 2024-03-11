var scale = 1.5; // 可調整

function FlipBookInit() {
    var flipbook = document.getElementById('flipbook');
    flipbook.classList.add('d-none');
    var default_testing_pdf_url = "https://raw.githubusercontent.com/mozilla/pdf.js/ba2edeae/web/compressed.tracemonkey-pldi-09.pdf";
    var url = flipbook.hasAttribute('data-url') ? flipbook.dataset.url : default_testing_pdf_url;
    // console.log(url, flipbook);
    getpdf(url);
}

function getpdf(url) {
    var pdfjsLib = window['pdfjs-dist/build/pdf'];
    pdfjsLib.GlobalWorkerOptions.workerSrc = '/lib/pdfjs/build/pdf.worker.min.js';  //放自己專案中worker.js的路徑

    var loadingTask = pdfjsLib.getDocument(url);
    loadingTask.promise.then(function (pdf) {
        // 根據總頁數新增固定的div和canvas
        // console.log("總頁數", pdf.numPages);

        // 使用遞迴方式依序渲染每個頁面
        renderPage(pdf, 1);
    });
}

function renderPage(pdf, pageNum) {
    pdf.getPage(pageNum).then(function (page) {
        var viewport = page.getViewport({ scale: scale });
        let id = 'canvaspage' + pageNum;
        let div = document.createElement('div');
        div.innerHTML = '<canvas id="' + id + '"></canvas>';
        flipbook.append(div);

        let canvas = document.getElementById(id);
        let context = canvas.getContext('2d');

        canvas.height = viewport.height;
        canvas.width = viewport.width;

        let renderContext = {
            canvasContext: context,
            viewport: viewport
        };

        // 將PDF頁面渲染到canvas中
        page.render(renderContext).promise.then(function () {
            // 渲染下一個頁面或呼叫loadApp()函式
            if (pageNum < pdf.numPages) {
                renderPage(pdf, pageNum + 1);
            } else {
                // 增加封底
                let div = document.createElement('div');
                div.classList.add('hard');
                div.innerHTML = '範例封底';
                flipbook.append(div);
                loadApp();
            }
        });
    });

}

function loadApp() {
    $("#flipbook").turn({
        autoCenter: true, // 是否置中
        // display: 'single', // 單頁顯示
    });
    $("#flipbook").removeClass("d-none");
}
//當畫面縮放時，要改變外殼大小，如果你想固定，不一定要加
//window.addEventListener('resize',function(e){
// flipbook.style.width="";
//flipbook.style.height="";
//  $(flipbook).turn("size", window.innerWidth,window.innerHeight*0.8);
//})
