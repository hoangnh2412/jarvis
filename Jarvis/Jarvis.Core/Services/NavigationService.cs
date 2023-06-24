using System.Collections.Generic;
using System.Linq;
using Infrastructure.Abstractions;
using Jarvis.Core.Constants;
using Jarvis.Core.Models;

namespace Jarvis.Core.Permissions
{
    public interface INavigationService
    {
        List<NavigationItem> GetNavigation(SessionModel session);
    }

    public class NavigationService : INavigationService
    {
        private readonly IEnumerable<INavigation> _navigations;

        public NavigationService(IEnumerable<INavigation> navigations)
        {
            _navigations = navigations;
        }

        public List<NavigationItem> GetNavigation(SessionModel session)
        {
            var items = new List<NavigationItem>();
            foreach (var item in _navigations)
            {
                var hasPermission = false;

                if (session.Type == UserType.SuperAdmin)
                {
                    hasPermission = true;
                }
                else if (session.Type == UserType.Admin)
                {
                    hasPermission = true;
                }
                else if (item.PermissionRequireds == null || item.PermissionRequireds.Length == 0)
                {
                    hasPermission = true;
                }
                else
                {
                    foreach (var permission in item.PermissionRequireds)
                    {
                        if (session.Claims.Values.SelectMany(x => x).Any(x => x == permission))
                            hasPermission = true;
                    }
                }

                if (hasPermission)
                {
                    items.Add(new NavigationItem
                    {
                        Code = item.Code,
                        Icon = item.Icon,
                        Name = item.Name,
                        Order = item.Order,
                        Url = item.Url
                    });
                }
            }

            items = items.OrderBy(x => x.Order).ToList();
            return items;
        }
    }
}
