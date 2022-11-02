function PageReady() {
    const forms = $('#ContactForm');

    (() => {
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    event.preventDefault();
                    Coker.sweet.success("成功送出表單！", null, true);
                    setTimeout(function () {
                        $("#ContactForm").submit();
                    }, 1500);
                }
                form.classList.add('was-validated')
            }, false)
        })
    })()
}