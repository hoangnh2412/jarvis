(function () {
    'use strict';

    angular
        .module('core')
        .component('uiEmailHistoryList', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiEmailHistoryList', '/app/core/emails/email-history-list.template.html');
            }],
            controller: 'emailHistoryListController',
            bindings: {
                context: '='
            }
        })
        .component('uiEmailHistoryCreate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiEmailHistoryCreate', '/app/core/emails/email-history-create.template.html');
            }],
            controller: 'emailHistoryCreateController',
            bindings: {
                context: '='
            }
        })
        .component('uiEmailHistoryUpdate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiEmailHistoryUpdate', '/app/core/emails/email-history-update.template.html');
            }],
            controller: 'emailHistoryUpdateController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('core.email-history', {
                url: '/email-history',
                redirectTo: 'core.email-history.list',
                resolve: {
                    emailHistoryService: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.replace('/app/core/emails/email-history.service.js'));
                    }]
                }
            });

            $stateProvider.state('core.email-history.list', {
                url: '/list',
                component: 'uiEmailHistoryList',
                resolve: {
                    emailHistoryListController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiEmailHistoryList', '/app/core/emails/email-history-list.controller.js'));
                    }]
                }
            });

            $stateProvider.state('core.email-history.create', {
                component: 'uiEmailHistoryCreate',
                url: '/create',
                resolve: {
                    emailHistoryCreateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiEmailHistoryCreate', '/app/core/emails/email-history-create.controller.js'));
                    }]
                }
            });

            $stateProvider.state('core.email-history.update', {
                component: 'uiEmailHistoryUpdate',
                url: '/update/:id',
                resolve: {
                    emailHistoryUpdateController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiEmailHistoryUpdate', '/app/core/emails/email-history-update.controller.js'));
                    }]
                }
            });
        });
}());