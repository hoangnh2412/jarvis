(function () {
    'use strict';

    angular
        .module('identity')
        .component('uiUserRead', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiUserRead', '/app/identity/users/user-read.template.html');
            }],
            controller: 'userReadController',
            bindings: {
                context: '='
            }
        })
        .component('uiUserCreate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiUserCreate', '/app/identity/users/user-create.template.html');
            }],
            controller: 'userCreateController',
            bindings: {
                context: '='
            }
        })
        .component('uiUserUpdate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiUserUpdate', '/app/identity/users/user-update.template.html');
            }],
            controller: 'userUpdateController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('identity.backend.user', {
                url: '/user',
                redirectTo: 'identity.backend.user.read',
                resolve: {
                    userService: ['$ocLazyLoad', 'APP_CONFIG', function ($ocLazyLoad, APP_CONFIG) {
                        return $ocLazyLoad.load((APP_CONFIG.BASE_PATH ? APP_CONFIG.BASE_PATH : '') + '/app/jarvis/identity/users/user.service.js');
                    }]
                }
            });

            $stateProvider.state('identity.backend.user.read', {
                url: '/read',
                component: 'uiUserRead',
                data: {
                    authorize: ['User_Read'],
                },
                resolve: {
                    userReadController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiUserRead', '/app/identity/users/user-read.controller.js'));
                    }]
                }
            });

            $stateProvider.state('identity.backend.user.create', {
                component: 'uiUserCreate',
                url: '/create',
                data: {
                    authorize: ['User_Create'],
                },
                resolve: {
                    userCreateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiUserCreate', '/app/identity/users/user-create.controller.js'));
                    }],
                    roleService: ['$ocLazyLoad', 'APP_CONFIG', function ($ocLazyLoad, APP_CONFIG) {
                        return $ocLazyLoad.load((APP_CONFIG.BASE_PATH ? APP_CONFIG.BASE_PATH : '') + '/app/jarvis/identity/roles/role.service.js');
                    }]
                }
            });

            $stateProvider.state('identity.backend.user.update', {
                component: 'uiUserUpdate',
                url: '/update/:id',
                data: {
                    authorize: ['User_Update'],
                },
                resolve: {
                    userUpdateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiUserUpdate', '/app/identity/users/user-update.controller.js'));
                    }],
                    roleService: ['$ocLazyLoad', 'APP_CONFIG', function ($ocLazyLoad, APP_CONFIG) {
                        return $ocLazyLoad.load((APP_CONFIG.BASE_PATH ? APP_CONFIG.BASE_PATH : '') + '/app/jarvis/identity/roles/role.service.js');
                    }]
                }
            });
        });
}());