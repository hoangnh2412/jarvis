(function () {
    'use strict';

    function organizationService(httpService) {
        var api = '/organizations';

        this.pagination = function (paging) {
            return httpService.get(api, { params: paging });
        };

        this.get = function (code) {
            return httpService.get(api + '/' + code);
        };

        this.post = function (organizationUnit) {
            return httpService.post(api, organizationUnit);
        };

        this.put = function (organizationUnit) {
            return httpService.put(api + '/' + organizationUnit.code, organizationUnit);
        };

        this.delete = function (code) {
            return httpService.delete(api + '/' + code);
        };
    };

    angular
        .module('app')
        .service('organizationService', organizationService);
    organizationService.$inject = ['httpService'];
}());