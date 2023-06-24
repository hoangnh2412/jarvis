(function () {
    'use strict';

    var roleReadController = function ($state, sweetAlert, roleService, permissionService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.permissionService = permissionService;

        ctrl.validationOptions = {
            rules: {
                value: {
                    required: true
                }
            }
        };
        ctrl.roles = [];
        ctrl.paging = {
            page: 1,
            size: 10,
            q: null
        }

        ctrl.getItems = function () {
            ctrl.loading = true;
            roleService.get(ctrl.paging).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    ctrl.roles = response.data.data;
                    ctrl.totalItems = response.data.totalItems;
                }
            });
        };

        ctrl.delete = function (id) {
            sweetAlert.confirm(function () {
                ctrl.loading = true;
                return roleService.delete(id);
            }, function (result) {
                ctrl.loading = false;
                if (result.value) {
                    if (result.value.status === 200) {
                        sweetAlert.success('Đã xóa', 'Bạn đã xóa QUYỀN thành công!');
                        ctrl.getItems();
                    } else {
                        sweetAlert.error("Lỗi", result.value.data);
                    }
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.getItems();
        };
    };

    angular
        .module('identity')
        .controller('roleReadController', roleReadController);

    roleReadController.$inject = ['$state', 'sweetAlert', 'roleService', 'permissionService'];
}());