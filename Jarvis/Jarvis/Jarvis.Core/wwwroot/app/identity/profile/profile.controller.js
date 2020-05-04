(function () {
    'use strict';

    function profileController($state, $q, sweetAlert, cacheService, profileService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.user = {};
        ctrl.validationOptions = {
            rules: {
                fullName: {
                    required: true
                },
                email: {
                    multipleEmails: true
                }
            }
        };
        ctrl.myCroppedImage = null;

        ctrl.$onInit = function () {
            ctrl.getProfile();
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            profileService.postProfile(ctrl.user).then(function (response) {
                if (response.status === 200) {
                    sweetAlert.success('Thành công', 'Bạn đã cập nhật thông tin thành công');

                    // cập nhật lại cache sau khi thông tin được sửa
                    ctrl.context.userInfo.fullName = angular.copy(ctrl.user.fullName);
                    ctrl.context.email = angular.copy(ctrl.user.email);
                    ctrl.context.phoneNumber = angular.copy(ctrl.user.phoneNumber);
                    if (ctrl.user.avatarPath)
                        ctrl.context.userInfo.avatarPath = angular.copy(ctrl.user.avatarPath);
                    cacheService.set('context', ctrl.context);
                }
            });
        };

        ctrl.getProfile = function () {
            profileService.getProfile().then(function (response) {
                if (response.status === 200) {
                    ctrl.user = response.data;
                }
            });
        };

        ctrl.delete = function (id) {
            sweetAlert.swal({
                title: 'Xác nhận',
                text: 'Bạn có chắc muốn xóa TÀI KHOẢN của mình? Tất cả dữ liệu bạn tạo ra sẽ bị xóa và không thể khôi phục',
                type: 'question',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Có',
                cancelButtonText: 'Không',
                showLoaderOnConfirm: true,
                allowOutsideClick: false,
                preConfirm: function () {
                    return profileService.deleteProfile(id);
                }
            }, function (result) {
                if (result.value) {
                    if (result.value.status === 200) {
                        sweetAlert.success('Đã xóa', 'Bạn đã xóa TÀI KHOẢN thành công!');
                        cacheService.clean();
                        $state.go('identity.frontend.login');
                    } else {
                        sweetAlert.error("Lỗi", result.value.data);
                    }
                }
            });
        };

        ctrl.changeAvatar = function () {
            if (ctrl.avatarPath) {
                $('#modal-avatar').modal('show');
                ctrl.typeImage = 'crop';
            }
        };

        ctrl.selectAvatar = function () {
            if (ctrl.typeImage === 'crop') {
                ctrl.user.avatarPath = angular.copy(ctrl.myCroppedImage);
            } else if (ctrl.typeImage === 'unCrop') {
                ctrl.user.avatarPath = angular.copy(ctrl.avatarPath);
            }
            $('#modal-avatar').modal('hide');
            ctrl.avatarPath = '';
            ctrl.myCroppedImage = '';
            angular.element('#fileAvatar').val('');
        };

        ctrl.cancelAvatar = function () {
            ctrl.avatarPath = '';
            ctrl.myCroppedImage = '';
            ctrl.user.avatarPath = '';
            angular.element('#fileAvatar').val('');
        };

        ctrl.showAvatar = function () {
            $('#modal-show-avatar').modal('show');
        };
    };

    angular
        .module('identity')
        .controller('profileController', profileController);

    profileController.$inject = ['$state', '$q', 'sweetAlert', 'cacheService', 'profileService'];
}());