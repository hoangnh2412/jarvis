(function () {
    'use strict';

    angular
        .module('admin', [
            'admin.catalog',
            'admin.business'
        ])
        .component('uiAdmin', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiAdmin', '/app/admin/admin.template.html');
            }],
            controller: 'adminController',
            bindings: {
                context: '='
            }
        })
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('admin', {
                abstract: true,
                component: 'uiAdmin',
                url: '/admin',
                resolve: {
                    uiBootstrap: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleUiBootstrap');
                    }],
                    validate: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleValidate');
                    }],
                    autofocus: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleAutofocus');
                    }],
                    tooltip: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleTooltip');
                    }]
                }
            });
        }])
        .config(['$ocLazyLoadProvider', function ($ocLazyLoadProvider) {
            $ocLazyLoadProvider.config({
                modules: [
                    {
                        name: 'moduleDaterangepicker',
                        serie: true,
                        files: [
                            'libs/jquery/daterangepicker/daterangepicker.css',
                            'libs/jquery/daterangepicker/moment-with-locales.js',
                            'libs/jquery/daterangepicker/daterangepicker.js',
                            'libs/angular/daterangepicker/angular-daterangepicker-init.js',
                            'libs/angular/daterangepicker/angular-daterangepicker-config.js',
                            'libs/angular/daterangepicker/angular-daterangepicker.js'
                        ]
                    },
                    {
                        name: 'moduleChart',
                        serie: true,
                        files: [
                            'libs/jquery/chart.js',
                            'libs/angular/angular-chart.js'
                        ]
                    },
                    {
                        name: 'moduleMultiselect',
                        serie: true,
                        files: [
                            'libs/jquery/bootstrap-multiselect/bootstrap-multiselect.css',
                            'libs/jquery/bootstrap-multiselect/bootstrap-multiselect.js',
                            'app/shared/directives/multiselect.js'
                        ]
                    },
                    {
                        name: 'moduleUiBootstrap',
                        serie: true,
                        files: [
                            'libs/jquery/moment.js',
                            'libs/jquery/moment-with-locales.js',
                            'libs/angular/ui-bootstrap-tpls-2-5-0.js'
                        ]
                    }
                ]
            });
        }])
        .controller('adminController', ['$scope', 'APP_CONFIG', 'cacheService', function ($scope, APP_CONFIG, cacheService) {
            var ctrl = $scope.$ctrl;

            ctrl.$onInit = function () {
                var context = cacheService.get('context');
                ctrl.context = context;
                ctrl.context.theme = APP_CONFIG.THEME;
            };
        }]);
})();