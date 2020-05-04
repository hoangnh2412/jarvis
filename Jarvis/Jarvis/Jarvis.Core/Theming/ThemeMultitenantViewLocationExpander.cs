using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Core.Multitenant;
using Jarvis.Core.Services;
using System.Threading.Tasks;

namespace Jarvis.Core.Theming
{
    public class ThemeMultitenantViewLocationExpander : IViewLocationExpander
    {
        private const string KeyTheme = "theme";
        private const string KeyModule = "module";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (viewLocations == null)
                throw new ArgumentNullException(nameof(viewLocations));
            
            context.Values.TryGetValue(KeyModule, out string module);
            context.Values.TryGetValue(KeyTheme, out string theme);

            ////Ignore theme for Administrator page
            //if (context.ActionContext.HttpContext.Request.Path.Value.StartsWith("/admin"))
            //    theme = null;

            var locations = new List<string>();
            foreach (var location in viewLocations)
            {
                //Without theme
                if (string.IsNullOrEmpty(theme))
                {
                    //Without module
                    if (string.IsNullOrEmpty(module))
                    {
                        locations.Add(location);
                    }
                    else
                    {
                        locations.Add($"Modules/{module}{location}");
                    }
                }
                else
                {
                    //Without module
                    if (string.IsNullOrEmpty(module))
                    {
                        locations.Add(location.Replace("Views", $"Themes/{theme}"));
                    }
                    else
                    {
                        locations.Add(location.Replace("Views", $"Modules/{module}/Themes/{theme}"));
                    }
                }

                //Default location should be here second, because ASP.NET MVC use view found before
                //locations.Add(location);
            }
            return locations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            
            var workContext = context.ActionContext.HttpContext.RequestServices.GetService<IWorkContext>();
            var tenant = workContext.GetCurrentTenantAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (tenant == null)
                throw new ArgumentNullException(nameof(tenant));
            
            var module = GetModuleName(context);
            if (module != null)
                context.Values[KeyModule] = module;

            context.Values[KeyTheme] = tenant.Theme;
        }

        private string GetModuleName(ViewLocationExpanderContext context)
        {
            var display = context.ActionContext.ActionDescriptor.DisplayName;
            var start = display.IndexOf("(Module.");
            var end = display.IndexOf(')');

            if (start == -1)
                return null;

            var name = display.Substring(start + 8, end - start - 8);
            return name;
        }
    }
}
