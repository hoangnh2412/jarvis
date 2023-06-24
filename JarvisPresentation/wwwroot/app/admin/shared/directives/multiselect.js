(function () {
    'use strict';

    var multiselect = function () {
        return {
            link: function (scope, element, attrs) {
                element.multiselect({
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
                    // buttonText: function (options, select) {
                    //     if (options.length == 0) {
                    //         return 'None selected <b class="caret"></b>';
                    //     }
                    //     else if (options.length > 3) {
                    //         return options.length + ' selected  <b class="caret"></b>';
                    //     }
                    //     else {
                    //         var selected = '';
                    //         options.each(function () {
                    //             selected += $(this).text() + ', ';
                    //         });
                    //         return selected.substr(0, selected.length - 2) + ' <b class="caret"></b>';
                    //     }
                    // },
                    // Replicate the native functionality on the elements so
                    // that angular can handle the changes for us.
                    onChange: function (optionElement, checked) {
                        if (optionElement != null) {
                            optionElement.removeAttr('selected');
                        }
                        if (checked) {
                            optionElement.prop('selected', 'selected');
                        }
                        element.change();
                    }
                });

                // Watch for any changes to the length of our select element
                scope.$watch(function () {
                    return element[0].length;
                }, function () {
                    scope.$applyAsync(element.multiselect('rebuild'));
                });

                // Watch for any changes from outside the directive and refresh
                scope.$watch(attrs.ngModel, function () {
                    element.multiselect('refresh');
                });

            }

        };
    };

    angular
        .module('ngMultiselect', [])
        .directive('multiselect', multiselect);
}());
