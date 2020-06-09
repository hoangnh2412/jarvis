(function () {
    'use strict';

    angular
        .module('identity')
        .component('uiRoleRead', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiRoleRead', '/app/identity/roles/role-read.template.html');
            }],
            controller: 'roleReadController',
            bindings: {
                context: '='
            }
        })
        .component('uiRoleCreate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiRoleCreate', '/app/identity/roles/role-create.template.html');
            }],
            controller: 'roleCreateController',
            bindings: {
                context: '='
            }
        })
        .component('uiRoleUpdate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiRoleUpdate', '/app/identity/roles/role-update.template.html');
            }],
            controller: 'roleUpdateController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('identity.backend.role', {
                url: '/role',
                redirectTo: 'identity.backend.role.read',
                resolve: {
                    roleService: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('/app/jarvis/identity/roles/role.service.js');
                    }]
                }
            });

            $stateProvider.state('identity.backend.role.read', {
                url: '/read',
                component: 'uiRoleRead',
                data: {
                    authorize: ['Role_Read'],
                },
                resolve: {
                    rolesController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiRoleRead', '/app/identity/roles/role-read.controller.js'));
                    }]
                }
            });

            $stateProvider.state('identity.backend.role.create', {
                component: 'uiRoleCreate',
                url: '/create',
                data: {
                    authorize: ['Role_Create'],
                },
                resolve: {
                    roleCreateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiRoleCreate', '/app/identity/roles/role-create.controller.js'));
                    }]
                }
            });

            $stateProvider.state('identity.backend.role.update', {
                component: 'uiRoleUpdate',
                url: '/update/:id',
                data: {
                    authorize: ['Role_Update'],
                },
                resolve: {
                    roleUpdateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiRoleUpdate', '/app/identity/roles/role-update.controller.js'));
                    }]
                }
            });
        });
}());