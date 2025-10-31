namespace AdmSchoolApp.Domain.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal class CodeSpecificationAttribute : Attribute
{
    public CodeSpecificationAttribute(string? code, string? message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        
        Message = message;
        Code = code;
    }

    public string? Code { get; set; }
    public string? Message { get; set; }
}