(function () {
    'use strict';

    function baseService(httpService) {
        this.endpoint = '';

        this.get = function (key) {
            return httpService.get(this.endpoint + '/' + key);
        };

        this.pagination = function (paging) {
            return httpService.get(this.endpoint, { params: paging });
        };

        this.post = function (item) {
            return httpService.post(this.endpoint, item);
        };

        this.put = function (item) {
            return httpService.put(this.endpoint + '/' + item.key, item);
        };

        this.delete = function (key) {
            return httpService.delete(this.endpoint + '/' + key);
        };
    };

    angular
        .module('core')
        .service('baseService', baseService);
    baseService.$inject = ['httpService'];
}());