using Jarvis.Core.Database.Poco;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Models
{
    public class LabelModel
    {
        public Guid Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        public static implicit operator LabelModel(Label entity)
        {
            if (entity == null)
                return null;

            var model = new LabelModel();
            model.Code = entity.Key;
            model.Name = entity.Name;
            model.Description = entity.Description;
            model.Icon = entity.Icon;
            model.Color = entity.Color;
            return model;
        }
    }
}
