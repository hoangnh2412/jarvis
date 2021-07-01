(function () {
    'use strict';

    var labelReadController = function ($state, sweetAlert, labelService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.labels = [];
        ctrl.paging = {
            page: 1,
            size: 10,
            q: null
        };

        ctrl.getLabels = function () {
            labelService.get(ctrl.paging).then(function (response) {
                if (response.status === 200) {
                    ctrl.labels = response.data.data.data;
                    ctrl.totalItems = response.data.data.totalItems;
                }
            });
        };

        ctrl.delete = function (code) {
            sweetAlert.confirm(function () {
                return labelService.delete(code);
            }, function (result) {
                if (result.value) {
                    ctrl.getLabels();
                    sweetAlert.success('Đã xóa', 'Bạn đã xóa Nhãn thành công!');
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.getLabels();
        };
    };

    angular
        .module('core')
        .controller('labelReadController', labelReadController);

    labelReadController.$inject = ['$state', 'sweetAlert', 'labelService'];
}());