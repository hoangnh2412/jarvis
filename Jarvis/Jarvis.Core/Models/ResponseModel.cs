using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Models
{
    public class ResponseModel
    {
        public bool Succeeded { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }

    public class ResponseModel<T> : ResponseModel
    {
        public T Data { get; set; }
    }
}
