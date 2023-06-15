function ShareBlockInit() {
    $('.shareBlock').each(function () {
        if (typeof ($(this).data("init")) == "undefined" || !$(this).data("init")) {
            $(this).cShare({
                description: 'jQuery plugin - C Share buttons...',
                showButtons: ['email', 'plurk', 'twitter', 'fb', 'line']
            });
            $(this).data("init", true);
        }
    });
}