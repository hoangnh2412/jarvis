(function () {
    'use strict';
    
    angular
        .module('presentation')
        .component('uiDashboard', {
            templateUrl: '/app/presentation/dashboard/dashboard.template.html',
            // controller: 'loginController',
            bindings: {
                context: '='
            }
        })
        .config(['$stateProvider', ($stateProvider) => {
            $stateProvider.state('presentation.dashboard', {
                url: '/dashboard',
                component: 'uiDashboard',
                data: {
                    authorize: [],
                },
                resolve: {
                    uiBootstrap: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleUiBootstrap');
                    }]
                    // validate: ['$ocLazyLoad', ($ocLazyLoad) => {
                    //     return $ocLazyLoad.load('moduleValidate');
                    // }],
                    // autofocus: ['$ocLazyLoad', ($ocLazyLoad) => {
                    //     return $ocLazyLoad.load('moduleAutofocus');
                    // }],
                    // loginController: ['$ocLazyLoad', ($ocLazyLoad) => {
                    //     return $ocLazyLoad.load('/app/identity/login/login.controller.js');
                    // }],
                    // authorized: ['APP_CONFIG', '$state', 'cacheService', (APP_CONFIG, $state, cacheService) => {
                    //     var token = cacheService.get('token');
                    //     if (token) {
                    //         $state.go(APP_CONFIG.DASHBOARD_STATE);
                    //         return;
                    //     }
                    // }]
                }
            });
        }]);
}());