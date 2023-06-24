(function () {
    'use strict';

    angular
        .module('admin')
        .component('uiDashboard', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiDashboard', '/app/admin/dashboard/dashboard.template.html');
            }],
            controller: 'dashboardController',
            bindings: {
                context: '='
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('admin.dashboard', {
                component: 'uiDashboard',
                url: '/dashboard?from&to',
                resolve: {
                    dashboardController: ['$ocLazyLoad', 'componentService', function ($ocLazyLoad, componentService) {
                        return $ocLazyLoad.load(componentService.getControllerUrl('uiDashboard', '/app/admin/dashboard/dashboard.controller.js'));
                    }],
                    widget: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load(['/app/shared/components/widgets/widget.component.js']);
                    }],
                    widgetChartNumber: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load(['/app/shared/components/widgets/widget-chart-number.component.js']);
                    }],
                    chart: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleChart');
                    }],
                    daterangepicker: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load('moduleDaterangepicker');
                    }],
                    selectize: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load(['/libs/jquery/selectize/selectize.css', '/libs/jquery/selectize/selectize.js', '/libs/angular/angular-selectize.js']);
                    }],
                    httpQueue: ['$ocLazyLoad', function ($ocLazyLoad) {
                        return $ocLazyLoad.load(['/app/shared/factories/http-queue.factory.js']);
                    }]
                }
            });
        });
}());