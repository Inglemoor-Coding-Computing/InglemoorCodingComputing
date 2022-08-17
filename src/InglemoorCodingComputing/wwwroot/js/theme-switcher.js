window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
    updateTheme();
});
updateTheme();

function updateTheme() {
    if (localStorage.theme === 'dark' || (!('theme' in localStorage) && window.matchMedia('(prefers-color-scheme: dark)').matches))
        document.documentElement.classList.add('dark')
    else
        document.documentElement.classList.remove('dark')
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
