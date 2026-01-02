using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using MccSoft.HttpClientExtension;
using MccSoft.TemplateApp.Http.Generated;

namespace MccSoft.TemplateApp.TestUtils;

public static class ExceptionHelper
{
    public static ExceptionAssertions<ApiException> WithDetail(
        this ExceptionAssertions<ApiException> assertion,
        string expectedWildcardPattern,
        string because = "",
        params object[] becauseArgs
    )
    {
        assertion
            .CurrentAssertionChain.BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith("Expected ApiException exception {reason}, but no exception was thrown.");

        var exception = assertion.Subject.First();
        var typedException = exception
            .Should()
            .BeOfType<ApiException<ValidationProblemDetails>>()
            .Subject;
        typedException
            .Result.Detail.Should()
            .MatchEquivalentOf(expectedWildcardPattern, because, becauseArgs);
        return assertion;
    }

    public static ExceptionAssertions<Exception> WithErrorForField(
        this ExceptionAssertions<Exception> assertion,
        string fieldName,
        string error = "",
        string because = "",
        params object[] becauseArgs
    )
    {
        assertion
            .CurrentAssertionChain.BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith(
                "Expected FailedRequestException exception {reason}, but no exception was thrown."
            );

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
            errors[fieldName].Should().ContainEquivalentOf(error);
        }

        return assertion;
    }

    public static ExceptionAssertions<ApiException> WithErrorForField(
        this ExceptionAssertions<ApiException> assertion,
        string fieldName,
        string error = "",
        string because = "",
        params object[] becauseArgs
    )
    {
        assertion
            .CurrentAssertionChain.BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith(
                "Expected FailedRequestException exception {reason}, but no exception was thrown."
            );

        var exception = assertion.Subject.First();
        var apiException = exception
            .Should()
            .BeOfType<ApiException<ValidationProblemDetails>>()
            .Subject;
        ValidationProblemDetails validationProblemDetails = apiException.Result;
        validationProblemDetails.Errors.Should().ContainKey(fieldName);
        if (!string.IsNullOrEmpty(error))
        {
            validationProblemDetails.Errors[fieldName].Should().ContainEquivalentOf(error);
        }

        return assertion;
    }

    public static ExceptionAssertions<FailedRequestException> WithErrorForField(
        this ExceptionAssertions<FailedRequestException> assertion,
        string fieldName,
        string error = "",
        string because = "",
        params object[] becauseArgs
    )
    {
        assertion
            .CurrentAssertionChain.BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith(
                "Expected FailedRequestException exception {reason}, but no exception was thrown."
            );

        var exception = assertion.Subject.First();
        var validationProblemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(
            exception.Content
        )!;

        validationProblemDetails.Errors.Should().ContainKey(fieldName);
        if (!string.IsNullOrEmpty(error))
        {
            validationProblemDetails.Errors[fieldName].Should().ContainEquivalentOf(error);
        }

        return assertion;
    }

    public static ExceptionAssertions<FailedRequestException> WithType(
        this ExceptionAssertions<FailedRequestException> assertion,
        string type,
        string because = "",
        params object[] becauseArgs
    )
    {
        assertion
            .CurrentAssertionChain.BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith(
                "Expected FailedRequestException exception {reason}, but no exception was thrown."
            );

        var exception = assertion.Subject.First();
        var errors = JsonSerializer.Deserialize<ValidationProblemDetails>(exception.Content)!;

        errors.Type.Should().BeEquivalentTo(type);

        return assertion;
    }

    public static ExceptionAssertions<ApiException> WithStatusCode404(
        this ExceptionAssertions<ApiException> assertion,
        string because = "",
        params object[] becauseArgs
    )
    {
        assertion
            .CurrentAssertionChain.BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith("Expected ApiException exception {reason}, but no exception was thrown.");

        var exception = assertion.Subject.First() as ApiException;
        exception.StatusCode.Should().Be(404, because, becauseArgs);

        var errors = JsonSerializer.Deserialize<ValidationProblemDetails>(exception.Response)!;
        errors.Type.Should().BeEquivalentTo("urn:MccSoft.not-found");

        return assertion;
    }

    public static ExceptionAssertions<ApiException> WithStatusCode403(
        this ExceptionAssertions<ApiException> assertion,
        string because = "",
        params object[] becauseArgs
    )
    {
        assertion
            .CurrentAssertionChain.BecauseOf(because, becauseArgs)
            .UsingLineBreaks.ForCondition(assertion.Subject.Any())
            .FailWith("Expected ApiException exception {reason}, but no exception was thrown.");

        var exception = assertion.Subject.First() as ApiException;
        exception.StatusCode.Should().Be(403, because, becauseArgs);

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
}
