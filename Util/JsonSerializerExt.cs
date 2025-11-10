using System.Text.Json;

namespace Util;

public static class JsonSerializerExt
{
    /// <summary>
    /// Метод, оборачивающий <c>System.Text.Json.JsonSerializer.Deserialize</c> для удобного использования анонимных типов.
    /// Десериализует строку с JSON'ом внутри, соотвтетствуя (анонимному) типу передаваемого параметра <c>_</c>
    /// <br/>
    /// <br/>
    /// Исключения смотрите в <c>System.Text.Json.JsonSerializer.Deserialize</c>
    /// </summary>
    /// <param name="content">Строка с данными</param>
    /// <param name="_">Объект, из которого выводится тип</param>
    /// <typeparam name="T">Выведенный тип</typeparam>
    /// <see cref="System.Text.Json.JsonSerializer"/>
    /// <returns>Десериализованный объект</returns>
    public static T? DeserializeBy<T>(string content, T _) => JsonSerializer.Deserialize<T>(content);
}