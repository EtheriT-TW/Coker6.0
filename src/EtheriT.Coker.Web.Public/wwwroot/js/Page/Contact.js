function PageReady() {
    (() => {
        const forms = document.querySelectorAll('.needs-validation')

        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                } else {
                    $('.btn_send').on("click", function () {
                        console.log("Click");
                    });
                }

                form.classList.add('was-validated')
            }, false)
        })
    })()
}