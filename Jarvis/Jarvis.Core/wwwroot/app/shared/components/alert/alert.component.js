﻿(function () {
    'use strict';

    angular
        .module('jarvis')
        .component('uiAlert', {
            templateUrl: ['componentService', function (componentService) {
                return componentService.getTemplateUrl('uiAlert', '/app/shared/components/alert/alert.template.html');
            }],
            controller: 'alertController',
            bindings: {
                context: '='
            }
        })
        .controller('alertController', ['$scope', 'APP_CONFIG', function ($scope, APP_CONFIG) {
            var ctrl = $scope.$ctrl;
            ctrl.message = APP_CONFIG.MESSAGE;
        }]);
}());