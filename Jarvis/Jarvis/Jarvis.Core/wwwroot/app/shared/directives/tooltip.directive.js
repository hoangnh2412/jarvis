(function () {
    'use strict';

    var title = function () {
        return {
            restrict: 'A',
            link: function (scope, element) {
                angular.element('[data-toggle="tooltip"]').tooltip();
            }
        };
    };

    angular
        .module('tooltip', [])
        .directive('title', title);
}());