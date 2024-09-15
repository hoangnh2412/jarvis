using System.ComponentModel;

namespace Sample;

public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine(DateTime.Now.ToString());
            await Task.Delay(1000, stoppingToken);
        }
    }
}
