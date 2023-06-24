(function () {
    'use strict';

    var baseListController = function ($state, sweetAlert, baseService) {
        var ctrl = this;
        ctrl.loading = false;

        ctrl.page = {
            header: {
                title: 'Danh sách'
            },
            body: {
                table: {
                    style: {
                        isHover: true,
                        color: 'info'
                    },
                    columns: [
                        {
                            title: 'Mã',
                            width: '',
                            style: {
                                'width': '250px'
                            },
                            content: {
                                type: 'text',
                                dataField: 'code'
                            }
                        },
                        {
                            title: 'Tên',
                            width: '',
                            content: {
                                type: 'text',
                                dataField: 'code'
                            }
                        },
                        {
                            title: 'Trạng thái',
                            style: {
                                'width': '250px'
                            },
                            content: {
                                type: 'status',
                                dataField: 'code'
                            }
                        },
                        {
                            title: '#',
                            style: {
                                'width': '100px'
                            },
                            content: {
                                type: 'actions'
                            }
                        }
                    ],
                    actions: {
                        create: {
                            enable: true,
                            text: 'Tạo mới',
                            style: {
                                size: '',
                                color: 'bg-green',
                                icon: 'fa fa-plus'
                            }
                        },
                        update: {
                            enable: true,
                            text: 'Sửa',
                            style: {
                                size: 'xs',
                                color: 'bg-',
                                icon: 'fa fa-edit'
                            },
                            onShow: function () {

                            },
                            onClick: function () {
                            }
                        },
                        delete: {
                            enable: true,
                            text: 'Xóa',
                            style: {
                                size: 'xs',
                                color: 'bg-',
                                icon: 'fa fa-remove'
                            },
                            onShow: function () {

                            },
                            onClick: function () {
                            }
                        },
                        import: {
                            enable: true,
                            text: 'Import',
                            style: {
                                size: 'xs',
                                color: 'bg-',
                                icon: 'fa fa-upload'
                            },
                            onShow: function () {

                            },
                            onClick: function () {
                            }
                        },
                        export: {
                            enable: true,
                            text: 'Export',
                            style: {
                                size: 'xs',
                                color: 'bg-',
                                icon: 'fa fa-download'
                            },
                            onShow: function () {

                            },
                            onClick: function () {
                            }
                        },
                    }
                }
            },
            components: [
                {
                    width: 'col-md-6',
                    alight: 'text-left',
                    widgets: [
                        {
                            type: 'text',
                            size: 'h1',
                            content: ''
                        }
                    ]
                },
                {
                    width: 'col-md-6',
                    alight: 'text-right',
                    widgets: [
                        {
                            type: 'search',
                        },
                        {
                            type: 'button'
                        }
                    ]
                }
            ]
        };

        ctrl.onPagination = function () {

        };

        ctrl.onCreate = function () {
            console.log('base');
        };

        ctrl.onDelete = function () {

        };

        ctrl.items = [];
        ctrl.paging = {
            page: 1,
            size: 10,
            q: null
        };

        ctrl.pagination = function () {
            baseService.pagination(ctrl.paging).then(function (response) {
                if (response.status === 200) {
                    ctrl.items = response.data.data;
                    ctrl.totalItems = response.data.totalItems;
                }
            });
        };

        ctrl.delete = function (code) {
            sweetAlert.confirm(
                "Xác nhận",
                "Bạn chắc chắn muốn xóa?",
                function () {
                    return baseService.delete(code);
                }, function (result) {
                    if (result.value) {
                        ctrl.pagination();
                        sweetAlert.success('Thành công', 'Xóa thành công!');
                    }
                }
            );
        };

        ctrl.$onInit = function () {
            ctrl.pagination();
        };
    };

    angular
        .module('core')
        .controller('baseListController', baseListController);

    baseListController.$inject = ['$state', 'sweetAlert', 'baseService'];
}());