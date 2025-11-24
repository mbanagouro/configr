using System;
using ConfigR.Core;
using FluentAssertions;
using Xunit;

namespace ConfigR.Tests.Core;

public sealed class SerializerTests
{
    private readonly DefaultConfigSerializer _serializer = new();

    [Fact]
    public void Serialize_Null_ReturnsEmptyString()
    {
        var result = _serializer.Serialize(null);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Serialize_Primitive_Works()
    {
        var result = _serializer.Serialize(10);
        result.Should().Be("10");
    }

    private sealed class Sample
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void Roundtrip_ComplexType_Works()
    {
        var original = new Sample { Name = "Michel", Age = 35 };

        var serialized = _serializer.Serialize(original);
        var deserialized = (Sample?)_serializer.Deserialize(serialized, typeof(Sample));

        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be(original.Name);
        deserialized.Age.Should().Be(original.Age);
    }
}
