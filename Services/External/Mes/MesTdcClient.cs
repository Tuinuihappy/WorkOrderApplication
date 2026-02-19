using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class MesTdcClient
{
    private readonly HttpClient _http;
    private readonly MesOptions _opt;

    public MesTdcClient(HttpClient http, MesOptions opt)
    {
        _http = http;
        _opt = opt;
    }

    public async Task<JsonElement> CallAsync(string testType, string routingData)
    {
        var body = new
        {
            factory = _opt.Factory,
            testType = testType,
            routingData = routingData,
            testData = Array.Empty<object>()
        };

        var json = JsonSerializer.Serialize(
            body,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            });

        var sign = Md5Upper(_opt.SecretKey + json);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_opt.BaseUrl}/TDC/DELTA_DEAL_TEST_DATA_I?sign={sign}"
        );

        request.Headers.Add("tokenID", _opt.TokenId);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var resp = await _http.SendAsync(request);
        resp.EnsureSuccessStatusCode();

        var doc = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return doc;
    }

    private static string Md5Upper(string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }
}
