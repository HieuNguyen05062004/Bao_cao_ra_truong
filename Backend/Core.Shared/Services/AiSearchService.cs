using Core.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Core.Shared.Services;

/// <summary>
/// Gọi Google Gemini API để phân tích câu tìm kiếm tự nhiên.
/// Đăng ký DI: services.AddHttpClient&lt;AiSearchService&gt;() + AddScoped&lt;IAiSearchService, AiSearchService&gt;()
/// Config cần: Gemini:ApiKey trong appsettings.json
/// </summary>
public class AiSearchService : IAiSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<AiSearchService> _logger;

    // Dùng gemini-flash-latest như trong URL Google cung cấp
    private const string GeminiBaseUrl =
    "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";

    public AiSearchService(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<AiSearchService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<AiSearchResult> ParseSearchQueryAsync(string naturalLanguageQuery)
    {
        if (string.IsNullOrWhiteSpace(naturalLanguageQuery))
            return Fallback(naturalLanguageQuery, "Câu tìm kiếm không được để trống.");

        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini:ApiKey chưa được cấu hình. Sử dụng tìm kiếm thông thường.");
            return Fallback(naturalLanguageQuery);
        }

        try
        {
            // Gemini nhận key qua query string, không dùng header Authorization
            var url = $"{GeminiBaseUrl}?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = BuildPrompt(naturalLanguageQuery) }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Gemini API trả lỗi {StatusCode}: {Body}",
                    response.StatusCode, errorBody);
                return Fallback(naturalLanguageQuery);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return ParseApiResponse(responseBody, naturalLanguageQuery);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Lỗi kết nối Gemini API");
            return Fallback(naturalLanguageQuery);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout khi gọi Gemini API");
            return Fallback(naturalLanguageQuery, "Tích hợp AI tìm kiếm nâng cao tạm thời không khả dụng (timeout).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi gọi Gemini API");
            return Fallback(naturalLanguageQuery);
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static string BuildPrompt(string query) => $$"""
        Bạn là trợ lý tìm kiếm sách trong thư viện. Người dùng nhập: "{{query}}"

        Hãy phân tích và trả về JSON sau (chỉ JSON, không có backtick, không giải thích):
        {
          "keyword": "từ khóa ngắn gọn nhất để tìm trong tên sách hoặc tác giả",
          "interpretedQuery": "một câu ngắn mô tả ý định tìm kiếm bằng tiếng Việt"
        }

        Ví dụ:
        - Input: "tôi muốn đọc sách về lập trình Python cơ bản"
          Output: {"keyword":"Python lập trình","interpretedQuery":"Sách lập trình Python cho người mới bắt đầu"}
        - Input: "sách của Nguyễn Nhật Ánh"
          Output: {"keyword":"Nguyễn Nhật Ánh","interpretedQuery":"Sách của tác giả Nguyễn Nhật Ánh"}
        """;

    /// <summary>
    /// Gemini response structure:
    /// candidates[0].content.parts[0].text  →  chuỗi JSON cần parse tiếp
    /// </summary>
    private AiSearchResult ParseApiResponse(string responseBody, string originalQuery)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            // Làm sạch markdown fence nếu Gemini vô tình thêm vào
            var cleanJson = text
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            using var innerDoc = JsonDocument.Parse(cleanJson);
            var root = innerDoc.RootElement;

            var keyword = root.TryGetProperty("keyword", out var kw)
                ? kw.GetString() ?? originalQuery
                : originalQuery;

            var interpreted = root.TryGetProperty("interpretedQuery", out var iq)
                ? iq.GetString() ?? originalQuery
                : originalQuery;

            return new AiSearchResult
            {
                Keyword = keyword.Trim(),
                InterpretedQuery = interpreted.Trim(),
                IsSuccess = true
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Không parse được JSON từ Gemini. Raw: {Body}", responseBody);
            return Fallback(originalQuery);
        }
    }

    private static AiSearchResult Fallback(string originalQuery, string? errorMessage = null) =>
        new()
        {
            Keyword = originalQuery.Trim(),
            InterpretedQuery = originalQuery.Trim(),
            IsSuccess = errorMessage == null,
            ErrorMessage = errorMessage
        };
}
