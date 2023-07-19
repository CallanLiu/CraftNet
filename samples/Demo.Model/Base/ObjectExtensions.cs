using System.Text.Json;

namespace Demo;

public static class ObjectExtensions
{
    public static string ToJson(this object self)
    {
        return self is null ? string.Empty : JsonSerializer.Serialize(self);
    }
}