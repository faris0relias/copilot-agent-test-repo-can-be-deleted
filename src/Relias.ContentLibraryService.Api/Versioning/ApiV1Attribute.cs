using Asp.Versioning;

namespace Relias.ContentLibraryService.Api.Versioning;

/// <summary>
/// API version attribute
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class ApiV1Attribute() : ApiVersionAttribute(new ApiVersion(1, 0));