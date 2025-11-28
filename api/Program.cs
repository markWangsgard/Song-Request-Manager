using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using DotNetEnv;
using Sprache;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System;
using Microsoft.AspNetCore.Mvc;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:5001");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors();


var app = builder.Build();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

var client = new HttpClient();
string accessToken = "";
DateTime accessTokenExpiresAt = DateTime.MinValue;
string userAccessToken = "";
string userRefreshToken = "";
var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(55));


async Task<string> AccessToken()
{
    // Request a new token if missing or expired (with a small buffer)
    if (string.IsNullOrWhiteSpace(accessToken) || DateTime.UtcNow >= accessTokenExpiresAt.AddSeconds(-30))
    {
        try
        {
            var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            tokenRequest.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await client.SendAsync(tokenRequest);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                // clear on failure
                accessToken = "";
                accessTokenExpiresAt = DateTime.MinValue;
                return "";
            }

            var doc = JsonDocument.Parse(content);
            accessToken = doc.RootElement.GetProperty("access_token").GetString() ?? "";

            // set expiry based on expires_in if provided
            if (doc.RootElement.TryGetProperty("expires_in", out JsonElement expiresElem) && expiresElem.ValueKind == JsonValueKind.Number)
            {
                var expiresIn = expiresElem.GetInt32();
                accessTokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            }
            else
            {
                // default to 1 hour if server doesn't provide expires_in
                accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(60);
            }

            return accessToken;
        }
        catch
        {
            accessToken = "";
            accessTokenExpiresAt = DateTime.MinValue;
            return "";
        }
    }
    else
    {
        return accessToken;
    }
}


SongData? filterSongData(JsonElement jsonObject)
{
    string id = "";
    if (jsonObject.TryGetProperty("id", out JsonElement idElem) && idElem.ValueKind != JsonValueKind.Null)
    {
        id = idElem.GetString() ?? "";
    }

    if (string.IsNullOrEmpty(id))
    {
        // missing id -> treat as invalid
        return null;
    }

    string trackName = "Unknown";
    if (jsonObject.TryGetProperty("name", out JsonElement nameElem) && nameElem.ValueKind != JsonValueKind.Null)
    {
        trackName = nameElem.GetString() ?? "Unknown";
    }

    string artistName = "Unknown Artist";
    if (jsonObject.TryGetProperty("artists", out JsonElement artistsElem) && artistsElem.ValueKind == JsonValueKind.Array)
    {
        var enumerator = artistsElem.EnumerateArray();
        if (enumerator.MoveNext())
        {
            var firstArtist = enumerator.Current;
            if (firstArtist.TryGetProperty("name", out JsonElement artistNameElem) && artistNameElem.ValueKind != JsonValueKind.Null)
            {
                artistName = artistNameElem.GetString() ?? "Unknown Artist";
            }
        }
    }

    string imgURL = "";
    if (jsonObject.TryGetProperty("album", out JsonElement albumElem) && albumElem.ValueKind == JsonValueKind.Object)
    {
        if (albumElem.TryGetProperty("images", out JsonElement imagesElem) && imagesElem.ValueKind == JsonValueKind.Array)
        {
            int idx = 0;
            foreach (var img in imagesElem.EnumerateArray())
            {
                if (idx == 2)
                {
                    if (img.TryGetProperty("url", out JsonElement urlElem) && urlElem.ValueKind != JsonValueKind.Null)
                    {
                        imgURL = urlElem.GetString() ?? "";
                    }
                    break;
                }
                idx++;
            }

            // fallback to the first image if index 2 doesn't exist
            if (string.IsNullOrEmpty(imgURL))
            {
                var enumerator2 = imagesElem.EnumerateArray();
                if (enumerator2.MoveNext())
                {
                    var firstImg = enumerator2.Current;
                    if (firstImg.TryGetProperty("url", out JsonElement firstUrlElem) && firstUrlElem.ValueKind != JsonValueKind.Null)
                    {
                        imgURL = firstUrlElem.GetString() ?? "";
                    }
                }
            }
        }
    }

    string Uri = "Unknown";
    if (jsonObject.TryGetProperty("uri", out JsonElement uriElem) && uriElem.ValueKind != JsonValueKind.Null)
    {
        Uri = uriElem.GetString() ?? "Unknown";
    }

    SongData songData = new()
    {
        id = id,
        trackName = trackName,
        artistName = artistName,
        imgURL = imgURL,
        uri = Uri,
        timeRequested = DateTime.Now
    };
    return songData;
}

