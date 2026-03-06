namespace ExaminationSystem.Application.Common.Attributes;

/// <summary>
/// Custom attribute for providing human-readable error messages for enum fields.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ErrorMessageAttribute : Attribute
{
    #region Public Methods

    public string Message { get; }

    #endregion

    #region Constructors

    public ErrorMessageAttribute(string message)
    {
        Message = message;
    }

    #endregion
}
