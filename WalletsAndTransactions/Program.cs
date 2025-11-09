using System.Globalization;
using WalletsAndTransactions.IO;
using WalletsAndTransactions.Model;
using WalletsAndTransactions.View;

var repository = new Repository();
var app = new ConsoleApp(repository);
var exit = false;

var commands = new (string Name, Action Invoke)[]
{
    ("Импортировать данные из файла", app.OnImportData),
    ("Добавить кошелёк", app.OnAddWallet),
    ("Добавить транзакцию", app.OnAddTransaction),
    ("Вывести отформатированные данные для заданного месяца", app.OnPrintMonthlyTransactionsReport),
    ("Вывести 3 самых больших траты для каждого кошелька для указанного месяца", app.OnPrintTop3Expenses),
    ("Выйти", () => exit = true)
};

var commandsPrinter = TablePrinter.OfStringCells(
    commands
        .Select((command, i) => new[] { i.ToString(), command.Name })
        .Prepend(["ID", "Действие"])
        .ToArray());

InitStatic();
AskTheme();

ConsoleExt.WriteWarningLine("Использовать консоль в ide (как минимум, Rider) не рекомендуется");
Console.WriteLine("Вы можете вводить CTRL+C или CTRL+D (в зависимости от платформы)");
Console.WriteLine("для выхода из операций и для отмены подтверждений");

while (!exit)
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

void InitStatic()
{
    TablePrinter.AddConverter<Wallet>(wallet =>
    [
        wallet.Id.ToString(),
        wallet.Name,
        wallet.CurrencyId,
        wallet.StartingBalance.ToString(CultureInfo.CurrentCulture),
        wallet.Balance.ToString(CultureInfo.CurrentCulture)
    ]);

    TablePrinter.AddConverter<Transaction>(transaction =>
    [
        transaction.Id.ToString(),
        transaction.Date.ToString(CultureInfo.CurrentCulture),
        transaction.AbsoluteAmount.ToString(CultureInfo.CurrentCulture),
        transaction.Type == TransactionType.Income ? "Зачисление" : "Списание", // TODO locale formatting
        transaction.Description ?? "/Пусто/" // TODO locale formatting
    ]);
}