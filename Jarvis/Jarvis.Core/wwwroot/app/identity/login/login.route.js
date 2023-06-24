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
                context: '=',
                transition: '<'
            }
        })
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('identity.auth.login', {
                url: '/login',
                component: 'uiLogin',
                // onEnter: ['$transition$', '$state', function ($transition$, $state) {
                //     window.$transition$ = $transition$;
                //     window.$state = $state;
                //     console.log($transition$, $state);
                // }],
                // onExit: function () {
                //     var $body = angular.element('body');
                //     $body.removeClass();
                // },
                resolve: {
                    transition: '$transition$',
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