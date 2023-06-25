﻿"use strict";
let isRtl = window.Helpers.isRtl(),
    isDarkStyle = window.Helpers.isDarkStyle(),
    menu,
    animate,
    isHorizontalLayout = !1;
document.getElementById("layout-menu") && (isHorizontalLayout = document.getElementById("layout-menu").classList.contains("menu-horizontal")),
    function () {
        document.querySelectorAll("#layout-menu").forEach(function (e) {
            menu = new Menu(e, {
                orientation: isHorizontalLayout ? "horizontal" : "vertical",
                closeChildren: !!isHorizontalLayout,
                showDropdownOnHover: localStorage.getItem("templateCustomizer-" + templateName + "--ShowDropdownOnHover") ? "true" === localStorage.getItem("templateCustomizer-" + templateName + "--ShowDropdownOnHover") : void 0 === window.templateCustomizer || window.templateCustomizer.settings.defaultShowDropdownOnHover
            }),
                window.Helpers.scrollToActive(animate = !1),
                window.Helpers.mainMenu = menu
        });
        document.querySelectorAll(".layout-menu-toggle").forEach(e => {
            e.addEventListener("click", e => {
                if (e.preventDefault(), window.Helpers.toggleCollapsed(), config.enableMenuLocalStorage && !window.Helpers.isSmallScreen())
                    try {
                        localStorage.setItem("templateCustomizer-" + templateName + "--LayoutCollapsed", String(window.Helpers.isCollapsed()))
                    } catch (e) { }
            })
        });
        if (document.getElementById("layout-menu")) {
            var t = document.getElementById("layout-menu");
            var n = function () {
                Helpers.isSmallScreen() || document.querySelector(".layout-menu-toggle").classList.add("d-block")
            };
            let e = null;
            t.onmouseenter = function () {
                e = Helpers.isSmallScreen() ? setTimeout(n, 0) : setTimeout(n, 300)
            },
                t.onmouseleave = function () {
                    document.querySelector(".layout-menu-toggle").classList.remove("d-block"),
                        clearTimeout(e)
                }
        }
        window.Helpers.swipeIn(".drag-target", function (e) {
            window.Helpers.setCollapsed(!1)
        }),
            window.Helpers.swipeOut("#layout-menu", function (e) {
                window.Helpers.isSmallScreen() && window.Helpers.setCollapsed(!0)
            });
        let e = document.getElementsByClassName("menu-inner"),
            o = document.getElementsByClassName("menu-inner-shadow")[0];
        0 < e.length && o && e[0].addEventListener("ps-scroll-y", function () {
            this.querySelector(".ps__thumb-y").offsetTop ? o.style.display = "block" : o.style.display = "none"
        });
        t = document.querySelector(".style-switcher-toggle");
        function s(n) {
            [].slice.call(document.querySelectorAll("[data-app-" + n + "-img]")).map(function (e) {
                var t = e.getAttribute("data-app-" + n + "-img");
                e.src = assetsPath + "img/" + t
            })
        }
        window.templateCustomizer ? (t && t.addEventListener("click", function () {
            window.Helpers.isLightStyle() ? window.templateCustomizer.setStyle("dark") : window.templateCustomizer.setStyle("light")
        }), window.Helpers.isLightStyle() ? (t && (t.querySelector("i").classList.add("bx-moon"), new bootstrap.Tooltip(t, {
            title: "Dark mode",
            fallbackPlacements: ["bottom"]
        })), s("light")) : (t && (t.querySelector("i").classList.add("bx-sun"), new bootstrap.Tooltip(t, {
            title: "Light mode",
            fallbackPlacements: ["bottom"]
        })), s("dark"))) : t.parentElement.remove(),
            "undefined" != typeof i18next && "undefined" != typeof i18NextHttpBackend && i18next.use(i18NextHttpBackend).init({
                lng: "en",
                debug: !1,
                fallbackLng: "en",
                backend: {
                    loadPath: assetsPath + "json/locales/{{lng}}.json"
                },
                returnObjects: !0
            }).then(function (e) {
                i()
            });
        let a = document.getElementsByClassName("dropdown-language");
        if (a.length) {
            var l = a[0].querySelectorAll(".dropdown-item");
            for (let e = 0; e < l.length; e++)
                l[e].addEventListener("click", function () {
                    let e = this.getAttribute("data-language"),
                        t = this.querySelector(".fi").getAttribute("class"),
                        n = t.split(" ").filter(function (e) {
                            return 0 !== e.lastIndexOf("fs-", 0)
                        });
                    t = n.join(" ").trim() + " fs-3";
                    for (var o of this.parentNode.children)
                        o.classList.remove("selected");
                    this.classList.add("selected"),
                        a[0].querySelector(".dropdown-toggle .fi").className = t,
                        i18next.changeLanguage(e, (e, t) => {
                            if (e)
                                return console.log("something went wrong loading", e);
                            i()
                        })
                })
        }
        function i() {
            var e = document.querySelectorAll("[data-i18n]"),
                t = document.querySelector('.dropdown-item[data-language="' + i18next.language + '"]');
            t && t.click(),
                e.forEach(function (e) {
                    e.innerHTML = i18next.t(e.dataset.i18n)
                })
        }
        var r = document.querySelector(".dropdown-notifications-all");
        function c(e) {
            "show.bs.collapse" == e.type || "show.bs.collapse" == e.type ? e.target.closest(".accordion-item").classList.add("active") : e.target.closest(".accordion-item").classList.remove("active")
        }
        const d = document.querySelectorAll(".dropdown-notifications-read");
        r && r.addEventListener("click", e => {
            d.forEach(e => {
                e.closest(".dropdown-notifications-item").classList.add("marked-as-read")
            })
        }),
            d && d.forEach(t => {
                t.addEventListener("click", e => {
                    t.closest(".dropdown-notifications-item").classList.toggle("marked-as-read")
                })
            }),
            document.querySelectorAll(".dropdown-notifications-archive").forEach(t => {
                t.addEventListener("click", e => {
                    t.closest(".dropdown-notifications-item").remove()
                })
            }),
            [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]')).map(function (e) {
                return new bootstrap.Tooltip(e)
            });
        [].slice.call(document.querySelectorAll(".accordion")).map(function (e) {
            e.addEventListener("show.bs.collapse", c),
                e.addEventListener("hide.bs.collapse", c)
        });
        if (isRtl && Helpers._addClass("dropdown-menu-end", document.querySelectorAll("#layout-navbar .dropdown-menu")), window.Helpers.setAutoUpdate(!0), window.Helpers.initPasswordToggle(), window.Helpers.initSpeechToText(), window.Helpers.initNavbarDropdownScrollbar(), window.addEventListener("resize", function (e) {
            window.innerWidth >= window.Helpers.LAYOUT_BREAKPOINT && document.querySelector(".search-input-wrapper") && (document.querySelector(".search-input-wrapper").classList.add("d-none"), document.querySelector(".search-input").value = ""),
                document.querySelector("[data-template^='horizontal-menu']") && setTimeout(function () {
                    window.innerWidth < window.Helpers.LAYOUT_BREAKPOINT ? document.getElementById("layout-menu") && document.getElementById("layout-menu").classList.contains("menu-horizontal") && menu.switchMenu("vertical") : document.getElementById("layout-menu") && document.getElementById("layout-menu").classList.contains("menu-vertical") && menu.switchMenu("horizontal")
                }, 100)
        }, !0), !isHorizontalLayout && !window.Helpers.isSmallScreen() && ("undefined" != typeof TemplateCustomizer && window.templateCustomizer.settings.defaultMenuCollapsed && window.Helpers.setCollapsed(!0, !1), "undefined" != typeof config) && config.enableMenuLocalStorage)
            try {
                null !== localStorage.getItem("templateCustomizer-" + templateName + "--LayoutCollapsed") && "false" !== localStorage.getItem("templateCustomizer-" + templateName + "--LayoutCollapsed") && window.Helpers.setCollapsed("true" === localStorage.getItem("templateCustomizer-" + templateName + "--LayoutCollapsed"), !1)
            } catch (e) { }
    }(),
    "undefined" != typeof $ && $(function () {
        window.Helpers.initSidebarToggle();
        var t,
            n,
            e,
            o = $(".search-toggler"),
            s = $(".search-input-wrapper"),
            a = $(".search-input"),
            l = $(".content-backdrop");
        o.length && o.on("click", function () {
            s.length && (s.toggleClass("d-none"), a.focus())
        }),
            $(document).on("keydown", function (e) {
                var t = e.ctrlKey,
                    e = 191 === e.which;
                t && e && s.length && (s.toggleClass("d-none"), a.focus())
            }),
            a.on("focus", function () {
                s.hasClass("container-xxl") && s.find(".twitter-typeahead").addClass("container-xxl")
            }),
            a.length && (t = function (o) {
                return function (t, e) {
                    let n;
                    n = [],
                        o.filter(function (e) {
                            if (e.name.toLowerCase().startsWith(t.toLowerCase()))
                                n.push(e);
                            else {
                                if (e.name.toLowerCase().startsWith(t.toLowerCase()) || !e.name.toLowerCase().includes(t.toLowerCase()))
                                    return [];
                                n.push(e),
                                    n.sort(function (e, t) {
                                        return t.name < e.name ? 1 : -1
                                    })
                            }
                        }),
                        e(n)
                }
            }, o = "search-vertical.json", $("#layout-menu").hasClass("menu-horizontal") && (o = "search-horizontal.json"), n = $.ajax({
                url: assetsPath + "json/" + o,
                dataType: "json",
                async: !1
            }).responseJSON, a.each(function () {
                var e = $(this);
                a.typeahead({
                    hint: !1,
                    classNames: {
                        menu: "tt-menu navbar-search-suggestion",
                        cursor: "active",
                        suggestion: "suggestion d-flex justify-content-between px-3 py-2 w-100"
                    }
                }, {
                    name: "pages",
                    display: "name",
                    limit: 5,
                    source: t(n.pages),
                    templates: {
                        header: '<h6 class="suggestions-header text-primary mb-0 mx-3 mt-3 pb-2">Pages</h6>',
                        suggestion: function ({ url: e, icon: t, name: n }) {
                            return '<a href="' + e + '"><div><i class="bx ' + t + ' me-2"></i><span class="align-middle">' + n + "</span></div></a>"
                        },
                        notFound: '<div class="not-found px-3 py-2"><h6 class="suggestions-header text-primary mb-2">Pages</h6><p class="py-2 mb-0"><i class="bx bx-error-circle bx-xs me-2"></i> No Results Found</p></div>'
                    }
                }, {
                    name: "files",
                    display: "name",
                    limit: 4,
                    source: t(n.files),
                    templates: {
                        header: '<h6 class="suggestions-header text-primary mb-0 mx-3 mt-3 pb-2">Files</h6>',
                        suggestion: function ({ src: e, name: t, subtitle: n, meta: o }) {
                            return '<a href="javascript:;"><div class="d-flex w-50"><img class="me-3" src="' + assetsPath + e + '" alt="' + t + '" height="32"><div class="w-75"><h6 class="mb-0">' + t + '</h6><small class="text-muted">' + n + '</small></div></div><small class="text-muted">' + o + "</small></a>"
                        },
                        notFound: '<div class="not-found px-3 py-2"><h6 class="suggestions-header text-primary mb-2">Files</h6><p class="py-2 mb-0"><i class="bx bx-error-circle bx-xs me-2"></i> No Results Found</p></div>'
                    }
                }, {
                    name: "members",
                    display: "name",
                    limit: 4,
                    source: t(n.members),
                    templates: {
                        header: '<h6 class="suggestions-header text-primary mb-0 mx-3 mt-3 pb-2">Members</h6>',
                        suggestion: function ({ name: e, src: t, subtitle: n }) {
                            return '<a href="app-user-view-account.html"><div class="d-flex align-items-center"><img class="rounded-circle me-3" src="' + assetsPath + t + '" alt="' + e + '" height="32"><div class="user-info"><h6 class="mb-0">' + e + '</h6><small class="text-muted">' + n + "</small></div></div></a>"
                        },
                        notFound: '<div class="not-found px-3 py-2"><h6 class="suggestions-header text-primary mb-2">Members</h6><p class="py-2 mb-0"><i class="bx bx-error-circle bx-xs me-2"></i> No Results Found</p></div>'
                    }
                }).bind("typeahead:render", function () {
                    l.addClass("show").removeClass("fade")
                }).bind("typeahead:select", function (e, t) {
                    t.url && (window.location = t.url)
                }).bind("typeahead:close", function () {
                    a.val(""),
                        e.typeahead("val", ""),
                        s.addClass("d-none"),
                        l.addClass("fade").removeClass("show")
                }),
                    a.on("keyup", function () {
                        "" == a.val() && l.addClass("fade").removeClass("show")
                    })
            }), $(".navbar-search-suggestion").each(function () {
                e = new PerfectScrollbar($(this)[0], {
                    wheelPropagation: !1,
                    suppressScrollX: !0
                })
            }), a.on("keyup", function () {
                e.update()
            }))
    });