(function () {
    'use strict';

    angular
        .module('presentation', [])
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('presentation', {
                abstract: true,
                template: '<ui-view context="$ctrl.context"></ui-view>',
                // resolve: {
                //     validate: ['$ocLazyLoad', function ($ocLazyLoad) {
                //         return $ocLazyLoad.load('moduleValidate');
                //     }],
                //     autofocus: ['$ocLazyLoad', function ($ocLazyLoad) {
                //         return $ocLazyLoad.load('moduleAutofocus');
                //     }],
                //     tooltip: ['$ocLazyLoad', function ($ocLazyLoad) {
                //         return $ocLazyLoad.load('moduleTooltip');
                //     }],
                //     uiBootstrap: ['$ocLazyLoad', function ($ocLazyLoad) {
                //         return $ocLazyLoad.load('moduleUiBootstrap');
                //     }]
                // }
            });
        }]);
}());