(function () {
    'use strict';

    var jvAbs = function () {
        return function (num) {
            return Math.abs(num);
        }
    };

    angular
        .module('abs', [])
        .filter('jvAbs', jvAbs);
}());