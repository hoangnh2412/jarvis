(function () {
    'use strict';

    angular
        .module('identity')
        .component('uiChangePassword', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiChangePassword', '/app/identity/change-password/change-password.template.html');
            }],
            controller: 'changePasswordController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('identity.backend.change-password', {
                url: '/change-password',
                component: 'uiChangePassword',
                data: {
                    authorize: [],
                },
                onEnter: function () {
                    var $body = angular.element('body');
                    $body.addClass('hold-transition login-page');
                },
                onExit: function () {
                    var $body = angular.element('body');
                    $body.removeClass();
                },
                resolve: {
                    changePasswordController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getJarvisControllerUrl('uiChangePassword', '/app/identity/change-password/change-password.controller.js'));
                    }]
                }
            });
        });

}());