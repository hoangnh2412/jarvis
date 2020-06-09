(function () {
    'use strict';

    function componentService(APP_CONFIG) {
        this.getJarvisTemplateUrl = function (component, defaultUrl) {
            var url = APP_CONFIG.TEMPLATE_URLS[component];
            if (!url || url === '')
                url = defaultUrl;

            if (APP_CONFIG.BASE_PATH)
                url = APP_CONFIG.BASE_PATH + url;

            url = url.replace('/app/', APP_CONFIG.BASE_PATH_JARVIS);
            return url;
        };

        this.getJarvisControllerUrl = function (component, defaultUrl) {
            var url = APP_CONFIG.CONTROLLER_URLS[component];
            if (!url || url === '')
                url = defaultUrl;

            if (APP_CONFIG.BASE_PATH)
                url = APP_CONFIG.BASE_PATH + url;

            url = url.replace('/app/', APP_CONFIG.BASE_PATH_JARVIS);
            return url;
        };

        this.getTemplateUrl = function (component, defaultUrl) {
            var url = APP_CONFIG.TEMPLATE_URLS[component];
            if (!url || url === '')
                url = defaultUrl;

            if (APP_CONFIG.BASE_PATH)
                url = APP_CONFIG.BASE_PATH + url;
            return url;
        };

        this.getControllerUrl = function (component, defaultUrl) {
            var url = APP_CONFIG.CONTROLLER_URLS[component];
            if (!url || url === '')
                url = defaultUrl;

            if (APP_CONFIG.BASE_PATH)
                url = APP_CONFIG.BASE_PATH + url;
            return url;
        };
    };

    angular
        .module('jarvis')
        .service('componentService', componentService);
    componentService.$inject = ['APP_CONFIG'];
}());