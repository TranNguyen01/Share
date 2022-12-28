namespace Moto.Services
{
    public interface IResponseCacheService
    {
        Task SetResponseCacheAsync(string key, object value, TimeSpan timespan);

        Task<string> GetResponseCacheAsync(string key);

        Task ClearResponseCacheAsync(string key);
    }
}
