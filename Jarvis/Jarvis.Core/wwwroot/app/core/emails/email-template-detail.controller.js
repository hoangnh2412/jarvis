(function () {
    'use strict';

    var emailTemplateDetailController = function ($state, $stateParams, sweetAlert, emailTemplateService) {
        var ctrl = this;
        ctrl.loading = false;
        ctrl.emailTemplate = {
            code: null,
            name: null,
            type: 0,
            subject: null,
            content: null,
            to: null,
            cc: null,
            bcc: null
        };
        ctrl.types = [
            {
                key: 0,
                value: 'Chung'
            },
            {
                key: 1,
                value: 'Application'
            }
        ];

        ctrl.validationOptions = {
            rules: {
                code: {
                    required: true
                },
                name: {
                    required: true
                },
                type: {
                    required: true
                },
                subject: {
                    required: true
                }
            }
        };

        ctrl.tinymceOptions = {
            height: 250,
            menubar: false,
            plugins: [
                'advlist autolink lists link image charmap print preview anchor',
                'searchreplace visualblocks code fullscreen',
                'insertdatetime media table paste code help wordcount'
            ],
            toolbar: 'undo redo | formatselect | bold italic backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | removeformat | help',
        };

        // ctrl.tinymceOptions = {
        //     selector: "textarea",
        //     height: 500,
        //     relative_urls: false,
        //     remove_script_host: false,
        //     plugins: [
        //         "advlist autolink autosave link image lists charmap print preview hr anchor pagebreak",
        //         "searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking",
        //         "table contextmenu directionality emoticons template textcolor paste fullpage textcolor colorpicker textpattern"
        //     ],

        //     toolbar1: "newdocument fullpage | bold italic underline strikethrough | alignleft aligncenter alignright alignjustify | styleselect formatselect fontselect fontsizeselect",
        //     toolbar2: "cut copy paste | searchreplace | bullist numlist | outdent indent blockquote | undo redo | link unlink anchor image media code | insertdatetime preview | forecolor backcolor",
        //     toolbar3: "table | hr removeformat | subscript superscript | charmap emoticons | print fullscreen | ltr rtl | visualchars visualblocks nonbreaking template pagebreak restoredraft",

        //     menubar: false,
        //     toolbar_items_size: 'big',

        //     style_formats: [{
        //         title: 'Bold text',
        //         inline: 'b'
        //     }, {
        //         title: 'Red text',
        //         inline: 'span',
        //         styles: {
        //             color: '#ff0000'
        //         }
        //     }, {
        //         title: 'Red header',
        //         block: 'h1',
        //         styles: {
        //             color: '#ff0000'
        //         }
        //     }, {
        //         title: 'Example 1',
        //         inline: 'span',
        //         classes: 'example1'
        //     }, {
        //         title: 'Example 2',
        //         inline: 'span',
        //         classes: 'example2'
        //     }, {
        //         title: 'Table styles'
        //     }, {
        //         title: 'Table row 1',
        //         selector: 'tr',
        //         classes: 'tablerow1'
        //     }],

        //     templates: [{
        //         title: 'Test template 1',
        //         content: 'Test 1'
        //     }, {
        //         title: 'Test template 2',
        //         content: 'Test 2'
        //     }]
        // };

        ctrl.getItem = function (id) {
            emailTemplateService.get(id).then(function (response) {
                if (response.status === 200) {
                    ctrl.emailTemplate = response.data;
                }
            });
        };

        ctrl.save = function (form) {
            if (!form.validate()) {
                return;
            }

            if ($stateParams.id) {
                emailTemplateService.put(ctrl.emailTemplate).then(function (response) {
                    if (response.status === 200) {
                        sweetAlert.swal({
                            title: "Thành công",
                            text: "Bạn đã sửa mẫu email thành công!",
                            type: "success",
                        });
                        $state.go('core.email-template.list');
                    }
                });
            } else {
                emailTemplateService.post(ctrl.emailTemplate).then(function (response) {
                    if (response.status === 200) {
                        sweetAlert.swal({
                            title: "Thành công",
                            text: "Bạn đã tạo mẫu email thành công!",
                            type: "success",
                        });
                        $state.go('core.email-template.list');
                    }
                });
            }
        };

        ctrl.$onInit = function () {
            if ($stateParams.id) {
                ctrl.getItem($stateParams.id);
            }
        };
    };

    angular
        .module('core')
        .controller('emailTemplateDetailController', emailTemplateDetailController);

    emailTemplateDetailController.$inject = ['$state', '$stateParams', 'sweetAlert', 'emailTemplateService'];
}());