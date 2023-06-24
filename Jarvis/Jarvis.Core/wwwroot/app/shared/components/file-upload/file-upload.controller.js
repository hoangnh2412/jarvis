(function () {
    'use strict';

    function fileUploadController($state, $stateParams, $timeout, $q, $uibModalInstance, httpService, sweetAlert, FileUploader, config) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.types = config.types;
        ctrl.success = [];
        ctrl.uploader = new FileUploader(config);

        ctrl.$onInit = function () {

        };

        ctrl.close = function () {
            // $uibModalInstance.dismiss('cancel');
            $uibModalInstance.close(ctrl.success);
        };

        ctrl.uploader.onErrorItem = function (fileItem, response, status, headers) {
            // console.info('onErrorItem', fileItem, response, status, headers);
            var file = ctrl.uploader.queue.find(function (x) { return x.$$hashKey === fileItem.$$hashKey; });
            file.message = status + ' - ' + response;
        };

        ctrl.uploader.onCompleteItem = function (fileItem, response, status, headers) {
            ctrl.success.push(response);
            // console.info('onCompleteItem', fileItem, response, status, headers);
        };

        ctrl.showReason = function (item) {
            sweetAlert.error('Lỗi', item.message);
        };
    };

    angular
        .module('jarvis')
        .controller('fileUploadController', fileUploadController);

    fileUploadController.$inject = ['$state', '$stateParams', '$timeout', '$q', '$uibModalInstance', 'httpService', 'sweetAlert', 'FileUploader', 'config'];
}());