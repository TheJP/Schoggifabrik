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
    const resultTransitionTime = 200;

    let timer = 0;
    let sentCodeSubmission = false;
    let refreshingResults = false;

    const hideAllIcons = () => ['.checkmark', '.glyphicon-send', '.glyphicon-refresh']
        .forEach(icon => $(`#code-submit ${icon}`).css('display', 'none'));

    const clearIconTimeout = () => {
        if (timer != 0) {
            clearTimeout(timer);
            timer = 0;
        }
    };

    const showDefaultIcon = () => {
        clearIconTimeout();
        hideAllIcons();
        $('#code-submit .glyphicon-send').css('display', 'inline-block');
    };

    const showLoadingIcon = () => {
        clearIconTimeout();
        hideAllIcons();
        $('#code-submit .glyphicon-refresh').css('display', 'inline-block');
    };

    const showDoneIcon = () => {
        clearIconTimeout();
        hideAllIcons();
        $('#code-submit .checkmark').css('display', 'inline-block');
        timer = setTimeout(() => { timer = 0; showDefaultIcon(); }, 5000);
    };

    const resetSendCodeStatus = () => {
        sentCodeSubmission = false;
        $('#code-submit').prop('disabled', false);
        showDoneIcon();
    };

    $('#code-form').submit(function (e) {
        e.preventDefault();

        if (sentCodeSubmission) { return; }
        sentCodeSubmission = true;
        $('#code-submit').prop('disabled', true);

        showLoadingIcon();

        const $this = $(this);
        $.ajax({
            type: $this.prop('method'),
            url: $this.prop('action'),
            dataType: 'json',
            data: $this.serialize(),
            success: data => {
                refreshResults();
                const $resultSection = $('#result-section');
                if ($resultSection.css('display') === 'none') {
                    $resultSection.stop().animate({ width: 'toggle', height: 'toggle' }, resultTransitionTime);
                }
            },
            error: (request, error, exception) => {
                if (error === 'error') {
                    alert(`Konnte Code nicht senden: ${request.responseJSON.error}`);
                } else {
                    alert(`Unerwarteter Fehler beim Senden: '${error}', '${exception}'`);
                    console.error('Unepxpected error while sending code', request, error, exception);
                }
                resetSendCodeStatus();
                showDefaultIcon();
            },
        });
    });

    const displayResults = results => {
        const resultSection = $('#results');
        resultSection.children().remove('.result');
        results.forEach(result => resultSection.append(
`<div class="result">
    <div class="result-status status-${result.status.toLowerCase()}">${result.statusText}</div>
    <div class="result-name" title="Task">${result.name}</div>
</div>`
        ));
    };

    const resetRefreshResults = () => setTimeout(() => {
        refreshingResults = false;
        $('#refresh-result .glyphicon').removeClass('spinning');
    }, 1000);

    const refreshResults = () => {
        if (refreshingResults) { return; }
        refreshingResults = true;
        $('#refresh-result .glyphicon').addClass('spinning');

        $.ajax({
            type: 'get',
            url: '/Home/Results',
            dataType: 'json',
            success: data => {
                resetRefreshResults();
                displayResults(data);
                if (data.find(result => result.status.toLowerCase() === 'pending') !== undefined) {
                    setTimeout(refreshResults, 3000); // Refresh result view after 3s if there are pending results
                } else {
                    resetSendCodeStatus();
                }
            },
            error: (request, error, exception) => {
                console.error(`Unexpected error while polling results: '${error}', '${exception}'`);
                resetRefreshResults();
            },
        });
    };

    $('#show-result').click(() => {
        $('#result-section').stop().animate({ width: 'toggle', height: 'toggle' }, resultTransitionTime);
        refreshResults();
    });

    $('#minimise-result').click(() => {
        $('#result-section').stop().animate({ width: 0, height: 0 }, resultTransitionTime, function () {
            $(this).css('width', '').css('height', '').css('display', 'none');
        });
    });

    $('#refresh-result').click(refreshResults);
});
