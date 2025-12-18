public class Settings
{
    List<string> acceptableEmails = new() { "mwangsgard25@gmail.com" };
    public PlaylistData currentPlaylist { get; set; }
    public int numbOfAllowedRequests { get; set; } = 3;
    public bool allowRepeats { get; set; } = true;
    public bool autoAdd { get; set; } = true;
    public int autoAddQuantity { get; set; } = 3;
    public Dictionary<string, bool> selectedDays { get; set; } = new()
    {
        {"monday", false},
        {"tuesday", false},
        {"wednesday", true},
        {"thursday", false},
        {"friday", false},
        {"saturday", false},
        {"sunday", false},
    };
    public string autoAddTime { get; set; } = "";
}
