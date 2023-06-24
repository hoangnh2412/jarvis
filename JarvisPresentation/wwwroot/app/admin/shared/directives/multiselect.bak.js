(function () {
    'use strict';

    var multiselect = function () {
        function link(scope, element, attrs) {
            var options = {
                enableClickableOptGroups: true,
                includeSelectAllOption: true,
                selectAllText: 'Chọn tất cả',
                enableFiltering: true,
                filterPlaceholder: 'Tìm kiếm...',
                enableCaseInsensitiveFiltering: true,
                buttonWidth: '100%',
                maxHeight: 200,
                nonSelectedText: attrs.nonSelectedText ? attrs.nonSelectedText : 'Chọn giá trị',
                allSelectedText: 'Đã chọn tất cả',
                onChange: function () {
                    //element.change();
                }
            };
            element.multiselect(options);
        }

        return {
            restrict: 'A',
            link: link
        };
    };

    angular
        .module('ngMultiselect', [])
        .directive('multiselect', multiselect)
        .config(['$provide', function ($provide) {
            $provide.decorator('selectDirective', ['$delegate', function ($delegate) {
                var directive = $delegate[0];

                directive.compile = function () {

                    function post(scope, element, attrs, ctrls) {
                        directive.link.post.apply(this, arguments);

                        var ngModelController = ctrls[1];
                        if (ngModelController && attrs.multiselect !== null && angular.isDefined(attrs.multiselect)) {
                            var originalRender = ngModelController.$render;
                            ngModelController.$render = function () {
                                originalRender();
                                element.multiselect('refresh');
                            };
                        }
                    }

                    return {
                        pre: directive.link.pre,
                        post: post
                    };
                };

                return $delegate;
            }]);
        }]);
}());