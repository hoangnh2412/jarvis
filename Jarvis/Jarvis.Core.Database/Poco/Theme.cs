using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Database.Poco
{
    public class Theme : IEntity<Guid>
    {
        public Guid Id { get; set; }
        
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
