using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Database.Models
{
    public interface IPaged<T>
    {
        int Size { get; set; }
        int TotalItems { get; set; }
        int Page { get; set; }
        int TotalPages { get; set; }
        string Q { get; set; }
        IEnumerable<T> Data { get; set; }

    }

    public class Paged<T> : IPaged<T>
    {
        public int Size { get; set; }
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public string Q { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
}
