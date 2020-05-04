(function () {
    'use strict';

    var jvNewLine = function () {
        return function (text) {
            if (text)
                return text.replace(/\n/g, '<br />');
        };
    };

    angular
        .module('jarvis.filters', [])
        .filter('jvNewLine', jvNewLine);
}());