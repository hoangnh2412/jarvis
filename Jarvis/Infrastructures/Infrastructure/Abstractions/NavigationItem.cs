using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Abstractions
{
    public class NavigationItem
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public string Icon { get; set; }

        public int Order { get; set; }

        public string Url { get; set; }
    }
}
