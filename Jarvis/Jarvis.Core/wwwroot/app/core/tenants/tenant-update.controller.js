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
                }
            }
        };

        ctrl.$onInit = function () {
            ctrl.getItem();
        };

        ctrl.getItem = function () {
            ctrl.loading = true;
            tenantService.get($stateParams.code).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    ctrl.model = response.data;
                }
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }
            ctrl.loading = true;
            tenantService.put(ctrl.model).then(function (response) {
                ctrl.loading = false;
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