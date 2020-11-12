(function () {
    'use strict';

    function organizationService(httpService) {
        var api = '/organizations';

        this.getUnits = function () {
            return httpService.get(api);
        };

        this.getUnit = function (code) {
            return httpService.get(api + '/' + code);
        };

        this.postUnit = function (organizationUnit) {
            return httpService.post(api, organizationUnit);
        };

        this.putUnit = function (organizationUnit) {
            return httpService.put(api + '/' + organizationUnit.code, organizationUnit);
        };

        this.deleteUnit = function (code) {
            return httpService.delete(api + '/' + code);
        };

        this.moveUnit = function (sourceCode, parentCode, leftCode) {
            return httpService.put(api + '/' + sourceCode + '/move', {}, {
                params: {
                    parentCode: parentCode,
                    leftCode: leftCode
                }
            });
        };

        this.getUsersInUnit = function (code, paging) {
            return httpService.get(api + '/' + code + '/members', { params: paging });
        };

        this.getUsersNotInUnit = function (code, paging) {
            return httpService.get(api + '/' + code + '/users', { params: paging });
        };

        this.postUser = function (code, userCode) {
            return httpService.post(api + '/' + code + '/user/' + userCode);
        };

        this.postUsers = function (code, userCodes) {
            return httpService.post(api + '/' + code + '/users', userCodes);
        };

        this.deleteUser = function (code, userCode) {
            return httpService.delete(api + '/' + code + '/user/' + userCode);
        };
    };

    angular
        .module('app')
        .service('organizationService', organizationService);
    organizationService.$inject = ['httpService'];
}());