(function () {
    'use strict';

    angular
        .module('jarvis')
        .component('uiUserInfo', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getJarvisTemplateUrl('uiUserInfo', '/app/shared/components/user-info/user-info.template.html');
            }],
            controller: 'userInfoController',
            bindings: {
                context: '='
            }
        })
        .controller('userInfoController', ['$scope', function ($scope) {
            var ctrl = $scope.$ctrl;
        }]);
}());