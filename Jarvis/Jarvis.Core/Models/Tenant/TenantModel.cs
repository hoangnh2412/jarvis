using System;

namespace Jarvis.Core.Models.Tenant
{
    public class TenantModel
    {
        public int Id { get; set; }
        public Guid Code { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Id tenant cha
        /// </summary>
        public Guid? IdParent { get; set; }

        /// <summary>
        /// Id tenant cha|ong noi|cu|ky
        /// </summary>
        public string Path { get; set; }

        public string HostName { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string DbConnectionString { get; set; }
        public string Theme { get; set; }

        public bool IsEnable { get; set; }
        public DateTime? ExpireDate { get; set; }

        public TenantInfoModel Info { get; set; }
    }
}
