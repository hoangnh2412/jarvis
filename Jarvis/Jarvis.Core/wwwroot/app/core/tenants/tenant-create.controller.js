(function () {
    'use strict';

    function tenantCreateController($state, sweetAlert, tenantService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.model = {
            info: {},
            user: {
                isAutoPassword: true
            }
        };
        ctrl.validationOptions = {
            rules: {
                hostName: {
                    required: true,
                    maxlength: 250
                },
                taxCode: {
                    required: true,
                    taxCode: true,
                    maxlength: 50
                },
                address: {
                    required: true,
                    whiteSpace: true,
                    maxlength: 500
                },
                fullNameVi: {
                    required: true,
                    whiteSpace: true,
                    maxlength: 250
                },
                fullName: {
                    required: true,
                    whiteSpace: true
                },
                username: {
                    required: true,
                    maxlength: 256
                },
                email: {
                    maxlength: 500
                },
                password: {
                    required: true,
                    minlength: 6,
                    regex: /^\S[^\t\n\r]+[\S]$/
                },
                confirmPassword: {
                    required: true,
                    equalTo: '#password'
                }
            },
            messages: {
                newPassword: {
                    regex: "mật khẩu không chứa ký tự tab và không bắt đầu hoặc kết thúc bằng khoảng trắng"
                },
                confirmPassword: {
                    equalTo: 'Mật khẩu không khớp!'
                }
            }
        };

        ctrl.$onInit = function () {
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }
            ctrl.loading = true;
            tenantService.post(ctrl.model).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã tạo CHI NHÁNH thành công",
                        type: "success",
                    });
                    // tenantService.setDefault(response.data.tenantCode);
                    $state.go('core.tenant.read');
                }
            });
        };
    };

    angular
        .module('core')
        .controller('tenantCreateController', tenantCreateController);

    tenantCreateController.$inject = ['$state', 'sweetAlert', 'tenantService'];
}());