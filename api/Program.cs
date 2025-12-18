using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DotNetEnv;
using Sprache;
using System.Text.Json.Nodes;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls("http://127.0.0.1:5001");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors();
builder.Services.AddSignalR();


var app = builder.Build();
app.UseCors(x => x.AllowAnyHeader().WithOrigins("http://127.0.0.1:5500", "https://markwangsgard.github.io").AllowAnyMethod().AllowCredentials());
app.MapHub<SongRequestManager>("/songRequestManager");

var client = new HttpClient();
var hub = new SongRequestManager();
string accessToken = "";
DateTime accessTokenExpiresAt = DateTime.MinValue;
var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(25));



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


async Task RefreshAccessTokenPeriodically()
{
    while (await periodicTimer.WaitForNextTickAsync())
    {
        await AccessToken();
        Console.WriteLine("Refreshing Access Tokens... " + DateTime.Now);
        var refreshTasks = PlaylistManager.Users.Keys.Select(user => RefreshAccessToken(user));

        await Task.WhenAll(refreshTasks);
    }
}

async Task RefreshAccessToken(string user)
{
    if (!PlaylistManager.Users.ContainsKey(user) || PlaylistManager.Users[user] == null)
    {
        return;
    }

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
        new KeyValuePair<string, string>("refresh_token", PlaylistManager.Users[user].userRefreshToken),
        new KeyValuePair<string, string>("client_id", clientId)
    });
    var response = await client.SendAsync(request);
    var JsonObjectResponse = JsonSerializer.Deserialize<JsonObject>(await response.Content.ReadAsStringAsync());
    JsonObjectResponse.TryGetPropertyValue("access_token", out JsonNode jsonNode);
    PlaylistManager.Users[user].userAccessToken = jsonNode.ToString();
}
async Task removeSongAsync(string user, string songId)
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
    await hub.SendSongRequestUpdate();
}


app.MapGet("/", async () =>
{
    accessToken = await AccessToken();
    return $"Spotify API Proxy is running. Access Token: {accessToken}";
});

app.MapGet("/login/{user}", (string user, string returnTo = "https://song-request-manager.onrender.com/me") =>
{
    var redirectUri = Uri.EscapeDataString(Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI"));
    string returnToUri = Uri.EscapeDataString(returnTo);
    State tempState = new(user, returnToUri);
    string state = JsonSerializer.Serialize(tempState);

    var ClientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");

    var url = $"https://accounts.spotify.com/authorize" +
              $"?client_id={ClientId}" +
              $"&response_type=code" +
              $"&redirect_uri={redirectUri}" +
              $"&state={state}" +
              $"&scope=user-read-private%20user-read-email%20playlist-modify-public%20playlist-modify-private%20playlist-read-private%20playlist-read-collaborative" +
              $"&show_dialog=true";

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

    var userAccessToken = doc.RootElement.GetProperty("access_token").GetString() ?? "";
    var userRefreshToken = doc.RootElement.GetProperty("refresh_token").GetString() ?? "";

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);

    var responseMe = client.GetAsync("https://api.spotify.com/v1/me/");
    var responseMessage = await responseMe;

    State currentState = JsonSerializer.Deserialize<State>(state);

    if (responseMessage.StatusCode.ToString() == "OK")
    {
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

        Admin newUser = new()
        {
            displayName = profileElement.GetProperty("display_name").GetString() ?? "",
            email = profileElement.GetProperty("email").GetString() ?? "",
            userAccessToken = userAccessToken,
            userRefreshToken = userRefreshToken,
            accessTokenExpiresAt = expirationTime
        };

        if (PlaylistManager.Users.ContainsKey(currentState.User))
        {
            PlaylistManager.Users[currentState.User] = newUser;
        }
        else
        {
            PlaylistManager.Users.Add(currentState.User, newUser);
        }

        RefreshAccessTokenPeriodically();

    }
    return Results.Redirect(currentState.ReturnTo);
});

app.MapGet("/logout/{user}", async (string user) =>
{
    PlaylistManager.Users[user] = null;
    var response = await client.GetAsync("https://accounts.spotify.com/en/logout ");
});

app.MapGet("/me/{user}", async (string user = "") =>
{
    if (PlaylistManager.Users.ContainsKey(user))
    {
        return Results.Content(JsonSerializer.Serialize(PlaylistManager.Users[user]), "application/json");
    }
    return Results.Json(new { error = "User not found" });
});

