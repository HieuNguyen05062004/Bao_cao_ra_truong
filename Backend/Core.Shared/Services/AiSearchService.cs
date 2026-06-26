using Core.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Core.Shared.Services;

/// <summary>
/// Gọi OpenAI ChatGPT API để phân tích câu tìm kiếm tự nhiên.
/// Đăng ký DI: services.AddHttpClient<AiSearchService>() + AddScoped<IAiSearchService, AiSearchService>()
/// Config cần: OpenAI:ApiKey trong appsettings.json
/// </summary>
public class AiSearchService : IAiSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<AiSearchService> _logger;

    private const string OpenAiBaseUrl = "https://api.openai.com/v1/chat/completions";

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

        var apiKey = _config["OpenAI:ApiKey"]
            ?? _config["OPENAI_API_KEY"]
            ?? Environment.GetEnvironmentVariable("OpenAI__ApiKey")
            ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("OpenAI:ApiKey chưa được cấu hình. Sử dụng tìm kiếm thông thường.");
            return Fallback(naturalLanguageQuery);
        }

        try
        {
            var model = _config["OpenAI:Model"]
                ?? Environment.GetEnvironmentVariable("OpenAI__Model")
                ?? "gpt-4o-mini";

            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = BuildPrompt(naturalLanguageQuery)
                    }
                },
                temperature = 0.0
            };

            var request = new HttpRequestMessage(HttpMethod.Post, OpenAiBaseUrl);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("OpenAI API trả lỗi {StatusCode}: {Body}", response.StatusCode, errorBody);
                return Fallback(naturalLanguageQuery);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return ParseApiResponse(responseBody, naturalLanguageQuery);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Lỗi kết nối OpenAI API");
            return Fallback(naturalLanguageQuery);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout khi gọi OpenAI API");
            return Fallback(naturalLanguageQuery, "Tích hợp AI tìm kiếm nâng cao tạm thời không khả dụng (timeout).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi gọi OpenAI API");
            return Fallback(naturalLanguageQuery);
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static string BuildPrompt(string query) => $$"""
        Bạn là trợ lý tìm kiếm sách trong thư viện. Người dùng nhập: "{{query}}"

        Hãy phân tích và mở rộng ý định tìm kiếm của người dùng. Nếu người dùng nhập một khái niệm (như "làm giàu", "kiếm tiền", "hạnh phúc"), hãy chuyển nó thành các thể loại sách và từ khóa liên quan thường có trong thư viện (ví dụ: Kinh tế, Phát triển bản thân, Kỹ năng sống, Tài chính). Trả về định dạng JSON sau (chỉ JSON, không có backtick, không giải thích):
        {
          "keyword": "từ khóa chính hoặc danh sách từ khóa cách nhau bởi dấu phẩy (bao gồm cả các từ khóa mở rộng về chủ đề)",
          "interpretedQuery": "một câu ngắn mô tả ý định tìm kiếm bằng tiếng Việt"
        }

        Ví dụ:
        - Input: "sách làm giàu"
          Output: {"keyword":"làm giàu, kinh tế, kinh doanh, phát triển bản thân, tài chính","interpretedQuery":"Sách về chủ đề làm giàu, kinh tế và phát triển bản thân"}
        - Input: "tôi muốn đọc sách về lập trình Python cơ bản"
          Output: {"keyword":"Python, lập trình, công nghệ thông tin","interpretedQuery":"Sách lập trình Python cho người mới bắt đầu"}
        - Input: "sách công nghệ thông tin, chính trị và kinh tế"
          Output: {"keyword":"công nghệ thông tin, chính trị, kinh tế","interpretedQuery":"Sách thuộc các chủ đề Công nghệ thông tin, Chính trị và Kinh tế"}
        - Input: "sách của Nguyễn Nhật Ánh"
          Output: {"keyword":"Nguyễn Nhật Ánh","interpretedQuery":"Sách của tác giả Nguyễn Nhật Ánh"}
        """;

    /// <summary>
    /// OpenAI response structure:
    /// choices[0].message.content  → chuỗi JSON cần parse tiếp
    /// </summary>
    private AiSearchResult ParseApiResponse(string responseBody, string originalQuery)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);

            if (!doc.RootElement.TryGetProperty("choices", out var choices) ||
                choices.ValueKind != JsonValueKind.Array ||
                choices.GetArrayLength() == 0)
            {
                _logger.LogWarning("OpenAI response không có choices hợp lệ. Raw: {Body}", responseBody);
                return Fallback(originalQuery);
            }

            var firstChoice = choices[0];
            if (!firstChoice.TryGetProperty("message", out var message) ||
                !message.TryGetProperty("content", out var contentElement))
            {
                _logger.LogWarning("OpenAI response không có message.content hợp lệ. Raw: {Body}", responseBody);
                return Fallback(originalQuery);
            }

            var text = contentElement.GetString() ?? string.Empty;
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
            _logger.LogWarning(ex, "Không parse được JSON từ OpenAI. Raw: {Body}", responseBody);
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
