(function () {
    'use strict';

    angular
        .module('identity')
        .component('uiProfile', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiProfile', '/app/identity/profile/profile.template.html');
            }],
            controller: 'profileController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('identity.backend.profile', {
                url: '/profile',
                component: 'uiProfile',
                data: {
                    authorize: [],
                },
                //onEnter: function () {
                //    var $body = angular.element('body');
                //    $body.addClass('hold-transition skin-blue sidebar-mini sidebar-collapse');
                //},
                resolve: {
                    profileService: ['$ocLazyLoad', 'APP_CONFIG', function ($ocLazyLoad, APP_CONFIG) {
                        return $ocLazyLoad.load(APP_CONFIG.BASE_PATH ? APP_CONFIG.BASE_PATH + '/app/jarvis/identity/profile/profile.service.js' : '/app/jarvis/identity/profile/profile.service.js');
                    }],
                    profileController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiProfile', '/app/identity/profile/profile.controller.js'));
                    }],
                    imgCrop: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleImgCrop');
                    }],
                }
            });
        });
}());