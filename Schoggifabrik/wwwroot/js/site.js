(() => {
    const textarea = document.getElementById("code");
    textarea.value = textarea.value.trim();
    const editor = CodeMirror.fromTextArea(document.getElementById("code"), {
        lineNumbers: true,
        matchBrackets: true,
        theme: "material"
    });
})();
