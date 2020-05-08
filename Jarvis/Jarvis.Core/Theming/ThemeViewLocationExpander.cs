using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarvis.Core.Theming
{
    public class ThemeViewLocationExpander : IViewLocationExpander
    {
        private const string KeyTheme = "theme";

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

            if (!string.IsNullOrEmpty(theme))
            {
                var locations = new List<string>();
                foreach (var location in viewLocations)
                {
                    locations.Add(location);
                    locations.Add(location.Replace("Views", $"/Themes/{theme}"));
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
        }
    }
}
