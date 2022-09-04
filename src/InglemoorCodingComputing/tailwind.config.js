const defaultTheme = require('tailwindcss/defaultTheme')

module.exports = {
    content: ["./**/*.{razor,html,cshtml}"],
    darkMode: 'class',
    theme: {
        extend: {
            fontFamily: {
                'sans': ['"Inter"', ...defaultTheme.fontFamily.sans],
                'code': ['"Cascadia Code"']
            },
        },
    },
    plugins: [
        require('@tailwindcss/typography')
    ],
}
