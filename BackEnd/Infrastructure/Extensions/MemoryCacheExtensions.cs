using System.Collections;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace LawyerProject.Infrastructure.Extensions;

public static class MemoryCacheExtensions
{
    public static IEnumerable<string> GetKeys(this IMemoryCache memoryCache)
    {
        var field = typeof(MemoryCache).GetProperty("EntriesCollection",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var collection = field!.GetValue(memoryCache) as ICollection;
        var keys = new List<string>();

        if (collection != null)
        {
            foreach (var item in collection)
            {
                var methodInfo = item.GetType().GetProperty("Key");
                var key = methodInfo!.GetValue(item) as string;
                if (key != null)
                {
                    keys.Add(key);
                }
            }
        }

        return keys;
    }
}
