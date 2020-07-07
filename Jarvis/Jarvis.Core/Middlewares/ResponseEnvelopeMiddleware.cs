using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Jarvis.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Jarvis.Core.Middlewares
{
    public class ResponseEnvelopeMiddleware
    {
        private readonly ILogger<ResponseEnvelopeMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ResponseEnvelopeMiddleware(
            ILogger<ResponseEnvelopeMiddleware> logger,
            RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Stream originalResponseBody = context.Response.Body;
            string responseBody = null;

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    context.Response.Body = stream;
                    await _next(context);
                    responseBody = await GetResponseBodyAsync(context.Response);
                    await stream.CopyToAsync(originalResponseBody);
                }
            }
            catch (Exception ex)
            {
                responseBody = ex.Message;
                // context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                throw ex;
            }
            // catch (Exception ex)
            // {
            //     // _logger.LogError(default(EventId), ex, ex.Message);

            //     // if (ex.InnerException != null)
            //     //     _logger.LogError(default(EventId), ex, ex.InnerException.Message);

            //     //var response = new ApiResultModel<object>();
            //     //response.Succeeded = false;
            //     //response.Code = StatusCodes.Status500InternalServerError;
            //     //response.Message = ex.Message;

            //     //if (int.TryParse(ex.Message, out int code) && ErrorStore.VI.ContainsKey((ErrorCodes.ErrorDefault)code))
            //     //{
            //     //    response.Code = code;
            //     //    response.Message = ErrorStore.VI[(ErrorCodes.ErrorDefault)code];
            //     //}
            //     //else
            //     //{
            //     //    response.Code = ErrorCodes.ErrorDefault.ChuaXacDinh.GetHashCode();
            //     //    response.Message = ex.Message;
            //     //}

            //     ////Thêm tham số cho thông báo
            //     //if (ex.Data != null && ex.Data.Count > 0)
            //     //{
            //     //    foreach (var item in ex.Data.Keys)
            //     //    {
            //     //        response.Message = response.Message.Replace(item.ToString(), ex.Data[item.ToString()].ToString());
            //     //    }
            //     //}
            // }
            finally
            {
                ResponseModel<object> response = new ResponseModel<object>();
                switch (context.Response.StatusCode)
                {
                    case StatusCodes.Status200OK:
                        response.Succeeded = true;
                        response.Code = StatusCodes.Status200OK;
                        response.Message = "Thành công";
                        response.Data = JsonConvert.DeserializeObject<object>(responseBody);
                        break;
                    case StatusCodes.Status400BadRequest:
                        response.Succeeded = false;
                        response.Code = StatusCodes.Status400BadRequest;
                        response.Message = "Tham số không hợp lệ";

                        // var errors = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(responseBody);
                        // response.Data = errors.Values.SelectMany(x => x).ToList();
                        // response.Data = errors.SelectMany(x => x.Value).Select(x => new ErrorModel { Description = x }).ToArray();
                        break;
                    case StatusCodes.Status403Forbidden:
                        response.Succeeded = false;
                        response.Code = StatusCodes.Status403Forbidden;
                        response.Message = "Bạn không có quyền sử dụng chức năng";
                        break;
                    case StatusCodes.Status401Unauthorized:
                        response.Succeeded = false;
                        response.Code = StatusCodes.Status401Unauthorized;
                        response.Message = "Bạn chưa đăng nhập";
                        break;
                    case StatusCodes.Status404NotFound:
                        response.Succeeded = false;
                        response.Code = StatusCodes.Status404NotFound;
                        response.Message = "Không tìm thấy dữ liệu";
                        break;
                    default:
                        response.Succeeded = false;
                        response.Code = StatusCodes.Status500InternalServerError;
                        response.Message = responseBody;

                        //int code = ErrorCodes.ErrorDefault.ChuaXacDinh.GetHashCode();

                        ////Xử lý trường hợp responseBody = "" không cho trả về code = 0
                        //if (!string.IsNullOrEmpty(responseBody))
                        //    int.TryParse(responseBody, out code);

                        //response.Code = ErrorCodes.ErrorDefault.ChuaXacDinh.GetHashCode();
                        //response.Message = ErrorStore.VI.ContainsKey((ErrorCodes.ErrorDefault)code) ? ErrorStore.VI[(ErrorCodes.ErrorDefault)code] : ErrorStore.VI[ErrorCodes.ErrorDefault.ChuaXacDinh];
                        break;
                }

                try
                {
                    context.Response.Body = originalResponseBody;
                    // context.Response.StatusCode = StatusCodes.Status200OK;
                    // context.Response.ContentType = "application/json;charset=UTF-8";

                    await WriteResponse(originalResponseBody, response);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static async Task<string> GetResponseBodyAsync(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return text;
        }

        private static async Task WriteResponse(Stream originalBody, ResponseModel<object> response)
        {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            }));
            //var buffer = Encoding.UTF8.GetBytes(response.Message);
            using (var output = new MemoryStream(buffer))
            {
                await output.CopyToAsync(originalBody);
            }
        }
    }
}
