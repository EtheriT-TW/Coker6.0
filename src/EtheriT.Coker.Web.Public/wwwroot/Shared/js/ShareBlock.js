function ShareBlockInit() {
    $('.shareBlock').each(function () {
        if (typeof ($(this).data("init")) == "undefined" || !$(this).data("init")) {
            $(this).cShare({
                description: 'jQuery plugin - C Share buttons...',
                showButtons: ['email', 'plurk', 'twitter', 'fb', 'line'],
                shareToText: "分享至"
            });
            $(this).hover(ProShare);
            $(this).data("init", true);
        }
    });
}
function ProShare() {
    var $self = $(this);
    $self.toggleClass('show');
}