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
                    required: true
                },
                confirmPassword: {
                    required: true,
                    equalTo: '#new_password'
                }
            },
            messages: {
                confirmPassword: {
                    equalTo: 'Mật khẩu không khớp!'
                }
            }
        };

        ctrl.$onInit = () => {

        };

        ctrl.login = (form) => {
            if (!form.validate()) {
                return;
            }

            httpService.post('/profile/change-password', ctrl.password).then((response) => {
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