using System;

namespace MccSoft.TemplateApp.Http;

public class SignInException : Exception
{
    public SignInException(string stringContent) : base(stringContent) { }
}
