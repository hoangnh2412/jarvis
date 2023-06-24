(function () {
    'use strict';

    function changeTenantController($scope, $uibModalInstance, httpService, currentTenant) {
        var ctrl = this;
        ctrl.tenants = [];

        ctrl.$onInit = function () {
            ctrl.getTenants();
        };

        ctrl.getTenants = function () {
            httpService.get('/core/tenants', { cache: true }).then(function (response) {
                if (response.status === 200) {
                    ctrl.tenants = response.data;
                    ctrl.tenants.forEach(tenant => {
                        if (tenant.code === currentTenant.code) {
                            tenant.selected = true;
                        } else {
                            tenant.selected = false;
                        }
                    });
                }
            });
        };

        ctrl.select = function (item) {
            var tenant = ctrl.tenants.find(function (x) { return x.code === item.code; });
            if (tenant) {
                ctrl.tenants.forEach(tenant => {
                    tenant.selected = false;
                });
                tenant.selected = true;
            }
            $uibModalInstance.close(tenant);
        };

        ctrl.close = function () {
            $uibModalInstance.close();
        };
    };

    angular
        .module('jarvis')
        .controller('changeTenantController', changeTenantController);

    changeTenantController.$inject = ['$scope', '$uibModalInstance', 'httpService', 'currentTenant'];
}());