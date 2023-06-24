(function () {
    'use strict';

    angular
        .module('core')
        .component('uiEmailTemplateList', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiEmailTemplateList', '/app/core/emails/email-template-list.template.html');
            }],
            controller: 'emailTemplateListController',
            bindings: {
                context: '='
            }
        })
        .component('uiEmailTemplateDetail', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiEmailTemplateDetail', '/app/core/emails/email-template-detail.template.html');
            }],
            controller: 'emailTemplateDetailController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('core.email-template', {
                url: '/email-template',
                redirectTo: 'core.email-template.list',
                resolve: {
                    emailTemplateService: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.replace('/app/core/emails/email-template.service.js'));
                    }]
                }
            });

            $stateProvider.state('core.email-template.list', {
                url: '/list',
                component: 'uiEmailTemplateList',
                resolve: {
                    emailTemplateListController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiEmailTemplateList', '/app/core/emails/email-template-list.controller.js'));
                    }]
                }
            });

            $stateProvider.state('core.email-template.detail', {
                component: 'uiEmailTemplateDetail',
                url: '/detail/:id?',
                params: {
                    id: null
                },
                resolve: {
                    tinymce: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleTinymce');
                    }],
                    emailTemplateDetailController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiEmailTemplateDetail', '/app/core/emails/email-template-detail.controller.js'));
                    }]
                }
            });
        });
}());