(function () {
    'use strict';

    function forgotPasswordController($state, httpService, $stateParams, sweetAlert, cacheService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.validationOptions = {
            rules: {
                new_password: {
                    required: true,
                    minlength: 6,
                    regex: /^\S[^\t\n\r]+[\S]$/
                },
                confirm_password: {
                    required: true,
                    equalTo: '#new_password'
                }
            },
            messages: {
                new_password: {
                    regex: "mật khẩu không chứa ký tự tab và không bắt đầu hoặc kết thúc bằng khoảng trắng"
                },
                confirm_password: {
                    equalTo: 'Mật khẩu không khớp!'
                }
            }
        };

        ctrl.identity = {
            id: null,
            newPassword: null,
            securityStamp: null
        };

        ctrl.changePassword = function (form) {
            if (!form.validate()) {
                return;
            }

            if (ctrl.identity.newPassword !== ctrl.confirmPassword) {
                sweetAlert.error('Lỗi', 'Mật khẩu xác nhận không khớp, yêu cầu nhập lại');
                return;
            }
            ctrl.loading = true;
            httpService.post('/identity/reset-forgot-password', ctrl.identity).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    sweetAlert.success('Thành công', 'Bạn đã đổi mật khẩu thành công, vui lòng đăng nhập lại');
                    cacheService.clean();
                    ctrl.context = {};
                    $state.go('identity.frontend.login');
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.identity = {
                id: $stateParams.idUser,
                newPassword: null,
                securityStamp: $stateParams.key
            };
        };
    };

    angular
        .module('identity')
        .controller('forgotPasswordController', forgotPasswordController);

    forgotPasswordController.$inject = ['$state', 'httpService', '$stateParams', 'sweetAlert', 'cacheService'];
}());