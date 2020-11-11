(function () {
    'use strict';

    var organizationUnitController = function ($state, sweetAlert, organizationService, $uibModalInstance, action, nodeScope) {
        var ctrl = this;
        ctrl.loading = false;

        ctrl.validationOptions = {
            rules: {
                name: {
                    required: true
                },
                fullName: {
                    required: true
                }
            }
        };

        ctrl.unit = {
            code: null,
            name: null,
            fullName: null,
            description: null,
            idParent: '00000000-0000-0000-0000-000000000000',
            parent: null,
            nodes: []
        };

        ctrl.$onInit = function () {
            ctrl.clean();

            if (action === 'create') {
                if (nodeScope) {
                    var item = nodeScope.$modelValue;
                    ctrl.unit.parent = item.name + ' - ' + item.fullName;
                    ctrl.unit.idParent = item.code;
                } else {
                    ctrl.unit.idParent = '00000000-0000-0000-0000-000000000000';
                }
                ctrl.unit.nodes = [];
            }

            if (action === 'update') {
                var item = nodeScope.$modelValue;
                ctrl.unit.code = item.code;
                ctrl.unit.name = item.name;
                ctrl.unit.fullName = item.fullName;
                ctrl.unit.description = item.description;

                if (nodeScope.$parentNodeScope) {
                    var parent = nodeScope.$parentNodeScope.$modelValue;
                    ctrl.unit.idParent = item.idParent;
                    ctrl.unit.parent = parent.name + ' - ' + parent.fullName;
                } else {
                    ctrl.unit.idParent = '00000000-0000-0000-0000-000000000000';
                    ctrl.unit.parent = null;
                }
            }
        };

        ctrl.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };

        ctrl.clean = function () {
            ctrl.unit = {
                code: null,
                name: null,
                fullName: null,
                description: null,
                idParent: '00000000-0000-0000-0000-000000000000',
                parent: null,
                nodes: []
            };
        };

        ctrl.update = function () {
            organizationService.putUnit(ctrl.unit).then(function (response) {
                if (response.status !== 200) {
                    return;
                }

                $uibModalInstance.close({
                    action: 'update',
                    data: ctrl.unit
                });

                sweetAlert.swal({
                    title: "Thành công",
                    text: "Bạn đã sửa đơn vị thành công!",
                    type: "success",
                });
            });
        };

        ctrl.create = function () {
            organizationService.postUnit(ctrl.unit).then(function (response) {
                if (response.status !== 200) {
                    return;
                }

                ctrl.unit.code = response.data;
                $uibModalInstance.close({
                    action: 'create',
                    data: ctrl.unit
                });

                sweetAlert.swal({
                    title: "Thành công",
                    text: "Bạn đã thêm đơn vị thành công!",
                    type: "success",
                });
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            if (ctrl.unit.code) {
                ctrl.update();
            } else {
                ctrl.create();
            }
        };
    };

    angular
        .module('core')
        .controller('organizationUnitController', organizationUnitController);

    organizationUnitController.$inject = ['$state', 'sweetAlert', 'organizationService', '$uibModalInstance', 'action', 'nodeScope'];
}());