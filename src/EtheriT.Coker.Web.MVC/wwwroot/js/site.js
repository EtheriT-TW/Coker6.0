// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
"use strict";
var PreLoader;

(function (a) {

    var now = new Date();
    var edt = !!co.Cookie.Get("endDateTime") ? parseInt(co.Cookie.Get("endDateTime")) : 0;
    console.log(co);
    if (!!!co.Cookie.Get("token")) {
        if (location.pathname != "/" && !/^\/Account/.test(location.pathname)) location.href = "/";
        else co.Page.Ready();
    } else if (edt > (now.getTime() + co.Data.Time.ReCheckTime)) {
        co.Page.Ready();
    } else {
        co.User.Check().done(function (result) {
            if (result.success) co.Page.Ready();
            else location.href = "/";
        });
    }
    $(".loader-wrapper").not(".incomponent").fadeOut(1000, function () { PreLoader = $(this).detach() })

    var tooltipTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });
    var popoverTriggerList = Array.prototype.slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    });
    a("body").delegate('[data-toggle="lightbox"]', "click", function (c) {
        c.preventDefault();
        a(this).ekkoLightbox()
    });
    a("body").delegate(".append-preloader", "click", function () {
        a(PreLoader).show();
        a("body").append(PreLoader);
        setTimeout(function () { a(".loader-wrapper").fadeOut(1000, function () { PreLoader = a(this).detach() }) }, 3000)
    });
    a("body").delegate('[data-toggle="loader"]', "click", function () {
        var c = a(this).attr("data-target"); a("#" + c).show()
    });
    a("body").delegate(".toggle-sidebar", "click", function () {
        a(".sidebar").toggleClass("collapsed");
        if (localStorage.getItem("asideMode") === "collapsed") {
            localStorage.setItem("asideMode", "expanded")
        } else { localStorage.setItem("asideMode", "collapsed") }
        return false
    });
    a("body").delegate(".logout", "click", function (e) {
        e.preventDefault();
        co.User.Logout();
    });
    var b;
    a("body").delegate(".hide-sidebar", "click", function () {
        if (b) {
            b.prependTo(".wrapper");
            b = null
        } else {
            b = a(".sidebar").detach()
        }
    });
    $(".app-switcher .webitem").on("click", function (e) {
        e.preventDefault();
        $(".active-app").removeClass("active-app");
        $(".app-selected").remove();
        $(this).find(".card").addClass("active-app");
        $(this).find(".card-body").append(`<span class="material-icons app-selected md-16">check</span>`);
    });
    $("#switchApp .switch").on("click", function () {
        co.WebSite.exchange($(".active-app").first().data("id")).done(function () {
            location.reload()
        });
    });
    a.fn.setAsideMode = function () {
        if (localStorage.getItem("asideMode") === null) {
        } else {
            if (localStorage.getItem("asideMode") === "collapsed") {
                a(".sidebar").addClass("collapsed")
            } else {
                a(".sidebar").removeClass("collapsed")
            }
        }
    };
    if (a(window).width() > 768) {
        a.fn.setAsideMode()
    }
    a("body").delegate(".navigation li:has(.sub-nav) > a", "click", function () {
        a(this).siblings(".sub-nav").slideToggle();
        a(this).parent().toggleClass("open");
        return false
    }); a("body").delegate(".navigation ul li:has(.sub-nav)", "mouseover", function () {
        if (a(".sidebar").hasClass("collapsed")) {
            var c = a(this), d = a("> .sub-nav", c);
            var e = c.position(); d.css({
                top: e.top, left: e.left + c.outerWidth()
            })
        }
    });
    a("body").delegate(".toggle-controls", "click", function () {
        a(".controls-wrapper").toggle().toggleClass("d-none")
    }); a("body").delegate('[data-toggle="toast"]', "click", function () {
        var c = a(this).attr("data-alignment");
        var e = a(this).attr("data-placement");
        var d = a(this).attr("data-content");
        var f = a(this).attr("data-style");
        if (a(".toast." + c + "-" + e).length) {
            a(".toast." + c + "-" + e).append('<div class="alert alert-dismissible fade show alert-' + f + ' "> ' + d + '<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true" class="material-icons md-18">clear</span></button></div>')
        } else {
            a("body").append('<div class="toast ' + c + "-" + e + '"> <div class="alert alert-dismissible fade show alert-' + f + ' "> ' + d + '<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true" class="material-icons md-18">clear</span></button></div> </div>')
        }
    });
    a(".form-control-chosen").chosen({ allow_single_deselect: true, width: "100%" });
    a(".form-control-chosen-required").chosen({ allow_single_deselect: false, width: "100%" });
    a(".form-control-chosen-search-threshold-100").chosen({ allow_single_deselect: true, disable_search_threshold: 100, width: "100%" });
    a(".form-control-chosen-optgroup").chosen({ width: "100%" });
    a(function () {
        a('[title="clickable_optgroup"]').addClass("chosen-container-optgroup-clickable")
    });
    a(document).delegate('[title="clickable_optgroup"] .group-result', "click", function () {
        var c = a(this).nextUntil(".group-result").not(".result-selected");
        if (c.length) {
            c.trigger("mouseup")
        } else {
            a(this).nextUntil(".group-result").each(function () {
                a('a.search-choice-close[data-option-array-index="' + a(this).data("option-array-index") + '"]').trigger("click")
            })
        }
    });
    a.fn.removeClassStartingWith = function (c) {
        a(this).removeClass(function (e, d) {
            return (d.match(new RegExp("\\S*" + c + "\\S*", "g")) || []).join(" ")
        });
        return this
    };
    a("body").delegate(".theme-changer", "click", function () {
        var e = a(this).attr("primary-color");
        var f = a(this).attr("sidebar-bg");
        var d = a(this).attr("logo-bg");
        var c = a(this).attr("header-bg");
        localStorage.setItem("primaryColor", e);
        localStorage.setItem("sidebarBg", f);
        localStorage.setItem("logoBg", d);
        localStorage.setItem("headerBg", c);
        a.fn.setThemeTone(e)
    });
    a.fn.setThemeTone = function (d) {
        if (localStorage.getItem("primaryColor") === null) {
        } else {
            if (localStorage.getItem("sidebarBg") === "light") {
                a(".sidebar ").addClass("sidebar-light")
            } else {
                a(".sidebar").removeClass("sidebar-light")
            } if (localStorage.getItem("primaryColor") === "primary") {
                document.documentElement.style.setProperty("--theme-colors-primary", "#4B89FC")
            } else {
                var c = getComputedStyle(document.body).getPropertyValue("--theme-colors-" + localStorage.getItem("primaryColor")); document.documentElement.style.setProperty("--theme-colors-primary", c)
            } if (localStorage.getItem("logoBg") === "white" || localStorage.getItem("logoBg") === "light") {
                a(".sidebar .navbar").removeClassStartingWith("bg").removeClassStartingWith("navbar-dark").addClass("navbar-light bg-" + localStorage.getItem("logoBg"))
            } else {
                a(".sidebar .navbar").removeClassStartingWith("bg").removeClassStartingWith("navbar-light").addClass("navbar-dark bg-" + localStorage.getItem("logoBg"))
            } if (localStorage.getItem("headerBg") === "light" || localStorage.getItem("headerBg") === "white") {
                a(".header .navbar").removeClassStartingWith("bg").removeClassStartingWith("navbar-dark").addClass("navbar-light bg-" + localStorage.getItem("headerBg"))
            } else {
                a(".header .navbar").removeClassStartingWith("bg").removeClassStartingWith("navbar-light").addClass("navbar-dark bg-" + localStorage.getItem("headerBg"))
            }
        }
    };
    a.fn.setThemeTone()

    $(".btn_gotop").on("click", function () {
        $('html, body').animate({ scrollTop: 0 }, 0);
    })
})(jQuery);

function toggleFullScreen() {
    if ((document.fullScreenElement && document.fullScreenElement !== null) || (!document.mozFullScreen && !document.webkitIsFullScreen)) {
        if (document.documentElement.requestFullScreen) { document.documentElement.requestFullScreen() } else {
            if (document.documentElement.mozRequestFullScreen) {
                document.documentElement.mozRequestFullScreen()
            } else {
                if (document.documentElement.webkitRequestFullScreen) {
                    document.documentElement.webkitRequestFullScreen(Element.ALLOW_KEYBOARD_INPUT)
                }
            }
        }
    } else {
        if (document.cancelFullScreen) { document.cancelFullScreen() } else {
            if (document.mozCancelFullScreen) { document.mozCancelFullScreen() } else {
                if (document.webkitCancelFullScreen) {
                    document.webkitCancelFullScreen()
                }
            }
        }
    }
};