namespace Core.Caching.HybridCaching
{
    public class HybridCachingOptions
    {
        public int DistributedCacheExpirationMinutes { get; set; }

        public int LocalCacheExpirationMinutes { get; set; }
    }
}