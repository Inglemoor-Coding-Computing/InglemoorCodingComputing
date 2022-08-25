window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
    updateTheme();
});
updateTheme();

function updateTheme() {
    if (localStorage.theme === 'dark' || (!('theme' in localStorage) && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
        document.documentElement.classList.add('dark')
        document.querySelector(`link[title="dark"]`).removeAttribute("disabled");
        document.querySelector(`link[title="light"]`).setAttribute("disabled", "disabled");
    }
    else {
        document.documentElement.classList.remove('dark')
        document.querySelector(`link[title="light"]`).removeAttribute("disabled");
        document.querySelector(`link[title="dark"]`).setAttribute("disabled", "disabled");
    }
    highlightSnippet();
}

export function setTheme(theme) {
    if (theme === 'system')
        localStorage.removeItem('theme')
    else
        localStorage.theme = theme;
    updateTheme();

}

export function getTheme() {
    updateTheme();
    if ('theme' in localStorage)
        return localStorage.theme;
    return 'system';
}
