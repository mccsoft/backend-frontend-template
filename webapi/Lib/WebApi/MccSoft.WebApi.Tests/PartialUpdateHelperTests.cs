using FluentAssertions;
using MccSoft.LowLevelPrimitives.Exceptions;
using MccSoft.Testing;
using MccSoft.WebApi.Patching;
using MccSoft.WebApi.Patching.Models;
using Xunit;

namespace MccSoft.WebApi.Tests;

public class PartialUpdateHelperTests
{
    public enum TestEnum
    {
        Undefined = 0,
        A = 1,
        B = 2,
    }

    public enum TestEnumWithout0Value
    {
        A = 1,
        B = 2,
    }

    public class DomainClass1
    {
        public string Asd { get; set; }
        public string Qwe { get; set; }
        public TestEnum? Enum { get; set; }
        public TestEnumWithout0Value? EnumWithout0 { get; set; }
        public TestEnumWithout0Value NonNullableEnumWithout0 { get; set; }
    }

    public class Patch1 : PatchRequest<DomainClass1>
    {
        [DoNotPatch]
        public string Asd { get; set; }
        public string Qwe { get; set; }
        public TestEnum? Enum { get; set; }
        public TestEnumWithout0Value? EnumWithout0 { get; set; }
        public TestEnumWithout0Value? NonNullableEnumWithout0 { get; set; }
    }

    [Fact]
    public void DoNotPatch_FieldNotPatched()
    {
        var domainClass = new DomainClass1() { Asd = "qwe" };
        var patch = new Patch1() { Asd = "asd", };
        domainClass.Update(patch.MarkAllNonDefaultPropertiesAsDefined());

        domainClass.Asd.Should().Be("qwe");
    }

    [Fact]
    public void FieldNotPresentInRequest_NotPatched()
    {
        var domainClass = new DomainClass1() { Qwe = "qwe" };
        var patch = new Patch1() { Qwe = "asd", };
        domainClass.Update(patch);

        domainClass.Qwe.Should().Be("qwe");
    }

    [Fact]
    public void FieldPresentInRequest_Patched()
    {
        var domainClass = new DomainClass1() { Qwe = "qwe" };
        var patch = new Patch1() { Qwe = "asd", };
        domainClass.Update(patch.MarkAllNonDefaultPropertiesAsDefined());

        domainClass.Qwe.Should().Be("asd");
    }

    [Fact]
    public void Enum_CorrectValue()
    {
        var domainClass = new DomainClass1() { Enum = TestEnum.B };
        var patch = new Patch1() { Enum = TestEnum.A, };
        domainClass.Update(patch.MarkAllNonDefaultPropertiesAsDefined());

        domainClass.Enum.Should().Be(TestEnum.A);
    }

    [Fact]
    public void Enum_InCorrectValue()
    {
        var domainClass = new DomainClass1() { Enum = TestEnum.B };
        var patch = new Patch1() { Enum = (TestEnum)10, };
        FluentActions
            .Invoking(() => domainClass.Update(patch.MarkAllNonDefaultPropertiesAsDefined()))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("Enum: Value '10' is out of range");

        domainClass.Enum.Should().Be(TestEnum.B);
    }

    [Fact]
    public void Enum_NullValue()
    {
        var domainClass = new DomainClass1() { Enum = TestEnum.B };
        var patch = new Patch1() { Enum = null, };
        patch.SetHasProperty(nameof(patch.Enum));
        domainClass.Update(patch);

        domainClass.Enum.Should().Be(null);
    }

    [Fact]
    public void EnumWithout0Value_Nullable_NullValue_Ok()
    {
        var domainClass = new DomainClass1() { EnumWithout0 = TestEnumWithout0Value.B };
        var patch = new Patch1() { EnumWithout0 = null, };
        patch.SetHasProperty(nameof(patch.EnumWithout0));
        domainClass.Update(patch);

        domainClass.EnumWithout0.Should().Be(null);
    }

    [Fact]
    public void EnumWithout0Value_NonNullable_NullValue_Error()
    {
        var domainClass = new DomainClass1() { NonNullableEnumWithout0 = TestEnumWithout0Value.B };
        var patch = new Patch1() { NonNullableEnumWithout0 = null, };
        patch.SetHasProperty(nameof(patch.NonNullableEnumWithout0));
        FluentActions
            .Invoking(() => domainClass.Update(patch))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("NonNullableEnumWithout0: Cannot assign null to non-nullable enum");

        domainClass.NonNullableEnumWithout0.Should().Be(TestEnumWithout0Value.B);
    }
}
