using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Theming
{
    public class ThemeModuleViewLocationExpander : IViewLocationExpander
    {
        private const string KeyTheme = "theme";
        private const string KeyModule = "module";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (viewLocations == null)
            {
                throw new ArgumentNullException(nameof(viewLocations));
            }

            context.Values.TryGetValue(KeyTheme, out string theme);
            context.Values.TryGetValue(KeyModule, out string module);

            if (!string.IsNullOrEmpty(theme) && !string.IsNullOrEmpty(module))
            {
                var locations = new List<string>();
                foreach (var location in viewLocations)
                {
                    locations.Add(location);
                    locations.Add(location.Replace("Views", $"Modules/{module}/Themes/{theme}"));
                }
                return locations;
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //var appSettings = context.ActionContext.HttpContext.RequestServices.GetService(typeof(IOptions<AppSettings>)) as IOptions<AppSettings>;
            //context.Values[ValueKey] = appSettings.Value.Theme;
            context.Values[KeyTheme] = "Dark";

            var module = GetModuleName(context);
            if (module != null)
                context.Values[KeyModule] = module;
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
