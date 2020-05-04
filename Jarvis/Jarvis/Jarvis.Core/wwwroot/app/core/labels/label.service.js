(function () {
    'use strict';

    function labelService(httpService) {
        var api = '/labels';

        this.getById = function (code) {
            return httpService.get(api + '/' + code);
        };

        this.get = function (paging) {
            return httpService.get(api, { params: paging });
        };

        this.post = function (label) {
            return httpService.post(api, label);
        };

        this.put = function (label) {
            return httpService.put(api + '/' + label.code, label);
        };

        this.delete = function (code) {
            return httpService.delete(api + '/' + code);
        };
    };

    angular
        .module('app')
        .service('labelService', labelService);
    labelService.$inject = ['httpService'];
}());