(function () {
    'use strict';

    function registerController($state, $timeout, APP_CONFIG, httpService, cacheService, sweetAlert) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.validationOptions = {
            rules: {
                fullName: {
                    required: true
                },
                username: {
                    required: true
                },
                password: {
                    required: true
                }
            }
        };
        ctrl.identity = {
            fullName: null,
            username: null,
            password: null
        };

        ctrl.$onInit = function () {
        };

        ctrl.register = function (form) {
            if (!form.validate()) {
                return;
            }

            httpService.post('/identity/register', ctrl.identity).then(function (response) {
                if (response.status === 200) {
                    $state.go('identity.auth.login');
                }
            });
        };
    };

    angular
        .module('identity')
        .controller('registerController', registerController);

    registerController.$inject = ['$state', '$timeout', 'APP_CONFIG', 'httpService', 'cacheService', 'sweetAlert'];
}());