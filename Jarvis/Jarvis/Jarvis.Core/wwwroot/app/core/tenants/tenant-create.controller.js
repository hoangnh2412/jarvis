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
                    required: true
                },
                taxCode: {
                    required: true,
                    taxCode: true
                },
                address: {
                    required: true
                },
                fullNameVi: {
                    required: true
                },
                fullName: {
                    required: true
                },
                username: {
                    required: true
                },
                password: {
                    required: true
                },
                confirmPassword: {
                    required: true,
                    equalTo: '#password'
                }
            },
            messages: {
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

            tenantService.post(ctrl.model).then(function (response) {
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã tạo CHI NHÁNH thành công",
                        type: "success",
                    });
                    tenantService.setDefault(response.data.tenantCode);
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