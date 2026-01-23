public class Admin
{
    public string displayName { get; set; }
    public string email { get; set; }
    public string userAccessToken { get; set; }
    public string userRefreshToken { get; set; }
    public DateTimeOffset accessTokenExpiresAt { get; set; }
}
