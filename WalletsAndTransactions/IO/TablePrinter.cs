namespace WalletsAndTransactions.IO;

public partial class TablePrinter
{
    private readonly int[] _lengths;
    private readonly string[][] _rows;

    private TablePrinter(string[][] rows)
    {
        _rows = rows;
        _lengths = CalculateLengths(rows);
    }

    public static TablePrinter OfStringCells(string[][] rows)
    {
        if (rows.Length == 0)
        {
            return new TablePrinter([]);
        }

        var cellsCount = rows[0].Length;
        if (rows.Any(row => row.Length != cellsCount))
        {
            throw new ArgumentException("Количество ячеек в ряду должно быть одинаковым для всех рядов", nameof(rows));
        }

        return new TablePrinter(rows);
    }

    public static TablePrinter OfAny(object[] rows) =>
        new ((from row in rows select Converters[row.GetType()](row)).ToArray());

    public static void Print(string[][] rows) => new TablePrinter(rows).Print();

    public static void Print(object[] rows) => OfAny(rows).Print();

    public IEnumerable<string> GetLines() => _rows.Select(row => row
        .Select((cell, i) => cell.PadRight(_lengths[i], ' ')))
        .Select(spacedCells => string.Join(" | ", spacedCells));

    public void Print()
    {
        foreach (var line in GetLines())
        {
            Console.WriteLine(line);
        }
    }

    private static int[] CalculateLengths(string[][] rows)
    {
        var lengths = new int[rows[0].Length];
        for (var i = 0; i < rows[0].Length; i++)
        {
            lengths[i] = rows.Select(row => row[i].Length).Max();
        }

        return lengths;
    }
}
