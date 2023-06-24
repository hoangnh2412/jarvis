using Infrastructure.Database.Abstractions;
using System;

namespace Jarvis.Core.Database.Poco
{
    public class TenantInfo : IEntity<int>
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public bool IsCurrent { get; set; }
        public string TaxCode { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
        public string FullNameVi { get; set; }
        public string FullNameEn { get; set; }
        public string ShortName { get; set; }
        public string Logo { get; set; }
        public string LegalName { get; set; }
        public string Fax { get; set; }
        public string BusinessType { get; set; }
        public string Emails { get; set; }
        public string Phones { get; set; }
        public string BranchName { get; set; }
        public string Metadata { get; set; }
    }
}
