(function () {
    'use strict';

    function settingService(httpService) {
        var api = '/settings';

        this.get = function () {
            return httpService.get(api);
        };

        this.post = function (group, settings) {
            return httpService.post(api + '/' + group, settings);
        };
    };

    angular
        .module('core')
        .service('settingService', settingService);
    settingService.$inject = ['httpService'];
}());