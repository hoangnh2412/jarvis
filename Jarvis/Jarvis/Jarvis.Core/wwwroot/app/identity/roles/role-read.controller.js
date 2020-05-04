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

        ctrl.getRoles = function () {
            roleService.get(ctrl.paging).then(function (response) {
                if (response.status === 200) {
                    ctrl.roles = response.data.data;
                    ctrl.totalItems = response.data.totalItems;
                }
            });
        };

        ctrl.delete = function (id) {
            sweetAlert.confirm(function () {
                return roleService.delete(id);
            }, function (result) {
                if (result.value) {
                    if (result.value.status === 200) {
                        sweetAlert.success('Đã xóa', 'Bạn đã xóa QUYỀN thành công!');
                        ctrl.getRoles();
                    } else {
                        sweetAlert.error("Lỗi", result.value.data);
                    }
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.getRoles();
        };
    };

    angular
        .module('identity')
        .controller('roleReadController', roleReadController);

    roleReadController.$inject = ['$state', 'sweetAlert', 'roleService', 'permissionService'];
}());