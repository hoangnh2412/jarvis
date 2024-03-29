(function () {
    'use strict';

    angular
        .module('identity')
        .component('uiRoleRead', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiRoleRead', '/app/identity/roles/role-read.template.html');
            }],
            controller: 'roleReadController',
            bindings: {
                context: '='
            }
        })
        .component('uiRoleCreate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiRoleCreate', '/app/identity/roles/role-create.template.html');
            }],
            controller: 'roleCreateController',
            bindings: {
                context: '='
            }
        })
        .component('uiRoleUpdate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiRoleUpdate', '/app/identity/roles/role-update.template.html');
            }],
            controller: 'roleUpdateController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('identity.management.role', {
                url: '/role',
                redirectTo: 'identity.management.role.read',
                resolve: {
                    roleService: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.replace('/app/identity/roles/role.service.js'));
                    }]
                }
            });

            $stateProvider.state('identity.management.role.read', {
                url: '/read',
                component: 'uiRoleRead',
                // data: {
                //     authorize: ['Role_Read'],
                // },
                resolve: {
                    rolesController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiRoleRead', '/app/identity/roles/role-read.controller.js'));
                    }]
                }
            });

            $stateProvider.state('identity.management.role.create', {
                component: 'uiRoleCreate',
                url: '/create',
                // data: {
                //     authorize: ['Role_Create'],
                // },
                resolve: {
                    roleCreateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiRoleCreate', '/app/identity/roles/role-create.controller.js'));
                    }]
                }
            });

            $stateProvider.state('identity.management.role.update', {
                component: 'uiRoleUpdate',
                url: '/update/:id',
                // data: {
                //     authorize: ['Role_Update'],
                // },
                resolve: {
                    roleUpdateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiRoleUpdate', '/app/identity/roles/role-update.controller.js'));
                    }]
                }
            });
        });
}());