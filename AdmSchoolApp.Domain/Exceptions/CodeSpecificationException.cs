namespace AdmSchoolApp.Domain.Exceptions;

public class CodeSpecificationException : Exception
{
    public CodeSpecificationException() { }

    public CodeSpecificationException(string message) : base(message) { }

    public CodeSpecificationException(string message, Exception inner) : base(message, inner) { }
}