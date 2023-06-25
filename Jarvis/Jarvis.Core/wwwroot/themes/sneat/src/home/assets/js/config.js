﻿"use strict";
let config = {
    colors: {
        primary: "#696cff",
        secondary: "#8592a3",
        success: "#71dd37",
        info: "#03c3ec",
        warning: "#ffab00",
        danger: "#ff3e1d",
        dark: "#233446",
        black: "#000",
        white: "#fff",
        cardColor: "#fff",
        bodyBg: "#f5f5f9",
        bodyColor: "#697a8d",
        headingColor: "#566a7f",
        textMuted: "#a1acb8",
        borderColor: "#eceef1"
    },
    colors_label: {
        primary: "#666ee81a",
        secondary: "#8897aa1a",
        success: "#28d0941a",
        info: "#1e9ff21a",
        warning: "#ff91491a",
        danger: "#ff49611a",
        dark: "#181c211a"
    },
    colors_dark: {
        cardColor: "#2b2c40",
        bodyBg: "#232333",
        bodyColor: "#a3a4cc",
        headingColor: "#cbcbe2",
        textMuted: "#7071a4",
        borderColor: "#444564"
    },
    enableMenuLocalStorage: !0
},
    assetsPath = document.querySelector("[data-assets-path]").getAttribute("data-assets-path"),
    templateName = document.querySelector("[data-template]")?.getAttribute("data-template"),
    rtlSupport = !0;

"undefined" != typeof TemplateCustomizer && (window.templateCustomizer = new TemplateCustomizer({
    console.log(assetsPath);
    cssPath: assetsPath + "vendor/css" + (rtlSupport ? "/rtl" : "") + "/",
    themesPath: assetsPath + "vendor/css" + (rtlSupport ? "/rtl" : "") + "/",
    displayCustomizer: !0,
    defaultShowDropdownOnHover: !0
}));

