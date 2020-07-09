(function () {
    'use strict';

    function tenantReadController($state, permissionService, sweetAlert, tenantService) {
        var ctrl = this;
        ctrl.permissionService = permissionService;
        ctrl.paging = {
            page: 1,
            size: 10,
            q: null
        };
        ctrl.items = [];

        ctrl.$onInit = function () {
            ctrl.getItems();
        };

        ctrl.getItems = function () {
            ctrl.loading = true;
            tenantService.paging(ctrl.paging).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    ctrl.items = response.data.data;
                    ctrl.totalItems = response.data.totalItems;
                }
            });
        };

        ctrl.create = function () {
            $state.go('core.tenant.create');
        };

        ctrl.update = function (code) {
            $state.go('core.tenant.update', {
                code: code
            });
        };

        ctrl.delete = function (code) {
            sweetAlert.confirm(function () {
                ctrl.loading = true;
                return tenantService.delete(code);
            }, function (result) {
                ctrl.loading = false;
                if (result.value) {
                    if (result.value.status === 200) {
                        sweetAlert.success('Thành công', 'Bạn đã xóa CHI NHÁNH thành công!');
                        ctrl.getItems();
                    } else {
                        sweetAlert.error("Lỗi", result.value.data);
                    }
                }
            });
        };
    };

    angular
        .module('core')
        .controller('tenantReadController', tenantReadController);

    tenantReadController.$inject = ['$state', 'permissionService', 'sweetAlert', 'tenantService'];
}());