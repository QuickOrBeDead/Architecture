using System.Text;
using System.Text.Json;

namespace RestaurantManagement.Api.FunctionalTests.Infrastructure;

/// <summary>
/// Helper methods for HTTP operations in tests
/// </summary>
public static class HttpHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static StringContent ToJsonContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    public static async Task<string> GetResponseContent(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }
}