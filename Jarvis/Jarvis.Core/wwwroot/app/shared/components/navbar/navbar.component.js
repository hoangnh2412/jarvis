(function () {
    'use strict';

    angular
        .module('jarvis')
        .component('uiNavbar', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiNavbar', '/app/shared/components/navbar/navbar.template.html');
            }],
            controller: 'navbarController',
            bindings: {
                context: '='
            }
        })
        .controller('navbarController', ['$scope', '$state', '$timeout', '$uibModal', 'APP_CONFIG', 'httpService', 'cacheService', function ($scope, $state, $timeout, $uibModal, APP_CONFIG, httpService, cacheService) {
            var ctrl = $scope.$ctrl;
            ctrl.currentTenant = {};

            ctrl.$onInit = function () {
                ctrl.currentTenant = cacheService.get('currentTenant');
            };

            ctrl.logout = function () {
                httpService.post('/identity/logout').then(function (response) {
                    if (response.status === 200) {
                        cacheService.clean();
                        ctrl.context = {};

                        $timeout(function () {
                            $state.go(getStateNameByUrl(APP_CONFIG.LOGIN_URL));
                        });
                    }
                });
            };

            var getStateNameByUrl = function (url) {
                var states = $state.get();
                for (var i = 0; i < states.length; i++) {
                    var element = states[i];
                    if (element.url && element.url.startsWith(url)) {
                        return element.name;
                    }
                }
            };

            ctrl.openTenants = function () {
                var modalTenants = $uibModal.open({
                    animation: true,
                    ariaLabelledBy: 'modal-title',
                    ariaDescribedBy: 'modal-body',
                    templateUrl: 'modalChangeTenant.html',
                    controller: 'changeTenantController',
                    controllerAs: '$ctrl',
                    appendTo: angular.element('.box-modal'),
                    backdrop: false,
                    resolve: {
                        currentTenant: function () {
                            return ctrl.currentTenant;
                        }
                    }
                });

                modalTenants.result.then(function (tenant) {
                    if (!tenant) {
                        return;
                    }

                    var currentTenant = cacheService.get('currentTenant');
                    if (currentTenant.code !== tenant.code) {
                        cacheService.set('currentTenant', tenant);
                        ctrl.currentTenant = currentTenant;
                        // reset cache luu t�m ki?m trang danh s�ch h�a don trong context
                        if (Object.keys(ctrl.context.cache).length !== 0) {
                            ctrl.context.cache = {};
                            cacheService.set('context', ctrl.context);
                        }
                        $state.reload();
                    }
                });
            };
        }])
        .controller('changeTenantController', ['$scope', '$uibModalInstance', 'httpService', 'currentTenant', function ($scope, $uibModalInstance, httpService, currentTenant) {
            var ctrl = $scope.$ctrl;
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
        }]);
}());