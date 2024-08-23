document.addEventListener('DOMContentLoaded', function () {
    var savedTheme = localStorage.getItem('selectedTheme') || 'default';
    var themeLink = document.getElementById('theme-link');

    // Set the initial theme
    if (savedTheme !== 'default') {
        themeLink.setAttribute('href', '/css/' + savedTheme + '-theme.css');
    }

    // Set the current theme in the dropdown
    document.getElementById('themeSelect').value = savedTheme;

    // Handle theme change
    document.getElementById('themeSelect').addEventListener('change', function () {
        var selectedTheme = this.value;
        if (selectedTheme === 'default') {
            themeLink.setAttribute('href', ''); // Reset to default theme
        } else {
            themeLink.setAttribute('href', '/css/' + selectedTheme + '-theme.css');
        }
        localStorage.setItem('selectedTheme', selectedTheme);
    });
});
