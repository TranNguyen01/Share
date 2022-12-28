using Microsoft.AspNetCore.Mvc.Filters;
using Moto.Services;
using System.Text;

namespace Moto.Attributes
{
    public class ClearCacheAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string[] _cacheKeys;
        private readonly bool _clearCurrentPath;

        public ClearCacheAttribute()
        {
            _cacheKeys = new string[0];
            _clearCurrentPath = true;
        }

        public ClearCacheAttribute(bool clearCurrentPath = false, params string[] cacheKeys)
        {
            _cacheKeys = cacheKeys;
            _clearCurrentPath = clearCurrentPath;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheSerivce = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();

            if (_clearCurrentPath)
            {
                var key = generateResponseCacheKey(context.HttpContext.Request);
                await cacheSerivce.ClearResponseCacheAsync(key);
            }

            if (_cacheKeys.Length > 0)
            {
                foreach (var cacheKey in _cacheKeys)
                {
                    await cacheSerivce.ClearResponseCacheAsync(cacheKey);
                }
            }
            await next();
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
