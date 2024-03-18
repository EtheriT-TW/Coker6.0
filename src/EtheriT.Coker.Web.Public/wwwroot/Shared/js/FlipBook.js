function FlipBookInit() {
    var $this = $(".FlipBook");
    $this.addClass("d-none");

    if (typeof ($this.data("pdf")) == "undefined" || !$this.data("pdf")) {
        console.log("No PDF file specified for FlipBook");
        return
}

    var target_pdf = $this.data('pdf');
    console.log(target_pdf)

    var flipbook_container = jQuery('<iframe>');
    flipbook_container.attr('src', '/lib/pdf-viewer/external/pdfjs-2.1.266-dist/web/viewer.html?file=' + encodeURIComponent(target_pdf));
    $this.append(flipbook_container);
    $this.removeClass("d-none");

}



