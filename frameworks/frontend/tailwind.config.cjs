/** @type {import('tailwindcss').Config} */
module.exports = {
  presets: [require('./tailwind.preset.cjs')],
  content: ['./src/**/*.{js,ts,jsx,tsx}', './dist/**/*.{js,cjs,mjs}'],
  plugins: [],
}
