(function () {
    'use strict';

    function componentService(APP_CONFIG) {
        this.getUrlByTheme = function (url) {
            if (APP_CONFIG.PRESENTATION.THEME_NAME && APP_CONFIG.PRESENTATION.THEME_NAME !== '') {
                var items = url.split('.');
                items.splice(1, 0, APP_CONFIG.PRESENTATION.THEME_NAME);
                return items.join('.');
            }
            return url;
        };

        this.prepare = function (url) {
            url = url.replace('/app/', APP_CONFIG.BASE_UI_PATH);
            url = this.getUrlByTheme(url);
            return url;
        };

        this.replace = function (url) {
            return url.replace('/app/', APP_CONFIG.BASE_UI_PATH);
        };

        this.getTemplateUrl = function (component, defaultUrl) {
            var url = this.prepare(defaultUrl);

            var customUrl = APP_CONFIG.PRESENTATION.TEMPLATE_URLS[component];
            if (customUrl && customUrl !== '')
                return customUrl;

            return url;
        };

        this.getControllerUrl = function (component, defaultUrl) {
            // var url = defaultUrl.replace('/app/', APP_CONFIG.BASE_UI_PATH);
            // url = this.getUrlByTheme(url);
            // return url;

            var url = this.prepare(defaultUrl);
            var customUrl = APP_CONFIG.PRESENTATION.CONTROLLER_URLS[component];
            if (customUrl && customUrl !== '')
                return customUrl;

            return url;
        };
    };

    angular
        .module('jarvis')
        .service('componentService', componentService);
    componentService.$inject = ['APP_CONFIG'];
}());