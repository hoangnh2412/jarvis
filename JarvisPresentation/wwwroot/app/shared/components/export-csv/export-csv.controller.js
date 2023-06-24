(function () {
    'use strict';

    function exportCsvCtrl($state, httpService, sweetAlert, $q, $uibModalInstance, FileSaver, data) {
        var ctrl = this;
        ctrl.data = data;

        ctrl.$onInit = function () {

        };

        ctrl.ok = function () {
            var promise;
            if (data.isAll) {
                promise = httpService.get(ctrl.data.url, {
                    params: ctrl.data.data,
                    headers: {
                        email: ctrl.email
                    },
                    responseType: 'arraybuffer'
                });
            } else {
                promise = httpService.post(ctrl.data.url, {
                    idCalls: ctrl.data.data
                }, {
                    headers: {
                        email: ctrl.email
                    },
                    responseType: 'arraybuffer'
                });
            }

            promise.then(function (response) {
                if (response.status !== 200)
                    return;

                // Nếu điền email => Thông báo thành công
                if (ctrl.email) {
                    sweetAlert.success('Thành công', 'Bạn đã export cuộc gọi thành công!');
                }
                // Nếu ko điềm email => Tải file
                else {
                    var contentDispositionHeader = response.headers('Content-Disposition');
                    var name = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');

                    var contentType = response.headers('Content-Type');

                    var blob = new Blob([response.data], { type: contentType + ';charset=utf-8' });
                    FileSaver.saveAs(blob, name);
                }
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
        .controller('exportCsvCtrl', exportCsvCtrl);

    exportCsvCtrl.$inject = ['$state', 'httpService', 'sweetAlert', '$q', '$uibModalInstance', 'FileSaver', 'data'];
}());