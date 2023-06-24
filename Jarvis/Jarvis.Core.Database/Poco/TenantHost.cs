using Infrastructure.Database.Abstractions;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class TenantHost : IEntity<int>, ILogDeletedVersionEntity<int?>
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public string HostName { get; set; }

        public int? DeletedVersion { get; set; }
    }
}
