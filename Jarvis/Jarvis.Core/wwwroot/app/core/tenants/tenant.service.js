(function () {
    'use strict';

    function tenantService(httpService) {
        var api = '/tenants';

        this.get = function (code) {
            return httpService.get(api + '/' + code);
        };

        this.paging = function (paging) {
            return httpService.get(api, { params: paging });
        };

        this.post = function (tenant) {
            return httpService.post(api, tenant);
        };

        this.put = function (tenant) {
            return httpService.put(api + '/' + tenant.code, tenant);
        };

        this.delete = function (code) {
            return httpService.delete(api + '/' + code);
        };

        this.setDefault = function (code) {
            return httpService.post('/tenants/catalog/' + code);
        };
    };

    angular
        .module('core')
        .service('tenantService', tenantService);
    tenantService.$inject = ['httpService'];
}());