(function () {
    'use strict';

    var jvUtc2Gmt = function ($filter) {
        return function (utcDateString, format) {
            if (!utcDateString) {
                return;
            }

            // append 'Z' to the date string to indicate UTC time if the timezone isn't already specified
            if (utcDateString.indexOf('Z') === -1 && utcDateString.indexOf('+') === -1) {
                utcDateString += 'Z';
            }

            return $filter('date')(utcDateString, format);
        };
    };

    angular
        .module('jarvis.filters', [])
        .filter('jvUtc2Gmt', jvUtc2Gmt);
}());