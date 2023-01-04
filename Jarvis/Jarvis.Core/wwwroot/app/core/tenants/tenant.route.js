(function () {
    'use strict';

    angular
        .module('core')
        .component('uiTenantRead', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiTenantRead', '/app/core/tenants/tenant-read.template.html');
            }],
            controller: 'tenantReadController',
            bindings: {
                context: '='
            }
        })
        .component('uiTenantCreate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiTenantCreate', '/app/core/tenants/tenant-create.template.html');
            }],
            controller: 'tenantCreateController',
            bindings: {
                context: '='
            }
        })
        .component('uiTenantUpdate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiTenantUpdate', '/app/core/tenants/tenant-update.template.html');
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
                    tenantService: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.replace('/app/core/tenants/tenant.service.js'));
                    }]
                }
            });

            $stateProvider.state('core.tenant.read', {
                url: '/read',
                component: 'uiTenantRead',
                // data: {
                //     authorize: ['Tenant_Read'],
                // },
                resolve: {
                    ngFile: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load('/app/shared/directives/ngFile.directive.js');
                    }],
                    tenantReadController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiTenantRead', '/app/core/tenants/tenant-read.controller.js'));
                    }],
                    tenantLogoController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiTenantLogo', '/app/core/tenants/tenant-logo.controller.js'));
                    }]
                }
            });

            $stateProvider.state('core.tenant.create', {
                component: 'uiTenantCreate',
                url: '/create',
                // data: {
                //     authorize: ['Tenant_Create'],
                // },
                resolve: {
                    tenantCreateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiTenantCreate', '/app/core/tenants/tenant-create.controller.js'));
                    }]
                }
            });

            $stateProvider.state('core.tenant.update', {
                component: 'uiTenantUpdate',
                url: '/update/:code',
                // data: {
                //     authorize: ['Tenant_Update'],
                // },
                resolve: {
                    tenantUpdateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiTenantUpdate', '/app/core/tenants/tenant-update.controller.js'));
                    }]
                }
            });
        });
}());