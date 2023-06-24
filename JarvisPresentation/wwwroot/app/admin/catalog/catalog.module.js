(function () {
    'use strict';

    angular
        .module('admin.catalog', [])
        .component('uiCatalog', {
            template: '<ui-view context="$ctrl.context"></ui-view>',
            bindings: {
                context: '='
            }
        })
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('admin.catalog', {
                abstract: true,
                url: '/catalog',
                component: 'uiCatalog',
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
                        name: 'moduleUiBootstrap',
                        serie: true,
                        files: [
                            'libs/jquery/moment.js',
                            'libs/jquery/moment-with-locales.js',
                            'libs/angular/ui-bootstrap-tpls-2-5-0.js'
                        ]
                    },
                    {
                        name: 'moduleNumberOnly',
                        serie: true,
                        files: [
                            'libs/angular/ng-number-only.js'
                        ]
                    }
                    // {
                    //     name: 'moduleDateRangePicker',
                    //     serie: true,
                    //     files: [
                    //         'libs/jquery/daterangepicker/daterangepicker.css',
                    //         'libs/jquery/daterangepicker/moment-with-locales.js',
                    //         'libs/jquery/daterangepicker/daterangepicker.js',
                    //         'libs/angular/daterangepicker/angular-daterangepicker-init.js',
                    //         'libs/angular/daterangepicker/angular-daterangepicker-config.js',
                    //         'libs/angular/daterangepicker/angular-daterangepicker.js'
                    //     ]
                    // },
                    // {
                    //     name: 'moduleVideo',
                    //     serie: true,
                    //     files: [
                    //         'libs/audio/video.css',
                    //         'libs/audio/video.js',
                    //         'libs/audio/wavesurfer.js',
                    //         'libs/audio/videojs.wavesurfer.css',
                    //         'libs/audio/videojs.wavesurfer.js',
                    //         'libs/angular/angular-scroll/angular-scroll.js'
                    //     ]
                    // }
                ]
            });
        }]);
})();