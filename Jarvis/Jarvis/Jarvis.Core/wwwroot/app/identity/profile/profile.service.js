(function () {
    'use strict';

    function profileService(httpService) {
        var api = '/profile';

        this.getProfile = () => {
            return httpService.get(api + '/user');
        };

        this.postProfile = (user) => {
            return httpService.post(api + '/user', user);
        };

        this.deleteProfile = () => {
            return httpService.delete(api + '/user');
        };
    };

    angular
        .module('identity')
        .service('profileService', profileService);
    profileService.$inject = ['httpService'];
}());