(function () {
    'use strict';

    angular
        .module('jarvis')
        .component('uiWidget', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiWidget', '/app/shared/components/widgets/widget.template.html');
            }],
            controller: 'widgetController',
            bindings: {
                context: '='
            }
        })
        .controller('widgetController', ['$scope', function ($scope) {
            var ctrl = $scope.$ctrl;

        }]);
}());