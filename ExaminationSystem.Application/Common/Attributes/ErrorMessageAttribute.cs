namespace ExaminationSystem.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ErrorMessageAttribute : Attribute
{
    public string Message { get; }
    
    public ErrorMessageAttribute(string message)
    {
        Message = message;
    }
}
