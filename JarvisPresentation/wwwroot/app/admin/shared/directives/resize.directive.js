(function () {
    'use strict';

    var jvResize = function ($window) {
        function resize(element) {
            var type = element.attr('jv-resize');
            var offsetHeight = isNaN(parseInt(element.attr('jv-resize-offset-height'))) ? 0 : parseInt(element.attr('jv-resize-offset-height'));
            var offsetWidth = isNaN(parseInt(element.attr('jv-resize-offset-width'))) ? 0 : parseInt(element.attr('jv-resize-offset-width'));

            if (type === 'width') {
                element.css({
                    width: $window.innerWidth - offsetWidth
                });
            }
            else if (type === 'height') {
                element.css({
                    height: $window.innerHeight - offsetHeight
                });
            }
            else {
                element.css({
                    width: $window.innerWidth - offsetWidth,
                    height: $window.innerHeight - offsetHeight
                });
            }
        };

        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                resize(element);

                angular.element($window).on('resize', function () {
                    resize(element);
                    scope.$apply();
                });
            }
        };
    };

    angular
        .module('resize', [])
        .directive('jvResize', jvResize);
}());