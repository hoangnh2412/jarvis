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
                    maxlength: 250,
                    multipleHostName: true
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
                    whiteSpace: true,
                    maxlength: 250
                },
                username: {
                    required: true,
                    maxlength: 256,
                    regex: /^[a-zA-Z0-9][\w-\/]{0,}[a-zA-Z0-9]$|^[a-zA-Z0-9]$/
                },
                email: {
                    maxlength: 500,
                    singleEmail: true
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
                },
                userName: {
                    regex: "Tài khoản đăng nhập viết không dấu, được phép chứa các ký tự đặc biệt -, _, / và không đặt ở đầu hoặc cuối"
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
                    tenantService.setDefault(response.data.data.tenantCode);
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