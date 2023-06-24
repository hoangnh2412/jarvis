using Jarvis.Core.Constants;
using System;
using System.Collections.Generic;

namespace Jarvis.Core.Models
{
    public class SettingModel
    {
        public Guid Key { get; set; }
        public Guid TenantCode { get; set; }

        /// <summary>
        /// Nhóm cấu hình, sử dụng để lấy ra nhiều Setting cùng 1 lúc khi CanDelete = true hoặc CanCreate = true
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Mã để lấy cấu hình, ko dc trùng
        /// </summary>

        public string Code { get; set; }
        
        /// <summary>
        /// Tên cấu hình, hiển thị trên UI
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Giá trị cấu hình
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Options để chọn khi cấu hình
        /// </summary>
        public Dictionary<string, string> Options { get; set; }

        /// <summary>
        /// Text, Textarea, Combobox
        /// </summary>
        public SettingType Type { get; set; }

        /// <summary>
        /// mô tả
        /// </summary>
        public string Description { get; set; }

        public string Note { get; set; }

        /// <summary>
        /// Những cấu hình không được hiển thị nút xóa = true, mặc định ko dc xóa = false
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Những cấu hình không được hiển thị nút sửa = true, mặc định ko dc sửa = false
        /// </summary>
        public bool CanCreate { get; set; }

        /// <summary>
        /// Những cấu hình cần ẩn = true => Không được sửa, mặc định hiện = false
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Chỉnh sửa lần cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
