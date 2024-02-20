$(document).ready(function () {
    // заборона абзаців
    $('.no-enter').on('input', function () {
        var currentText = $(this).val();
        var newText = currentText.replace(/\n/g, ' ');
        $(this).val(newText);
    });
});
