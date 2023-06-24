(function () {
    'use strict';

    function loginController($state, $timeout, APP_CONFIG, httpService, cacheService, sweetAlert) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.validationOptions = {
            rules: {
                username: {
                    required: true
                },
                password: {
                    required: true
                }
            }
        };
        ctrl.identity = {
            username: null,
            password: null
        };

        ctrl.$onInit = function () {
        };

        ctrl.login = function (form) {
            if (!form.validate()) {
                return;
            }

            httpService.post('/identity/login', ctrl.identity).then(function (response) {
                if (response.status === 200) {
                    ctrl.getContext(response.data);
                }
            });
        };

        ctrl.getContext = function (token) {
            httpService.get('/identity/session', {
                headers: {
                    'Authorization': 'Bearer ' + token.accessToken
                }
            }).then(function (response) {
                if (response.status === 200) {
                    ctrl.context = response.data;
                    ctrl.context.cache = {};
                    ctrl.context.theme = APP_CONFIG.PRESENTATION.THEME_NAME;
                    ctrl.context.skin = APP_CONFIG.PRESENTATION.SKIN_NAME;
                    cacheService.set('token', token);
                    cacheService.set('context', ctrl.context);
                    cacheService.set('currentTenant', {
                        code: ctrl.context.tenantInfo.code,
                        name: ctrl.context.tenantInfo.taxCode,
                        fullName: ctrl.context.tenantInfo.fullNameVi
                    });

                    $timeout(function () {
                        var dashboardState = getStateNameByUrl(APP_CONFIG.DASHBOARD_URL);
                        var fromState = ctrl.transition.from();
                        var fromParams = ctrl.transition.params('from');

                        if (fromState.name === '') {
                            $state.go(dashboardState);
                        } else {
                            $state.go(fromState, fromParams);
                        }
                    });
                }
            });
        };

        var getStateNameByUrl = function (url) {
            var states = $state.get();
            for (var i = 0; i < states.length; i++) {
                var element = states[i];
                if (element.url && element.url.startsWith(url)) {
                    return element.name;
                }
            }
        };
    };

    angular
        .module('identity')
        .controller('loginController', loginController);

    loginController.$inject = ['$state', '$timeout', 'APP_CONFIG', 'httpService', 'cacheService', 'sweetAlert'];
}());