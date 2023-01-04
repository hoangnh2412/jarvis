(function () {
    'use strict';

    function userService(httpService) {
        var api = '/users';

        this.get = function (paging) {
            return httpService.get(api, { params: paging });
        };

        this.getById = function (id) {
            return httpService.get(api + '/' + id);
        };

        this.post = function (user) {
            return httpService.post(api, user);
        };

        this.put = function (user) {
            return httpService.put(api + '/' + user.id, user);
        };

        this.delete = function (id) {
            return httpService.delete(api + '/' + id);
        };


        this.lock = function (id, time) {
            if (time === undefined) {
                return httpService.patch(api + '/' + id + '/lock');
            } else {
                return httpService.patch(api + '/' + id + '/lock/' + time);
            }
        };

        this.unlock = function (id, time) {
            if (time === undefined) {
                return httpService.patch(api + '/' + id + '/unlock');
            } else {
                return httpService.patch(api + '/' + id + '/unlock/' + time);
            }
        };

        this.resetPassword = function (id, emails) {
            return httpService.put(api + '/reset-password/' + id, emails);
        };


        this.getRoles = function (paging) {
            return httpService.get(api + '/roles', { params: paging });
        };

        this.getClaims = function () {
            return httpService.get(api + '/claims');
        };
    };

    angular
        .module('identity')
        .service('userService', userService);
    userService.$inject = ['httpService'];
}());