async Task<SongData?> GetSong(string id)
{
    if (string.IsNullOrWhiteSpace(id))
    {
        return null;
    }

    accessToken = await AccessToken();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    using var response = await client.GetAsync($"https://api.spotify.com/v1/tracks/{id}/");
    if (!response.IsSuccessStatusCode)
    {
        return null;
    }

    var content = await response.Content.ReadAsStringAsync();
    var jsonObject = JsonSerializer.Deserialize<JsonElement>(content);

    // ensure parsed object actually has an id
    if (!jsonObject.TryGetProperty("id", out JsonElement idElem) || idElem.ValueKind == JsonValueKind.Null)
    {
        return null;
    }

    SongData? songData = filterSongData(jsonObject);
    return songData;
}


async Task RefreshAccessTokenPerodiclly()
{
    while (await periodicTimer.WaitForNextTickAsync())
    {
        var client = new HttpClient();
        // ACCEPT header
        client.DefaultRequestHeaders.Accept.Add(
             new MediaTypeWithQualityHeaderValue("application/json"));
        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        // CONTENT-TYPE header
        request.Content = new StringContent("{\"name\":\"John Doe\",\"age\":33}",
                             Encoding.UTF8, "application/json");
        var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

        request.Content = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("grant_type", "refresh_token"),
        new KeyValuePair<string, string>("refresh_token", PlaylistManager.settings.currentUser.userRefreshToken),
        new KeyValuePair<string, string>("client_id", clientId)
    });
        var response = await client.SendAsync(request);
        var JsonObjectResponse = JsonSerializer.Deserialize<JsonObject>(await response.Content.ReadAsStringAsync());
        JsonObjectResponse.TryGetPropertyValue("access_token", out JsonNode jsonNode);
        PlaylistManager.settings.currentUser.userAccessToken = jsonNode.ToString();
    }
}


app.MapGet("/", async () =>
{
    accessToken = await AccessToken();
    return $"Spotify API Proxy is running. Access Token: {accessToken}";
});

app.MapGet("/login", (string returnTo = "http://127.0.0.1:5001/me") =>
{
    var redirectUri = Uri.EscapeDataString(Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI"));
    var state = Uri.EscapeDataString(returnTo);

    var ClientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");

    var url = $"https://accounts.spotify.com/authorize" +
              $"?client_id={ClientId}" +
              $"&response_type=code" +
              $"&redirect_uri={redirectUri}" +
              $"&state={state}" +
              $"&scope=user-read-private%20user-read-email%20playlist-modify-public%20playlist-modify-private%20playlist-read-private%20playlist-read-collaborative";

    return Results.Redirect(url);
});

app.MapGet("/callback", async (string code, string state) =>
{
    var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
    var redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI");

    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
    var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
    tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
    tokenRequest.Content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("redirect_uri", redirectUri ?? "")
    });

    var response = await client.SendAsync(tokenRequest);
    var content = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
    {
        return Results.Content(content, "application/json");
    }

    var doc = JsonDocument.Parse(content);
    userAccessToken = doc.RootElement.GetProperty("access_token").GetString() ?? "";
    userRefreshToken = doc.RootElement.GetProperty("refresh_token").GetString() ?? "";

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

    var responseMe = client.GetAsync("https://api.spotify.com/v1/me/");
    var responseMessage = await responseMe;
    var profileContent = await responseMessage.Content.ReadAsStringAsync();
    var profileElement = JsonSerializer.Deserialize<JsonElement>(profileContent);

    DateTime expirationTime;
    if (profileElement.TryGetProperty("expires_in", out JsonElement expiresElem) && expiresElem.ValueKind == JsonValueKind.Number)
    {
        var expiresIn = expiresElem.GetInt32();
        expirationTime = DateTime.UtcNow.AddSeconds(expiresIn);
    }
    else
    {
        // default to 1 hour if server doesn't provide expires_in
        expirationTime = DateTime.UtcNow.AddMinutes(60);
    }

    User newUser = new()
    {
        displayName = profileElement.GetProperty("display_name").GetString() ?? "",
        email = profileElement.GetProperty("email").GetString() ?? "",
        userAccessToken = userAccessToken,
        userRefreshToken = userRefreshToken,
    };

    PlaylistManager.settings.currentUser = newUser;

    RefreshAccessTokenPerodiclly();

    return Results.Redirect(state);
});

