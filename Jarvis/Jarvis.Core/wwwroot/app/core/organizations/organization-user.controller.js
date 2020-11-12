(function () {
    'use strict';

    var organizationUserController = function ($state, sweetAlert, organizationService, $uibModalInstance, unit) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.paging = {
            page: 1,
            size: 10,
            q: null,
            totalItems: 0,
            totalPages: 0
        };
        ctrl.users = [];
        ctrl.all = false;
        ctrl.selectedCodes = [];
        ctrl.unit = unit;

        ctrl.$onInit = function () {
            ctrl.getUsers();
        };

        ctrl.ok = function () {
            organizationService.postUsers(ctrl.unit.code, ctrl.selectedCodes).then(function (response) {
                if (response.status !== 200) {
                    return;
                }

                sweetAlert.swal({
                    title: "Thành công",
                    text: "Bạn đã thêm thành viên thành công!",
                    type: "success",
                });

                $uibModalInstance.dismiss('close');
            });
        };

        ctrl.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };

        ctrl.getUsers = function () {
            organizationService.getUsersNotInUnit(ctrl.unit.code, ctrl.paging).then(function (response) {
                if (response.status !== 200) {
                    return;
                }
                ctrl.users = response.data.data;
                ctrl.paging.totalItems = response.data.totalItems;
                ctrl.paging.totalPages = response.data.totalPages;
                ctrl.paging.page = response.data.page;
                ctrl.paging.size = response.data.size;
            });
        };

        ctrl.find = function (code) {
            for (let i = 0; i < ctrl.selectedCodes.length; i++) {
                const element = ctrl.selectedCodes[i];
                if (element === code) {
                    return i;
                }
            }
            return -1;
        };

        ctrl.select = function (index) {
            if (index === -1) { //select all
                if (ctrl.all) {
                    for (let i = 0; i < ctrl.users.length; i++) {
                        const user = ctrl.users[i];

                        var x = ctrl.find(user.code);
                        if (x === -1) {
                            ctrl.selectedCodes.push(user.code);
                            user.selected = true;
                        }
                    }
                } else {
                    for (let i = 0; i < ctrl.users.length; i++) {
                        const user = ctrl.users[i];

                        var x = ctrl.find(user.code);
                        if (x !== -1) {
                            ctrl.selectedCodes.splice(x, 1);
                            user.selected = false;
                        }
                    }
                }
            } else { //select
                var user = ctrl.users[index];
                if (user.selected) {
                    var x = ctrl.find(user.code);
                    if (x === -1) {
                        ctrl.selectedCodes.push(user.code);
                    }
                } else {
                    var x = ctrl.find(user.code);
                    if (x !== -1) {
                        ctrl.selectedCodes.splice(x, 1);
                    }
                }
            }
        };
    };

    angular
        .module('core')
        .controller('organizationUserController', organizationUserController);

    organizationUserController.$inject = ['$state', 'sweetAlert', 'organizationService', '$uibModalInstance', 'unit'];
}());