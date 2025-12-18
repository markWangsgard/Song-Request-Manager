public class SongData
{
    public string id { get; set; }
    public string trackName { get; set; }
    public string artistName { get; set; }
    public string imgURL { get; set; }
    public string uri { get; set; }
    public DateTime timeRequested { get; set; }
    public int requestCount => PlaylistManager.getSongRequestCount(id);
}