app.MapGet("/me", async () =>
{
    return PlaylistManager.settings.currentUser;
});

app.MapGet("/me/playlists", async () =>
{
    if (PlaylistManager.settings.currentUser.userAccessToken == null || PlaylistManager.settings.currentUser.userAccessToken == "")
    {
        await AccessToken();
    }

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

    var response = client.GetAsync("https://api.spotify.com/v1/me/playlists");
    var responseMessage = await response;
    var content = response.Result.Content.ReadAsStringAsync().Result;
    var doc = JsonDocument.Parse(content);
    var playlists = doc.RootElement.GetProperty("items");

    List<PlaylistData> playlistDatas = new();
    foreach (var playlist in playlists.EnumerateArray())
    {
        PlaylistData currentPlaylist = new();
        currentPlaylist.Id = playlist.GetProperty("id").GetString() ?? "";
        currentPlaylist.Name = playlist.GetProperty("name").GetString() ?? "";
        var images = playlist.GetProperty("images");
        // Console.WriteLine(images);
        var image = images[images.GetArrayLength() - 1];
        currentPlaylist.ImgUrl = image.GetProperty("url").GetString() ?? "";

        playlistDatas.Add(currentPlaylist);
    }

    return playlistDatas;
});







app.MapGet("/song/{songID}", async (HttpContext http) =>
{
    var songID = http.Request.RouteValues["songID"]?.ToString() ?? "";
    var song = await GetSong(songID);
    if (song == null)
    {
        return Results.Json(new { error = "Song not found" });
    }
    return Results.Json(song);
});

app.MapGet("/artist/{artistID}", async (string artistID) =>
{
    accessToken = await AccessToken();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = client.GetAsync($"https://api.spotify.com/v1/artists/{artistID}/");
    var content = response.Result.Content.ReadAsStringAsync().Result;

    return Results.Content(content, "application/json");
});

app.MapGet("/search/{query}", async (string query) =>
{
    accessToken = await AccessToken();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    using var response = await client.GetAsync($"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=10");
    if (!response.IsSuccessStatusCode)
        return Results.Json(new { error = "search failed", status = response.StatusCode });

    var content = await response.Content.ReadAsStringAsync();
    var jsonObject = JsonSerializer.Deserialize<JsonElement>(content);
    var jsonObjectTracks = jsonObject.GetProperty("tracks").GetProperty("items");
    var listOfTracks = new List<SongData>();
    foreach (var songElem in jsonObjectTracks.EnumerateArray())
    {
        var sd = filterSongData(songElem);
        if (sd != null) listOfTracks.Add(sd);
    }
    return Results.Json(listOfTracks);
});










app.MapGet("/request-song/{user}/{songID}", async (string user, string songID) =>
{
    SongData song;
    var songData = await GetSong(songID);
    if (songData == null)
    {
        return Results.Json(new { error = "Song not found" });
    }
    song = songData;
    lock (PlaylistManager.RequestedSongs)
    {
        if (PlaylistManager.RequestedSongs.ContainsKey(user))
        {
            PlaylistManager.RequestedSongs[user].Add(song);
        }
        else
        {
            PlaylistManager.RequestedSongs.Add(user, new() { song });

        }
    }
    lock (PlaylistManager.AllSongs)
    {
        if (PlaylistManager.AllSongs.Where(s => s.id == songID).Count() == 0)
        {
            PlaylistManager.AllSongs.Add(song);
        }
    }

    return Results.Json(new { songID, requests = PlaylistManager.getSongRequestCount(songID) });
});

