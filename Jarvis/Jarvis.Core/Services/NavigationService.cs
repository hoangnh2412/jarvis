using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Abstractions;
using Jarvis.Core.Constants;
using Jarvis.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Core.Permissions
{
    public interface INavigationService
    {
        List<NavigationItem> GetNavigation(IServiceProvider serviceProvider, SessionModel session);
    }

    public class NavigationService : INavigationService
    {
        //private readonly IModuleManager _moduleManager;

        //public NavigationService(IModuleManager moduleManager)
        //{
        //    _moduleManager = moduleManager;
        //}

        public List<NavigationItem> GetNavigation(IServiceProvider serviceProvider, SessionModel session)
        {
            var moduleManager = serviceProvider.GetService<IModuleManager>();
            var items = new List<NavigationItem>();
            var instances = moduleManager.GetInstances<INavigation>();
            foreach (var item in instances)
            {
                var hasPermission = false;

                //Không cần quyền gì vẫn được sử dụng
                if (item.PermissionRequireds == null || item.PermissionRequireds.Length == 0)
                {
                    hasPermission = true;
                }
                else if (session.Claims.ContainsKey(nameof(SpecialPolicy.Special_DoEnything)))
                {
                    hasPermission = true;
                }else if (session.Claims.ContainsKey(nameof(SpecialPolicy.Special_TenantAdmin)) && ClaimOfTenantAdmin.Claims.Any(x => x.Type == item.PermissionRequireds.FirstOrDefault())){
                    hasPermission = true;
                }
                else
                {
                    foreach (var permission in item.PermissionRequireds)
                    {
                        if (session.Claims.ContainsKey(permission))
                        {
                            hasPermission = true;
                        }
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
