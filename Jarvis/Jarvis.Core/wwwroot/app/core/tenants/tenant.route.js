(function () {
    'use strict';

    angular
        .module('core')
        .component('uiTenantRead', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiTenantRead', '/app/core/tenants/tenant-read.template.html');
            }],
            controller: 'tenantReadController',
            bindings: {
                context: '='
            }
        })
        .component('uiTenantCreate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiTenantCreate', '/app/core/tenants/tenant-create.template.html');
            }],
            controller: 'tenantCreateController',
            bindings: {
                context: '='
            }
        })
        .component('uiTenantUpdate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiTenantUpdate', '/app/core/tenants/tenant-update.template.html');
            }],
            controller: 'tenantUpdateController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('core.tenant', {
                url: '/tenant',
                redirectTo: 'core.tenant.read',
                resolve: {
                    tenantService: ['$ocLazyLoad', 'APP_CONFIG', function ($ocLazyLoad, APP_CONFIG) {
                        return $ocLazyLoad.load(APP_CONFIG.BASE_PATH ? APP_CONFIG.BASE_PATH + '/app/jarvis/core/tenants/tenant.service.js' : '/app/jarvis/core/tenants/tenant.service.js');
                    }]
                }
            });

            $stateProvider.state('core.tenant.read', {
                url: '/read',
                component: 'uiTenantRead',
                data: {
                    authorize: ['Tenant_Read'],
                },
                resolve: {
                    tenantReadController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiTenantRead', '/app/core/tenants/tenant-read.controller.js'));
                    }]
                }
            });

            $stateProvider.state('core.tenant.create', {
                component: 'uiTenantCreate',
                url: '/create',
                data: {
                    authorize: ['Tenant_Create'],
                },
                resolve: {
                    tenantCreateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiTenantCreate', '/app/core/tenants/tenant-create.controller.js'));
                    }]
                }
            });

            $stateProvider.state('core.tenant.update', {
                component: 'uiTenantUpdate',
                url: '/update/:code',
                data: {
                    authorize: ['Tenant_Update'],
                },
                resolve: {
                    tenantUpdateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiTenantUpdate', '/app/core/tenants/tenant-update.controller.js'));
                    }]
                }
            });
        });
}());