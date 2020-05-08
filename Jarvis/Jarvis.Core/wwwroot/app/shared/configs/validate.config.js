(function () {
    'use strict';

    angular
        .module('jvValidate', [
            'angular.validate'
        ])
        .config(['$validatorProvider', function ($validatorProvider) {
            //From validation $validatorProvider
            $validatorProvider.setDefaults({
                errorElement: 'span',
                errorClass: 'help-block',
                highlight: function (element, errorClass, validClass) {
                    if (element.type === "radio") {
                        this.findByName(element.name).addClass(errorClass).removeClass(validClass);
                    } else {
                        angular.element(element).addClass(errorClass).removeClass(validClass);
                    }

                    var parent = angular.element(element).parent()[0];
                    if (parent.tagName === 'DIV' || parent.tagName === 'SPAN') {
                        angular.element(parent).addClass('has-error').removeClass('has-success');
                    }
                },
                unhighlight: function (element, errorClass, validClass) {
                    if (element.type === "radio") {
                        this.findByName(element.name).removeClass(errorClass).addClass(validClass);
                    } else {
                        angular.element(element).removeClass(errorClass).addClass(validClass);
                    }

                    var parent = angular.element(element).parent()[0];
                    if (parent.tagName === 'DIV' || parent.tagName === 'SPAN') {
                        angular.element(parent).removeClass('has-error').addClass('has-success');
                    }
                }
            });

            $validatorProvider.setDefaultMessages({
                required: "Vui lòng nhập trường này.",
                remote: "Please fix this field.",
                email: "Vui lòng nhập địa chỉ email hợp lệ.",
                url: "Vui lòng nhập URL hợp lệ.",
                date: "Vui lòng nhập ngày tháng hợp lệ.",
                dateISO: "Vui lòng nhập ngày tháng (ISO) hợp lệ.",
                number: "Vui lòng nhập số.",
                digits: "Vui lòng nhập ký tự.",
                creditcard: "Vui lòng nhập số thẻ.",
                equalTo: "Please enter the same value again.",
                accept: "Please enter a value with a valid extension.",
                maxlength: $validatorProvider.format("Vui lòng nhập nhiều nhất {0} ký tự."),
                minlength: $validatorProvider.format("Vui lòng nhập ít nhất {0} ký tự."),
                rangelength: $validatorProvider.format("Vui lòng nhập giá trị có độ dài từ {0} đến {1}."),
                range: $validatorProvider.format("Vui lòng nhập giá trị từ {0} đến {1}."),
                max: $validatorProvider.format("Vui lòng nhập giá trị nhỏ hơn hoặc bằng {0}."),
                min: $validatorProvider.format("Vui lòng nhập giá trị lớn hơn hoặc bằng {0}.")
            });

            $validatorProvider.addMethod("taxCode", function (value, element) {
                return this.optional(element) || validateTaxCode(value);
            }, "Vui lòng nhập mã số thuế hợp lệ.");

            $validatorProvider.addMethod("multipleEmails", function (value, element) {
                return validateMultipleEmail(this, value, element);
            }, "Vui lòng nhập địa chỉ email hợp lệ.");

            $validatorProvider.addMethod("regex", function (value, element, params) {
                return this.optional(element) || params.test(value);
            }, "Vui lòng nhập chuỗi hợp lệ.");

            function validateTaxCode(code) {
                // valid chỉ được nhập số và dấu -
                var checkRegexCode = /^[0-9~\-]*$/.test(code);
                if (!checkRegexCode)
                    return false;

                var taxCode = code.split('');
                var lengthTaxCode = taxCode.length;
                if (lengthTaxCode == 10 || lengthTaxCode == 14) {
                } else
                    return false;

                var checktaxcode =
                    parseInt(taxCode[0]) * 31 +
                    parseInt(taxCode[1]) * 29 +
                    parseInt(taxCode[2]) * 23 +
                    parseInt(taxCode[3]) * 19 +
                    parseInt(taxCode[4]) * 17 +
                    parseInt(taxCode[5]) * 13 +
                    parseInt(taxCode[6]) * 7 +
                    parseInt(taxCode[7]) * 5 +
                    parseInt(taxCode[8]) * 3;

                if (10 - (checktaxcode % 11) === parseInt(taxCode[9])) {
                }
                else return false;

                if (lengthTaxCode > 10) {
                    if (taxCode[10] !== '-') return false;
                    if (parseInt(taxCode[11]) + parseInt(taxCode[12]) + parseInt(taxCode[13]) <= 0) return false;
                }
                return true;
            };

            function validateMultipleEmail(contruct, emailStrings, element) {
                var listEmail = emailStrings.split(';');

                var len = listEmail.length;

                if (len == 0) return false;

                // kiểm tra từng email trong chuỗi có hợp lệ theo method email mặc định của validator
                for (var i = 0; i < len; i++) {
                    if (listEmail[i] != "") {
                        listEmail[i] = listEmail[i].trim();
                        if (!$.validator.methods.email.call(contruct, listEmail[i], element))
                            return false;
                    }
                }

                return true;
            };
        }]);
}());