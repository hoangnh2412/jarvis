(function () {
    'use strict';

    angular
        .module('identity')
        .component('uiLogin', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiLogin', '/app/identity/login/login.template.html');
            }],
            controller: 'loginController',
            bindings: {
                context: '='
            }
        })
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('identity.frontend.login', {
                url: '/login',
                component: 'uiLogin',
                // onEnter: function () {
                //     var $body = angular.element('body');
                //     $body.addClass('hold-transition login-page');
                // },
                // onExit: function () {
                //     var $body = angular.element('body');
                //     $body.removeClass();
                // },
                resolve: {
                    validate: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleValidate');
                    }],
                    autofocus: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleAutofocus');
                    }],
                    loginController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiLogin', '/app/identity/login/login.controller.js'));
                    }]
                }
            });
        }]);
}());