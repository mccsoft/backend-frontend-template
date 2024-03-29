﻿using System.ComponentModel.DataAnnotations;
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
        public int Qwe { get; set; }
        public int? Zxc { get; set; }

        public PatchDto1 Asd { get; set; }
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

    [Fact]
    public void AssignNullToNonNullableProperty()
    {
        FluentActions
            .Invoking(
                () =>
                    JsonSerializer.Deserialize<NullableTestProp>(
                        "{\"Qwe\": null}",
                        _deserializationOptions
                    )
            )
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The Qwe field is required.");
    }

    [Fact]
    public void AssignNullToNullableProperty()
    {
        var result = JsonSerializer.Deserialize<NullableTestProp>(
            "{\"Zxc\": null, \"Asd\": null}",
            _deserializationOptions
        );

        result.Zxc.Should().BeNull();
        result.Asd.Should().BeNull();
    }
}
