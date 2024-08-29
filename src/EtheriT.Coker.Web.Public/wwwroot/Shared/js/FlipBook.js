function FlipBookInit() {
    var $this = $(".FlipBook");
    const pdfVersion = [{
        version: "pdfjs-4.5.136-dist", ext: "mjs"
    }, {
        version: "pdfjs-2.1.266-dist", ext: "js"
    }];
    const usePdf = pdfVersion[0];
    $this.addClass("d-none");

    if (typeof ($this.data("pdf")) == "undefined" || !$this.data("pdf")) {
        console.log("No PDF file specified for FlipBook");
        target_pdf = `/lib/pdf-viewer/external/${usePdf.version}/web/compressed.tracemonkey-pldi-09.pdf`;
    }
    else {
        var target_pdf = $this.data('pdf');
    }

    var flipbook_container = document.createElement("div");

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4) {
            if (this.status == 200) { flipbook_container.innerHTML = this.responseText; }
            if (this.status == 404) { flipbook_container.innerHTML = "Page not found."; }
            $(flipbook_container).find("meta,title,script").remove();
            const loadJs = [];
            $.LoadJs(`/lib/pdf-viewer/external/${usePdf.version}/build/pdf.${usePdf.ext}`).done(function () {
                loadJs.push(...[
                    $.LoadJs(`/lib/pdf-viewer/external/${usePdf.version}/web/viewer.${usePdf.ext}`),
                    $.LoadJs(`/lib/pdf-viewer/external/turn.js`),
                    $.LoadJs(`/lib/pdf-viewer/pdf-turn/pdf-turn.js`)
                ])
            });
            $this.append(flipbook_container);
            $this.removeClass("d-none");
            if ($(".FlipBookModal").length > 0) {
                $.when.apply(null, loadJs).done(function () {
                    FlipBookModalInit();
                });
            }
        }
    }
    xhttp.open("GET", `/lib/pdf-viewer/external/${usePdf.version}/web/viewer.html?file=${encodeURIComponent(target_pdf)}`, true);
    xhttp.send();
}

function FlipBookModalInit() {
    var $this = $(".FlipBookModal");
    const modal = bootstrap.Modal.getOrCreateInstance($this[0]);
    $this.on('show.bs.modal', event => {
        // Button that triggered the modal
        var button = event.relatedTarget;
        var target_pdf = button.getAttribute('data-pdf-url');
        PDFViewerApplication.closeModal = function () {
            modal.hide();
        };
        PDFViewerApplication.setInitialView();
        PDFViewerApplication.open({ url: target_pdf, originalUrl: target_pdf });
        if (typeof ($(this).data("init")) == "undefined" && !$(this).data("init")) {
            $(this).data("init", true);
            bookFlip.init();
        }
    });
}



