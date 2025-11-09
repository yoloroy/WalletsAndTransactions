namespace WalletsAndTransactions.IO;

public partial class TablePrinter
{
    private static readonly Dictionary<Type, Func<object, string[]>> Converters = new()
    {
        [typeof(string[])] = arr => (string[]) arr
    };

    public static void AddConverter<T>(Func<T, string[]> converter) => Converters.Add(typeof(T), obj => converter((T) obj));
}