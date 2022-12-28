using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace Moto.Services
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        public ResponseCacheService(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer)
        {
            _distributedCache = distributedCache;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<string> GetResponseCacheAsync(string key)
        {
            var response = await _distributedCache.GetStringAsync(key);
            return string.IsNullOrEmpty(response) ? string.Empty : response;
        }

        public async Task SetResponseCacheAsync(string key, object response, TimeSpan timespan)
        {
            var serializerResponse = JsonConvert.SerializeObject(response, Formatting.Indented,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            await _distributedCache.SetStringAsync(key, serializerResponse, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = timespan
            });
        }

        public async Task ClearResponseCacheAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }
    }
}
