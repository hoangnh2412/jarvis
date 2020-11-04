(function () {
    'use strict';

    var organizationsController = function ($state, sweetAlert, organizationService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.labels = [];
        ctrl.paging = {
            page: 1,
            size: 10,
            q: null
        };

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

        ctrl.units = [];
        ctrl.tree = [];
        ctrl.unit = {
            code: null,
            name: null,
            fullName: null,
            description: null
        };

        ctrl.treeOptions = {
            beforeDrop: function (e) {
                if (e.source.index === e.dest.index) {
                    return false;
                }

                var promise = sweetAlert.pConfirm('Bạn chắc chắn muốn chuyển đơn vị?', function () {
                    // return labelService.delete(code);
                });

                return promise.then(function (result) {
                    if (result.value) {
                        return true;
                    }
                    return false;
                });
            }
        };

        ctrl.paginationUnits = function () {
            organizationService.pagination(ctrl.paging).then(function (response) {
                if (response.status !== 200) {
                    return;
                }

                ctrl.tree = [];
                for (let i = 0; i < response.data.data.length; i++) {
                    const element = response.data.data[i];
                    ctrl.tree.push({
                        code: element.code,
                        name: element.name,
                        fullName: element.fullName,
                        description: element.description,
                        idParent: element.idParent,
                        collapsed: false,
                        nodes: []
                    });
                }
            });
        };

        ctrl.editUnit = function (scope) {
            var item = scope.$modelValue;
            ctrl.unit.code = item.code;
            ctrl.unit.name = item.name;
            ctrl.unit.fullName = item.fullName;
            ctrl.unit.description = item.description;

            if (scope.$parentNodeScope) {
                ctrl.unit.idParent = item.idParent;
                var parent = scope.$parentNodeScope.$modelValue;
                ctrl.unit.parent = parent.name + ' - ' + parent.fullName;
            }
        };

        ctrl.addUnit = function (scope) {
            var node = scope.$modelValue;
            ctrl.unit.parent = node.name + ' - ' + node.fullName;
            ctrl.clean();
            // nodeData.items.push({
            //     id: nodeData.id * 10 + nodeData.nodes.length,
            //     title: nodeData.title + '.' + (nodeData.nodes.length + 1),
            //     nodes: []
            // });
        };

        ctrl.updateUnit = function () {
            organizationService.put(ctrl.unit).then(function (response) {
                if (response.status === 200) {
                    var node = ctrl.findNode(ctrl.tree, ctrl.unit.code);
                    if (node) {
                        node.name = ctrl.unit.name;
                        node.fullName = ctrl.unit.fullName;
                        node.description = ctrl.unit.description;
                    }

                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã sửa đơn vị thành công!",
                        type: "success",
                    });
                }
            });
        };

        ctrl.createUnit = function () {
            // organizationService.post(ctrl.unit).then(function (response) {
            //     console.log(response);
            // });
        };

        ctrl.updateParent = function () {

        };

        ctrl.save = function () {
            if (ctrl.unit.code) {
                ctrl.updateUnit();
            } else {
                // ctrl.createUnit();
            }
        };

        ctrl.clean = function () {
            ctrl.unit = {
                code: null,
                name: null,
                fullName: null,
                description: null
            };
        };

        ctrl.findNode = function (nodes, code) {
            for (let i = 0; i < nodes.length; i++) {
                const node = nodes[i];
                if (node.code === code) {
                    return node;
                }

                if (node.nodes && node.nodes.length > 0) {
                    return ctrl.findNode(node.nodes, code);
                }
            }
            return null;
        };

        ctrl.$onInit = function () {
            ctrl.paginationUnits();
        };
    };

    angular
        .module('core')
        .controller('organizationsController', organizationsController);

    organizationsController.$inject = ['$state', 'sweetAlert', 'organizationService'];
}());