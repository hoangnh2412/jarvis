(function () {
    'use strict';

    function roleService(httpService) {
        var api = '/roles';

        this.get = function (paging) {
            return httpService.get(api, { params: paging });
        };

        this.getById = function (id) {
            return httpService.get(api + '/' + id);
        };

        this.getClaims = function (id) {
            if (id === undefined) {
                return httpService.get(api + '/claims');
            } else {
                return httpService.get(api + '/claims/' + id);
            }
        };

        this.post = function (role) {
            return httpService.post(api, role);
        };

        this.put = function (role) {
            return httpService.put(api + '/' + role.id, role);
        };

        this.delete = function (id) {
            return httpService.delete(api + '/' + id);
        };
    };

    angular
        .module('identity')
        .service('roleService', roleService);
    roleService.$inject = ['httpService'];
}());