(function () {
    'use strict';

    function profileService(httpService) {
        var api = '/profile';

        this.getProfile = function () {
            return httpService.get(api + '/user');
        };

        this.postProfile = function (user) {
            return httpService.post(api + '/user', user);
        };

        this.deleteProfile = function () {
            return httpService.delete(api + '/user');
        };

        this.changePassword = function (password) {
            return httpService.post(api + '/change-password', password);
        };
    };

    angular
        .module('identity')
        .service('profileService', profileService);
    profileService.$inject = ['httpService'];
}());