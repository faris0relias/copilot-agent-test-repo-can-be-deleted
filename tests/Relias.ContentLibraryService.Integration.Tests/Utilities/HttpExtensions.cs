using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Relias.ContentLibraryService.Integration.Tests.Utilities;

internal static class HttpExtensions
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal static async Task<TResult?> As<TResult>(this HttpContent httpContent, JsonSerializerOptions? jsonSerializerOptions = null) where TResult : class
    {
        var streamContent = await httpContent.ReadAsStreamAsync();

        return await JsonSerializer.DeserializeAsync<TResult>(streamContent, jsonSerializerOptions ?? DefaultSerializerOptions);
    }

    internal static JsonContent CreateRequestBody<TRequest>(TRequest data, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonContent.Create(data,MediaTypeHeaderValue.Parse("application/json"), jsonSerializerOptions ?? DefaultSerializerOptions);
    }

    internal static JsonContent UpdateRequestBody<TRequest>(TRequest data, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonContent.Create(data, MediaTypeHeaderValue.Parse("application/json"), jsonSerializerOptions ?? DefaultSerializerOptions);
    }

    internal static void SetAuthHeaderWithOrg(this HttpClient client, int organizationId, params int[] organizationIds)
    {
        var claims = new List<KeyValuePair<string, string>>
        {
            new(UserTokenKeys.Subject, "100"),
            new(UserTokenKeys.UserId, "100"),
            new(UserTokenKeys.ClientId, "platform-integration-tests"),
            new(UserTokenKeys.Permissions, "32"),
            new(UserTokenKeys.OrganizationId, organizationId.ToString())
        };

        claims.Clear();
        
        claims.AddRange(organizationIds.Select(id => new KeyValuePair<string, string>(UserTokenKeys.OrganizationIds, id.ToString())));

        client.DefaultRequestHeaders.Authorization = IntegrationTestAuthHelper.GenerateAuthHeaderValueFromClaims(claims);
    }
}