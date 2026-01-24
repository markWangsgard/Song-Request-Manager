public class Settings
{
    List<string> acceptableEmails = new() { "mwangsgard25@gmail.com", "millermyers0@gmail.com", "masonjhansen2@gmail.com", "connorheadman88@gmail.com", "ethanhintze011@gmail.com", "mw3dprinting@outlook.com" };
    public string masterAdminId { get; set; }
    public PlaylistData currentPlaylist { get; set; }
    public int numbOfAllowedRequests { get; set; } = 3;
    public bool allowRepeats { get; set; } = true;
    public bool autoAdd { get; set; } = false;
    public int autoAddQuantity { get; set; } = 2;
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
    public string autoAddTime { get; set; } = "22:15";
}
