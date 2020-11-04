(function () {
    'use strict';


    angular
        .module('jarvis', [
            'ui.router',
            'oc.lazyLoad',
            'ngSanitize',
            'ngStorage',
            'ngLoadingBar',
            'ngSweetAlert',
            // 'ui.bootstrap',
            // 'ngAnimate',
            // 'ngColorPicker',
            // 'datatables',

            'core',
            'identity',
            'app'
        ])
        .config(['APP_CONFIG', '$locationProvider', '$urlRouterProvider', function (APP_CONFIG, $locationProvider, $urlRouterProvider) {
            //Remove !# from URL
            // if (window.history && window.history.pushState) {
            //     $locationProvider.html5Mode({
            //         enabled: true,
            //         requireBase: true,
            //         rewriteLinks: true
            //     })
            //     $locationProvider.hashPrefix('');
            // };
            //$urlMatcherFactoryProvider.strictMode(false);
            //$compileProvider.debugInfoEnabled(false);

            // $urlRouterProvider.otherwise(APP_CONFIG.DEFAULT_URL);
            $urlRouterProvider.otherwise(APP_CONFIG.DEFAULT_URL);
            // $urlRouterProvider.when('/', APP_CONFIG.DEFAULT_URL);
        }])
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('jarvis', {
                abstract: true,
                template: '<ui-view context="$ctrl.context"></ui-view>',
                resolve: {
                    validate: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleValidate');
                    }],
                    autofocus: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleAutofocus');
                    }],
                    tooltip: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleTooltip');
                    }],
                    uiBootstrap: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleUiBootstrap');
                    }]
                }
            });

            $stateProvider.state('jarvis.shared', {
                abstract: true,
                template: '<ui-view context="$ctrl.context"></ui-view>',
            });
        }])
        .config(['$ocLazyLoadProvider', 'APP_CONFIG', function ($ocLazyLoadProvider, APP_CONFIG) {
            var replaceUrl = function (url) {
                return url.replace('/app/', APP_CONFIG.BASE_UI_PATH);
            };
            $ocLazyLoadProvider.config({
                // debug: true,
                // events: true,
                modules: [
                    {
                        name: 'moduleValidate',
                        serie: true,
                        files: ['/libs/jquery/jquery-validate.js', '/libs/angular/angular-validate.js', replaceUrl('/app/shared/configs/validate.config.js')]
                    },
                    {
                        name: 'moduleAutofocus',
                        files: [replaceUrl('/app/shared/directives/autofocus.directive.js')]
                    },
                    {
                        name: 'moduleTooltip',
                        files: [replaceUrl('/app/shared/directives/tooltip.directive.js')]
                    },
                    {
                        name: 'moduleUiBootstrap',
                        serie: true,
                        files: ['/libs/jquery/moment.js', '/libs/jquery/moment-with-locales.js', '/libs/angular/ui-bootstrap-tpls-2-5-0.js']
                    },
                    {
                        name: 'moduleTree',
                        serie: true,
                        files: ['/libs/angular/angular-tree/angular-ui-tree.min.js', '/libs/angular/angular-tree/angular-ui-tree.min.css', '/libs/angular/angular-tree/jarvis-tree.css']
                    }
                ]
            });
        }])
        .config(['cfpLoadingBarProvider', function (cfpLoadingBarProvider) {
            cfpLoadingBarProvider.includeSpinner = true;
            cfpLoadingBarProvider.includeBar = true;
        }])
        .config(['storeProvider', function (storeProvider) {
            storeProvider.setStore('sessionStorage');
        }])
        .run(['$uiRouter', 'APP_CONFIG', function ($uiRouter, APP_CONFIG) {
            if (APP_CONFIG.VISUALIZER) {
                var Visualizer = window['@uirouter/visualizer'].Visualizer;
                var pluginInstance = $uiRouter.plugin(Visualizer);
            }
        }])
        .component('uiJarvis', {
            template: '<ui-loading context="$ctrl.context"></ui-loading><ui-view context="$ctrl.context"></ui-view>',
            controller: 'jarvisController',
            controllerAs: '$ctrl',
        })
        .controller('jarvisController', ['$scope', 'cacheService', function ($scope, cacheService) {
            var ctrl = $scope.$ctrl;
            ctrl.context = {};

            ctrl.$onInit = function () {
                ctrl.context = cacheService.get('context');

                if (!ctrl.context) {
                    ctrl.context = {};
                }
            };
        }]);
})();