using ExaminationSystem.API.Models.Responses;
using ExaminationSystem.Application.Common.Attributes;
using System.Collections.Concurrent;
using System.Reflection;

namespace ExaminationSystem.API.Extensions;

public static class EnumExtensions
{
    // Cache for performance - reflection only happens once per enum value
    private static readonly ConcurrentDictionary<ApiErrorCode, string> _messageCache = new();

    /// <summary>
    /// Gets the error message from the ErrorMessage attribute on the enum value.
    /// Messages are cached for performance.
    /// </summary>
    public static string GetErrorMessage(this ApiErrorCode errorCode)
    {
        return _messageCache.GetOrAdd(errorCode, code =>
        {
            var field = typeof(ApiErrorCode).GetField(code.ToString());
            var attribute = field?.GetCustomAttribute<ErrorMessageAttribute>();
            return attribute?.Message ?? "An error occurred";
        });
    }
}
