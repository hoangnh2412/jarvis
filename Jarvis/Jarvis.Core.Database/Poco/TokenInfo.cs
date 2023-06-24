using Infrastructure.Database.Abstractions;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class TokenInfo : IEntity<int>, ITenantEntity
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public Guid TenantCode { get; set; }
        public Guid IdUser { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Metadata { get; set; }
        public double TimeToLife { get; set; }
        public string LocalIpAddress { get; set; }
        public string PublicIpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Source { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpireAt { get; set; }
        public DateTime ExpireAtUtc { get; set; }
    }
}
