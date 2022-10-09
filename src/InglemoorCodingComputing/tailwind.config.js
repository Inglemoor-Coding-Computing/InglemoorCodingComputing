const defaultTheme = require('tailwindcss/defaultTheme')

module.exports = {
    content: ["./**/*.{razor,html,cshtml}"],
    darkMode: 'class',
    theme: {
        extend: {
            fontFamily: {
                'sans': ['"Inter"', ...defaultTheme.fontFamily.sans],
                'mono': ['"Inconsolata"', ...defaultTheme.fontFamily.mono],
                'code': ['"Cascadia Code"']
            },
        },
    },
    plugins: [
        require('@tailwindcss/typography')
    ],
}
