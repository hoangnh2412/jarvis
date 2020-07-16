(function () {
    'use strict';

    angular
        .module('core', [])
        .component('uiCore', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiCore', '/app/core/core.template.html');
            }],
            bindings: {
                context: '='
            }
        })
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('core', {
                abstract: true,
                component: 'uiCore',
                resolve: {
                    validate: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleValidate');
                    }],
                    autofocus: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleAutofocus');
                    }],
                    tooltip: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleTooltip');
                    }],
                    uiBootstrap: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleUiBootstrap');
                    }]
                }
            });
        }]);
}());