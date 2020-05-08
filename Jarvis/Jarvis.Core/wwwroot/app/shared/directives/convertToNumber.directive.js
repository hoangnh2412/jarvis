(function () {
    'use strict';

    var jvConvertToNumber = function () {
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, ngModel) {
                ngModel.$parsers.push(function (val) {
                    return parseInt(val, 10);
                });
                ngModel.$formatters.push(function (val) {
                    return '' + val;
                });
            }
        };
    };

    angular
        .module('convertToNumber', [])
        .directive('jvConvertToNumber', jvConvertToNumber);
}());