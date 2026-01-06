using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using AwesomeAssertions.Specialized;
using MccSoft.HttpClientExtension;
using MccSoft.TemplateApp.Http.Generated;
using MccSoft.WebApi.Serialization;

namespace MccSoft.TemplateApp.TestUtils;

public static class ExceptionHelper
{
    public static async Task<ExceptionAssertions<ApiException>> WithDetail(
        this Task<ExceptionAssertions<ApiException>> assertionTask,
        string expectedWildcardPattern,
        string because = "",
        params object[] becauseArgs
    )
    {
        var assertion = await assertionTask;
        var assertionChain = assertion.CurrentAssertionChain;
        assertionChain
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith("Expected ApiException exception {reason}, but no exception was thrown.");

        assertionChain.PerformValidation(() =>
        {
            var exception = assertion.Subject.First();
            var typedException = exception
                .Should()
                .BeOfType<ApiException<ValidationProblemDetails>>()
                .Subject;
            typedException
                .Result.Detail.Should()
                .MatchEquivalentOf(expectedWildcardPattern, because, becauseArgs);
        });

        return assertion;
    }

    public static async Task<ExceptionAssertions<Exception>> WithErrorForField(
        this Task<ExceptionAssertions<Exception>> assertionTask,
        string fieldName,
        string error = "",
        string because = "",
        params object[] becauseArgs
    )
    {
        var assertion = await assertionTask;
        var assertionChain = assertion.CurrentAssertionChain;
        assertionChain
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith(
                "Expected FailedRequestException exception {reason}, but no exception was thrown."
            );

        assertionChain.PerformValidation(() =>
        {
            IDictionary<string, List<string>> errors;
            var exception = assertion.Subject.First();
            if (exception is ApiException<ValidationProblemDetails> apiException)
            {
                errors = apiException.Result.Errors.ToDictionary(x => x.Key, x => x.Value.ToList());
            }
            else
            {
                var serialized = JsonSerializer.SerializeToElement(exception);
                var validationProblemDetails = serialized.Deserialize<ValidationProblemDetails>();
                errors = validationProblemDetails.Errors.ToDictionary(
                    x => x.Key,
                    x => x.Value.ToList()
                );
            }

            errors.Should().ContainKey(fieldName);
            if (!string.IsNullOrEmpty(error))
            {
                errors[fieldName].Should().ContainMatch(error);
            }
        });

        return assertion;
    }

    public static async Task<ExceptionAssertions<ApiException>> WithErrorForField(
        this Task<ExceptionAssertions<ApiException>> assertionTask,
        string fieldName,
        string error = "",
        string because = "",
        params object[] becauseArgs
    )
    {
        var assertion = await assertionTask;
        var assertionChain = assertion.CurrentAssertionChain;
        assertionChain
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith(
                "Expected FailedRequestException exception {reason}, but no exception was thrown."
            );

        assertionChain.PerformValidation(() =>
        {
            var exception = assertion.Subject.First();
            var apiException = exception
                .Should()
                .BeOfType<ApiException<ValidationProblemDetails>>()
                .Subject;
            ValidationProblemDetails validationProblemDetails = apiException.Result;
            validationProblemDetails.Errors.Should().ContainKey(fieldName);
            if (!string.IsNullOrEmpty(error))
            {
                validationProblemDetails.Errors[fieldName].Should().ContainMatch(error);
            }
        });

        return assertion;
    }

    public static async Task<ExceptionAssertions<FailedRequestException>> WithErrorForField(
        this Task<ExceptionAssertions<FailedRequestException>> assertionTask,
        string fieldName,
        string error = "",
        string because = "",
        params object[] becauseArgs
    )
    {
        var assertion = await assertionTask;
        var assertionChain = assertion.CurrentAssertionChain;
        assertionChain
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith(
                "Expected FailedRequestException exception {reason}, but no exception was thrown."
            );

        assertionChain.PerformValidation(() =>
        {
            var exception = assertion.Which;
            var validationProblemDetails =
                DefaultJsonSerializer.Deserialize<ValidationProblemDetails>(exception.Content)!;

            validationProblemDetails.Errors.Should().ContainKey(fieldName);
            if (!string.IsNullOrEmpty(error))
            {
                var fieldErrors = validationProblemDetails.Errors[fieldName];
                fieldErrors.Should().ContainMatch(error);
            }
        });

        return assertion;
    }

