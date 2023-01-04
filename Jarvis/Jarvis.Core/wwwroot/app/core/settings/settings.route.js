(function () {
    'use strict';

    angular
        .module('core')
        .component('uiSettings', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiSettings', '/app/core/settings/settings.template.html');
            }],
            controller: 'settingsController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('core.settings', {
                url: '/settings',
                component: 'uiSettings',
                // data: {
                //     authorize: ['Setting_Read']
                // },
                resolve: {
                    settingsController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiSettings', '/app/core/settings/settings.controller.js'));
                    }],
                    settingService: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.replace('/app/core/settings/setting.service.js'));
                    }],
                }
            });
        });
}());