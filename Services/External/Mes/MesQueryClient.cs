using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class MesQueryClient
{
    private readonly HttpClient _http;
    private readonly MesOptions _opt;

    public MesQueryClient(HttpClient http, MesOptions opt)
    {
        _http = http;
        _opt = opt;
    }
    public async Task<JsonElement> GetLineInfoAsync(
        string empNo,
        string? lineName)
    {
        var query = new SortedDictionary<string, string>
        {
            ["EMP_NO"] = empNo,
            ["FACTORY"] = _opt.Factory
        };

        if (!string.IsNullOrWhiteSpace(lineName))
            query["LINE_NAME"] = lineName;

        var sign = BuildSign(query);

        var qs = string.Join("&",
            query.Select(kv => $"{kv.Key}={kv.Value}"));

        var url = $"{_opt.BaseUrl}/QueryData/LineInfo?{qs}&sign={sign}";

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("tokenID", _opt.TokenId);

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<JsonElement>();
    }
    public async Task<JsonElement> GetSnInfoAsync(string sn)
    {
        var query = new SortedDictionary<string, string>
        {
            ["FACTORY"] = _opt.Factory,
            ["SN"] = sn
        };

        var sign = BuildSign(query);

        var qs = string.Join("&",
            query.Select(kv => $"{kv.Key}={kv.Value}"));

        var url = $"{_opt.BaseUrl}/QueryData/SNInfo?{qs}&sign={sign}";

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("tokenID", _opt.TokenId);

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<JsonElement>();
    }
    private string BuildSign(
        SortedDictionary<string, string> query)
    {
        var sb = new StringBuilder();
        foreach (var kv in query)
            sb.Append(kv.Key).Append(kv.Value);

        using var md5 = MD5.Create();
        return Convert.ToHexString(
            md5.ComputeHash(
                Encoding.UTF8.GetBytes(_opt.SecretKey + sb)
            )
        );
    }


    public async Task<JsonElement> GetMoListAsync(
        string empNo,
        string getDataType,
        string moType,
        string? line)
    {
        var query = new SortedDictionary<string, string>
        {
            ["EMP_NO"] = empNo,
            ["FACTORY"] = _opt.Factory,
            ["GETDATA_TYPE"] = getDataType,
            ["MO_TYPE"] = moType
        };

        if (!string.IsNullOrWhiteSpace(line))
            query["LINE_NAME"] = line;

        // 1️⃣ concat key+value ตามลำดับ A-Z
        var sb = new StringBuilder();
        foreach (var kv in query)
            sb.Append(kv.Key).Append(kv.Value);

        // 2️⃣ sign
        var sign = Md5Upper(_opt.SecretKey + sb);

        // 3️⃣ build url
        var qs = string.Join("&",
            query.Select(kv => $"{kv.Key}={kv.Value}"));

        var url = $"{_opt.BaseUrl}/QueryData/MOList?{qs}&sign={sign}";

        // 4️⃣ request
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("tokenID", _opt.TokenId);

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static string Md5Upper(string input)
    {
        using var md5 = MD5.Create();
        return Convert.ToHexString(
            md5.ComputeHash(Encoding.UTF8.GetBytes(input))
        );
    }
}
