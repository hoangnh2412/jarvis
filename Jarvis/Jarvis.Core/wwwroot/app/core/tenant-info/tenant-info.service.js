(function () {
    'use strict';

    function tenantInfoService(httpService) {
        var api = '/tenant-info';

        this.get = function (id) {
            return httpService.get(api);
        };

        this.put = function (tenant) {
            return httpService.put(api, tenant);
        };
    };

    angular
        .module('core')
        .service('tenantInfoService', tenantInfoService);
    tenantInfoService.$inject = ['httpService'];
}());