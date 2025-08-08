using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Relias.ContentLibraryService.Infra.Cosmos;

[ExcludeFromCodeCoverage(Justification = "Cosmos LINQ serializer adapter; uses framework serializers with minimal logic.")]

public class CosmosSystemTextJsonLinqSerializer : CosmosLinqSerializer
{
    private readonly JsonObjectSerializer _serializer;

    public CosmosSystemTextJsonLinqSerializer(JsonSerializerOptions options)
    {
        _serializer = new JsonObjectSerializer(options);
    }

    public override T FromStream<T>(Stream stream)
    {
        using var s = stream;
        if (s.Length == 0) return default!;
        return (T)_serializer.Deserialize(s, typeof(T), default)!;
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        _serializer.Serialize(stream, input, typeof(T), default);
        stream.Position = 0;
        return stream;
    }

    public override string SerializeMemberName(MemberInfo memberInfo)
    {
        return memberInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? memberInfo.Name;
    }
}