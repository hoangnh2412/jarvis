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
            ctrl.$state = $state;
            ctrl.currentTenant = {};

            ctrl.$onInit = function () {
                ctrl.currentTenant = cacheService.get('currentTenant');

                ctrl.navigations = cacheService.get('menu');
                if (!ctrl.navigations) {
                    var token = cacheService.get('token');
                    if (token) {
                        ctrl.getNavigationAsync();
                    } else {
                        ctrl.getNavigation();
                    }
                }
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

            ctrl.childIsActive = function (item) {
                if (item.items && item.items.length > 0) {
                    for (let i = 0; i < item.items.length; i++) {
                        const element = item.items[i];
                        if ($state.current.name.startsWith(element.state)) {
                            return true;
                        }
                    }
                }

                return false;
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
                        // reset cache luu t�m ki?m trang danh s�ch h�a don trong context
                        if (Object.keys(ctrl.context.cache).length !== 0) {
                            ctrl.context.cache = {};
                            cacheService.set('context', ctrl.context);
                        }
                        $state.reload();
                    }
                });
            };
        }]);
}());