(function () {
    'use strict';

    angular
        .module('admin.business', [])
        .component('uiBusiness', {
            template: '<ui-view context="$ctrl.context"></ui-view>',
            bindings: {
                context: '='
            }
        })
        .config(['$stateProvider', function ($stateProvider) {
            $stateProvider.state('admin.business', {
                abstract: true,
                url: '/business',
                component: 'uiBusiness',
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
                    },
                    {
                        name: 'moduleVideo',
                        serie: true,
                        files: [
                            'libs/audio/video.css',
                            'libs/audio/video.js',
                            'libs/audio/wavesurfer.js',
                            'libs/audio/videojs.wavesurfer.css',
                            'libs/audio/videojs.wavesurfer.js',
                            'libs/audio/soundtouch.js',
                            'libs/angular/angular-scroll/angular-scroll.js'
                        ]
                    }
                ]
            });
        }]);
})();