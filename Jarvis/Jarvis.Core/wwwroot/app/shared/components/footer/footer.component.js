(function () {
    'use strict';

    angular
        .module('jarvis')
        .component('uiFooter', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiFooter', '/app/shared/components/footer/footer.template.html');
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