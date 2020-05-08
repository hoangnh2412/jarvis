(function () {
    'use strict';

    var settingsController = function ($state, settingService, httpService, cacheService, sweetAlert, permissionService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.template = 'group-setting.tpl';
        ctrl.permissionService = permissionService;

        ctrl.validationOptions = {
            rules: {
                value: {
                    required: true
                }
            }
        };
        ctrl.groupSettings = [];

        ctrl.getSettings = function () {
            settingService.get().then(function (response) {
                if (response.status === 200) {
                    ctrl.groupSettings = response.data;
                }
            });
        };

        ctrl.save = function (form, code) {
            if (!form.validate()) {
                return;
            }

            var group = ctrl.groupSettings.find(function (x) { return x.key === code; });
            if (!group) {
                return;
            }

            var settings = [];
            for (var i = 0; i < group.settings.length; i++) {
                settings.push({
                    key: group.settings[i].key,
                    value: group.settings[i].value
                });
            }
            settingService.post(group.key, settings).then(function (response) {
                // gọi api lấy config rồi lưu lại vào cache einvoice.config để ko phải out ra xóa cache mỗi lần lưu cài đặt
                if (response.status === 200) {
                    sweetAlert.swal({
                        title: 'Thành công',
                        text: 'Bạn đã sửa cài đặt ' + group.name.toUpperCase() + ' thành công!',
                        type: 'success',
                        timer: 2000
                    });
                    // ctrl.getConfig();
                }
            });
        };

        ctrl.getConfig = function () {
            httpService.get('/config').then(function (response) {
                if (response.status === 200)
                    cacheService.set('config', response.data);
            })
        };

        ctrl.$onInit = function () {
            ctrl.getSettings();
        };
    };

    angular
        .module('core')
        .controller('settingsController', settingsController);

    settingsController.$inject = ['$state', 'settingService', 'httpService', 'cacheService', 'sweetAlert', 'permissionService'];
}());