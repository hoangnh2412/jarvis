(function () {
    'use strict';

    angular
        .module('jarvis')
        .component('uiSidebar', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiSidebar', '/app/shared/components/sidebar/sidebar.template.html');
            }],
            controller: 'sidebarController',
            bindings: {
                context: '='
            }
        })
        .controller('sidebarController', ['$scope', 'APP_CONFIG', 'httpService', 'cacheService', function ($scope, APP_CONFIG, httpService, cacheService) {
            var ctrl = $scope.$ctrl;
            ctrl.navigations = [];

            ctrl.$onInit = function () {
                var navigation = cacheService.get('menu');
                if (navigation) {
                    ctrl.navigations = navigation;
                } else {
                    ctrl.getNavigation();
                }
            };

            ctrl.getNavigation = function () {
                httpService.get('/core/navigation').then(function (response) {
                    if (response.status !== 200) {
                        return;
                    }

                    var items = response.data.data;

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
        }]);
}());