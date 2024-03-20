function FlipBookInit() {
    var $this = $(".FlipBook");
    $this.addClass("d-none");

    if (typeof ($this.data("pdf")) == "undefined" || !$this.data("pdf")) {
        console.log("No PDF file specified for FlipBook");
        return
    }

    var target_pdf = $this.data('pdf');
    console.log(target_pdf);

    var flipbook_container = jQuery('<iframe>');
    flipbook_container.attr('src', '/lib/pdf-viewer/external/pdfjs-2.1.266-dist/web/viewer.html?file=' + encodeURIComponent(target_pdf));
    $this.append(flipbook_container);
    $this.removeClass("d-none");
}

function FlipBookModalInit() {
    var $this = $(".FlipBookModal");
    $this.addEventListener('show.bs.modal', event => {
        // Button that triggered the modal
        const button = event.relatedTarget
        // Extract info from data-bs-* attributes
        const recipient = button.getAttribute('data-bs-pdf-url')
        // If necessary, you could initiate an Ajax request here
        // and then do the updating in a callback.

        // Update the modal's content.
        const modalTitle = $this.querySelector('.modal-title')
        const modalBodyInput = $this.querySelector('.modal-body input')

        modalTitle.textContent = `New message to ${recipient}`
        modalBodyInput.value = recipient
    });
}



