using Infrastructure.Database.Abstractions;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class Tenant : IEntity<int>, ILogCreatedEntity, ILogUpdatedEntity, ILogDeletedEntity
    {
        public int Id { get; set; }

        public Guid Code { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Id tenant cha
        /// </summary>
        public Guid? IdParent { get; set; }

        /// <summary>
        /// Tenant Code ong|cha|chau
        /// </summary>
        public string Path { get; set; }

        public string Server { get; set; }
        public string Database { get; set; }
        public string DbConnectionString { get; set; }
        public string Theme { get; set; }
        public string SecretKey { get; set; }

        /// <summary>
        /// có đang sử dụng và có dkph/hóa đơn hay không
        /// true: đã được dùng để tạo dkph => không được xóa/sửa
        /// false: chưa dkph => được xóa/sửa
        /// </summary>
        public bool IsEnable { get; set; }

        public DateTime? ExpireDate { get; set; }

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
    }
}