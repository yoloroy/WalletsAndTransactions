using WalletsAndTransactions.IO;
using WalletsAndTransactions.Model;
using WalletsAndTransactions.View;

var repository = new Repository();
var app = new ConsoleApp(repository);
var exit = false;

var commands = new (string Name, Action Invoke)[]
{
    ("Импортировать данные из файла", app.AskToImportData),
    ("Добавить кошелёк", app.AskToAddWallet),
    ("Добавить транзакцию", app.AskToAddTransaction),
    ("Вывести отформатированные данные для заданного месяца", () => {}),
    ("Вывести 3 самых больших траты для каждого кошелька для указанного месяца", () => {}),
    ("Выйти", () => exit = true)
};

var commandsPrinter = new TablePrinter(
    commands
        .Select((command, i) => new[] { i.ToString(), command.Name })
        .Prepend(["ID", "Действие"])
        .ToArray());

AskTheme();
Console.WriteLine("Вы можете вводить CTRL+C для выхода из операций и для отмены подтверждений");
while (!exit)
{
    Loop();
}

Console.WriteLine("\nПрощайте");
return 0;

void AskTheme()
{
    Console.WriteLine("Если тема вашей консоли белая, введите что-либо и нажмите <Enter>,\n" +
                      "иначе нажмите <Enter> без ввода");

    var line = Console.ReadLine();
    if (line == null)
    {
        exit = true;
    }
    else
    {
        ConsoleExt.Init(line == ""
            ? new ConsoleExt.ColorsTheme(ConsoleColor.White, ConsoleColor.Yellow)
            : new ConsoleExt.ColorsTheme(ConsoleColor.Black, ConsoleColor.DarkYellow));
    }
}

void Loop()
{
    Console.WriteLine();
    commandsPrinter.Print();
    Console.WriteLine();

    try
    {
        commands[ConsoleExt.ReadIntOrThrow()].Invoke();
    }
    catch (CancellationException)
    {
        exit = true;
    }
    catch (FormatException)
    {
        ConsoleExt.WriteWarningLine("Вы ошиблись при вводе ID команды");
    }
    catch (IndexOutOfRangeException)
    {
        ConsoleExt.WriteWarningLine("Команды под таким ID не существует, перепрочтите список");
    }
}