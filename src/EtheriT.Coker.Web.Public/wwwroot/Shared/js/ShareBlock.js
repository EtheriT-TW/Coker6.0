function ShareBlockInit() {
    $('.shareBlock').each((idx, $share) => {
        $this = $($share);
        if (typeof ($this.data("init")) == "undefined" || !$this.data("init")) {
            var href = "";
            if (typeof ($this.data("href")) == "string") {
                href = $this.data("href");
            } else href = location.pathname;
            $this.cShare({
                description: 'jQuery plugin - C Share buttons...',
                showButtons: ['email', 'plurk', 'twitter', 'fb', 'line'],
                href: href,
                orgName: typeof OrgName === "undefined" ? "" : OrgName,
                i18n: {
                    shareToText: local.shareTo,
                    confirmTitle: local.MailtoHintTitle,
                    confirmHtml: local.MailtoHintBody,
                    okText: local.MailtoHintOk,
                    cancelText: local.MailtoHintCancel,
                    mobileTitle: local.MailtoHintMobileTitle,
                    mobileText: local.MailtoHintMobileBody
                },
                email: {
                    desktopConfirm: true,
                    mobileHint: true,
                    // 你想要 subject 明確：用頁面 title
                    subject: null, // null 表示由 plugin 自己抓 page title
                    // body 內容要不要含 description
                    includeDescriptionInBody: true
                }
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