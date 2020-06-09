(function () {
    'use strict';

    angular
        .module('core')
        .component('uiLabelRead', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiLabelRead', '/app/core/labels/label-read.template.html');
            }],
            controller: 'labelReadController',
            bindings: {
                context: '='
            }
        })
        .component('uiLabelCreate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiLabelCreate', '/app/core/labels/label-create.template.html');
            }],
            controller: 'labelCreateController',
            bindings: {
                context: '='
            }
        })
        .component('uiLabelUpdate', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiLabelUpdate', '/app/core/labels/label-update.template.html');
            }],
            controller: 'labelUpdateController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('core.label', {
                url: '/label',
                redirectTo: 'core.label.read',
                resolve: {
                    labelService: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('/app/jarvis/core/labels/label.service.js');
                    }]
                }
            });

            $stateProvider.state('core.label.read', {
                url: '/read',
                component: 'uiLabelRead',
                data: {
                    authorize: ['Label_Read'],
                },
                resolve: {
                    labelReadController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiLabelRead', '/app/core/labels/label-read.controller.js'));
                    }]
                }
            });

            $stateProvider.state('core.label.create', {
                component: 'uiLabelCreate',
                url: '/create',
                data: {
                    authorize: ['Label_Create']
                },
                resolve: {
                    labelController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiLabelCreate', '/app/core/labels/label-create.controller.js'));
                    }]
                }
            });

            $stateProvider.state('core.label.update', {
                component: 'uiLabelUpdate',
                url: '/update/:id',
                data: {
                    authorize: ['Label_Update']
                },
                resolve: {
                    labelController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiLabelUpdate', '/app/core/labels/label-update.controller.js'));
                    }]
                }
            });
        });
}());