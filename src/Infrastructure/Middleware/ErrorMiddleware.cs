using Microsoft.AspNetCore.Http;

namespace Infrastructure.Middleware
{
    public class ErrorMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorMiddleware(RequestDelegate request) => _next = request;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var response = "Ocorreu um erro, valide com o suporte!";

#if DEBUG
                response = ex.Message;
#endif
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
