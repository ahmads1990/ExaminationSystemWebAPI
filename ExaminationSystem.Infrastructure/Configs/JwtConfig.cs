namespace ExaminationSystem.Infrastructure.Configs;

public static class JwtConfig
{
    public static string Key { get; set; } = string.Empty;
    public static string Issuer { get; set; } = string.Empty;
    public static string Audience { get; set; } = string.Empty;
    public static double DurationInHours { get; set; }
}

