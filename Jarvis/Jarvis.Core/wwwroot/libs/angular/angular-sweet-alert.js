//https://github.com/oitozero/ngSweetAlert

(function (root, factory) {
    "use strict";

    /*global define*/
    if (typeof define === 'function' && define.amd) {
        define(['angular', 'sweetalert'], factory);  // AMD
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory(require('angular'), require('sweetalert')); // Node
    } else {
        factory(root.angular, root.swal);					// Browser
    }

}(this, function (angular, swal) {
    "use strict";

    angular.module('ngSweetAlert', [])
        .factory('sweetAlert', ['$rootScope', 'SweetAlertConfig', function ($rootScope, SweetAlertConfig) {
            //public methods
            var self = {
                swal: function (arg1, arg2) {

                    //merge with default config
                    var arg1 = angular.extend(SweetAlertConfig, arg1);

                    $rootScope.$evalAsync(function () {
                        if (typeof (arg2) === 'function') {
                            Swal.fire(arg1).then(function (isConfirm) {
                                $rootScope.$evalAsync(function () {
                                    arg2(isConfirm);
                                });
                            });
                        } else {
                            Swal.fire(arg1, arg2);
                        }
                    });
                },
                success: function (title, message) {
                    $rootScope.$evalAsync(function () {
                        Swal.fire(title, message, 'success');
                    });
                },
                error: function (title, message) {
                    $rootScope.$evalAsync(function () {
                        Swal.fire(title, message, 'error');
                    });
                },
                warning: function (title, message) {
                    $rootScope.$evalAsync(function () {
                        Swal.fire(title, message, 'warning');
                    });
                },
                info: function (title, message) {
                    $rootScope.$evalAsync(function () {
                        Swal.fire(title, message, 'info');
                    });
                },
                confirm: function (preConfirm, callback) {
                    $rootScope.$evalAsync(function () {
                        Swal.fire({
                            title: 'Xác nhận',
                            text: 'Bạn có chắc muốn xóa?',
                            type: 'question',
                            showCancelButton: true,
                            confirmButtonColor: '#3085d6',
                            cancelButtonColor: '#d33',
                            confirmButtonText: 'Có',
                            cancelButtonText: 'Không',
                            showLoaderOnConfirm: true,
                            allowOutsideClick: false,
                            preConfirm: preConfirm
                        }).then(callback);
                    });
                },
                mConfirm: function (title, message, preConfirm, callback) {
                    $rootScope.$evalAsync(function () {
                        Swal.fire({
                            title: title,
                            text: message,
                            type: 'question',
                            showCancelButton: true,
                            confirmButtonColor: '#3085d6',
                            cancelButtonColor: '#d33',
                            confirmButtonText: 'Có',
                            cancelButtonText: 'Không',
                            showLoaderOnConfirm: true,
                            allowOutsideClick: false,
                            preConfirm: preConfirm
                        }).then(callback);
                    });
                },
                pConfirm: function (text, preConfirm) {
                    return Swal.fire({
                        title: 'Xác nhận',
                        text: text,
                        type: 'question',
                        showCancelButton: true,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33',
                        confirmButtonText: 'Có',
                        cancelButtonText: 'Không',
                        showLoaderOnConfirm: true,
                        allowOutsideClick: false,
                        preConfirm: preConfirm
                    });
                }
            };

            return self;
        }]).constant('SweetAlertConfig', {
            title: '',
            text: '',
            type: null,
            allowOutsideClick: false,
            showConfirmButton: true,
            showCancelButton: false,
            confirmButtonText: 'OK',
            confirmButtonColor: '#8CD4F5',
            cancelButtonText: 'Cancel',
            imageUrl: null,
            timer: null,
            customClass: '',
            html: false,
            animation: true,
            allowEscapeKey: true,
            inputPlaceholder: '',
            inputValue: '',
            showLoaderOnConfirm: false
        });
}));
