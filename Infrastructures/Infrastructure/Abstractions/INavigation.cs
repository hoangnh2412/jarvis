using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Abstractions
{
    public interface INavigation
    {
        string Name { get; }
        string Code { get; }
        string Icon { get; }
        int Order { get; }
        string Url { get; }
        string[] PermissionRequireds { get; }
    }
}
