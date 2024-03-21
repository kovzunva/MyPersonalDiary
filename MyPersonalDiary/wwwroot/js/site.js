$(document).ready(function () {
    // заборона абзаців
    $('.no-enter').on('input', function () {
        var currentText = $(this).val();
        var newText = currentText.replace(/\n/g, ' ');
        $(this).val(newText);
    });

    // генерація нового коду реєстрації
    $("#createNewRegistrationCode").click(function () {
        $.ajax({
            type: "POST",
            url: "/Admin/Create",
            success: function (data) {
                // Додаємо новий реєстраційний код до списку
                var listItem = $("<li></li>").text(data.link);
                $("#codeList").append(listItem);
                copyToClipboard(data.link);

                Toastify({
                    text: "Новий реєстраційний код створено та скопійовано в буфер обміну.",
                    duration: 3000,
                    destination: "#main",
                    newWindow: true,
                    close: true,
                    gravity: "bottom",
                    position: "center",
                    style: {
                        background: "var(--anti-base-color)", // Колір фону
                        color: "var(--text-color)", // Колір тексту
                        boxShadow: "var(--base-shadow)", // Тінь
                        borderRadius: "10px", // Радіус кутів
                    },
                    stopOnFocus: true,
                }).showToast();

            }
        });
    });

    $("#codeList li").click(function () {
        var dataLink = $(this).text();
        copyToClipboard(dataLink);
        Toastify({
            text: "Посилання скопійовано в буфер обміну.",
            duration: 3000,
            destination: "#main",
            newWindow: true,
            close: true,
            gravity: "bottom",
            position: "center",
            style: {
                background: "var(--anti-base-color)", // Колір фону
                color: "var(--text-color)", // Колір тексту
                boxShadow: "var(--base-shadow)", // Тінь
                borderRadius: "10px", // Радіус кутів
            },
            stopOnFocus: true,
        }).showToast();
    });

    function copyToClipboard(text) {
        var textarea = $("<textarea></textarea>").val(text).appendTo('body').select();
        document.execCommand("copy");
        textarea.remove();
    }

});
