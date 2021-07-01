using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jarvis.Core.Extensions
{
    public static class ModelStateExtension
    {
        public static IEnumerable<ModelError> GetErrors(this ModelStateDictionary modelState)
        {
            foreach (var key in modelState.Keys)
            {
                var value = modelState[key];
                if (value.Errors.Count <= 0)
                    continue;

                foreach (var valueError in value.Errors)
                {
                    yield return new ModelError
                    {
                        Field = key,
                        Message = valueError.ErrorMessage,
                        Exception = valueError.Exception
                    };
                }
            }
        }

        public static string GetMessage(this ModelStateDictionary modelState)
        {
            return string.Join(";", modelState.GetErrors().Select(x => x.Message));
        }
    }

    public class ModelError
    {
        public string Field { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
