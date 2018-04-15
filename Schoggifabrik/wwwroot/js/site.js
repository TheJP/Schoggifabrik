'use strict';

// Setup code editor
(() => {
    const textarea = document.getElementById('code');
    textarea.value = textarea.value.trim() + '\n';
    const editor = CodeMirror.fromTextArea(textarea, {
        lineNumbers: true,
        matchBrackets: true,
        theme: 'material'
    });

    editor.focus();
    editor.setCursor(editor.lineCount(), 0);
})();

$(document).ready(() => {
    let timer = 0;

    const hideAllIcons = () => ['.checkmark', '.glyphicon-send', '.glyphicon-refresh']
        .forEach(icon => $(`#code-submit ${icon}`).css('display', 'none'));

    const showDefaultIcon = () => {
        clearTimeout(timer);
        hideAllIcons();
        $('#code-submit .glyphicon-send').css('display', 'inline-block');
    };

    const showSendingIcon = () => {
        clearTimeout(timer);
        hideAllIcons();
        $('#code-submit .glyphicon-refresh').css('display', 'inline-block');
    };

    const showSentIcon = () => {
        clearTimeout(timer);
        hideAllIcons();
        $('#code-submit .checkmark').css('display', 'inline-block');
        timer = setTimeout(() => showDefaultIcon(), 2000);
    };

    $('#code-form').submit(function (e) {
        e.preventDefault();

        showSendingIcon();

        const $this = $(this);
        $.ajax({
            type: $this.attr('method'),
            url: $this.attr('action'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: $this.serialize(),
            success: data => {
                console.log(data); // should be js object
                showSentIcon();
            },
            error: (request, error, exception) => {
                if (error === 'error') {
                    alert(`Konnte Code nicht senden: ${request.responseJSON.error}`);
                } else {
                    alert(`Unerwarteter Fehler beim Senden: '${error}', '${exception}'`);
                }
                showDefaultIcon();
            },
        });
    });

    $('#show-result').click(() => $('#result-section').stop().animate({ width: 'toggle', height: 'toggle' }, 200));

    $('#minimise-result').click(() => {
        $('#result-section').stop().animate({ width: 0, height: 0 }, 200, function () {
            $(this).css('width', '').css('height', '').css('display', 'none');
        });
    });

    $('#refresh-result').click(function () { $(this).children('.glyphicon').toggleClass('spinning'); });
});
