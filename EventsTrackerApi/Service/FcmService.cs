using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EventsTrackerApi.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace EventsTrackerApi.Service;

public class FcmService
{
    public readonly static String TOPIC = "reuniones";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FirebaseOptionsConfig _opts;

    public FcmService(IHttpClientFactory httpClientFactory, IOptions<FirebaseOptionsConfig> opts)
    {
        _httpClientFactory = httpClientFactory;
        _opts = opts.Value;
    }

    public async Task<string> GetAccessTokenAsync()
    {

        if (string.IsNullOrWhiteSpace(_opts.CredentialsPath))
            throw new InvalidOperationException("Firebase:CredentialsPath no configurado.");

        var credential = GoogleCredential
            .FromFile(_opts.CredentialsPath)
            .CreateScoped(_opts.CreateScope);

        var accessToken = await credential.UnderlyingCredential
            .GetAccessTokenForRequestAsync();

        return accessToken;
    }

    public async Task SendToDivaceTokenAsync(
        string deviceToken,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken ct = default
        )
    {
        var accessToken = await GetAccessTokenAsync();
        var url = $"https://fcm.googleapis.com/v1/projects/{_opts.ProjectId}/messages:send";

        var payload = new
        {
            message = new
            {
                token = deviceToken,
        //    notification = new { title, body }, SACARLO PARA Q FUNCIONE EL onMessageReceived DE FCM
                data
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var http = _httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var res = await http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        var resp = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"FCM v1 send failed: {(int)res.StatusCode} {res.StatusCode} - {resp}");
    }
    public async Task SendToTokenAsync(
        string deviceToken,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        var accessToken = await GetAccessTokenAsync();

        var url = $"https://fcm.googleapis.com/v1/projects/{_opts.ProjectId}/messages:send";

        var safeData = (data ?? new Dictionary<string, string>())
        .ToDictionary(k => k.Key, v => v.Value ?? "");

        safeData["title"] = title ?? "";
        safeData["body"]  = body  ?? "";

        // Construimos el mensaje
        var message = new
        {
            token = deviceToken,
            //    notification = new { title, body }, SACARLO PARA Q FUNCIONE EL onMessageReceived DE FCM
            android = new
            {
                priority = "HIGH",
                ttl = "3600s" // opcional: 1 hora
            },
            data = safeData
        };

        var payload = new { message };

        var json = JsonSerializer.Serialize(payload);
        var http = _httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var res = await http.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json"),
            ct
        );

        var resp = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"FCM v1 send failed: {(int)res.StatusCode} {res.StatusCode} - {resp}"
            );
        }
    }

    public async Task SendDataToTopicAsync(string topic, IDictionary<string, string> data)
    {
        var accessToken = await GetAccessTokenAsync();
        var url = $"https://fcm.googleapis.com/v1/projects/{_opts.ProjectId}/messages:send";

        var payload = new { message = new { topic, data } };

        var json = JsonSerializer.Serialize(payload);
        var http = _httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var res = await http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        var resp = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"FCM v1 send failed: {(int)res.StatusCode} {res.StatusCode} - {resp}");
    }

}
