(function () {
    'use strict';

    function changePasswordController($state, $q, cacheService, sweetAlert, httpService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.validationOptions = {
            rules: {
                oldPassword: {
                    required: true
                },
                newPassword: {
                    required: true,
                    minlength: 6,
                    maxlength: 100,
                    regex: /^\S[^\t\n\r]+[\S]$/
                },
                confirmPassword: {
                    required: true,
                    equalTo: '#new_password'
                }
            },
            messages: {
                newPassword: {
                    regex: "Mật khẩu không chứa ký tự tab và không bắt đầu hoặc kết thúc bằng khoảng trắng"
                },
                confirmPassword: {
                    equalTo: 'Mật khẩu không khớp!'
                }
            }
        };

        ctrl.$onInit = function () {

        };

        ctrl.changePass = function (form) {
            if (!form.validate()) {
                return;
            }
            ctrl.loading = true;
            httpService.post('/profile/change-password', ctrl.password).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    sweetAlert.success('Thành công', 'Bạn đã đổi mật khẩu thành công, vui lòng đăng nhập lại');
                    cacheService.clean();
                    $state.go('identity.frontend.login');
                }
            });
        };
    };

    angular
        .module('identity')
        .controller('changePasswordController', changePasswordController);

    changePasswordController.$inject = ['$state', '$q', 'cacheService', 'sweetAlert', 'httpService'];
}());