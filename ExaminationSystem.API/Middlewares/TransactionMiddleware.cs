using ExaminationSystem.Infrastructure.Data;
using System.Diagnostics;

namespace ExaminationSystem.API.Middlewares;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TransactionMiddleware> _logger;

    public TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        // Skip transactions for read operations
        var method = context.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
        {
            await _next(context);
            return;
        }

        // Start transaction for write operations
        var transaction = await dbContext.Database.BeginTransactionAsync();
        _logger.LogDebug("Transaction started for {Method} {Path}", 
            context.Request.Method, context.Request.Path);
        
        try
        {
            await _next(context);

            // Commit if response is successful (2xx status codes)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                await transaction.CommitAsync();
                _logger.LogDebug("Transaction committed for {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
            }
            else
            {
                // Debugger breakpoint for rollback scenarios
                if (Debugger.IsAttached) Debugger.Break();
                
                await transaction.RollbackAsync();
                _logger.LogWarning("Transaction rolled back for {Method} {Path} (Status: {StatusCode})", 
                    context.Request.Method, context.Request.Path, context.Response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            // Debugger breakpoint for exception rollback
            if (Debugger.IsAttached) Debugger.Break();
            
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Transaction rolled back due to exception");
            throw; // Re-throw to be handled by global exception handler
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }
}
