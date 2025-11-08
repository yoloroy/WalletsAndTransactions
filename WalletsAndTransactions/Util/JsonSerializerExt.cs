using System.Text.Json;

namespace WalletsAndTransactions.Util;

public static class JsonSerializerExt
{
    public static T? DeserializeBy<T>(string content, T _) => JsonSerializer.Deserialize<T>(content);
}