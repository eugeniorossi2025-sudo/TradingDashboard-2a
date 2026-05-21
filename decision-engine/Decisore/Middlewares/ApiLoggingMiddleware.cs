using Decisore.Repository;
using Decisore.Services;

namespace Decisore.Middlewares
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            LoggingService logService,
            DatabaseRepository db)
        {
            try
            {
                await _next(context);
            }
            finally
            {
                if (logService.HasLogs)
                {
                    db.SaveApiLog(
                        description: logService.GetConcatenatedLogs(),
                        category: logService.Category,
                        action: logService.Action
                    );
                }
            }
        }
    }
}