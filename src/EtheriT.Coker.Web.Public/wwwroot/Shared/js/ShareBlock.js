function ShareBlockInit() {
    $('.shareBlock').each((idx, $share) => {
        $this = $($share);
        if (typeof ($this.data("init")) == "undefined" || !$this.data("init")) {
            var href = "";
            if (typeof ($this.data("href")) == "string") {
                href = $this.data("href");
            }
            $this.cShare({
                description: 'jQuery plugin - C Share buttons...',
                showButtons: ['email', 'plurk', 'twitter', 'fb', 'line'],
                shareToText: "分享至",
                href: href
            });
            $this.hover(ProShare);
            $this.data("init", true);
        }
    });
}
function ProShare() {
    var $self = $(this);
    $self.toggleClass('show');
}