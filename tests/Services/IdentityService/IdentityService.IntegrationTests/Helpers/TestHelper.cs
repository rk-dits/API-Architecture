using System.Text;
using Newtonsoft.Json;

namespace IdentityService.IntegrationTests.Helpers;

public static class TestHelper
{
    public static StringContent CreateJsonContent<T>(T obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
    }

    public static string GenerateUniqueEmail()
    {
        return $"test{Guid.NewGuid():N}@example.com";
    }

    public static string GenerateUniqueName(string prefix = "Test")
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }
}