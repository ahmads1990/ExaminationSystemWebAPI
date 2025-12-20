namespace ExaminationSystem.Infrastructure.Configs;

public class RedisConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Ssl { get; set; } = false;
    public string InstanceName { get; set; } = string.Empty;
    public bool AbortOnConnectFail { get; set; } = false;
}
