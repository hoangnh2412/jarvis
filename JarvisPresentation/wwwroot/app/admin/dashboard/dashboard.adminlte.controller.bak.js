(function () {
    'use strict';

    function dashboardController($state, $stateParams, $timeout, $q, httpService, sweetAlert, $window, httpQueueService) {
        var ctrl = this;

        ctrl.times = {
            day: 1,
            week: 2,
            month: 3
        };

        ctrl.queryable = {
            limit: 10,
            from: $stateParams.from ? moment($stateParams.from).format('YYYY-MM-DD') : moment().subtract(7, 'days').format('YYYY-MM-DD'),
            to: $stateParams.to ? moment($stateParams.to).format('YYYY-MM-DD') : moment().format('YYYY-MM-DD'),
            type: ctrl.times.day,
            dimension: null,
            measure: null
        };

        ctrl.dimensions = {
            paging: {
                size: 999999,
                page: 1,
                q: null,
                dimension: 0
            },
            data: [
                { key: 5, code: 'scriptId', value: 'Kịch bản', measures: [], url: '' },
                { key: 1, code: 'agentId', value: 'Điện thoại viên', measures: [], url: '' }
            ]
        };

        ctrl.measures = [];

        ctrl.selectizeDimensionConfig = {
            create: false,
            maxItems: 1,
            valueField: 'key',
            labelField: 'value',
            searchField: ['value'],
            delimiter: '|',
            placeholder: 'Tiêu chí thống kê',
            onInitialize: function (selectize) {
                ctrl.selectizeDimension = selectize;
            },
            onChange: function (value) {
                ctrl.getMeasures(value);
            }
        };

        ctrl.selectizeMeasureConfig = {
            create: false,
            maxItems: 1,
            valueField: 'key',
            labelField: 'name',
            searchField: ['name'],
            delimiter: '|',
            placeholder: 'Dữ liệu thống kê',
            onInitialize: function (selectize) {
                ctrl.selectizeMeasure = selectize;
            },
            onChange: function (value) {
                if ((ctrl.queryable.dimension && ctrl.queryable.measure) || (!ctrl.queryable.dimension && !ctrl.queryable.measure)) {
                    ctrl.fetchData();
                }
            }
        };

        ctrl.statistic = {
            summary: {
                totalCall: 0, //Tổng số cuộc gọi
                totalCallerManyTimes: 0, //Tổng số khách hàng gọi nhiều lần
                totalCallUnHappy: 0, //Tổng số cuộc gọi UnHappy
                totalCallHigh: 0, //Tổng số cuộc gọi Cao
                totalCallSilence: 0, //Tổng số cuộc có khoảng lặng
                totalCallEndSatisfy: 0, //Tổng số cuộc gọi kết thúc hài lòng
                totalCallSentencePatternMiss: 0, //Tổng số cuộc gọi thiếu mẫu câu
                totalCallSentencePatternError: 0, //Tổng số cuộc gọi sai mẫu câu
                totalCallAnalysisSuccess: 0,
                callerManyTimesPercent: 0,
                callUnHappyPercent: 0,
                callEndSatisfyPercent: 0,
                callHighPercent: 0,
                callSilencePercent: 0,
                callSentencePatternErrorPercent: 0,
                callAnalysisSuccessPercent: 0,
            },
            chart: {
                title: '',
                data: [],
                labels: [],
                series: []
            },
            growth: {
                calls: {
                    value: 0,
                    percent: 0,
                    symbol: 0
                },
                callerManyTimes: {
                    value: 0,
                    percent: 0,
                    symbol: 0
                },
                callsUnHappy: {
                    value: 0,
                    percent: 0,
                    symbol: 0
                },
                callsHigh: {
                    value: 0,
                    percent: 0,
                    symbol: 0
                }
            },
            keywords: []
        };

        ctrl.dateRangePicker = {
            date: {
                startDate: moment($stateParams.from),
                endDate: moment($stateParams.to)
            },
            picker: null,
            options: {
                locale: {
                    separator: ' - ',
                    format: 'DD/MM/YYYY',
                    daysOfWeek: [
                        "CN",
                        "T2",
                        "T3",
                        "T4",
                        "T5",
                        "T6",
                        "T7"
                    ],
                    monthNames: [
                        "Tháng 1",
                        "Tháng 2",
                        "Tháng 3",
                        "Tháng 4",
                        "Tháng 5",
                        "Tháng 6",
                        "Tháng 7",
                        "Tháng 8",
                        "Tháng 9",
                        "Tháng 10",
                        "Tháng 11",
                        "Tháng 12"
                    ],
                    customRangeLabel: "Chọn ngày",
                    applyLabel: "Chọn",
                    cancelLabel: "Hủy"
                },
                maxDate: moment(),
                ranges: {
                    'Hôm nay': [moment(), moment()],
                    'Hôm qua': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                    '7 ngày trước': [moment().subtract(6, 'days'), moment()],
                    '30 ngày trước': [moment().subtract(29, 'days'), moment()],
                    'Tháng này': [moment().startOf('month'), moment().endOf('month')],
                    'Tháng trước': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
                },
                opens: "right",
                applyButtonClasses: 'btn-primary',
                cancelButtonClasses: 'btn-danger',
                alwaysShowCalendars: true,
                showCustomRangeLabel: true,
                autoApply: false
            }
        };

        angular.element('#drpTime').on('apply.daterangepicker', function (ev, picker) {
            ctrl.queryable.from = picker.startDate.format('YYYY-MM-DD');
            ctrl.queryable.to = picker.endDate.format('YYYY-MM-DD');

            $state.go('jarvis.dashboard', ctrl.queryable);
        });

        ctrl.data = {
            url: '/#!/calls?',
            summary: {
                total: {
                    value: 0,
                    percent: 0,
                    loading: true,
                    load: function () {
                        ctrl.data.summary.total.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/total',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.summary.total.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            ctrl.data.summary.total.value = response.data.value;
                            ctrl.data.summary.total.percent = response.data.percent;
                        });
                    },
                    view: function () {
                        var type = 2;
                        if (ctrl.queryable.dimension !== '5') {
                            type = 1;
                        }

                        ctrl.view(type);
                    }
                },
                hasQC: {
                    value: 0,
                    percent: 0,
                    loading: true,
                    load: function () {
                        ctrl.data.summary.hasQC.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/total-has-qc',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.summary.hasQC.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            ctrl.data.summary.hasQC.value = response.data.value;
                            ctrl.data.summary.hasQC.percent = response.data.percent;
                        });
                    },
                    view: function () {
                        ctrl.view(2);
                    }
                },
                notQC: {
                    value: 0,
                    percent: 0,
                    loading: true,
                    load: function () {
                        ctrl.data.summary.notQC.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/total-not-qc',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.summary.notQC.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            ctrl.data.summary.notQC.value = response.data.value;
                            ctrl.data.summary.notQC.percent = response.data.percent;
                        });
                    },
                    view: function () {
                        var type = 2;
                        if (ctrl.queryable.dimension !== '5') {
                            type = 1;
                        }

                        var url = ctrl.buildUrl(type);
                        url += '&evaluateType=0'
                        $window.open(url);
                    }
                },
                failQCauto: {
                    value: 0,
                    percent: 0,
                    loading: true,
                    load: function () {
                        ctrl.data.summary.failQCauto.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/total-qc-auto-fail',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.summary.failQCauto.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            ctrl.data.summary.failQCauto.value = response.data.value;
                            ctrl.data.summary.failQCauto.percent = response.data.percent;
                        });
                    },
                    view: function () {
                        var type = 2;
                        var url = ctrl.buildUrl(type);
                        url += '&evaluateType=2&evaluateResult=0'
                        $window.open(url);
                    }
                },
                failQCmanual: {
                    value: 0,
                    percent: 0,
                    loading: true,
                    load: function () {
                        ctrl.data.summary.failQCmanual.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/total-qc-manual-fail',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.summary.failQCmanual.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            ctrl.data.summary.failQCmanual.value = response.data.value;
                            ctrl.data.summary.failQCmanual.percent = response.data.percent;
                        });
                    },
                    view: function () {
                        var type = 2;
                        var url = ctrl.buildUrl(type);
                        url += '&evaluateType=1&evaluateResult=0'
                        $window.open(url);
                    }
                },
                bad: {
                    value: 0,
                    percent: 0,
                    loading: true,
                    load: function () {
                        ctrl.data.summary.bad.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/total-bad',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.summary.bad.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            ctrl.data.summary.bad.value = response.data.value;
                            ctrl.data.summary.bad.percent = response.data.percent;
                        });
                    },
                    view: function () {
                        var type = 2;
                        if (ctrl.queryable.dimension !== '5') {
                            type = 1;
                        }

                        var url = ctrl.buildUrl(type);
                        url += '&emotionAgent=UnHappy&foundAgentWordPatternError=1'
                        $window.open(url);
                    }
                }
            },
            charts: {
                callQC: {
                    loading: true,
                    config: {
                        labels: [
                            // "05/09", "06/09", "07/09", "08/09", "09/09", "10/09", "11/09"
                        ],
                        series: [
                            // 'Tổng', 'QC thủ công', 'QC tự động', 'Không QC'
                        ],
                        colors: ['#3c8dbc', '#f39c12', '#00a65a', '#c7251a'],
                        data: [
                            // [98, 103, 104, 120, 125, 119, 140],
                            // [10, 20, 13, 20, 16, 13, 18],
                            // [90, 93, 84, 90, 120, 106, 121],
                            // [8, 10, 20, 30, 5, 13, 19]
                        ],
                        datasetOverride: [
                            {
                                borderWidth: 1,
                                type: 'line',
                                fill: false
                            },
                            {
                                borderWidth: 1,
                                type: 'bar'
                            },
                            {
                                borderWidth: 1,
                                type: 'bar'
                            },
                            {
                                borderWidth: 1,
                                type: 'bar'
                            }
                        ],
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return numeral(value).format('0,0')
                                        }
                                    }
                                }]
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem, chart) {
                                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                                        return datasetLabel + ': ' + numeral(tooltipItem.yLabel).format('0,0');
                                    }
                                }
                            },
                            legend: {
                                display: true,
                                position: 'bottom',
                                fullWidth: true
                            },
                        },
                        onClick: function (points, event) {
                            // console.log(points, event);
                        }
                    },
                    load: function () {
                        ctrl.data.charts.callQC.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/call-qc',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.charts.callQC.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            var labels = [];
                            var series = new Set();
                            var data = [];

                            for (let i = 0; i < response.data.dates.length; i++) {
                                const element = response.data.dates[i];

                                labels.push(element.label);

                                if (element.datas) {
                                    for (let j = 0; j < element.datas.length; j++) {
                                        const item = element.datas[j];
                                        series.add(item.series);
                                    }
                                }
                            }

                            ctrl.data.charts.callQC.config.labels = labels;
                            ctrl.data.charts.callQC.config.series = [...series];

                            for (let i = 0; i < ctrl.data.charts.callQC.config.series.length; i++) {
                                const element = ctrl.data.charts.callQC.config.series[i];

                                data.push([]);
                            }

                            for (let i = 0; i < ctrl.data.charts.callQC.config.series.length; i++) {
                                const element = ctrl.data.charts.callQC.config.series[i];

                                for (let j = 0; j < response.data.dates.length; j++) {
                                    const item = response.data.dates[j];
                                    var value = item.datas.find(function (x, index) {
                                        return x.series === element;
                                    });

                                    if (value) {
                                        data[i].push(value.value);
                                    } else {
                                        data[i].push(0);
                                    }
                                }
                            }

                            ctrl.data.charts.callQC.config.data = data;
                        });
                    },
                    view: function () {
                        var type = 2;
                        if (ctrl.queryable.dimension !== '5') {
                            type = 1;
                        }

                        ctrl.view(type);
                    }
                },
                keyword: {
                    loading: true,
                    config: {
                        labels: [
                            // "05/09", "06/09", "07/09", "08/09", "09/09", "10/09", "11/09"
                        ],
                        series: [
                            // 'Không chuyên nghiệp', 'Không được phép'
                        ],
                        colors: ['#f39c12', '#c7251a'],
                        data: [
                            // [11, 5, 7, 4, 3, 0, 0],
                            // [5, 3, 4, 2, 1, 0, 0]
                        ],
                        datasetOverride: [
                            {
                                borderWidth: 1,
                                type: 'line',
                                fill: false
                            },
                            {
                                borderWidth: 1,
                                type: 'line',
                                fill: false
                            }
                        ],
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return numeral(value).format('0,0')
                                        }
                                    }
                                }]
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem, chart) {
                                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                                        return datasetLabel + ': ' + numeral(tooltipItem.yLabel).format('0,0');
                                    }
                                }
                            },
                            legend: {
                                display: true,
                                position: 'bottom',
                                fullWidth: true
                            }
                        },
                        onClick: function (points, event) {
                            // console.log(points, event);
                        }
                    },
                    load: function () {
                        ctrl.data.charts.keyword.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/keyword-agent',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.charts.keyword.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            var labels = [];
                            var series = new Set();
                            var data = [];

                            for (let i = 0; i < response.data.dates.length; i++) {
                                const element = response.data.dates[i];

                                labels.push(element.label);

                                if (element.datas) {
                                    for (let j = 0; j < element.datas.length; j++) {
                                        const item = element.datas[j];
                                        series.add(item.series);
                                    }
                                }
                            }

                            ctrl.data.charts.keyword.config.labels = labels;
                            ctrl.data.charts.keyword.config.series = [...series];

                            for (let i = 0; i < ctrl.data.charts.keyword.config.series.length; i++) {
                                const element = ctrl.data.charts.keyword.config.series[i];

                                data.push([]);
                            }

                            for (let i = 0; i < ctrl.data.charts.keyword.config.series.length; i++) {
                                const element = ctrl.data.charts.keyword.config.series[i];

                                for (let j = 0; j < response.data.dates.length; j++) {
                                    const item = response.data.dates[j];
                                    var value = item.datas.find(function (x, index) {
                                        return x.series === element;
                                    });

                                    if (value) {
                                        data[i].push(value.value);
                                    } else {
                                        data[i].push(0);
                                    }
                                }
                            }

                            ctrl.data.charts.keyword.config.data = data;
                        });
                    },
                    view: function () {
                        var type = 2;
                        if (ctrl.queryable.dimension !== '5') {
                            type = 1;
                        }

                        var url = ctrl.buildUrl(type);
                        url += '&foundAgentWordPatternError=1'
                        $window.open(url);
                    }
                },
                QC: {
                    loading: true,
                    config: {
                        labels: [
                            // "05/09", "06/09", "07/09", "08/09", "09/09", "10/09", "11/09"
                        ],
                        series: [
                            // 'Điểm QC trung bình'
                        ],
                        colors: [
                            '#3c8dbc', '#f39c12', '#00a65a', '#c7251a'
                        ],
                        data: [
                            // [40, 46, 57, 69, 73, 84, 92]
                        ],
                        datasetOverride: [
                            {
                                borderWidth: 1,
                                type: 'line'
                            }
                        ],
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return numeral(value).format('0,0')
                                        }
                                    }
                                }]
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem, chart) {
                                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                                        return datasetLabel + ': ' + numeral(tooltipItem.yLabel).format('0,0');
                                    }
                                }
                            },
                            legend: {
                                display: true,
                                position: 'bottom',
                                fullWidth: true
                            }
                        },
                        onClick: function (points, event) {
                            // console.log(points, event);
                        }
                    },
                    load: function () {
                        ctrl.data.charts.QC.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/qc',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.charts.QC.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            var labels = [];
                            var series = new Set();
                            var data = [];

                            for (let i = 0; i < response.data.dates.length; i++) {
                                const element = response.data.dates[i];

                                labels.push(element.label);

                                if (element.datas) {
                                    for (let j = 0; j < element.datas.length; j++) {
                                        const item = element.datas[j];
                                        series.add(item.series);
                                    }
                                }
                            }

                            ctrl.data.charts.QC.config.labels = labels;
                            ctrl.data.charts.QC.config.series = [...series];

                            for (let i = 0; i < ctrl.data.charts.QC.config.series.length; i++) {
                                const element = ctrl.data.charts.QC.config.series[i];

                                data.push([]);
                            }

                            for (let i = 0; i < ctrl.data.charts.QC.config.series.length; i++) {
                                const element = ctrl.data.charts.QC.config.series[i];

                                for (let j = 0; j < response.data.dates.length; j++) {
                                    const item = response.data.dates[j];
                                    var value = item.datas.find(function (x, index) {
                                        return x.series === element;
                                    });

                                    if (value) {
                                        data[i].push(value.value);
                                    } else {
                                        data[i].push(0);
                                    }
                                }
                            }

                            ctrl.data.charts.QC.config.data = data;
                        });
                    },
                    view: function () {
                        ctrl.view(2);
                    }
                },
                topIntentMiss: {
                    loading: true,
                    config: {
                        data: []
                    },
                    load: function () {
                        ctrl.data.charts.topIntentMiss.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/top-intent-miss',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.charts.topIntentMiss.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            ctrl.data.charts.topIntentMiss.config.data = response.data;
                            for (let i = 0; i < ctrl.data.charts.topIntentMiss.config.data.length; i++) {
                                const element = ctrl.data.charts.topIntentMiss.config.data[i];

                                element.percent = (element.value / ctrl.data.summary.total.value) * 100;
                            }
                        });
                    },
                    view: function (intent) {
                        var url = "";
                        if (intent) {
                            url = ctrl.buildUrl(1);
                            url += '&intentMisses=' + intent;
                        } else {
                            url = ctrl.buildUrl(2);
                        }

                        $window.open(url);
                    }
                },
                emotionCalling: {
                    loading: true,
                    config: {
                        labels: [
                            // "05/09", "06/09", "07/09", "08/09", "09/09", "10/09", "11/09"
                        ],
                        series: [
                            // 'Low', 'Medium', 'High', 'Special'
                        ],
                        colors: ['#3c8dbc', '#f39c12', '#00a65a', '#c7251a'],
                        data: [
                            // [65, 59, 80, 81, 56, 55, 40],
                            // [50, 90, 100, 110, 60, 56, 120],
                            // [50, 90, 100, 110, 60, 56, 120],
                            // [50, 90, 100, 110, 60, 56, 120]
                        ],
                        datasetOverride: [
                            {
                                borderWidth: 1
                            },
                            {
                                borderWidth: 1
                            },
                            {
                                borderWidth: 1
                            },
                            {
                                borderWidth: 1
                            }
                        ],
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                yAxes: [{
                                    stacked: true,
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return numeral(value).format('0,0')
                                        }
                                    }
                                }],
                                xAxes: [{
                                    stacked: true
                                }]
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem, chart) {
                                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                                        return datasetLabel + ': ' + numeral(tooltipItem.yLabel).format('0,0');
                                    }
                                }
                            },
                            legend: {
                                display: true,
                                position: 'bottom',
                                fullWidth: true
                            }
                        },
                        onClick: function (points, event) {
                            // console.log(points, event);
                        }
                    },
                    load: function () {
                        ctrl.data.charts.emotionCalling.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/emotion-calling',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.charts.emotionCalling.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            var labels = [];
                            var series = new Set();
                            var data = [];

                            for (let i = 0; i < response.data.dates.length; i++) {
                                const element = response.data.dates[i];

                                labels.push(element.label);

                                if (element.datas) {
                                    for (let j = 0; j < element.datas.length; j++) {
                                        const item = element.datas[j];
                                        series.add(item.series);
                                    }
                                }
                            }

                            ctrl.data.charts.emotionCalling.config.labels = labels;
                            ctrl.data.charts.emotionCalling.config.series = [...series];

                            for (let i = 0; i < ctrl.data.charts.emotionCalling.config.series.length; i++) {
                                const element = ctrl.data.charts.emotionCalling.config.series[i];

                                data.push([]);
                            }

                            for (let i = 0; i < ctrl.data.charts.emotionCalling.config.series.length; i++) {
                                const element = ctrl.data.charts.emotionCalling.config.series[i];

                                for (let j = 0; j < response.data.dates.length; j++) {
                                    const item = response.data.dates[j];
                                    var value = item.datas.find(function (x, index) {
                                        return x.series === element;
                                    });

                                    if (value) {
                                        data[i].push(value.value);
                                    } else {
                                        data[i].push(0);
                                    }
                                }
                            }

                            ctrl.data.charts.emotionCalling.config.data = data;
                        });
                    },
                    view: function () {
                        var type = 2;
                        if (ctrl.queryable.dimension !== '5') {
                            type = 1;
                        }

                        ctrl.view(type);
                    }
                },
                endCallStatus: {
                    loading: true,
                    config: {
                        labels: [
                            // "05/09", "06/09", "07/09", "08/09", "09/09", "10/09", "11/09"
                        ],
                        series: [
                            // 'Bad', 'Satisfy', 'Normal'
                        ],
                        colors: ['#3c8dbc', '#f39c12', '#00a65a'],
                        data: [
                            //     [65, 59, 80, 81, 56, 55, 40],
                            //     [50, 90, 100, 110, 60, 56, 120],
                            //     [50, 90, 100, 110, 60, 56, 120]
                        ],
                        datasetOverride: [
                            {
                                borderWidth: 1
                            },
                            {
                                borderWidth: 1
                            },
                            {
                                borderWidth: 1
                            },
                            {
                                borderWidth: 1
                            }
                        ],
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                yAxes: [{
                                    stacked: true,
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return numeral(value).format('0,0')
                                        }
                                    }
                                }],
                                xAxes: [{
                                    stacked: true
                                }]
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem, chart) {
                                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                                        return datasetLabel + ': ' + numeral(tooltipItem.yLabel).format('0,0');
                                    }
                                }
                            },
                            legend: {
                                display: true,
                                position: 'bottom',
                                fullWidth: true
                            }
                        },
                        onClick: function (points, event) {
                            // console.log(points, event);
                        }
                    },
                    load: function () {
                        ctrl.data.charts.endCallStatus.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/end-call-status',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.charts.endCallStatus.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            var labels = [];
                            var series = new Set();
                            var data = [];

                            for (let i = 0; i < response.data.dates.length; i++) {
                                const element = response.data.dates[i];

                                labels.push(element.label);

                                if (element.datas) {
                                    for (let j = 0; j < element.datas.length; j++) {
                                        const item = element.datas[j];
                                        series.add(item.series);
                                    }
                                }
                            }

                            ctrl.data.charts.endCallStatus.config.labels = labels;
                            ctrl.data.charts.endCallStatus.config.series = [...series];

                            for (let i = 0; i < ctrl.data.charts.endCallStatus.config.series.length; i++) {
                                const element = ctrl.data.charts.endCallStatus.config.series[i];

                                data.push([]);
                            }

                            for (let i = 0; i < ctrl.data.charts.endCallStatus.config.series.length; i++) {
                                const element = ctrl.data.charts.endCallStatus.config.series[i];

                                for (let j = 0; j < response.data.dates.length; j++) {
                                    const item = response.data.dates[j];
                                    var value = item.datas.find(function (x, index) {
                                        return x.series === element;
                                    });

                                    if (value) {
                                        data[i].push(value.value);
                                    } else {
                                        data[i].push(0);
                                    }
                                }
                            }

                            ctrl.data.charts.endCallStatus.config.data = data;
                        });
                    },
                    view: function () {
                        var type = 2;
                        if (ctrl.queryable.dimension !== '5') {
                            type = 1;
                        }

                        ctrl.view(type);
                    }
                },
                emotionAgent: {
                    loading: true,
                    config: {
                        labels: [
                            // "05/09", "06/09", "07/09", "08/09", "09/09", "10/09", "11/09"
                        ],
                        series: [
                            // 'Bad', 'Satisfy', 'Normal'
                        ],
                        colors: ['#3c8dbc', '#f39c12', '#00a65a', '#c7251a'],
                        data: [
                            // [65, 59, 80, 81, 56, 55, 40],
                            // [50, 90, 100, 110, 60, 56, 120],
                            // [50, 90, 100, 110, 60, 56, 120]
                        ],
                        datasetOverride: [
                            {
                                borderWidth: 1
                            },
                            {
                                borderWidth: 1
                            },
                            {
                                borderWidth: 1
                            },
                            {
                                borderWidth: 1
                            }
                        ],
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                yAxes: [{
                                    stacked: true,
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return numeral(value).format('0,0')
                                        }
                                    }
                                }],
                                xAxes: [{
                                    stacked: true
                                }]
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem, chart) {
                                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                                        return datasetLabel + ': ' + numeral(tooltipItem.yLabel).format('0,0');
                                    }
                                }
                            },
                            legend: {
                                display: true,
                                position: 'bottom',
                                fullWidth: true
                            }
                        },
                        onClick: function (points, event) {
                            // console.log(points, event);
                        }
                    },
                    load: function () {
                        ctrl.data.charts.emotionAgent.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/emotion-agent',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.charts.emotionAgent.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            var labels = [];
                            var series = new Set();
                            var data = [];

                            for (let i = 0; i < response.data.dates.length; i++) {
                                const element = response.data.dates[i];

                                labels.push(element.label);

                                if (element.datas) {
                                    for (let j = 0; j < element.datas.length; j++) {
                                        const item = element.datas[j];
                                        series.add(item.series);
                                    }
                                }
                            }

                            ctrl.data.charts.emotionAgent.config.labels = labels;
                            ctrl.data.charts.emotionAgent.config.series = [...series];

                            for (let i = 0; i < ctrl.data.charts.emotionAgent.config.series.length; i++) {
                                const element = ctrl.data.charts.emotionAgent.config.series[i];

                                data.push([]);
                            }

                            for (let i = 0; i < ctrl.data.charts.emotionAgent.config.series.length; i++) {
                                const element = ctrl.data.charts.emotionAgent.config.series[i];

                                for (let j = 0; j < response.data.dates.length; j++) {
                                    const item = response.data.dates[j];
                                    var value = item.datas.find(function (x, index) {
                                        return x.series === element;
                                    });

                                    if (value) {
                                        data[i].push(value.value);
                                    } else {
                                        data[i].push(0);
                                    }
                                }
                            }

                            ctrl.data.charts.emotionAgent.config.data = data;
                        });
                    },
                    view: function () {
                        var type = 2;
                        if (ctrl.queryable.dimension !== '5') {
                            type = 1;
                        }

                        ctrl.view(type);
                    }
                },
                failQCauto: {
                    loading: true,
                    config: {
                        labels: [
                            // "05/09", "06/09", "07/09", "08/09", "09/09", "10/09", "11/09"
                        ],
                        series: [
                            // 'Tổng', 'PROMISE B01 - GB', 'REFUSE - GB', 'Skip - không thành công'
                        ],
                        colors: ['#c7251a', '#3c8dbc', '#f39c12', '#00a65a'],
                        data: [
                            // [108, 123, 117, 140, 141, 132, 158],
                            // [10, 20, 13, 20, 16, 13, 18],
                            // [90, 93, 84, 90, 120, 106, 121],
                            // [8, 10, 20, 30, 5, 13, 19]
                        ],
                        datasetOverride: [
                            {
                                borderWidth: 1,
                                type: 'line',
                                fill: false
                            },
                            {
                                borderWidth: 1,
                                type: 'bar'
                            },
                            {
                                borderWidth: 1,
                                type: 'bar'
                            },
                            {
                                borderWidth: 1,
                                type: 'bar'
                            }
                        ],
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                yAxes: [{
                                    stacked: true,
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return numeral(value).format('0,0')
                                        }
                                    }
                                }],
                                xAxes: [{
                                    stacked: true
                                }]
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem, chart) {
                                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                                        return datasetLabel + ': ' + numeral(tooltipItem.yLabel).format('0,0');
                                    }
                                }
                            },
                            legend: {
                                display: true,
                                position: 'bottom',
                                fullWidth: true
                            },
                        },
                        onClick: function (points, event) {
                            // console.log(points, event);
                        }
                    },
                    load: function () {
                        ctrl.data.charts.failQCauto.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/qc-auto-fail',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.charts.failQCauto.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            var labels = [];
                            var series = new Set();
                            var data = [];

                            for (let i = 0; i < response.data.dates.length; i++) {
                                const element = response.data.dates[i];

                                labels.push(element.label);

                                if (element.datas) {
                                    series.add('Tổng');
                                    for (let j = 0; j < element.datas.length; j++) {
                                        const item = element.datas[j];
                                        series.add(item.series);
                                    }
                                }
                            }

                            ctrl.data.charts.failQCauto.config.labels = labels;
                            ctrl.data.charts.failQCauto.config.series = [...series];

                            for (let i = 0; i < ctrl.data.charts.failQCauto.config.series.length; i++) {
                                const element = ctrl.data.charts.failQCauto.config.series[i];

                                data.push([]);
                            }

                            for (let i = 0; i < ctrl.data.charts.failQCauto.config.series.length; i++) {
                                const element = ctrl.data.charts.failQCauto.config.series[i];

                                for (let j = 0; j < response.data.dates.length; j++) {
                                    const item = response.data.dates[j];
                                    var value = item.datas.find(function (x, index) {
                                        return x.series === element;
                                    });

                                    if (value) {
                                        data[i].push(value.value);
                                    } else {
                                        data[i].push(0);
                                    }
                                }
                            }

                            ctrl.data.charts.failQCauto.config.data = data;
                        });
                    },
                    view: function () {
                        var url = ctrl.buildUrl(2);
                        url += '&evaluateType=2&evaluateResult=0'
                        $window.open(url);
                    }
                },
                failQCmanual: {
                    loading: true,
                    config: {
                        labels: [
                            // "05/09", "06/09", "07/09", "08/09", "09/09", "10/09", "11/09"
                        ],
                        series: [
                            // 'Tổng', 'PROMISE B01 - GB', 'REFUSE - GB', 'Skip - không thành công', 'REFUSE - BB', 'THIRD PARTY - BB'
                        ],
                        colors: ['#c7251a', '#3c8dbc', '#f39c12', '#00a65a'],
                        data: [
                            // [68, 93, 107, 80, 41, 72, 58],
                            // [10, 20, 13, 20, 16, 13, 18],
                            // [50, 63, 74, 30, 20, 46, 21],
                            // [10, 20, 13, 20, 16, 13, 18],
                            // [50, 63, 74, 30, 20, 46, 21],
                            // [8, 10, 20, 30, 5, 13, 19]
                        ],
                        datasetOverride: [
                            {
                                borderWidth: 1,
                                type: 'line',
                                fill: false
                            },
                            {
                                borderWidth: 1,
                                type: 'bar'
                            },
                            {
                                borderWidth: 1,
                                type: 'bar'
                            },
                            {
                                borderWidth: 1,
                                type: 'bar'
                            }
                        ],
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                yAxes: [{
                                    stacked: true,
                                    ticks: {
                                        beginAtZero: true,
                                        callback: function (value) {
                                            return numeral(value).format('0,0')
                                        }
                                    }
                                }],
                                xAxes: [{
                                    stacked: true
                                }]
                            },
                            tooltips: {
                                callbacks: {
                                    label: function (tooltipItem, chart) {
                                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                                        return datasetLabel + ': ' + numeral(tooltipItem.yLabel).format('0,0');
                                    }
                                }
                            },
                            legend: {
                                display: true,
                                position: 'bottom',
                                fullWidth: true
                            },
                        },
                        onClick: function (points, event) {
                            // console.log(points, event);
                        }
                    },
                    load: function () {
                        ctrl.data.charts.failQCmanual.loading = true;
                        httpQueueService({
                            method: 'GET',
                            url: '/summary/qc-manual-fail',
                            params: ctrl.queryable
                        }).then(function (response) {
                            ctrl.data.charts.failQCmanual.loading = false;
                            if (response.status !== 200 || response.data === null || response.data.dates === null) {
                                return;
                            }

                            var labels = [];
                            var series = new Set();
                            var data = [];

                            for (let i = 0; i < response.data.dates.length; i++) {
                                const element = response.data.dates[i];

                                labels.push(element.label);

                                if (element.datas) {
                                    series.add('Tổng');
                                    for (let j = 0; j < element.datas.length; j++) {
                                        const item = element.datas[j];
                                        series.add(item.series);
                                    }
                                }
                            }

                            ctrl.data.charts.failQCmanual.config.labels = labels;
                            ctrl.data.charts.failQCmanual.config.series = [...series];

                            for (let i = 0; i < ctrl.data.charts.failQCmanual.config.series.length; i++) {
                                const element = ctrl.data.charts.failQCmanual.config.series[i];

                                data.push([]);
                            }

                            for (let i = 0; i < ctrl.data.charts.failQCmanual.config.series.length; i++) {
                                const element = ctrl.data.charts.failQCmanual.config.series[i];

                                for (let j = 0; j < response.data.dates.length; j++) {
                                    const item = response.data.dates[j];
                                    var value = item.datas.find(function (x, index) {
                                        return x.series === element;
                                    });

                                    if (value) {
                                        data[i].push(value.value);
                                    } else {
                                        data[i].push(0);
                                    }
                                }
                            }

                            ctrl.data.charts.failQCmanual.config.data = data;
                        });
                    },
                    view: function () {
                        var url = ctrl.buildUrl(2);
                        url += '&evaluateType=1&evaluateResult=0'
                        $window.open(url);
                    }
                }
            },
        };

        ctrl.$onInit = function () {
            // ctrl.fetchData();
        };

        ctrl.getMeasures = function () {
            if (ctrl.queryable.dimension) {
                ctrl.dimensions.paging.dimension = ctrl.queryable.dimension;

                httpService.get('/statistic/measures', {
                    params: ctrl.dimensions.paging
                }).then(function (response) {
                    if (response.status !== 200 || response.data === null || response.data.dates === null) {
                        return;
                    }

                    $timeout(function () {
                        ctrl.measures = angular.copy(response.data.data);

                        for (var i = 0; i < ctrl.measures.length; i++) {
                            if (ctrl.measures[i].name !== ctrl.measures[i].code) {
                                if (ctrl.measures[i].name === undefined || ctrl.measures[i].name === null) {
                                    ctrl.measures[i].name = ctrl.measures[i].code;
                                } else {
                                    ctrl.measures[i].name = ctrl.measures[i].code + " - " + ctrl.measures[i].name;
                                }
                            }
                        }

                        ctrl.selectizeMeasure.setValue('');
                        ctrl.selectizeMeasure.refreshOptions(false);
                    });
                });
            }
            else {
                ctrl.measures = [];
                ctrl.selectizeMeasure.setValue('');
                ctrl.selectizeMeasure.refreshOptions(false);
            }
        };

        ctrl.getDimension = function (key) {
            if (!key) {
                return null;
            }

            return ctrl.dimensions.data.find(function (item, index) {
                return item.key === parseInt(key);
            });
        };

        ctrl.fetchData = function () {
            ctrl.data.summary.total.load();
            ctrl.data.summary.hasQC.load();
            ctrl.data.summary.notQC.load();
            ctrl.data.summary.failQCauto.load();
            ctrl.data.summary.failQCmanual.load();
            ctrl.data.summary.bad.load();

            ctrl.data.charts.callQC.load();
            ctrl.data.charts.keyword.load();
            ctrl.data.charts.QC.load();
            ctrl.data.charts.topIntentMiss.load();
            ctrl.data.charts.emotionCalling.load();
            ctrl.data.charts.endCallStatus.load();
            ctrl.data.charts.emotionAgent.load();
            ctrl.data.charts.failQCauto.load();
            ctrl.data.charts.failQCmanual.load();
        };

        ctrl.view = function (type) {
            var url = ctrl.buildUrl(type);
            $window.open(url);
        };

        ctrl.buildQueryString = function () {
            var query = {
                limit: ctrl.queryable.limit,
                from: ctrl.queryable.from,
                to: ctrl.queryable.to
            };

            if (ctrl.queryable.dimension && ctrl.queryable.measure) {
                if (ctrl.queryable.dimension === '1') {
                    query.agentId = ctrl.queryable.measure;
                }

                if (ctrl.queryable.dimension === '5') {
                    var measure = ctrl.measures.find(function (x) { return x.key === ctrl.queryable.measure; });
                    if (measure) {
                        query.scriptCode = measure.code;
                    }
                }
            }

            var queryString = new URLSearchParams(query).toString();
            return queryString;
        };

        ctrl.buildUrl = function (type) {
            var queryString = ctrl.buildQueryString();

            var url = '';
            if (type === 1) {
                url += '/#!/calls?' + queryString;
            } else {
                url += '/#!/calls-qc?' + queryString;
            }
            return url;
        };
    };

    angular
        .module('admin')
        .controller('dashboardController', dashboardController);

    dashboardController.$inject = ['$state', '$stateParams', '$timeout', '$q', 'httpService', 'sweetAlert', '$window', 'httpQueueService'];
}());