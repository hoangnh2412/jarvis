(function () {
    'use strict';

    var ngFileModel = function () {
        return {
            restrict: "A",
            require: '?ngModel',
            scope: {
                ngFileName: "="
            },
            link: function (scope, element, attributes, ngModel) {
                element.bind("change", function (changeEvent) {
                    if (changeEvent.target.files == null || changeEvent.target.files.length === 0) {
                        return;
                    }

                    var reader = new FileReader();
                    reader.onload = function (loadEvent) {
                        scope.$apply(function () {
                            var file = {
                                lastModified: changeEvent.target.files[0].lastModified,
                                lastModifiedDate: changeEvent.target.files[0].lastModifiedDate,
                                name: changeEvent.target.files[0].name,
                                size: changeEvent.target.files[0].size,
                                type: changeEvent.target.files[0].type,
                                data: loadEvent.target.result
                            };

                            //ngModel.$setViewValue(changeEvent.target.files[0]);
                            //scope.ngFileName = changeEvent.target.files[0].name;
                            ngModel.$setViewValue(file.data);
                            scope.ngFileName = file.name;
                        });
                    }

                    reader.readAsDataURL(changeEvent.target.files[0]);
                });
            }
        };
    };

    angular
        .module('ngFileModel', [])
        .directive('ngFileModel', ngFileModel);

}());