app.MapGet("/me/{user}/playlists", async (string user) =>
{
    if (!PlaylistManager.Users.ContainsKey(user))
    {
        return Results.Json(new { error = "User not found" });
    }

    await AccessToken();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PlaylistManager.Users[user].userAccessToken);

    var response = await client.GetAsync("https://api.spotify.com/v1/me/playlists");
    var JsonObjectResponse = JsonSerializer.Deserialize<JsonObject>(await response.Content.ReadAsStringAsync());

    // JsonObjectResponse.TryGetPropertyValue("items", out JsonObject playlists);
    var playlists = (JsonArray)JsonObjectResponse["items"];

    List<PlaylistData> playlistDatas = new();


    foreach (JsonObject playlist in playlists)
    {
        PlaylistData currentPlaylist = new();

        playlist.TryGetPropertyValue("id", out JsonNode newId);
        currentPlaylist.Id = newId.ToString();
        playlist.TryGetPropertyValue("name", out JsonNode newName);
        currentPlaylist.Name = newName.ToString();
        playlist.TryGetPropertyValue("images", out JsonNode newImages);
        JsonObject image;
        if (newImages != null)
        {
            image = (JsonObject)newImages[newImages.AsArray().Count() - 1];

            image.TryGetPropertyValue("url", out JsonNode imgUrl);
            currentPlaylist.ImgUrl = imgUrl.ToString();
        }

        playlistDatas.Add(currentPlaylist);
    }

    return Results.Content(JsonSerializer.Serialize(playlistDatas), "application/json");
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

    await hub.SendSongRequestUpdate();

    return Results.Json(new { songID, requests = PlaylistManager.getSongRequestCount(songID) });
});

app.MapGet("/remove-song/{user}/{songId}", (string user, string songId) =>
{

    removeSongAsync(user, songId);

    return Task.FromResult(Results.Json(new { user, status = "removed" }));
});


app.MapGet("/clear-requests", async () =>
{
    PlaylistManager.RequestedSongs.Clear();
    PlaylistManager.AllSongs.Clear();
    
    await hub.SendSongRequestUpdate();

    return Results.Json(new { status = "cleared" });
});

app.MapGet("/playlist/{playlistId}/{user}/add-song/{songId}", async (string playlistId, string user, string songId) =>
{
    var song = await GetSong(songId);
    string trackUri;
    if (song != null)
    {
        trackUri = song.uri;
    }
    else
    {
        return Results.Json(new { statusCode = "Song Not found" });
    }

    string token;
    if (PlaylistManager.Users.ContainsKey(user))
    {
        token = PlaylistManager.Users[user].userAccessToken;
    }
    else
    {
        return Results.Json(new { statusCode = "User Not found" });
    }


    using var client = new HttpClient();

    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

    var url = $"https://api.spotify.com/v1/playlists/{playlistId}/tracks";

    var json = $"{{\"uris\": [\"{trackUri}\"]}}";
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await client.PostAsync(url, content);
    string responseBody = await response.Content.ReadAsStringAsync();

    Console.WriteLine("Response: " + responseBody);

    return Results.Content(responseBody, "application/json");
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

app.MapPost("/store-settings/{user}", async (string user, Settings settings) =>
{
    if (PlaylistManager.Users.ContainsKey(user) && PlaylistManager.Users[user] != null)
    {
        bool limitDecreased = PlaylistManager.settings.numbOfAllowedRequests > settings.numbOfAllowedRequests;
        bool allowRepeatsChanged = PlaylistManager.settings.allowRepeats != settings.allowRepeats;

        PlaylistManager.settings.currentPlaylist = settings.currentPlaylist;
        PlaylistManager.settings.numbOfAllowedRequests = settings.numbOfAllowedRequests;
        PlaylistManager.settings.allowRepeats = settings.allowRepeats;
        PlaylistManager.settings.autoAdd = settings.autoAdd;

        foreach (var day in settings.selectedDays)
        {
            PlaylistManager.settings.selectedDays[day.Key] = day.Value;
        }

        PlaylistManager.settings.autoAddTime = settings.autoAddTime;

        if (limitDecreased || allowRepeatsChanged)
        {
            foreach (var newUser in PlaylistManager.Users.Keys)
            {
                if (PlaylistManager.RequestedSongs.ContainsKey(newUser))
                {
                    // var requestedSongs = PlaylistManager.RequestedSongs[newUser];
                    if (limitDecreased)
                    {
                        while (PlaylistManager.RequestedSongs[newUser].Count > settings.numbOfAllowedRequests)
                        {
                            string songId = PlaylistManager.RequestedSongs[newUser][PlaylistManager.RequestedSongs[newUser].Count - 1].id;
                            removeSongAsync(newUser, songId);
                        }
                    }

                    if (allowRepeatsChanged && !settings.allowRepeats)
                    {
                        var uniqueSongs = PlaylistManager.RequestedSongs[newUser].DistinctBy(s => s.id).ToList();
                        PlaylistManager.RequestedSongs[newUser] = uniqueSongs;
                    }

                }
            }
        }

        await hub.SendSongRequestUpdate();

        return Results.Ok(PlaylistManager.settings);
    }
    return Results.Unauthorized();
});
app.MapGet("/get-settings/", () =>
{
    return PlaylistManager.settings;
});

app.Run();
