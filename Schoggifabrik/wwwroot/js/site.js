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
