using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using MccSoft.HttpClientExtension;
using MccSoft.TemplateApp.Http.Generated;
using MccSoft.TemplateApp.TestUtils;
using MccSoft.WebApi.Serialization;

namespace MccSoft.TemplateApp.App.Tests;

public class ExceptionHelperTests
{
    [Fact]
    public async Task WithErrorForField_Error()
    {
        await FluentActions
            .Invoking(async () =>
            {
                await FluentActions
                    .Invoking(async () =>
                    {
                        await Task.Delay(1);
                        var validationProblemDetails = new ValidationProblemDetails()
                        {
                            Errors = new Dictionary<string, ICollection<string>>()
                            {
                                {
                                    "Field",
                                    new List<string>() { "blablabla" }
                                },
                            },
                        };
                        throw new FailedRequestException(
                            HttpStatusCode.BadRequest,
                            DefaultJsonSerializer.Serialize(validationProblemDetails)!,
                            "",
                            ""
                        );
                    })
                    .Should()
                    .ThrowAsync<FailedRequestException>()
                    .WithErrorForField("Field", "*is out of range");
            })
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Expected collection*to contain a match of \"*is out of range\".");
    }

    [Fact]
    public async Task WithDetail_DetailIsIncorrect()
    {
        await FluentActions
            .Invoking(async () =>
            {
                await FluentActions
                    .Invoking(async () =>
                    {
                        await Task.Delay(1);
                        throw new ApiException<ValidationProblemDetails>(
                            "blablabla",
                            400,
                            "{}",
                            new ConcurrentDictionary<string, IEnumerable<string>>(),
                            new ValidationProblemDetails() { Detail = "qwe" },
                            null
                        );
                    })
                    .Should()
                    .ThrowAsync<ApiException>()
                    .WithDetail("zxc");
            })
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage(
                "*Expected string to match the equivalent of \"zxc\", but \"qwe\" does not.*"
            );
    }

    [Fact]
    public async Task WithDetail_DetailIsCorrect()
    {
        await FluentActions
            .Invoking(async () =>
            {
                await Task.Delay(1);
                throw new ApiException<ValidationProblemDetails>(
                    "blablabla",
                    400,
                    "{}",
                    new ConcurrentDictionary<string, IEnumerable<string>>(),
                    new ValidationProblemDetails() { Detail = "qwe" },
                    null
                );
            })
            .Should()
            .ThrowAsync<ApiException>()
            .WithDetail("qwe");
    }

    [Fact]
    public async Task WithStatusCode403_ErrorThrown()
    {
        await FluentActions
            .Invoking(async () =>
            {
                await Task.Delay(1);
                throw new ApiException(
                    "blablabla",
                    403,
                    "{}",
                    new ConcurrentDictionary<string, IEnumerable<string>>(),
                    null
                );
            })
            .Should()
            .ThrowAsync<ApiException>()
            .WithStatusCode403();
    }

    [Fact]
    public async Task WithStatusCode404_ErrorNotThrown()
    {
        await FluentActions
            .Invoking(async () =>
            {
                await FluentActions
                    .Invoking(async () =>
                    {
                        await Task.Delay(1);
                    })
                    .Should()
                    .ThrowAsync<ApiException>()
                    .WithStatusCode404();
            })
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("*to be thrown, but no exception was thrown*");
    }

    [Fact]
    public async Task WithStatusCode404_StatusCodeIsWrong()
    {
        await FluentActions
            .Invoking(async () =>
            {
                await FluentActions
                    .Invoking(async () =>
                    {
                        await Task.Delay(1);
                        throw new ApiException(
                            "blablabla",
                            403,
                            "data",
                            new ConcurrentDictionary<string, IEnumerable<string>>(),
                            null
                        );
                    })
                    .Should()
                    .ThrowAsync<ApiException>()
                    .WithStatusCode404();
            })
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("*Expected value to be 404, but found 403*");
    }
}
