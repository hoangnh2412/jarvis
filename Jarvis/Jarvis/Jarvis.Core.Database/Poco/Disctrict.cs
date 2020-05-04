using Infrastructure.Database.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Database.Poco
{
    public class Disctrict : IEntity<int>
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }

        //FK
        public int IdCity { get; set; }
        public virtual City City { get; set; }
        public virtual ICollection<Ward> Wards { get; set; }
    }
}
