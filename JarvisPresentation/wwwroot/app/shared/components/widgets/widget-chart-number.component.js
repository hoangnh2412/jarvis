(function () {
    'use strict';

    angular
        .module('jarvis')
        .component('uiWidgetChartNumber', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiWidgetChartNumber', '/app/shared/components/widgets/widget-chart-number.template.html');
            }],
            controller: 'footerController',
            bindings: {
                context: '='
            }
        })
        .controller('footerController', ['$scope', function ($scope) {
            var ctrl = $scope.$ctrl;

        }]);
}());