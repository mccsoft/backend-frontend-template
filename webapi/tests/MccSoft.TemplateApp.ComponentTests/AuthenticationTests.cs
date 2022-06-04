using System;
using FluentAssertions;
using MccSoft.TemplateApp.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MccSoft.TemplateApp.ComponentTests
{
    public class AuthenticationTests : TestBase
    {
        public AuthenticationTests(ITestOutputHelper outputHelper) : base(outputHelper) { }
    }
}
