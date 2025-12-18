using Microsoft.AspNetCore.SignalR;

public class SongRequestManager : Hub
{
    public async Task SendSongRequestUpdate()
    {
        Console.WriteLine("Sending song request update to all clients.");
        await Clients.All.SendAsync("ReceiveSongRequestUpdate");
    }
}
