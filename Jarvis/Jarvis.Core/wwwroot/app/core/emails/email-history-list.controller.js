(function () {
    'use strict';

    var emailHistoryListController = function ($state, sweetAlert, emailHistoryService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.items = [];
        ctrl.paging = {
            totalItems: 0,
            page: 1,
            size: 10,
            q: null
        };
        ctrl.types = [
            {
                key: 0,
                value: 'Chung'
            }
        ];

        ctrl.getTypeName = function (key) {
            var type = ctrl.types.find(function (x) { return x.key === key; });
            if (type) {
                return type.value;
            }
            return null;
        };

        ctrl.getItems = function () {
            emailHistoryService.pagination(ctrl.paging).then(function (response) {
                if (response.status === 200) {
                    ctrl.items = response.data.data;
                    ctrl.paging.totalItems = response.data.totalItems;
                }
            });
        };

        ctrl.delete = function (item) {
            sweetAlert.confirm(function () {
                return emailHistoryService.delete(item.key);
            }, function (result) {
                if (result.value) {
                    ctrl.getItems();
                    sweetAlert.success('Đã xóa', 'Bạn đã xóa mẫu email thành công!');
                }
            });
        };

        ctrl.$onInit = function () {
            ctrl.getItems();
        };
    };

    angular
        .module('core')
        .controller('emailHistoryListController', emailHistoryListController);

    emailHistoryListController.$inject = ['$state', 'sweetAlert', 'emailHistoryService'];
}());