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

        ctrl.forgotPassword = {
            username: null,
            email: null
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

        ctrl.getSettingLogin = function () {
            httpService.get('/settings/share?key=DangNhap').then(function (response) {
                if (response.status === 200 || response.status === 204) {
                    console.log(response);
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

        ctrl.sendforgotPassword = function (form) {
            if (!form.validate()) {
                return;
            }

            ctrl.forgotPassword.hostName = window.location.host;

            httpService.post('/identity/forgot-password', ctrl.forgotPassword).then(function (response) {
                if (response.status === 200) {
                    $('#modal-forgot-password').modal('hide');
                    sweetAlert.success('Thành công', 'Yêu cầu lấy lại mật khẩu đã được gửi, hãy kiểm tra email của bạn để đổi mật khẩu');
                }
            });
        };

        ctrl.openForgotPassword = function () {
            $('[name=frmForgotPassword]').validate().resetForm();
            ctrl.forgotPassword = {
                username: null,
                email: null
            };
            $('#modal-forgot-password').modal('show');
        };

        angular.element('#modal-forgot-password').on('shown.bs.modal', function () {
            angular.element('[name=userName]').focus();
        });

        angular.element('#modal-forgot-password').on('hidden.bs.modal', function () {
            angular.element('[name=username]').focus();
        });
    };

    angular
        .module('identity')
        .controller('loginController', loginController);

    loginController.$inject = ['$state', '$timeout', 'APP_CONFIG', 'httpService', 'cacheService', 'sweetAlert'];
}());