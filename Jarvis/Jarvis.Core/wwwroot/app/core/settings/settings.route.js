(function () {
    'use strict';

    angular
        .module('core')
        .component('uiSettings', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiSettings', '/app/core/settings/settings.template.html');
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
                data: {
                    authorize: ['Setting_Read']
                },
                resolve: {
                    settingsController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiSettings', '/app/core/settings/settings.controller.js'));
                    }],
                    settingService: ['$ocLazyLoad', 'APP_CONFIG', function ($ocLazyLoad, APP_CONFIG) {
                        return $ocLazyLoad.load((APP_CONFIG.BASE_PATH ? APP_CONFIG.BASE_PATH : '') + '/app/core/settings/setting.service.js');
                    }],
                }
            });
        });
}());