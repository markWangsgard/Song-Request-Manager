using DotNetEnv;
using Sprache;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading;

public static class PlaylistManager
{
    public static Dictionary<string, Admin> Admins = new();
    public static Dictionary<string, List<SongData>> RequestedSongs { get; set; } = new Dictionary<string, List<SongData>>();
    // public static Dictionary<string, Settings> UserSettings = new();
    public static Settings settings = new Settings();
    public static List<SongData> AllSongs { get; set; } = new();
    public static CancellationTokenSource autoAddCancellation { get; private set; } = new CancellationTokenSource();
    // Async-friendly signal to block the background task while auto-add is disabled.
    private static SemaphoreSlim autoAddSemaphore = new(0, 1);
    private static readonly object autoAddLock = new();

    static PlaylistManager()
    {
        try { autoAddSemaphore = new SemaphoreSlim(settings.autoAdd ? 1 : 0, 1); } catch { autoAddSemaphore = new SemaphoreSlim(0, 1); }
    }

    public static void SetMasterAdmin(string deviceId)
    {
        if (Admins.ContainsKey(deviceId))
        {
            settings.masterAdminId = deviceId;
        }
    }

    public static void CancelAndResetAutoAdd()
    {
        lock (autoAddLock)
        {
            try { autoAddCancellation?.Cancel(); } catch { }
            try { autoAddCancellation?.Dispose(); } catch { }
            autoAddCancellation = new CancellationTokenSource();
        }
    }

    // Call this when settings.autoAdd changes to unblock or block the background worker.
    public static void SignalAutoAddEnabled(bool enabled)
    {
        if (enabled)
        {
            if (autoAddSemaphore.CurrentCount == 0)
            {
                try { autoAddSemaphore.Release(); } catch { }
            }
        }
        else
        {
            // Ensure semaphore count is zero
            while (autoAddSemaphore.CurrentCount > 0)
            {
                autoAddSemaphore.Wait(0);
            }
        }
    }

    public static int getSongRequestCount(string songID)
    {
        int count = 0;
        foreach (var user in RequestedSongs)
        {
            count += user.Value.Count(s => s.id == songID);
        }
        return count;
    }

    public static async Task autoAddFunction(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!settings.autoAdd)
            {
                try
                {
                    await autoAddSemaphore.WaitAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    continue;
                }
            }

            if (settings.autoAdd)
            {
                var now = DateTime.Now;
                List<int> daysSelected = settings.selectedDays
                    .Where(d => d.Value).Select(d => (int)Enum.Parse<DayOfWeek>(d.Key, true))
                    .ToList();
                // Validate autoAddTime before using it
                if (!TimeSpan.TryParse(settings.autoAddTime, out var parsedAutoAddTime))
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        if (stoppingToken.IsCancellationRequested) break;
                    }
                    continue;
                }

                int nextDayToRun = daysSelected
                    .Select(d => (d - (int)now.DayOfWeek + 7) % 7) // gets days until next selected day
                    .Where(d => d != 0 || now.TimeOfDay < parsedAutoAddTime) // exclude today if time has passed
                    .DefaultIfEmpty(7) // if no days selected, default to one week later
                    .Min(); // get the minimum days until next run

                var nextRunDate = now.Date.AddDays(nextDayToRun).Add(parsedAutoAddTime);
                var timeUntilRun = nextRunDate - now;

                try
                {
                    using var linked = CancellationTokenSource.CreateLinkedTokenSource(autoAddCancellation.Token, stoppingToken);
                    await Task.Delay(timeUntilRun, linked.Token);
                }
                catch (OperationCanceledException)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        // Host is shutting down - exit loop
                        break;
                    }
                    // Settings changed, restart the loop to recalculate timing
                    continue;
                }
                List<string> selectedDays = settings.selectedDays.Where(d => d.Value).Select(d => d.Key).ToList();
                if (selectedDays.Any(d => string.Equals(d, DateTime.Now.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    var topSongs = AllSongs.Take(settings.autoAddQuantity).ToList();
                    if (topSongs.Any() && settings.currentPlaylist != null)
                    {
                        string admin = Admins.FirstOrDefault(u => u.Value != null).Key;
                        if (string.IsNullOrEmpty(admin))
                        {
                            Console.WriteLine("No admin user available to perform auto-add; skipping.");
                        }
                        else
                        {
                            for (int i = 0; i < topSongs.Count; i++)
                            {
                                // Add topSongs to current playlist
                                await APIManager.addSongToPlaylistAsync(settings.currentPlaylist.Id, admin, topSongs[i].id);
                            }
                            Console.WriteLine("added songs automatically");
                        }

                    }
                }
            }
        }
    }
}
