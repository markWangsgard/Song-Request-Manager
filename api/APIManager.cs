using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public static class APIManager
{
    public static HttpClient client = new HttpClient();
    public static string accessToken = "";
    public static DateTime accessTokenExpiresAt = DateTime.MinValue;
    public static async Task<string> AccessToken()
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

    private static Dictionary<string, SongData> songCache {get; set;} = new();
    public static async Task<SongData?> GetSong(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        if (songCache.ContainsKey(id))
        {
            return songCache[id];
        }

        accessToken = await AccessToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.GetAsync($"https://api.spotify.com/v1/tracks/{id}/");
        Console.WriteLine(response);
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

    public static SongData? filterSongData(JsonElement jsonObject)
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

        if (!songCache.ContainsKey(songData.id))
        {
            songCache.Add(songData.id, songData);
        }
        return songData;
    }
    public static async Task<IResult> addSongToPlaylistAsync(string playlistId, string user, string songId)
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
        if (PlaylistManager.Admins.ContainsKey(user))
        {
            token = PlaylistManager.Admins[PlaylistManager.settings.masterAdminId].userAccessToken;
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

        return Results.Content(responseBody, "application/json");

    }
}