using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class AutoAddService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run the PlaylistManager auto-add loop and let it observe the host cancellation token
        return PlaylistManager.autoAddFunction(stoppingToken);
    }
}
