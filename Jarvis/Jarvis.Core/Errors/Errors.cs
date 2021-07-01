using System;
using System.Collections.Generic;

namespace Jarvis.Core.Errors
{
    public class Errors
    {
        public static void ThrowException(int code, Dictionary<string, string> dictionary)
        {
            var ex = new Exception(code.ToString());

            if (dictionary != null)
            {
                foreach (var item in dictionary)
                    ex.Data.Add(item.Key, item.Value);
            }

            throw ex;
        }
    }
}
