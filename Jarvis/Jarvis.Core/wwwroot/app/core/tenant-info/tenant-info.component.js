(function () {
    'use strict';

    angular
        .module('core')
        .component('uiTenantInfo', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiTenantInfo', '/app/core/tenant-info/tenant-info.template.html');
            }],
            controller: 'tenantInfoController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('core.tenant-info', {
                component: 'uiTenantInfo',
                url: '/tenant-info',
                data: {
                    authorize: []
                },
                resolve: {
                    tenantInfoController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiTenantInfo', '/app/core/tenant-info/tenant-info.controller.js'));
                    }],
                    tenantInfoService: ['$ocLazyLoad', 'APP_CONFIG', function ($ocLazyLoad, APP_CONFIG) {
                        return $ocLazyLoad.load((APP_CONFIG.BASE_PATH ? APP_CONFIG.BASE_PATH : '') + '/app/jarvis/core/tenant-info/tenant-info.service.js');
                    }]
                }
            });
        });
}());