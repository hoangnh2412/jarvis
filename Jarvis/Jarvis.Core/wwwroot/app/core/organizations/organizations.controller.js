(function () {
    'use strict';

    var organizationsController = function ($state, sweetAlert, organizationService, $uibModal, $document) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.labels = [];
        ctrl.paging = {
            page: 1,
            size: 10,
            q: null,
            totalItems: 0,
            totalPages: 0
        };

        ctrl.units = [];
        ctrl.unit = null;
        ctrl.users = [];

        ctrl.treeOptions = {
            beforeDrop: function (e) {
                ctrl.moveUnit(e);
            }
        };

        ctrl.$onInit = function () {
            ctrl.getUnits();
        };

        ctrl.getUnits = function () {
            organizationService.getUnits().then(function (response) {
                if (response.status !== 200) {
                    return;
                }
                ctrl.units = response.data;
            });
        };

        ctrl.openUnitInfo = function (action, scope) {
            var modalInstance = $uibModal.open({
                animation: true,
                ariaLabelledBy: 'modal-title',
                ariaDescribedBy: 'modal-body',
                templateUrl: 'organization-unit.html',
                controller: 'organizationUnitController',
                controllerAs: '$ctrl',
                size: 'md',
                appendTo: angular.element($document[0].querySelector('.box-modal')),
                resolve: {
                    action: function () {
                        return action;
                    },
                    nodeScope: function () {
                        return scope;
                    }
                }
            });

            modalInstance.result.then(function (result) {
                if (result.action === 'update') {
                    var node = ctrl.find(result.data.code);
                    if (node) {
                        node.name = result.data.name;
                        node.fullName = result.data.fullName;
                        node.description = result.data.description;
                    }
                }

                if (result.action === 'create') {
                    var unit = angular.copy(result.data);
                    var node = ctrl.find(result.data.idParent);
                    if (node) {
                        node.nodes.push(unit);
                    } else {
                        ctrl.units.push(unit);
                    }
                }
            });

            modalInstance.opened.then(function (value) {
                angular.element('input[name="name"]').focus();
            });
        };

        ctrl.deleteUnit = function (scope) {
            var item = scope.$modelValue;

            sweetAlert.confirm(function () {
                return organizationService.deleteUnit(item.code);
            }, function (result) {
                if (result.value) {
                    scope.remove();

                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã xoá đơn vị thành công!",
                        type: "success",
                    });
                }
            });
        };

        ctrl.moveUnit = function (e) {
            var oldIndex = e.source.index;
            var newIndex = e.dest.index;

            if ((e.dest.nodesScope.$parent.$modelValue && e.source.nodesScope.$parent.$modelValue) || (!e.dest.nodesScope.$parent.$modelValue && !e.source.nodesScope.$parent.$modelValue)) {
                //Nếu ko có node cha và vị trí ko đổi
                if (!e.dest.nodesScope.$parent.$modelValue && !e.source.nodesScope.$parent.$modelValue) {
                    if (oldIndex === newIndex) {
                        return false;
                    }
                }

                //Nếu có node cha nhưng node cha ko đổi và vị trí ko đổi thì bỏ qua
                if (e.dest.nodesScope.$parent.$modelValue && e.source.nodesScope.$parent.$modelValue) {
                    if (e.dest.nodesScope.$parent.$modelValue.code === e.source.nodesScope.$parent.$modelValue.code && oldIndex === newIndex) {
                        return false;
                    }
                }
            }

            var sourceCode = e.source.nodeScope.$modelValue.code;
            var parentCode = e.dest.nodesScope.$parent.$modelValue ? e.dest.nodesScope.$parent.$modelValue.code : null;

            var leftCode = null;
            if (newIndex === 0) {
                leftCode = null;
            } else {
                //Nếu ko thay đổi node cha
                if (e.dest.nodesScope.$nodeScope && e.dest.nodesScope.$nodeScope.$modelValue.code === e.source.nodeScope.$modelValue.idParent) {
                    if (oldIndex === newIndex) {
                        return false;
                    } else {
                        //Clone list node con và di chuyển để tìm left node
                        var items = JSON.parse(JSON.stringify(e.dest.nodesScope.$modelValue));
                        items.move(oldIndex, newIndex);

                        leftCode = items[newIndex - 1].code;
                    }
                } else {
                    leftCode = e.dest.nodesScope.$modelValue[newIndex - 1].code;
                }
            }


            var promise = sweetAlert.pConfirm('Bạn chắc chắn muốn chuyển đơn vị?', function () {
                return organizationService.moveUnit(sourceCode, parentCode, leftCode);
            });

            return promise.then(function (result) {
                if (result.value) {
                    if (result.value.status === 200) {
                        sweetAlert.swal({
                            title: "Thành công",
                            text: "Bạn đã chuyển đơn vị thành công!",
                            type: "success",
                        });
                        return true;
                    }
                    return false;
                }
                return false;
            });
        };

        ctrl.findNode = function (node, code) {
            if (node.code === code) {
                return node;
            }

            if (node.nodes && node.nodes.length > 0) {
                for (const element of node.nodes) {
                    const result = ctrl.findNode(element, code);
                    if (result) {
                        return result;
                    }
                }
            }
            return null;
        };

        ctrl.find = function (code) {
            for (let i = 0; i < ctrl.units.length; i++) {
                const element = ctrl.units[i];
                var result = ctrl.findNode(element, code);
                if (result) {
                    return result;
                }
            }
            return null;
        };


        ctrl.openUsersNotInUnit = function () {
            if (!ctrl.unit) {
                sweetAlert.swal({
                    title: "Lỗi",
                    text: "Chưa chọn đơn vị",
                    type: "error",
                });
            }

            var modalInstance = $uibModal.open({
                animation: true,
                ariaLabelledBy: 'modal-title',
                ariaDescribedBy: 'modal-body',
                templateUrl: 'organization-users.html',
                controller: 'organizationUserController',
                controllerAs: '$ctrl',
                size: 'md',
                appendTo: angular.element($document[0].querySelector('.box-modal')),
                resolve: {
                    unit: function () {
                        return ctrl.unit;
                    }
                }
            });
            modalInstance.closed.then(function () {
                ctrl.getMembers();
            });
        };

        ctrl.getMembers = function () {
            organizationService.getUsersInUnit(ctrl.unit.code, ctrl.paging).then(function (response) {
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

        ctrl.getMembersUnit = function (scope) {
            ctrl.unit = scope.$modelValue;
            ctrl.getMembers();
        };

        ctrl.deleteUser = function (index) {
            var user = ctrl.users[index];

            sweetAlert.confirm(function () {
                return organizationService.deleteUser(ctrl.unit.code, user.code);
            }, function (result) {
                if (result.value) {
                    if (result.value.status !== 200) {
                        return;
                    }
                    ctrl.users.splice(index, 1);
                    sweetAlert.success('Đã xóa', 'Bạn đã xóa thành viên thành công!');
                }
            });
        };
    };

    angular
        .module('core')
        .controller('organizationsController', organizationsController);

    organizationsController.$inject = ['$state', 'sweetAlert', 'organizationService', '$uibModal', '$document'];
}());