function FlipBookInit() {
    var $this = $(".FlipBook");
    $this.addClass("d-none");

    if (typeof ($this.data("pdf")) == "undefined" || !$this.data("pdf")) {
        console.log("No PDF file specified for FlipBook");
        return
    }

    var target_pdf = $this.data('pdf');
    console.log(target_pdf);

    var flipbook_container = document.createElement("div");

    const url = new URL(window.location.href);
    url.searchParams.set('file', target_pdf);
    window.history.replaceState(null, null, url);

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4) {
            if (this.status == 200) { flipbook_container.innerHTML = this.responseText; }
            if (this.status == 404) { flipbook_container.innerHTML = "Page not found."; }
    $this.append(flipbook_container);
    $this.removeClass("d-none");
            console.log("show pdf");
        }
    }
    xhttp.open("GET", '/lib/pdf-viewer/external/pdfjs-2.1.266-dist/web/viewer.html?file=' + encodeURIComponent(target_pdf), true);
    xhttp.send();


        modalTitle.textContent = `New message to ${recipient}`
        modalBodyInput.value = recipient
    });
}



