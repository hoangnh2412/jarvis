(function () {
    'use strict';

    function componentService(APP_CONFIG) {
        this.getTemplateUrl = function (component, defaultUrl) {
            var url = APP_CONFIG.TEMPLATE_URLS[component];
            if (!url || url === '')
                url = defaultUrl;

            url = url.replace('/app/', APP_CONFIG.BASE_UI_PATH);
            return url;
        };

        this.getControllerUrl = function (component, defaultUrl) {
            var url = APP_CONFIG.CONTROLLER_URLS[component];
            if (!url || url === '')
                url = defaultUrl;

            url = url.replace('/app/', APP_CONFIG.BASE_UI_PATH);
            return url;
        };
    };

    angular
        .module('jarvis')
        .service('componentService', componentService);
    componentService.$inject = ['APP_CONFIG'];
}());