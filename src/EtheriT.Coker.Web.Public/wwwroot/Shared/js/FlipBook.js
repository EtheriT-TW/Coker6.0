let flipBookReadyPromise = null;

function FlipBookInit() {
    const $flipBook = $(".FlipBook").first();
    const $flipBookItem = $(".FlipBookItem").first();
    if ($flipBook.length === 0 || $flipBookItem.length === 0) return Promise.resolve();

    const pdfVersion = [{
        version: "pdfjs-4.5.136-dist", ext: "mjs"
    }, {
        version: "pdfjs-2.1.266-dist", ext: "js"
    }];

    let index = parseInt($flipBookItem.attr("data-type"), 10);
    if (isNaN(index) || !pdfVersion[index]) index = 1;

    const usePdf = pdfVersion[index];
    const pare = /^pdfjs-4/.test(usePdf.version)
        ? { mode: 4, lName: "#listeners" }
        : { mode: 3, lName: "_listeners" };

    if (flipBookReadyPromise === null) {
        $flipBook.addClass("d-none");
        flipBookReadyPromise = loadFlipBookRuntime($flipBook, usePdf, pare).catch(function (error) {
            flipBookReadyPromise = null;
            console.error("FlipBook initialization failed.", error);
            throw error;
        });
    }

    bindFlipBookModal(flipBookReadyPromise);
    return flipBookReadyPromise;
}

async function loadFlipBookRuntime($flipBook, usePdf, pare) {
    const viewerHtml = await loadFlipBookViewerHtml(
        `/lib/pdf-viewer/external/${usePdf.version}/web/viewer.html?file=`
    );
    const flipbookContainer = document.createElement("div");
    flipbookContainer.innerHTML = viewerHtml;
    $(flipbookContainer).find("meta,title,script").remove();

    // The runtime expects a single viewer DOM. Re-entry must not append a second copy.
    $flipBook.empty().append(flipbookContainer);

    await loadFlipBookScript(`/lib/pdf-viewer/external/${usePdf.version}/build/pdf.${usePdf.ext}`);
    await Promise.all([
        loadFlipBookScript(`/lib/pdf-viewer/external/${usePdf.version}/web/viewer.${usePdf.ext}`),
        loadFlipBookScript("/lib/pdf-viewer/external/turn.js")
    ]);
    await loadFlipBookScript("/lib/pdf-viewer/pdf-turn/pdf-turn.js");
    await waitForFlipBookRuntime();

    if (typeof PDFViewerApplication.initializedPromise !== "undefined") {
        await PDFViewerApplication.initializedPromise;
    }

    initializeFlipBookModalRuntime(pare);
    $flipBook.removeClass("d-none");
}

function loadFlipBookViewerHtml(url) {
    return new Promise(function (resolve, reject) {
        const request = new XMLHttpRequest();
        request.onreadystatechange = function () {
            if (request.readyState !== 4) return;

            if (request.status >= 200 && request.status < 300) {
                resolve(request.responseText);
            } else {
                reject(new Error(`Unable to load FlipBook viewer (${request.status}).`));
            }
        };
        request.onerror = function () {
            reject(new Error("Unable to load FlipBook viewer."));
        };
        request.open("GET", url, true);
        request.send();
    });
}

function loadFlipBookScript(url) {
    return Promise.resolve($.LoadJs(url));
}

function waitForFlipBookRuntime() {
    const timeout = 15000;
    const startedAt = Date.now();

    return new Promise(function (resolve, reject) {
        const check = function () {
            if (typeof bookFlip !== "undefined" &&
                typeof PDFViewerApplication !== "undefined" &&
                PDFViewerApplication.eventBus != null) {
                resolve();
                return;
            }

            if (Date.now() - startedAt >= timeout) {
                reject(new Error("Timed out while waiting for the FlipBook runtime."));
                return;
            }

            setTimeout(check, 100);
        };

        check();
    });
}

function initializeFlipBookModalRuntime(pare) {
    const $modal = $(".FlipBookModal").first();
    if ($modal.length === 0 || $modal.data("flipbook-init")) return;

    const modal = bootstrap.Modal.getOrCreateInstance($modal[0]);
    $modal.data("flipbook-init", true);

    PDFViewerApplication.closeModal = function () {
        modal.hide();
    };
    PDFViewerApplication.setTitleUsingUrl = function () {
        // Keep the website title when PDF.js opens a document.
    };
    bookFlip.init(pare);
}

function bindFlipBookModal(readyPromise) {
    const $modal = $(".FlipBookModal").first();
    if ($modal.length === 0) return;

    $modal.off("shown.bs.modal.flipBook").on("shown.bs.modal.flipBook", async function (event) {
        const button = event.relatedTarget;
        const targetPdf = button ? button.getAttribute("data-pdf-url") : null;
        if (!targetPdf) {
            console.warn("No PDF file specified for FlipBook.");
            return;
        }

        try {
            await readyPromise;
            PDFViewerApplication.setInitialView();
            await Promise.resolve(PDFViewerApplication.open({
                url: targetPdf,
                originalUrl: targetPdf
            }));
        } catch (error) {
            console.error("Unable to open FlipBook PDF.", error);
        }
    });
}
