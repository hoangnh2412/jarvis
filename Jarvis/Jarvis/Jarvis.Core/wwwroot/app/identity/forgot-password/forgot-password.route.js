(function () {
    'use strict';

    angular
        .module('identity')
        .component('uiForgotPassword', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiForgotPassword', '/app/identity/forgot-password/forgot-password.template.html');
            }],
            controller: 'forgotPasswordController',
            bindings: {
                context: '='
            }
        })
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('identity.frontend.forgot-password', {
                url: '/forgot-password?:idUser&:key',
                component: 'uiForgotPassword',
                resolve: {
                    validate: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleValidate');
                    }],
                    autofocus: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleAutofocus');
                    }],
                    forgotPasswordController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiForgotPassword', '/app/identity/forgot-password/forgot-password.controller.js'));
                    }]
                }
            });
        }]);
}());