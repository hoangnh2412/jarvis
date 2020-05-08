using Infrastructure.Database.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Database.Poco
{
    public class File : IEntity<Guid>, ITenantEntity, ILogCreatedEntity, IPermissionEntity
    {
        public Guid Id { get; set; }
        public Guid TenantCode { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Tên file vật lý
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Đuôi file. Ví dụ jpg
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Tên file và cả đuôi file. Ví dụ image.jpg
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Dung lượng file
        /// </summary>
        public long Length { get; set; }
    }
}
