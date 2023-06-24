using Infrastructure.Database.Abstractions;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class Setting : IEntity<int>, ITenantEntity, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity, ILogDeletedVersionEntity<int?>, IPermissionEntity
    {
        public int Id { get; set; }
        public Guid TenantCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public Guid? DeletedBy { get; set; }
        public int? DeletedVersion { get; set; }
        

        public Guid Key { get; set; }

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
        /// Options để chọn khi cấu hình. Ví dụ: yes|no hoặc 1:có|0:không
        /// </summary>
        public string Options { get; set; }

        /// <summary>
        /// Text, Textarea, Combobox
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }

        public string Note { get; set; }

        public bool IsReadOnly { get; set; }

    }
}
