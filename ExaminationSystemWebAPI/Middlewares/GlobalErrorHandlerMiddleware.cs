namespace ExaminationSystemWebAPI.Middlewares;

public class GlobalErrorHandlerMiddleware
{
    RequestDelegate _nextAction;
    ILogger<GlobalErrorHandlerMiddleware> _logger;

    public GlobalErrorHandlerMiddleware(RequestDelegate nextAction, ILogger<GlobalErrorHandlerMiddleware> logger)
    {
        _nextAction = nextAction;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _nextAction(context);
        }
        catch (Exception ex)
        {
            File.WriteAllText("D:\\Logs.txt", $"Error occures: {ex.Message}");

            _logger.LogError(ex.Message);

            //context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync("Exception");
        }

        //return Task.CompletedTask;
    }
}
