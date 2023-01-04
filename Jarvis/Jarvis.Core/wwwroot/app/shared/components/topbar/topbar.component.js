(function () {
    'use strict';

    angular
        .module('jarvis')
        .component('uiTopbar', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiTopbar', '/app/shared/components/topbar/topbar.template.html');
            }],
            controller: 'topbarController',
            bindings: {
                context: '='
            }
        })
        .controller('topbarController', ['$scope', '$state', '$timeout', '$uibModal', 'APP_CONFIG', 'httpService', 'cacheService', function ($scope, $state, $timeout, $uibModal, APP_CONFIG, httpService, cacheService) {
            var ctrl = $scope.$ctrl;
            ctrl.currentTenant = {};
            ctrl.message = APP_CONFIG.MESSAGE;

            ctrl.$onInit = function () {
                ctrl.navigations = cacheService.set('menu');
                if (!ctrl.navigations) {
                    var token = cacheService.get('token');
                    if (token) {
                        ctrl.getNavigationAsync();
                    } else {
                        ctrl.getNavigation();
                    }
                }

                ctrl.currentTenant = cacheService.get('currentTenant');
            };

            ctrl.getNavigationAsync = function () {
                httpService.get('/core/navigation').then(function (response) {
                    if (response.status !== 200) {
                        return;
                    }

                    var items = response.data;

                    var parents = [];
                    for (var i = 0; i < items.length; i++) {
                        if (items[i].order % 1000 === 0) {
                            parents.push(items[i]);
                        }
                    }

                    var menu = [];
                    for (var i = 0; i < parents.length; i++) {
                        var item = parents[i];
                        item.state = APP_CONFIG.NAVIGATION_CODE_MAPPING_TO_STATE[item.code];
                        item.url = null;
                        item.items = [];
                        for (var j = 0; j < items.length; j++) {
                            var index = Math.floor(items[j].order / 1000) * 1000;
                            var remider = items[j].order % item.order;
                            if (index === item.order && remider > 0 && remider < 1000) {
                                item.items.push({
                                    code: items[j].code,
                                    icon: items[j].icon,
                                    name: items[j].name,
                                    order: items[j].order,
                                    url: null,
                                    state: APP_CONFIG.NAVIGATION_CODE_MAPPING_TO_STATE[items[j].code]
                                });
                            }
                        }
                        menu.push(item);
                    }

                    ctrl.navigations = menu;
                    cacheService.set('menu', ctrl.navigations);
                });
            };

            ctrl.getNavigation = function () {
                ctrl.navigations = APP_CONFIG.NAVIGATION;
                // cacheService.set('menu', ctrl.navigations);
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
                    templateUrl: '/app/shared/components/change-tenant/change-tenant.template.html',
                    controller: 'changeTenantController',
                    controllerAs: '$ctrl',
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