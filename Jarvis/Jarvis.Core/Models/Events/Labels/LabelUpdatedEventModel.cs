using System;

namespace Jarvis.Core.Models.Events.Labels
{
    public class LabelUpdatedEventModel
    {
        public Guid TenantCode { get; set; }
        public Guid Code { get; set; }
        public string Name { get; set; }
    }
}