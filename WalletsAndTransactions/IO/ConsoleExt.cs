using System.Globalization;

namespace WalletsAndTransactions.IO;

public static class ConsoleExt
{
    private static ColorsTheme _theme;

    public static void Init(ColorsTheme theme)
    {
        _theme = theme;
        Console.ForegroundColor = _theme.TextColor;
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
        };
    }

    public static T Retrying<T>(Func<T> readOperation, params (string failMessage, Func<T, bool> check)[] predicates) =>
        Retrying(readOperation, null, predicates);

    public static T Retrying<T>(
        Func<T> readOperation,
        string? formatFailMessage = null,
        params (string failMessage, Func<T, bool> check)[] predicates)
    {
        while (true)
        {
            try
            {
                var value = readOperation();

                foreach (var predicate in predicates)
                {
                    if (!predicate.check(value))
                    {
                        throw new ArgumentException(predicate.failMessage);
                    }
                }

                return value;
            }
            catch (FormatException)
            {
                if (formatFailMessage == null)
                {
                    throw;
                }

                WriteWarningLine(formatFailMessage);
            }
            catch (ArgumentException e)
            {
                WriteWarningLine(e.Message);
            }
        }
    }

    /// <exception cref="CancellationException">Выбрасывается, если ввод прерван (конец файла (EOF) или сигнал прерывания)</exception>
    /// <exception cref="FormatException">Выбрасывается, если введено не число</exception>
    /// <returns>Введённое целое число</returns>
    public static int ReadIntOrThrow() => int.Parse(ReadLineOrThrow());

    /// <exception cref="CancellationException">Выбрасывается, если ввод прерван (конец файла (EOF) или сигнал прерывания)</exception>
    /// <exception cref="FormatException">Выбрасывается, если введено не число</exception>
    /// <returns>Введённое десятичное число</returns>
    public static decimal ReadDecimalOrThrow()
    {
        decimal value;
        var line = ReadLineOrThrow();
        var parsed =
            decimal.TryParse(line, CultureInfo.CurrentCulture, out value) ||
            decimal.TryParse(line, CultureInfo.InvariantCulture, out value);

        if (!parsed)
        {
            throw new FormatException();
        }

        return value;
    }

    public static string ReadLineOrThrow()
    {
        Console.Write("> ");
        var line = Console.ReadLine();
        if (line == null)
        {
            throw new CancellationException();
        }

        return line;
    }

    public static void WriteWarningLine(string line)
    {
        Console.ForegroundColor = _theme.WarningColor;
        Console.WriteLine($"! {line}");
        Console.ForegroundColor = _theme.TextColor;
    }

    public record struct ColorsTheme(
        ConsoleColor TextColor,
        ConsoleColor WarningColor);
}