using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moto.Services;
using System.Text;

namespace Moto.Atributes
{
    public class CacheAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeValid;

        public CacheAttribute(int timeValid = 10)
        {
            _timeValid = timeValid;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheSerivce = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
            var responseCacheKey = generateResponseCacheKey(context.HttpContext.Request);
            var responseMessage = await cacheSerivce.GetResponseCacheAsync(responseCacheKey);
            if (string.IsNullOrEmpty(responseMessage))
            {
                var resolvedContext = await next();
                if (resolvedContext.Result is OkObjectResult newResponseResult)
                {
                    await cacheSerivce.SetResponseCacheAsync(responseCacheKey, newResponseResult.Value, new TimeSpan(0, 0, _timeValid));
                    return;
                }
            }

            var result = new ContentResult()
            {
                Content = responseMessage,
                ContentType = "application/json",
                StatusCode = 200
            };

            context.Result = result;
        }

        private string generateResponseCacheKey(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach (var (key, value) in request.Query)
            {
                keyBuilder.Append($"_{key} = {value}");
            }
            return keyBuilder.ToString();
        }
    }
}
