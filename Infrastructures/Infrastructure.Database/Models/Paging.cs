using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Database.Models
{
    public interface IPaging
    {
        /// <summary>
        /// Số bản ghi trên 1 trang
        /// </summary>
        int? Size { get; set; }

        /// <summary>
        /// Số trang hiện tại
        /// </summary>
        int? Page { get; set; }

        /// <summary>
        /// Từ khóa tìm kiếm
        /// </summary>
        string Q { get; set; }

        /// <summary>
        /// Các cột tìm kiếm
        /// </summary>
        Dictionary<string, string> Search { get; set; }

        /// <summary>
        /// Các cột sắp xếp
        /// </summary>
        Dictionary<string, string> Sort { get; set; }

        /// <summary>
        /// Các cột hiển thị, trả theo thứ tự cột
        /// </summary>
        Dictionary<string, bool> Columns { get; set; }
    }

    public class Paging : IPaging
    {
        public int? Size { get; set; }

        public int? Page { get; set; }

        public string Q { get; set; }

        public Dictionary<string, string> Search { get; set; }

        public Dictionary<string, string> Sort { get; set; }

        public Dictionary<string, bool> Columns { get; set; }
    }
}
