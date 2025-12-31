using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MccSoft.WebApi.Patching;
using MccSoft.WebApi.Patching.Models;

namespace MccSoft.WebApi.Tests.Patching;

public class PatchingTests
{
    private readonly JsonSerializerOptions _deserializationOptions;

    public class PatchDto2 : PatchRequest<object>
    {
        public string? Qwe { get; set; }
        public string? Asd { get; set; }
    }

    public class PatchDto1 : PatchRequest<object>
    {
        public string? Qwe { get; set; }
        public string? Asd { get; set; }

        public PatchDto2? Zxc { get; set; }
    }

    public class NullableTestProp : PatchRequest<object>
    {
        public int NotNullableValueProperty { get; set; }
        public int? NullableValueProperty { get; set; }

        public string NotNullableStringProperty { get; set; }
        public string? NullableStringProperty { get; set; }

        public PatchDto1 NotNullableReferenceProperty { get; set; }
        public PatchDto1? NullableReferenceProperty { get; set; }
    }

    public PatchingTests()
    {
        _deserializationOptions = new JsonSerializerOptions() { };
        _deserializationOptions.Converters.Add(new PatchRequestConverterFactory());
    }

    [Fact]
    public void Simple()
    {
        var result = JsonSerializer.Deserialize<PatchDto1>(
            "{\"Qwe\": \"zxc\"}",
            _deserializationOptions
        )!;

        result.Qwe.Should().Be("zxc");
        result.Asd.Should().BeNull();

        result.IsFieldPresent(nameof(result.Qwe)).Should().BeTrue();
        result.IsFieldPresent(nameof(result.Asd)).Should().BeFalse();
    }

    [Fact]
    public void NonPresentField_Simple()
    {
        var result = JsonSerializer.Deserialize<PatchDto1>(
            "{\"uyt\": \"zxc\", \"Qwe\": \"zxc\"}",
            _deserializationOptions
        )!;

        result.Qwe.Should().Be("zxc");
        result.Asd.Should().BeNull();

        result.IsFieldPresent(nameof(result.Qwe)).Should().BeTrue();
        result.IsFieldPresent(nameof(result.Asd)).Should().BeFalse();
    }

    [Fact]
    public void NonPresentField_Object()
    {
        var result = JsonSerializer.Deserialize<PatchDto1>(
            "{\"uyt\": {\"asd\": \"zxc\"}, \"Qwe\": \"zxc\"}",
            _deserializationOptions
        )!;

        result.Qwe.Should().Be("zxc");
        result.Asd.Should().BeNull();

        result.IsFieldPresent(nameof(result.Qwe)).Should().BeTrue();
        result.IsFieldPresent(nameof(result.Asd)).Should().BeFalse();
    }

    [Fact]
    public void Nested()
    {
        var result = JsonSerializer.Deserialize<PatchDto1>(
            "{\"Qwe\": \"1\", \"Zxc\": { \"Asd\": \"2\"} }",
            _deserializationOptions
        )!;

        result.Qwe.Should().Be("1");
        result.Asd.Should().BeNull();
        result.Zxc.Should().NotBeNull();
        result.Zxc!.Asd.Should().Be("2");
        result.Zxc.Qwe.Should().BeNull();

        result.IsFieldPresent(nameof(result.Qwe)).Should().BeTrue();
        result.IsFieldPresent(nameof(result.Zxc)).Should().BeTrue();
        result.IsFieldPresent(nameof(result.Asd)).Should().BeFalse();
        result.Zxc.IsFieldPresent(nameof(result.Asd)).Should().BeTrue();
        result.Zxc.IsFieldPresent(nameof(result.Zxc)).Should().BeFalse();
        result.Zxc.IsFieldPresent(nameof(result.Qwe)).Should().BeFalse();
    }

    [Theory]
    [InlineData("NotNullableValueProperty")]
    [InlineData("NotNullableStringProperty")]
    [InlineData("NotNullableReferenceProperty")]
    public void AssignNullToNonNullableProperty(string propName)
    {
        FluentActions
            .Invoking(() =>
                JsonSerializer.Deserialize<NullableTestProp>(
                    $"{{\"{propName}\": null}}",
                    _deserializationOptions
                )
            )
            .Should()
            .Throw<ValidationException>()
            .WithMessage($"The {propName} field is required.");
    }

    [Fact]
    public void AssignNullToNullableProperty()
    {
        var result = JsonSerializer.Deserialize<NullableTestProp>(
            "{\"NullableValueProperty\": null, \"Asd\": null, \"NullableStringProperty\": null}",
            _deserializationOptions
        );

        result.NullableValueProperty.Should().BeNull();
        result.NullableStringProperty.Should().BeNull();
        result.NullableReferenceProperty.Should().BeNull();
    }
}