    public static async Task<ExceptionAssertions<FailedRequestException>> WithType(
        this Task<ExceptionAssertions<FailedRequestException>> assertionTask,
        string type,
        string because = "",
        params object[] becauseArgs
    )
    {
        var assertion = await assertionTask;
        var assertionChain = assertion.CurrentAssertionChain;
        assertionChain
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith(
                "Expected FailedRequestException exception {reason}, but no exception was thrown."
            );

        assertionChain.PerformValidation(() =>
        {
            var exception = assertion.Subject.First();
            var errors = DefaultJsonSerializer.Deserialize<ValidationProblemDetails>(
                exception.Content
            )!;

            errors.Type.Should().BeEquivalentTo(type);
        });

        return assertion;
    }

    public static async Task<ExceptionAssertions<ApiException>> WithStatusCode404(
        this Task<ExceptionAssertions<ApiException>> assertionTask,
        string because = "",
        params object[] becauseArgs
    )
    {
        var assertion = await assertionTask;
        var assertionChain = assertion.CurrentAssertionChain;
        assertionChain
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith("Expected ApiException exception {reason}, but no exception was thrown.");

        assertionChain.PerformValidation(() =>
        {
            var exception = assertion.Subject.First() as ApiException;
            exception.StatusCode.Should().Be(404, because, becauseArgs);

            var errors = DefaultJsonSerializer.Deserialize<ValidationProblemDetails>(
                exception.Response
            )!;
            errors.Type.Should().BeEquivalentTo("urn:lmt:not-found");
        });
        return assertion;
    }

    public static async Task<ExceptionAssertions<ApiException>> WithStatusCode403(
        this Task<ExceptionAssertions<ApiException>> assertionTask,
        string because = "",
        params object[] becauseArgs
    )
    {
        var assertion = await assertionTask;
        var assertionChain = assertion.CurrentAssertionChain;
        assertionChain
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith("Expected ApiException exception {reason}, but no exception was thrown.");

        assertionChain.PerformValidation(() =>
        {
            var exception = assertion.Subject.First() as ApiException;
            exception.StatusCode.Should().Be(403, because, becauseArgs);
        });
        return assertion;
    }

    public static async Task<ExceptionAssertions<ApiException>> WithStatusCode409(
        this Task<ExceptionAssertions<ApiException>> assertionTask,
        string because = "",
        params object[] becauseArgs
    )
    {
        var assertion = await assertionTask;
        var assertionChain = assertion.CurrentAssertionChain;
        assertionChain
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith("Expected ApiException exception {reason}, but no exception was thrown.");

        assertionChain.PerformValidation(() =>
        {
            var exception = assertion.Subject.First();
            exception.StatusCode.Should().Be(409, because, becauseArgs);
        });

        return assertion;
    }

    public static Task<ExceptionAssertions<ApiException>> ThrowNotAuthorizedException<T>(
        this GenericAsyncFunctionAssertions<T> assertion,
        string because = "",
        params object[] becauseArgs
    )
    {
        return assertion
            .ThrowExactlyAsync<ApiException>(because, becauseArgs)
            .WithMessage("*Status: 403*", because, becauseArgs);
    }

    public static string EscapePlaceholders(this string value) =>
        value
            .Replace("{", "{{", StringComparison.Ordinal)
            .Replace("}", "}}", StringComparison.Ordinal);

    public static void PerformValidation(this AssertionChain assertionChain, Action validation)
    {
        List<string> errors;
        using (var scope = new AssertionScope())
        {
            validation();

            errors = scope.Discard().ToList();
        }

        errors.ForEach(error => assertionChain.FailWith(error.EscapePlaceholders()));
    }
}
