﻿window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
    updateTheme();
});
updateTheme();

function updateTheme() {
    var replacement = '<svg xmlns="http://www.w3.org/2000/svg" id="theme-toggle" class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d = "M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" /></svg>';
    if (localStorage.theme === 'dark' || (!('theme' in localStorage) && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
        document.documentElement.classList.add('dark')
        document.querySelector(`link[title="dark"]`).removeAttribute("disabled");
        document.querySelector(`link[title="light"]`).setAttribute("disabled", "disabled");
    }
    else {
        document.documentElement.classList.remove('dark')
        document.querySelector(`link[title="light"]`).removeAttribute("disabled");
        document.querySelector(`link[title="dark"]`).setAttribute("disabled", "disabled");
        replacement = '<svg xmlns="http://www.w3.org/2000/svg" id="theme-toggle" class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d = "M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"/></svg>';
    }
    var themeToggle = document.getElementById('theme-toggle');
    if (themeToggle && themeToggle.outerHTML) {
        themeToggle.outerHTML = replacement;
    }
    highlightSnippet();
}

function setTheme(theme) {
    if (theme === 'system')
        localStorage.removeItem('theme')
    else
        localStorage.theme = theme;
    updateTheme();
}

function toggleThemeButtonClicked() {
    console.log("help");
    var theme = getTheme();
    if (theme === 'dark')
        localStorage.theme = "light";
    else
        localStorage.theme = 'dark';
    updateTheme();
}

function getTheme() {
    if ('theme' in localStorage)
        return localStorage.theme;
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

