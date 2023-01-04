(function () {
    'use strict';

    angular
        .module('core')
        .component('uiTenantInfo', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiTenantInfo', '/app/core/tenant-info/tenant-info.template.html');
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
                // data: {
                //     authorize: []
                // },
                resolve: {
                    tenantInfoController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiTenantInfo', '/app/core/tenant-info/tenant-info.controller.js'));
                    }],
                    tenantInfoService: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.replace('/app/core/tenant-info/tenant-info.service.js'));
                    }]
                }
            });
        });
}());