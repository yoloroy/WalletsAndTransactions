namespace WalletsAndTransactions;

public class TablePrinter(string[][] rows) // TODO Row as class with Divider as special case for handling headers and other stuff
{
    private readonly int[] _lengths = CalculateLengths(rows);

    // TODO guard checks for rows and move it to factory method

    public void Print(Action<string>? writeLine = null)
    {
        writeLine ??= Console.WriteLine;

        foreach (var row in rows)
        {
            writeLine(string.Join(" | ", row.Select((cell, i) => cell.PadRight(_lengths[i], ' '))));
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