app.MapGet("/remove-song/{user}/{songId}", (string user, string songId) =>
{

    int index;
    SongData song;

    song = PlaylistManager.RequestedSongs[user].Find(s => s.id == songId);
    PlaylistManager.RequestedSongs[user].Remove(song);

    if (PlaylistManager.RequestedSongs[user].Count() <= 0)
    {
        PlaylistManager.RequestedSongs.Remove(user);
    }

    if (PlaylistManager.getSongRequestCount(songId) == 0)
    {
        index = PlaylistManager.AllSongs.FindIndex(s => s.id == songId);
        PlaylistManager.AllSongs.RemoveAt(index);
    }

    return Task.FromResult(Results.Json(new { user, status = "removed" }));
});

app.MapGet("/clear-requests", () =>
{
    PlaylistManager.RequestedSongs.Clear();
    PlaylistManager.AllSongs.Clear();
    return Results.Json(new { status = "cleared" });
});

app.MapGet("/playlist/{playlistId}/add-song/{songId}", async (string playlistId, string songId) =>
{
    var song = await GetSong(songId);
    var trackUri = song.uri;
    var token = PlaylistManager.settings.currentUser.userAccessToken;

    using var client = new HttpClient();

    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

    var url = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks";

    var json = $"{{\"uris\": [\"{trackUri}\"]}}";
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync(url, content);
    string responseBody = await response.Content.ReadAsStringAsync();

    // Console.WriteLine("Response: " + responseBody);

    return responseBody;
});








app.MapGet("/get-user-requests/{user}", (string user) =>
{

    if (!PlaylistManager.RequestedSongs.ContainsKey(user))
    {
        return Results.Json(new { statusCode = "User Not found" });
    }
    return Results.Json(PlaylistManager.RequestedSongs[user].OrderBy(s => s.timeRequested));
});

app.MapGet("/requested-songs", async () =>
{
    return PlaylistManager.AllSongs.OrderBy(s => s.requestCount).Reverse();
});

app.MapPost("/store-settings", (Settings settings) =>
{
    PlaylistManager.settings.currentUser = settings.currentUser;
    PlaylistManager.settings.currentPlaylist = settings.currentPlaylist;
    PlaylistManager.settings.numbOfAllowedRequests = settings.numbOfAllowedRequests;
    PlaylistManager.settings.allowRepeats = settings.allowRepeats;
    PlaylistManager.settings.autoAdd = settings.autoAdd;

    foreach (var day in settings.selectedDays)
    {
        PlaylistManager.settings.selectedDays[day.Key] = day.Value;
    }

    PlaylistManager.settings.autoAddTime = settings.autoAddTime;

    return Results.Ok(PlaylistManager.settings);
});
app.MapGet("/get-settings", () =>
{
    return PlaylistManager.settings;
});

app.Run();

public static class PlaylistManager
{
    public static Dictionary<string, List<SongData>> RequestedSongs { get; set; } = new Dictionary<string, List<SongData>>();
    // public static Dictionary<string, Settings> UserSettings = new();
    public static Settings settings = new Settings();
    public static List<SongData> AllSongs { get; set; } = new();
    public static int getSongRequestCount(string songID)
    {
        int count = 0;
        foreach (var user in RequestedSongs)
        {
            count += user.Value.Where(s => s.id == songID).Count();
        }
        return count;
    }
}

public class Settings
{
    public User currentUser { get; set; }
    public PlaylistData currentPlaylist { get; set; }
    public int numbOfAllowedRequests { get; set; } = 3;
    public bool allowRepeats { get; set; } = true;
    public bool autoAdd { get; set; } = false;
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

public class User
{
    public string displayName { get; set; }
    public string email { get; set; }
    public string userAccessToken { get; set; }
    public string userRefreshToken { get; set; }
    // public DateTime accessTokenExpiresAt { get; set; }
}

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

public class PlaylistData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ImgUrl { get; set; }
}