using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

[ApiController]
[Route("test")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet("calc")]
    public IActionResult Get()
    {
        var totalViews = 300;
        var brands = Calc(
            brands: new Dictionary<string, int> {
                { "BVPhuTho", 6 },
                { "Minvoice", 1 },
                { "BSL", 1 }
            },
            totalViews: totalViews
        );

        return Ok(new
        {
            TotalViews = totalViews,
            Brands = brands,
            Check = totalViews == brands.Sum(x => x.Views)
        });
    }

    private List<BrandTraffic> Calc(Dictionary<string, int> brands, int totalViews)
    {
        var result = new List<BrandTraffic>();

        var totalSpot = brands.Sum(x => x.Value);
        foreach (var brand in brands)
        {
            var item = new BrandTraffic
            {
                Name = brand.Key,
                TotalBanner = brand.Value,
            };

            // TODO: Random incre or decre percentage
            item.Percentage = (double)brand.Value / (double)totalSpot;
            item.Views = (int)Math.Round(totalViews * item.Percentage, 0);

            result.Add(item);
        }

        // Double check
        var total = result.Sum(x => x.Views);
        if (total == totalViews)
            return result;

        var offset = totalViews - total;

        // TODO: Random pick
        var last = result[brands.Count - 1];
        last.Views = last.Views + offset;

        return result;
    }

    private class BrandTraffic
    {
        public string Name { get; set; }
        public int TotalBanner { get; set; }
        public double Percentage { get; set; }
        public int Views { get; set; }
    }
}
