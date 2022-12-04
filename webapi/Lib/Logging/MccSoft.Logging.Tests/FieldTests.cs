namespace MccSoft.Logging.Tests;

public class FieldTests
{
    [Fact]
    public void ToStringConversion()
    {
        void Method(string s)
        {
            s.Should().Be("f_Method");
        }
        $"{Field.Method}".Should().Be("{f_Method}");
        ((string)Field.Method).Should().Be("f_Method");
        Method(Field.Method);
    }
}
