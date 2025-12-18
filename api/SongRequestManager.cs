using Microsoft.AspNetCore.SignalR;

public class SongRequestManager : Hub
{
    public async Task SendSongRequestUpdate()
    {
        await Clients.All.SendAsync("ReceiveSongRequestUpdate");
    }
}
