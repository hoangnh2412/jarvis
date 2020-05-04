(function () {
    'use strict';

    var jvAutofocus = function ($timeout) {
        return {
            restrict: 'A',
            link: function (scope, element) {
                //element.focus();

                //`.focus()` (without `[0]`) only works when you have jQuery loaded. The `$timeout` is required because at execution time, the element may not be present in the DOM yet.
                $timeout(function () {
                    element[0].focus();
                });
            }
        };
    };

    angular
        .module('autofocus', [])
        .directive('jvAutofocus', jvAutofocus);
}());