﻿(function () {
    'use strict';

    function loginController($state, $timeout, $location, APP_CONFIG, httpService, cacheService, sweetAlert) {
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
                    ctrl.getContext(response.data.data);
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
                    ctrl.context = response.data.data;
                    ctrl.context.cache = {};
                    ctrl.context.theme = APP_CONFIG.THEME;
                    cacheService.set('token', token);
                    cacheService.set('context', ctrl.context);
                    cacheService.set('currentTenant', {
                        code: ctrl.context.tenantInfo.code,
                        name: ctrl.context.tenantInfo.taxCode,
                        fullName: ctrl.context.tenantInfo.fullNameVi
                    });

                    $timeout(function () {
                        if (Object.keys(ctrl.context.claims).length === 0)
                            $state.go('portals.portal');
                        else {
                            $state.go(getStateNameByUrl(APP_CONFIG.DASHBOARD_URL));
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
                if (element.url === url) {
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

    loginController.$inject = ['$state', '$timeout', '$location', 'APP_CONFIG', 'httpService', 'cacheService', 'sweetAlert'];
}());