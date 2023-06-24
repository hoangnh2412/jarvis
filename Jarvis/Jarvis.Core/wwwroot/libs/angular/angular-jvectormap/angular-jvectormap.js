(function () {
    'use strict';

    var ngVectorMap = function () {
        return {
            restrict: 'EA',
            scope: {
                chartMap: '=?',
                chartData: '=?',
                chartOptions: '=?',
                onRegionTipShow: '=?'
            },
            link: function (scope, elm, attr) {
                // console.log('scope', scope);
                // console.log('elm', elm);
                // console.log('attr', attr);

                new jvm.Map({
                    container: elm,
                    map: scope.chartMap,
                    series: {
                        regions: [
                            {
                                values: scope.chartData,
                                scale: ['#C8EEFF', '#0071A4'],
                                normalizeFunction: 'polynomial',
                                legend: {
                                    horizontal: false,
                                    vertical: true,
                                    title: 'Mức độ'
                                }
                            }
                        ]
                    }
                });
            }
        };
    };

    angular
        .module('ngVectorMap', [])
        .directive('ngVectorMap', ngVectorMap);
}());