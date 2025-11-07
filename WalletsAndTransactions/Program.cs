using WalletsAndTransactions;

var exit = false;

var commands = new (string Name, Action Invoke)[]
{
    ("Внести данные", () => {}),
    ("Вывести данные", () => {}),
    ("Отформатировать данные для заданного месяца", () => {}),
    ("Вывести 3 самых больших траты для каждого кошелька для указанного месяца", () => {}),
    ("Выйти", () => exit = true)
};

var commandsPrinter = new TablePrinter
(
    commands
        .Select((command, i) => new[] { i.ToString(), command.Name })
        .Prepend(["ID", "Действие"])
        .ToArray()
);

do
{
    Console.ForegroundColor = ConsoleColor.White;
    commandsPrinter.Print();
    Console.WriteLine();

    try
    {
        commands[ReadInt()].Invoke();
    }
    catch (CancelException)
    {
        break;
    }
    catch (FormatException)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("! Вы ошиблись при вводе ID команды");
    }
    catch (IndexOutOfRangeException)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("! Команды под таким ID не существует, перепрочтите список");
    }
} while (!exit);

Console.WriteLine("Прощайте");
return;

int ReadInt()
{
    Console.Write("> ");
    var line = Console.ReadLine();
    if (line == null)
    {
        throw new CancelException();
    }

    try
    {
        return Convert.ToInt32(line);
    }
    catch (FormatException)
    {
        throw new FormatException();
    }
}

internal class CancelException : Exception;
