using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WorkOrderApplication.API.Configurations;
using WorkOrderApplication.API.Dtos;
using WorkOrderApplication.API.Helpers;

namespace WorkOrderApplication.API.Services;

public class OrderProxyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderProxyService> _logger;

    // -------------------- Constructor --------------------
    public OrderProxyService(HttpClient httpClient, ILogger<OrderProxyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // -------------------- POST: add/byOrderGroup --------------------------------------------------------------------
    public async Task<string> AddOrderGroupAsync(OrderGroupRequestDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("add/byOrderGroup", content);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    // -------------------- GET: orderRecord --------------------
    public async Task<OrderRecordResponseDto?> GetOrderRecordsAsync(int pageNum = 1, int pageSize = 10)
    {
        var url = $"orderRecord?pageNum={pageNum}&pageSize={pageSize}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // ✅ ใช้ global converter ที่รองรับหลาย DateTime format
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new FlexibleDateTimeConverter() }
            };

            var result = JsonSerializer.Deserialize<OrderRecordResponseDto>(json, options);

            if (result == null)
            {
                _logger.LogWarning("[Proxy] Empty response from external API: {Url}", url);
                return null;
            }

            _logger.LogInformation("[Proxy] Successfully fetched {Count} orders (Page {PageNum})",
                result.Result?.Records?.Count ?? 0, pageNum);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[Proxy] HTTP request failed: {Message}", ex.Message);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[Proxy] JSON parse error when reading orderRecord");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Proxy] Unexpected error: {Message}", ex.Message);
            return null;
        }
    }
    
    // -------------------- GET: orderRecord/{id} --------------------
    public async Task<OrderRecordByIdDto?> GetOrderRecordByIdAsync(int id)
    {
        var url = $"orderRecord/{id}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReadCommentHandling = JsonCommentHandling.Skip,   // ✅ รองรับ comment ใน JSON
                AllowTrailingCommas = true,                       // ✅ รองรับ comma เกินท้าย
                Converters = { new FlexibleDateTimeConverter() }
            };

            // ✅ Deserialize เป็น response wrapper ก่อน
            var wrapper = JsonSerializer.Deserialize<OrderRecordByIdResponse>(json, options);

            // ✅ ดึงเฉพาะ result (OrderRecordByIdDto)
            var dto = wrapper?.Result;

            if (dto == null)
            {
                _logger.LogWarning("[Proxy] Empty response for orderRecord/{Id}", id);
                return null;
            }

            _logger.LogInformation("[Proxy] ✅ orderRecord/{Id} fetched successfully", id);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Proxy] ❌ Failed to fetch orderRecord/{Id}", id);
            return null;
        }
    }
}
