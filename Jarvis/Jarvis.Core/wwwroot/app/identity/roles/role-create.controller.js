(function () {
    'use strict';

    var roleCreateController = function ($state, sweetAlert, roleService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.role = {};
        ctrl.modules = [];

        ctrl.validationOptions = {
            rules: {
                name: {
                    required: true,
                    whiteSpace: true
                }
            }
        };

        ctrl.$onInit = function () {
            ctrl.getClaims();
        };

        ctrl.getRole = function (id) {
            ctrl.loading = true;
            roleService.getById(id).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    ctrl.role = response.data.data;
                }
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            var role = ctrl.prepareSave();

            ctrl.loading = true;
            roleService.post(role).then(function (response) {
                ctrl.loading = false;
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: "Thành công",
                        text: "Bạn đã tạo QUYỀN thành công!",
                        type: "success",
                    });
                    $state.go('identity.backend.role.read');
                }
            });
        };

        ctrl.getClaims = function (id) {
            ctrl.loading = true;
            roleService.getClaims(id).then(function (response) {
                ctrl.loading = false;
                if (response.status !== 200) {
                    return;
                }
                var modules = [];
                for (var i = 0; i < response.data.length; i++) {
                    var module = response.data[i];
                    var element = modules.find(function (x) { return x.code === module.moduleCode; });
                    if (element) {
                        element.groups.push(module);
                    } else {
                        modules.push({
                            code: module.moduleCode,
                            name: module.moduleName,
                            groups: [module]
                        });
                    }
                }

                var tempGroup = [];
                for (var i = 0; i < modules.length; i++) {
                    var module = modules[i];
                    tempGroup = [];
                    for (var j = 0; j < module.groups.length; j++) {
                        var group = module.groups[j];
                        var element = tempGroup.find(function (x) { return x.code === group.groupCode; });
                        if (element) {
                            element.claims.push(group);
                        } else {
                            tempGroup.push({
                                code: group.groupCode,
                                name: group.groupName,
                                claims: [group]
                            })
                        }
                    }
                    module.groups = tempGroup;
                }
                ctrl.modules = modules;
            });
        };

        ctrl.moduleSelectAll = function (code) {
            for (var m = 0; m < ctrl.modules.length; m++) {
                var module = ctrl.modules[m];
                if (module.code === code) {
                    for (var g = 0; g < module.groups.length; g++) {
                        var group = module.groups[g];
                        for (var c = 0; c < group.claims.length; c++) {
                            var claim = group.claims[c];
                            claim.selected = true;
                        }
                    }
                }
            }
        };

        ctrl.moduleUnselectAll = function (code) {
            for (var m = 0; m < ctrl.modules.length; m++) {
                var module = ctrl.modules[m];
                if (module.code === code) {
                    for (var g = 0; g < module.groups.length; g++) {
                        var group = module.groups[g];
                        for (var c = 0; c < group.claims.length; c++) {
                            var claim = group.claims[c];
                            claim.selected = false;
                        }
                    }
                }
            }
        };

        ctrl.groupSelectAll = function (code) {
            for (var m = 0; m < ctrl.modules.length; m++) {
                var module = ctrl.modules[m];
                for (var g = 0; g < module.groups.length; g++) {
                    var group = module.groups[g];
                    if (group.code === code) {
                        for (var c = 0; c < group.claims.length; c++) {
                            var claim = group.claims[c];
                            claim.selected = true;
                        }
                    }
                }
            }
        };

        ctrl.groupUnselectAll = function (code) {
            for (var m = 0; m < ctrl.modules.length; m++) {
                var module = ctrl.modules[m];
                for (var g = 0; g < module.groups.length; g++) {
                    var group = module.groups[g];
                    if (group.code === code) {
                        for (var c = 0; c < group.claims.length; c++) {
                            var claim = group.claims[c];
                            claim.selected = false;
                        }
                    }
                }
            }
        };

        ctrl.prepareSave = function () {
            var role = {
                id: ctrl.role.id,
                name: ctrl.role.name
            };

            var claims = {};

            for (var m = 0; m < ctrl.modules.length; m++) {
                var module = ctrl.modules[m];
                for (var g = 0; g < module.groups.length; g++) {
                    var group = module.groups[g];
                    for (var c = 0; c < group.claims.length; c++) {
                        var claim = group.claims[c];
                        if (claim.selected) {
                            claims[claim.code] = claim.resource + '|' + claim.childResource;
                        }
                    }
                }
            }
            role.claims = claims;
            return role;
        };
    };

    angular
        .module('identity')
        .controller('roleCreateController', roleCreateController);

    roleCreateController.$inject = ['$state', 'sweetAlert', 'roleService'];
}());