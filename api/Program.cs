using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using DotNetEnv;
using Sprache;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls(Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors();


var app = builder.Build();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

var client = new HttpClient();
string accessToken = "";
string userAccessToken = "";
string userRefreshToken = "";

Dictionary<string, int> requestedSongs = new();

async Task<string> AccessToken()
{
    if (accessToken == "")
    {
        try
        {
            var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return "";
            }

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            // include client_id and client_secret in the form as a fallback for environments
            // where the Authorization header might be stripped
            tokenRequest.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

            var response = await client.SendAsync(tokenRequest);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return "";
            }

            var doc = JsonDocument.Parse(content);
            accessToken = doc.RootElement.GetProperty("access_token").GetString() ?? "";
            return accessToken;
        }
        catch
        {
            return "";
        }
    }
    else
    {
        return accessToken;
    }
}

async Task<SongData> GetSong(string id)
{
    accessToken = await AccessToken();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = client.GetAsync($"https://api.spotify.com/v1/tracks/{id}/");
    var content = response.Result.Content.ReadAsStringAsync().Result;
    var jsonObject = JsonSerializer.Deserialize<JsonElement>(content);
    SongData songData = filterSongData(jsonObject);
    return songData;
}

SongData filterSongData(JsonElement jsonObject)
{
    string id = jsonObject.GetProperty("id").GetString() ?? "";
    string trackName = jsonObject.GetProperty("name").GetString() ?? "Unknown";
    string artistName = jsonObject.GetProperty("artists")[0].GetProperty("name").GetString() ?? "Unknown Artist";
    string imgURL = jsonObject.GetProperty("album").GetProperty("images")[2].GetProperty("url").GetString() ?? "";
    SongData songData = new()
    {
        id = id,
        trackName = trackName,
        artistName = artistName,
        imgURL = imgURL,
    };
    return songData;
}


app.MapGet("/", async () =>
{
    accessToken = await AccessToken();
    return $"Spotify API Proxy is running. Access Token: {accessToken}";
});

app.MapGet("/artist/{artistID}", async (string artistID) =>
{
    accessToken = await AccessToken();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = client.GetAsync($"https://api.spotify.com/v1/artists/{artistID}/");
    var content = response.Result.Content.ReadAsStringAsync().Result;
    
    return Results.Content(content, "application/json");
});

app.MapGet("/song/{songID}", async (string songID) =>
{
    return await GetSong(songID);
});

app.MapGet("/search/{query}", async (string query) =>
{
    accessToken = await AccessToken();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = client.GetAsync($"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track&limit=10");
    var content = response.Result.Content.ReadAsStringAsync().Result;
    var jsonObject = JsonSerializer.Deserialize<JsonElement>(content);
    var jsonObjectTracks = jsonObject.GetProperty("tracks").GetProperty("items");
    var jsonListOfTracks = jsonObjectTracks.EnumerateArray().ToList();
    var listOfTracks = new List<SongData>();
    foreach (var song in jsonListOfTracks)
    {
        listOfTracks.Add(filterSongData(song));
    }
    return Results.Json(listOfTracks);
    // return Results.Content(content, "application/json");
});

app.MapGet("/login", () =>
{
    var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
    var redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI");
    var scope = "user-read-private user-read-email";
    var authUrl = $"https://accounts.spotify.com/authorize?response_type=code&client_id={clientId}&scope={Uri.EscapeDataString(scope)}&redirect_uri={Uri.EscapeDataString(redirectUri ?? "")}";
    return Results.Redirect(authUrl);
});

app.MapGet("/callback", async (string code) =>
{
    var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
    var redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI");

    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
    {
        return Results.Content(JsonSerializer.Serialize(new { error = "missing_client_credentials" }), "application/json");
    }

    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
    var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
    tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
    tokenRequest.Content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("redirect_uri", redirectUri ?? ""),
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret)
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
    return Results.Content(content, "application/json");
});


app.MapGet("/me", async () =>
{
    accessToken = await AccessToken();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

    var response = client.GetAsync("https://api.spotify.com/v1/me/");
    var responseMessage = await response;
    var profileContent = await responseMessage.Content.ReadAsStringAsync();
    var profileElement = JsonSerializer.Deserialize<JsonElement>(profileContent);
    var content = JsonSerializer.Serialize(new
    {
        profile = profileElement,
        userAccessToken,
        userRefreshToken
    });
    return Results.Content(content, "application/json");
});

app.MapGet("/request-song/{songID}", (string songID) =>
{
    if (requestedSongs.ContainsKey(songID))
    {
        requestedSongs[songID]++;
    }
    else
    {
        requestedSongs[songID] = 1;
    }
    return Results.Json(new { songID, requests = requestedSongs[songID] });
});

app.MapGet("/clear-requests", () =>
{
    requestedSongs.Clear();
    return Results.Json(new { status = "cleared" });
});

app.MapGet("/remove-song/{songID}", (string songID) =>
{
    if (requestedSongs.ContainsKey(songID))
    {
        requestedSongs[songID]--;
    }
    else
    {
        return Results.Json(new { status = "Unable to remove", songID });
    }
    if (requestedSongs[songID] <= 0)
    {
        requestedSongs.Remove(songID);
        return Results.Json(new { status = "removed", songID });
    }
    return Results.Json(new { songID, requests = requestedSongs[songID] });
});

app.MapGet("/requested-songs", async () =>
{
    List<SongDataWithCount> detailedRequestedSongs = new();
    foreach (var kv in requestedSongs)
    {
        var songData = await GetSong(kv.Key);
        detailedRequestedSongs.Add(new() { id = songData.id, trackName = songData.trackName, artistName = songData.artistName, imgURL = songData.imgURL, requestCount = kv.Value });

    }
    try
    {
        return Results.Json(detailedRequestedSongs.OrderByDescending(song => song.requestCount).ToList());
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message });
    }
});

app.MapGet("/debug/spotify-config", () => Results.Json(new {
    clientIdPresent = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID")),
    redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI")
}));

app.Run();

struct SongData
{
    public string id { get; set; }
    public string trackName { get; set; }
    public string artistName { get; set; }
    public string imgURL { get; set; }
}
struct SongDataWithCount
{
    public string id { get; set; }
    public string trackName { get; set; }
    public string artistName { get; set; }
    public string imgURL { get; set; }
    public int requestCount { get; set; }
}