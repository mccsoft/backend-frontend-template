using MccSoft.WebApi.Exceptions;
using MccSoft.WebApi.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace MccSoft.WebApi.Tests
{
    public class ErrorHandlerMiddlewareTests
    {
        private readonly Mock<ILogger<ErrorHandlerMiddleware>> _logger;
        private readonly Mock<IHostingEnvironment> _environment;
        private readonly DefaultHttpContext _defaultHttpContext;
        private Action<int?> _setResponseStatusCode;

        public ErrorHandlerMiddlewareTests()
        {
            _logger = new Mock<ILogger<ErrorHandlerMiddleware>>();
            _environment = new Mock<IHostingEnvironment>();

            var contentResultExecutor = new Mock<IActionResultExecutor<ContentResult>>();
            contentResultExecutor.Setup(
                    x => x.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ContentResult>())
                )
                .Returns(Task.CompletedTask)
                .Callback(
                    (ActionContext x, ContentResult y) =>
                    {
                        _setResponseStatusCode?.Invoke(y.StatusCode);
                    }
                );

            var objectResultExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            objectResultExecutor.Setup(
                    x => x.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>())
                )
                .Returns(Task.CompletedTask)
                .Callback(
                    (ActionContext x, ObjectResult y) =>
                    {
                        _setResponseStatusCode?.Invoke(y.StatusCode);
                    }
                );

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(
                    s => s.GetService(typeof(IActionResultExecutor<ContentResult>))
                )
                .Returns(contentResultExecutor.Object);

            serviceProviderMock.Setup(
                    s => s.GetService(typeof(IActionResultExecutor<ObjectResult>))
                )
                .Returns(objectResultExecutor.Object);

            _defaultHttpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProviderMock.Object
            };
        }

        [Fact]
        public async Task Invoke_ThrowException_SetHttpResponseInternalServerError()
        {
            int? responseHttpCode = null;
            _setResponseStatusCode = content => responseHttpCode = content;
            var ex = new Exception("Test");

            var target = new ErrorHandlerMiddleware(
                context => throw ex,
                _logger.Object,
                _environment.Object
            );

            await target.Invoke(_defaultHttpContext);

            Assert.True(responseHttpCode.HasValue);
            Assert.Equal((int)HttpStatusCode.InternalServerError, responseHttpCode.Value);
        }

        [Fact]
        public async Task Invoke_ThrowResourceNotFoundException_SetHttpResponse404()
        {
            int? responseHttpCode = null;
            _setResponseStatusCode = content => responseHttpCode = content;
            var ex = new ResourceNotFoundException("Test");

            var target = new ErrorHandlerMiddleware(
                context => throw ex,
                _logger.Object,
                _environment.Object
            );

            await target.Invoke(_defaultHttpContext);

            Assert.True(responseHttpCode.HasValue);
            Assert.Equal(404, responseHttpCode.Value);
        }

        [Fact]
        public async Task Invoke_ThrowValidationException_SetHttpResponseBadRequest()
        {
            int? responseHttpCode = null;
            _setResponseStatusCode = content => responseHttpCode = content;
            var ex = new ValidationException("Test");

            var target = new ErrorHandlerMiddleware(
                context => throw ex,
                _logger.Object,
                _environment.Object
            );

            await target.Invoke(_defaultHttpContext);

            Assert.True(responseHttpCode.HasValue);
            Assert.Equal((int)HttpStatusCode.BadRequest, responseHttpCode.Value);
        }
    }
}
