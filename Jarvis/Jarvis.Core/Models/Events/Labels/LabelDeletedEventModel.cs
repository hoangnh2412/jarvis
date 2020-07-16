using System;

namespace Jarvis.Core.Models.Events.Labels
{
    public class LabelDeletedEventModel
    {
        public Guid TenantCode { get; set; }
        public Guid Code { get; set; }
    }
}