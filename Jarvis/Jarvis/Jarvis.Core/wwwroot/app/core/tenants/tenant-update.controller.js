(function () {
    'use strict';

    function tenantUpdateController($state, $stateParams, sweetAlert, tenantService) {
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
                    required: true
                }
            }
        };

        ctrl.$onInit = function () {
            ctrl.getItem();
        };

        ctrl.getItem = function () {
            tenantService.get($stateParams.code).then(function (response) {
                if (response.status === 200) {
                    ctrl.model = response.data;
                }
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            tenantService.put(ctrl.model).then(function (response) {
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã sửa CHI NHÁNH thành công",
                        type: "success",
                    });
                    $state.go('core.tenant.read');
                }
            });
        };
    };

    angular
        .module('core')
        .controller('tenantUpdateController', tenantUpdateController);

    tenantUpdateController.$inject = ['$state', '$stateParams', 'sweetAlert', 'tenantService'];
}());