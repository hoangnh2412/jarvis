using Infrastructure.Extensions;
using Jarvis.Core.Constants;
using Jarvis.Core.Errors;
using Jarvis.Core.Extensions;
using Jarvis.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jarvis.Core.Middlewares
{
    public class WrapResponseMiddleware
    {
        private readonly ILogger<WrapResponseMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly List<string> _fileContentTypes = new List<string> { ContentType.Pdf, ContentType.Xml, ContentType.Zip, ContentType.Stream };

        public WrapResponseMiddleware(
            ILogger<WrapResponseMiddleware> logger,
            RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var ignorePath = new List<string> { "/swagger", "/api", "/tool" };
            var ignoreEndPaths = new List<string> { ".css", ".ico", ".js", ".html", ".png", ".ttf", ".map" };

            if (ignorePath.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.CurrentCultureIgnoreCase) || path == context.Request.Path)
                //|| ignoreEndPaths.Any(path => context.Request.Path.Value.EndsWith(path))
                || context.Request.Path.Value.Contains("."))
            {
                await _next.Invoke(context);
                return;
            }

            //if (context.Request.Path.Value.EndsWith(".css") || context.Request.Path.Value.EndsWith(".ico")
            //    || context.Request.Path.Value.EndsWith(".js") || context.Request.Path.Value.EndsWith(".html")
            //    || context.Request.Path.Value.EndsWith("/") || context.Request.Path.Value.EndsWith("map")
            //    || context.Request.Path.Value.EndsWith("font") || context.Request.Path.Value.EndsWith(".ttf")
            //    || context.Request.Path.Value.EndsWith(".png") || context.Request.Path.Value.EndsWith("woff2"))
            //{
            //    await _next.Invoke(context);
            //    return;
            //}

            Stream originalBody = context.Response.Body;
            try
            {
                string responseBody = null;

                using (var stream = new MemoryStream())
                {
                    context.Response.Body = stream;

                    await _next.Invoke(context);

                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    using (var streamReader = new StreamReader(context.Response.Body))
                    {
                        responseBody = streamReader.ReadToEnd();
                        context.Response.Body.Seek(0, SeekOrigin.Begin);
                    }
                }

                ResultViewModel<object> response = new ResultViewModel<object>();
                switch (context.Response.StatusCode)
                {
                    case StatusCodes.Status200OK:
                        response.Succeeded = true;
                        response.Code = ErrorDefaults.ThanhCong.Code;
                        response.Message = ErrorDefaults.ThanhCong.Message;
                        response.Data = responseBody.JsonDeserialize<object>();
                        break;
                    case StatusCodes.Status400BadRequest:
                        response.Succeeded = false;
                        response.Code = ErrorDefaults.DinhDangDuLieu.Code;
                        response.Message = ErrorDefaults.DinhDangDuLieu.Message;
                        var errors = responseBody.JsonDeserialize<Dictionary<string, List<string>>>();
                        response.Errors = errors.Select(x => new ErrorModel { Field = x.Key, Description = string.Join(";", x.Value) }).ToArray();
                        break;
                    case StatusCodes.Status403Forbidden:
                        response.Succeeded = false;
                        response.Code = ErrorDefaults.KhongCoQuyen.Code;
                        response.Message = ErrorDefaults.KhongCoQuyen.Message;
                        break;
                    case StatusCodes.Status401Unauthorized:
                        response.Succeeded = false;
                        response.Code = ErrorDefaults.BanChuaDangNhap.Code;
                        response.Message = ErrorDefaults.BanChuaDangNhap.Message;
                        break;
                    case StatusCodes.Status404NotFound:
                        response.Succeeded = false;
                        response.Code = ErrorDefaults.KhongTimThayServer.Code;
                        if (!string.IsNullOrEmpty(responseBody))
                            response.Message = responseBody;
                        else
                            response.Message = ErrorDefaults.KhongTimThayServer.Message;
                        break;
                    default:
                        response.Succeeded = false;
                        int code = ErrorDefaults.ChuaXacDinh.Code;

                        //Xử lý trường hợp responseBody = "" không cho trả về code = 0
                        if (!string.IsNullOrEmpty(responseBody))
                            int.TryParse(responseBody, out code);

                        response.Code = ErrorDefaults.ChuaXacDinh.Code;
                        response.Message = ErrorStore.VI2.ContainsKey(code) ? ErrorStore.VI2[code] : ErrorDefaults.ChuaXacDinh.Message;
                        break;
                }

                var buffer = Encoding.UTF8.GetBytes(response.JsonSerialize(true, true));
                using (var output = new MemoryStream(buffer))
                {
                    await output.CopyToAsync(originalBody);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(default(EventId), ex, ex.Message);

                    if (ex.InnerException != null)
                        _logger.LogError(default(EventId), ex, ex.InnerException.Message);

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    var response = new ResponseModel();
                    response.Succeeded = false;

                    if (int.TryParse(ex.Message, out int code) && ErrorStore.VI2.ContainsKey(code))
                    {
                        response.Code = code;
                        response.Message = ErrorStore.VI2[code];
                    }
                    else
                    {
                        response.Code = ErrorDefaults.ChuaXacDinh.Code;
                        response.Message = ex.Message;
                    }

                    //Thêm tham số cho thông báo
                    if (ex.Data != null && ex.Data.Count > 0)
                    {
                        foreach (var item in ex.Data.Keys)
                        {
                            response.Message = response.Message.Replace(item.ToString(), ex.Data[item.ToString()].ToString());
                        }
                    }

                    var buffer = Encoding.UTF8.GetBytes(response.JsonSerialize(true, true));
                    using (var output = new MemoryStream(buffer))
                    {
                        await output.CopyToAsync(originalBody);
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(default(EventId), innerEx, innerEx.Message);

                    if (innerEx.InnerException != null)
                        _logger.LogError(default(EventId), innerEx, innerEx.InnerException.Message);
                }
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        //private static ResultModel<object> TryParseJson(string responseBody)
        //{
        //    ResultModel<object> mResult = null;
        //    try
        //    {
        //        mResult = JsonConvert.DeserializeObject<ResultModel<object>>(responseBody);
        //    }
        //    catch (Exception)
        //    {
        //        mResult = null;
        //    }

        //    return mResult;
        //}

        //private static async Task WriteBody(Stream originalBodyStream, string data)
        //{
        //    var buffer = Encoding.UTF8.GetBytes(data);
        //    using (var output = new MemoryStream(buffer))
        //    {
        //        await output.CopyToAsync(originalBodyStream);
        //    }
        //}

        //private static string WrapResponse500(string responseBody)
        //{
        //    ResultModel<object> mResult = TryParseJson(responseBody);
        //    if (mResult != null)
        //    {
        //        var vmResult = new ResponseModel<object>(mResult);
        //        return vmResult.JsonSerialize(true, true);
        //    }
        //    else
        //    {
        //        return new ResponseModel(false, responseBody).JsonSerialize(true, true);
        //    }
        //}

        //private static string WrapResponse200(string responseBody)
        //{
        //    ResultModel<object> mResult = TryParseJson(responseBody);
        //    if (mResult != null)
        //    {
        //        var vmResult = new ResultViewModel<object>(mResult);
        //        return vmResult.JsonSerialize(true, true);
        //    }
        //    else
        //    {
        //        return new ResultViewModel(true, responseBody).JsonSerialize(true, true);
        //    }
        //}

        //private static string WrapResponse400(string responseBody)
        //{
        //    var modelState = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(responseBody);
        //    return new ResultViewModel(false, string.Join(";", modelState.Values.SelectMany(x => x))).JsonSerialize(true, true);
        //}
    }

}
