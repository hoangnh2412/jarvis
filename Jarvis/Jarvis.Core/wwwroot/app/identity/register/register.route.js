(function () {
    'use strict';

    angular
        .module('identity')
        .component('uiRegister', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiRegister', '/app/identity/register/register.template.html');
            }],
            controller: 'registerController',
            bindings: {
                context: '=',
                transition: '<'
            }
        })
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('identity.auth.register', {
                url: '/register',
                component: 'uiRegister',
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
                    registerController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiRegister', '/app/identity/register/register.controller.js'));
                    }]
                }
            });
        }]);
}());