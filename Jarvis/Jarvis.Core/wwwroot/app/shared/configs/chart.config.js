(function () {
    'use strict';

    angular.module('jvChart', ['ngChartjs'])
        .config(function (ChartJsProvider) {
            ChartJsProvider.setOptions({ responsive: true });
            ChartJsProvider.setOptions('Line', { responsive: true });
            ChartJsProvider.setOptions({ colors: ['#803690', '#00ADF9', '#DCDCDC', '#46BFBD', '#FDB45C', '#949FB1', '#4D5360'] });
        });
}());