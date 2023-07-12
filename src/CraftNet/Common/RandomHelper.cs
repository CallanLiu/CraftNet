#nullable enable
namespace CraftNet;

public static class RandomHelper
{
    public static T? Rand<T>(this IReadOnlyList<T> self)
    {
        if (self is null or { Count: 0 })
            return default;

        int idx = Random.Shared.Next(0, self.Count);
        return self[idx];
    }
}