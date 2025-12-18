public static class PlaylistManager
{
    public static Dictionary<string, Admin> Users = new();
    public static Dictionary<string, List<SongData>> RequestedSongs { get; set; } = new Dictionary<string, List<SongData>>();
    // public static Dictionary<string, Settings> UserSettings = new();
    public static Settings settings = new Settings();
    public static List<SongData> AllSongs { get; set; } = new();
    public static int getSongRequestCount(string songID)
    {
        int count = 0;
        foreach (var user in RequestedSongs)
        {
            count += user.Value.Count(s => s.id == songID);
        }
        return count;
    }
}
