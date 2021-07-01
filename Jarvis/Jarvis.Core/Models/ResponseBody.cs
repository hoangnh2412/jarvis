using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Jarvis.Core.Errors;
using Jarvis.Core.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Jarvis.Core.Models
{
    public class ErrorResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
    }

    public class ErrorResponse<T> : ErrorResponse
    {
        public T Data { get; set; }
    }

    public class ErrorModel
    {
        public int Code { get; set; }
        public string Field { get; set; }
        public string Description { get; set; }

        public static implicit operator ErrorModel(Extensions.ModelError error)
        {
            if (error == null)
                return null;

            return new ErrorModel
            {
                Field = error.Field,
                Description = error.Message
            };
        }
    }

    public class ServiceResult
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; set; }
    }

    public class ResultModel
    {
        public bool Succeeded { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public ErrorModel[] Errors { get; set; }

        public ResultModel()
        {
        }

        public ResultModel(bool succeeded)
        {
            Succeeded = succeeded;
        }

        public ResultModel(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Code = ErrorDefaults.ChuaXacDinh.Code;
            Message = message;
        }

        public ResultModel(bool succeeded, int code)
        {
            Succeeded = succeeded;

            if (succeeded)
                Code = ErrorDefaults.ThanhCong.Code;
            else
                Code = code;

            Message = ErrorStore.VI2.ContainsKey(code) ? ErrorStore.VI2[code] : ErrorDefaults.ChuaXacDinh.Message;

        }

        public ResultModel(bool succeeded, int code, string message)
        {
            Succeeded = succeeded;
            Code = code;
            Message = message;
        }
    }

    public class ResultModel<T> : ResultModel
    {
        public T Data { get; set; }

        public ResultModel()
        {
        }

        public ResultModel(T data)
        {
            Succeeded = true;
            Data = data;
        }

        public ResultModel(bool succeeded)
        {
            Succeeded = succeeded;
        }

        public ResultModel(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Code = ErrorDefaults.ChuaXacDinh.Code;
            Message = message;
        }

        public ResultModel(bool succeeded, int code, string message)
        {
            Succeeded = succeeded;
            Code = code;
            Message = message;
        }

        public ResultModel(bool succeeded, int code)
        {
            Succeeded = succeeded;

            if (succeeded)
                Code = ErrorDefaults.ThanhCong.Code;
            else
                Code = code;

            Message = ErrorStore.VI2.ContainsKey(code) ? ErrorStore.VI2[code] : ErrorDefaults.ChuaXacDinh.Message;
        }
    }

    public class ResultViewModel
    {
        public bool Succeeded { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public ErrorModel[] Errors { get; set; }


        public ResultViewModel() { }

        public ResultViewModel(ModelStateDictionary modelState)
        {
            var errors = modelState.GetErrors().Select(x => (ErrorModel)x).ToArray();

            Succeeded = false;
            Code = ErrorDefaults.ChuaXacDinh.Code;
            Message = string.Join(";", errors.Select(x => x.Description));
            Errors = errors;
        }

        public ResultViewModel(ResultModel model)
        {
            Succeeded = model.Succeeded;
            Code = model.Code;
            Message = model.Message;
            Errors = model.Errors;
        }

        public ResultViewModel(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Code = ErrorDefaults.ChuaXacDinh.Code;
            Message = message;
        }

        public ResultViewModel(bool succeeded, int code)
        {
            Succeeded = succeeded;

            if (succeeded)
                Code = ErrorDefaults.ThanhCong.Code;
            else
                Code = (int)code;

            Message = ErrorStore.VI2.ContainsKey(code) ? ErrorStore.VI2[code] : ErrorDefaults.ChuaXacDinh.Message;
        }

        public static implicit operator ResultViewModel(ResultModel model)
        {
            if (model == null)
                return null;

            var vm = new ResultViewModel();

            vm.Succeeded = model.Succeeded;
            vm.Code = model.Code;
            vm.Message = model.Message;
            vm.Errors = model.Errors;

            return vm;
        }

        public static implicit operator ResultViewModel(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                return null;
            }

            var vm = new ResultViewModel();

            var errors = modelState.GetErrors().Select(x => (ErrorModel)x).ToArray();
            vm.Succeeded = false;
            vm.Code = ErrorDefaults.ChuaXacDinh.Code;
            vm.Message = string.Join(";", errors.Select(x => x.Description));
            vm.Errors = errors;

            return vm;
        }
    }

    public class ResultViewModel<T>
    {
        public bool Succeeded { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public ErrorModel[] Errors { get; set; }

        public T Data { get; set; }


        public ResultViewModel() { }

        public ResultViewModel(ModelStateDictionary modelState)
        {
            var errors = modelState.GetErrors().Select(x => (ErrorModel)x).ToArray();
            Succeeded = false;
            Code = ErrorDefaults.ChuaXacDinh.Code;
            Message = string.Join(";", errors.Select(x => x.Description));
            Errors = errors;
        }

        public ResultViewModel(ResultModel model)
        {
            Succeeded = model.Succeeded;
            Code = model.Code;
            Message = ErrorStore.VI2.ContainsKey(model.Code) ? ErrorStore.VI2[model.Code] : model.Message;
            Errors = model.Errors;
        }

        public ResultViewModel(ResultModel<T> model)
        {
            Succeeded = model.Succeeded;
            Code = model.Code;
            Message = ErrorStore.VI2.ContainsKey(model.Code) ? ErrorStore.VI2[model.Code] : model.Message;
            Errors = model.Errors;
            Data = model.Data;
        }

        public ResultViewModel(bool succeeded, string message)
        {
            Succeeded = succeeded;
            Code = ErrorDefaults.ChuaXacDinh.Code;
            Message = message;
        }

        public ResultViewModel(bool succeeded, int code, string message)
        {
            Succeeded = succeeded;
            Code = code;
            Message = message;
        }

        public static implicit operator ResultViewModel<T>(ResultModel<T> model)
        {
            if (model == null)
                return null;

            var vm = new ResultViewModel<T>();

            vm.Succeeded = model.Succeeded;
            vm.Code = model.Code;
            vm.Message = ErrorStore.VI2.ContainsKey(model.Code) ? ErrorStore.VI2[model.Code] : model.Message;
            vm.Errors = model.Errors;

            return vm;
        }

        public static implicit operator ResultViewModel<T>(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                return null;
            }

            var vm = new ResultViewModel<T>();

            var errors = modelState.GetErrors().Select(x => (ErrorModel)x).ToArray();
            vm.Succeeded = false;
            vm.Code = ErrorDefaults.ChuaXacDinh.Code;
            vm.Message = string.Join(";", errors.Select(x => x.Description));
            vm.Errors = errors;

            return vm;
        }
    }



    //API
    public class ResultApiModel
    {
        public bool Succeeded { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public ResultApiModel()
        {
        }


        public ResultApiModel(bool succeeded, int code)
        {
            Succeeded = succeeded;

            if (succeeded)
            {
                Code = ErrorDefaults.ThanhCong.Code;
                return;
            }

            Code = code;
        }
    }

    public class ResultApiModel<T> : ResultApiModel
    {
        public T Data { get; set; }

        public ResultApiModel()
        {
        }

        public ResultApiModel(T data)
        {
            Succeeded = true;
            Code = ErrorDefaults.ThanhCong.Code;
            Data = data;
        }

        public ResultApiModel(bool succeeded, int? code)
        {

            Succeeded = succeeded;

            if (succeeded)
            {
                Code = ErrorDefaults.ThanhCong.Code;
                return;
            }

            if (code.HasValue)
                Code = code.Value;
            else
                Code = ErrorDefaults.ChuaXacDinh.Code;
        }
    }

    public class ResultApiViewModel
    {
        public bool Succeeded { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }
        public string[] Errors { get; set; }


        public ResultApiViewModel() { }

        public ResultApiViewModel(ModelStateDictionary modelState)
        {
            var errors = modelState.GetErrors().Select(x => x.Field + ": " + x.Message).ToArray();

            Succeeded = false;
            Code = ErrorDefaults.ChuaXacDinh.Code;
            Message = string.Join(";", errors);
            Errors = errors;
        }

        public ResultApiViewModel(ResultApiModel model)
        {
            Succeeded = model.Succeeded;
            Code = (int)model.Code;
            Message = ErrorStore.VI2.ContainsKey(model.Code) ? ErrorStore.VI2[model.Code] : ErrorDefaults.ChuaXacDinh.Message;
        }

        public ResultApiViewModel(bool succeeded, int code)
        {
            Succeeded = succeeded;

            if (succeeded)
            {
                Code = ErrorDefaults.ThanhCong.Code;
                Message = ErrorDefaults.ThanhCong.Message;
            }
            else
            {
                Code = ErrorDefaults.ChuaXacDinh.Code;
                Message = ErrorDefaults.ChuaXacDinh.Message;
            }
        }

        public ResultApiViewModel(bool succeeded, int code, string message = null)
        {
            Succeeded = succeeded;

            if (succeeded)
            {
                Code = ErrorDefaults.ThanhCong.Code;
                Message = ErrorDefaults.ThanhCong.Message;
            }
            else
            {
                Code = code;
                Message = message;
                if (string.IsNullOrEmpty(message) || int.TryParse(message, out int outMess))
                    Message = ErrorStore.VI2.ContainsKey(code) ? ErrorStore.VI2[code] : ErrorDefaults.ChuaXacDinh.Message;

            }
        }

        public static implicit operator ResultApiViewModel(ResultApiModel model)
        {
            if (model == null)
                return null;

            var vm = new ResultApiViewModel();

            vm.Succeeded = model.Succeeded;
            vm.Code = (int)model.Code;
            vm.Message = ErrorStore.VI2.ContainsKey(model.Code) ? ErrorStore.VI2[model.Code] : ErrorDefaults.ChuaXacDinh.Message;

            return vm;
        }

        public static implicit operator ResultApiViewModel(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                return null;
            }

            var vm = new ResultApiViewModel();

            var errors = modelState.GetErrors().Select(x => x.Field + ": " + x.Message).ToArray();
            vm.Succeeded = false;
            vm.Code = ErrorDefaults.ChuaXacDinh.Code;
            vm.Message = string.Join("; ", errors);
            vm.Errors = errors;

            return vm;
        }
    }

    public class ResultApiViewModel<T> : ResultApiViewModel
    {
        public T Data { get; set; }


        public ResultApiViewModel() { }

        public ResultApiViewModel(ModelStateDictionary modelState)
        {
            var errors = modelState.GetErrors().Select(x => x.Field + ": " + x.Message).ToArray();
            Succeeded = false;
            Code = ErrorDefaults.ChuaXacDinh.Code;
            Message = string.Join("; ", errors);
            Errors = errors;
        }

        public ResultApiViewModel(IList<ValidationResult> validationResults)
        {
            var errors = validationResults.Select(x => x.ErrorMessage).ToArray();
            Succeeded = false;
            Code = ErrorDefaults.ChuaXacDinh.Code;
            Message = string.Join("; ", errors);
            Errors = errors;
        }


        public ResultApiViewModel(ResultApiModel model)
        {
            Succeeded = model.Succeeded;
            Code = (int)model.Code;
            Message = ErrorStore.VI2.ContainsKey(model.Code) ? ErrorStore.VI2[model.Code] : ErrorDefaults.ChuaXacDinh.Message;
        }

        public ResultApiViewModel(ResultApiModel<T> model)
        {
            Succeeded = model.Succeeded;
            Code = (int)model.Code;
            Message = ErrorStore.VI2.ContainsKey(model.Code) ? ErrorStore.VI2[model.Code] : ErrorDefaults.ChuaXacDinh.Message;
            Data = model.Data;
        }

        public ResultApiViewModel(bool succeeded, int? code)
        {
            //Succeeded = succeeded;

            //if (succeeded)
            //{
            //    Code = ErrorCodes.ErrorDefault.ThanhCong.GetHashCode();
            //    Message = ErrorStore.VI[ErrorCodes.ErrorDefault.ThanhCong];
            //}
            //else
            //{
            //    if (code.HasValue)
            //    {
            //        Code = (int)code.Value;
            //        Message = ErrorStore.VI.ContainsKey((ErrorCodes.ErrorDefault)code.Value) ? ErrorStore.VI[(ErrorCodes.ErrorDefault)code.Value] : ErrorStore.VI[ErrorCodes.ErrorDefault.ChuaXacDinh];
            //    }
            //    else
            //    {
            //        Code = ErrorCodes.ErrorDefault.ChuaXacDinh.GetHashCode();
            //        Message = ErrorStore.VI[ErrorCodes.ErrorDefault.ChuaXacDinh];
            //    }
            //}

            //chỉ dùng cho api nên để hardcode 0 là thành công, -1 là chưa xác định

            Succeeded = succeeded;

            if (!ErrorStore.VI2.ContainsKey(ErrorDefaults.ThanhCong.Code))
                ErrorStore.VI2.Add(ErrorDefaults.ThanhCong.Code, ErrorDefaults.ThanhCong.Message);

            if (!ErrorStore.VI2.ContainsKey(ErrorDefaults.ChuaXacDinh.Code))
                ErrorStore.VI2.Add(ErrorDefaults.ChuaXacDinh.Code, ErrorDefaults.ChuaXacDinh.Message);

            if (succeeded)
            {
                Code = ErrorDefaults.ThanhCong.Code; //key = 0 là thành công
                Message = ErrorDefaults.ThanhCong.Message;
            }
            else
            {
                if (code.HasValue)
                {
                    Code = (int)code.Value;
                    Message = ErrorStore.VI2.ContainsKey(code.Value) ? ErrorStore.VI2[code.Value] : ErrorDefaults.ChuaXacDinh.Message;
                }
                else
                {
                    Code = ErrorDefaults.ChuaXacDinh.Code;
                    Message = ErrorDefaults.ChuaXacDinh.Message;
                }
            }
        }


        public ResultApiViewModel(bool succeeded, int code, string message = null)
        {
            Succeeded = succeeded;

            if (succeeded)
            {
                Code = ErrorDefaults.ThanhCong.Code;
                Message = ErrorDefaults.ThanhCong.Message;
            }
            else
            {
                Code = code;
                Message = message;
                if (string.IsNullOrEmpty(message) || int.TryParse(message, out int outMess))
                    Message = ErrorStore.VI2.ContainsKey(code) ? ErrorStore.VI2[code] : ErrorDefaults.ChuaXacDinh.Message;
            }
        }

        public static implicit operator ResultApiViewModel<T>(ResultApiModel<T> model)
        {
            if (model == null)
                return null;

            var vm = new ResultApiViewModel<T>();

            vm.Succeeded = model.Succeeded;
            vm.Code = (int)model.Code;
            vm.Message = ErrorStore.VI2.ContainsKey(model.Code) ? ErrorStore.VI2[model.Code] : ErrorDefaults.ChuaXacDinh.Message;

            return vm;
        }

        public static implicit operator ResultApiViewModel<T>(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                return null;
            }

            var vm = new ResultApiViewModel<T>();

            var errors = modelState.GetErrors().Select(x => x.Field + ": " + x.Message).ToArray();
            vm.Succeeded = false;
            vm.Code = ErrorDefaults.ChuaXacDinh.Code;
            vm.Message = string.Join("; ", errors);
            vm.Errors = errors;

            return vm;
        }

    }
}
