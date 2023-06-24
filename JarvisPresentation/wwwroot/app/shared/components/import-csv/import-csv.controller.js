(function () {
    'use strict';

    function importCsvCtrl($state, httpService, sweetAlert, $q, $uibModalInstance, option) {
        var ctrl = this;
        ctrl.files = [];
        ctrl.option = option;


        ctrl.$onInit = function () {
        };

        ctrl.ok = function () {
            if (ctrl.files.length === 0) {
                sweetAlert.error('Lỗi', 'Chưa chọn file import');
                return;
            }

            var fd = new FormData();
            for (let i = 0; i < ctrl.files.length; i++) {
                fd.append('file', ctrl.files[i]);
            }

            httpService.post(ctrl.option.apiEndpoint, fd, {
                transformRequest: angular.identity,
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            }).then(function (response) {
                if (response.status !== 200) {
                    sweetAlert.error('Lỗi', response.data);
                    return;
                }

                sweetAlert.success('Thành công', 'Import dữ liệu thành công!');
                $uibModalInstance.close();
            });
        };

        ctrl.cancel = function () {
            // $uibModalInstance.dismiss('cancel');
            $uibModalInstance.close();
        };
    };

    angular
        .module('jarvis')
        .controller('importCsvCtrl', importCsvCtrl);

    importCsvCtrl.$inject = ['$state', 'httpService', 'sweetAlert', '$q', '$uibModalInstance', 'option'];
}());