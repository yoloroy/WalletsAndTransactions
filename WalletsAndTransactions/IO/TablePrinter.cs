namespace WalletsAndTransactions.IO;

public partial class TablePrinter(string[][] rows) // TODO Row as class with Divider as special case for handling headers and other stuff
{
    private readonly int[] _lengths = CalculateLengths(rows);

    // TODO guard checks for rows and move it to factory method

    public static TablePrinter OfAny(object[] rows) =>
        new ((from row in rows select Converters[row.GetType()](row)).ToArray());

    public static void Print(string[][] rows) => new TablePrinter(rows).Print();

    public static void Print(object[] rows) => OfAny(rows).Print();

    public IEnumerable<string> GetLines()
    {
        foreach (var row in rows)
        {
            yield return string.Join(" | ", row.Select((cell, i) => cell.PadRight(_lengths[i], ' ')));
        }
    }

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
