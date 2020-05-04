(function () {
    'use strict';

    var userReadController = function ($state, sweetAlert, permissionService, userService) {
        var ctrl = this;
        ctrl.permissionService = permissionService;
        ctrl.validationOptions = {
            rules: {
                value: {
                    required: true
                }
            }
        };
        ctrl.users = [];
        ctrl.paging = {
            page: 1,
            size: 10,
            q: null
        };

        ctrl.emails = null;

        ctrl.$onInit = function () {
            ctrl.getUsers();
        };

        ctrl.getUsers = function () {
            userService.get(ctrl.paging).then(function (response) {
                if (response.status === 200) {
                    ctrl.users = response.data.data;
                    ctrl.totalItems = response.data.totalItems;
                }
            });
        };

        ctrl.delete = function (id) {
            sweetAlert.confirm(function () {
                return userService.delete(id);
            }, function (result) {
                if (result.value) {
                    if (result.value.status === 200) {
                        sweetAlert.success('Thành công', 'Bạn đã xóa TÀI KHOẢN thành công!');
                        ctrl.getUsers();
                    } else {
                        sweetAlert.error("Lỗi", result.value.data);
                    }
                }
            });
        };

        ctrl.isLockedout = function (lockoutEnd) {
            if (lockoutEnd === undefined || lockoutEnd === null) {
                return false;
            }

            var now = new Date();
            var end = new Date(lockoutEnd);

            if (now > end) {
                return false;
            } else {
                return true;
            }
        };

        ctrl.lock = function (id) {
            userService.lock(id).then(function (response) {
                if (response.status === 200) {
                    sweetAlert.success('Thành công', 'Bạn đã KHÓA TÀI KHOẢN thành công!');
                    ctrl.getUsers();
                }
            });
        };

        ctrl.unlock = function (id) {
            userService.unlock(id).then(function (response) {
                if (response.status === 200) {
                    sweetAlert.success('Thành công', 'Bạn đã MỞ KHÓA TÀI KHOẢN thành công!');
                    ctrl.getUsers();
                }
            });
        };

        ctrl.sendResetPassword = function (form) {
            if (!form.validate()) {
                return;
            }

            var email = { emails: ctrl.emails };

            userService.resetPassword(ctrl.idSelect, email).then(function (response) {
                if (response.status === 200) {
                    $('#modal-reset-password').modal('hide');
                    sweetAlert.success('Thành công', 'Email đổi mật khẩu đã được xếp vào hàng đợi để gửi. Vui lòng kiểm tra email để lấy mật khẩu mới cho tài khoản!');
                    ctrl.getUsers();
                }
            });
        };

        ctrl.openResetPassword = function (user) {
            $('[name=frmResetPassword]').validate().resetForm();
            ctrl.emails = user.email ? angular.copy(user.email) : "";
            $('#modal-reset-password').modal('show');
            ctrl.idSelect = user.id;
        };

        angular.element('#modal-reset-password').on('shown.bs.modal', function () {
            angular.element('[name=emails]').focus();
        })
    };

    angular
        .module('identity')
        .controller('userReadController', userReadController);

    userReadController.$inject = ['$state', 'sweetAlert', 'permissionService', 'userService'];
}());