namespace Sample.EventBus;

public class SampleHostedService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
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

        Console.WriteLine($"TotalViews: {totalViews}");
        Console.WriteLine($"Check: {totalViews == brands.Sum(x => x.Views)}");

        foreach (var brand in brands)
        {
            Console.WriteLine($"Brand: {brand.Name} | Spot: {brand.Spot} | Percentage: {brand.Percentage * 100}% | Views: {brand.Views}");
        }

        return Task.CompletedTask;
    }

    private List<Brand> Calc(Dictionary<string, int> brands, int totalViews, int weight = 10)
    {
        var result = new List<Brand>();

        var totalSpot = brands.Sum(x => x.Value);
        foreach (var brand in brands)
        {
            var element = new Brand
            {
                Name = brand.Key,
                Spot = brand.Value,
            };

            element.Percentage = Change((double)brand.Value / (double)totalSpot, weight);
            element.Views = (int)Math.Round(totalViews * element.Percentage, 0);

            result.Add(element);
        }

        // Double check
        var total = result.Sum(x => x.Views);
        if (total == totalViews)
            return result;

        var item = PickBrand(result);
        item.Views = item.Views + (totalViews - total);

        return result;
    }

    private Brand PickBrand(List<Brand> brands)
    {
        var index = new Random().Next(0, brands.Count);
        return brands[index];
    }

    // Increment or decrement percentage with range 10%
    private static double Change(double value, int weight)
    {
        // Range 10%
        var percent = (value * weight / 100) * new Random().Next(-1, 2);
        if (percent == 0)
            return value;

        return (value * percent) + value;
    }

    private class Brand
    {
        public string Name { get; set; }
        public int Spot { get; set; }
        public double Percentage { get; set; }
        public int Views { get; set; }
    }
}