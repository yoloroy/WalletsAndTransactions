using App.IO;
using Core.Model;
using System.Globalization;

namespace App.View;

public sealed class MainLoop(ConsoleApp app)
{
    private readonly (string Name, Action Invoke)[] _commands =
    [
        ("Импортировать данные из файла", app.OnImportData),
        ("Добавить кошелёк", app.OnAddWallet),
        ("Добавить транзакцию", app.OnAddTransaction),
        ("Вывести отформатированные данные для заданного месяца", app.OnPrintMonthlyTransactionsReport),
        ("Вывести 3 самых больших траты для каждого кошелька для указанного месяца", app.OnPrintTop3Expenses)
    ];

    public void Run()
    {
        var commandsPrinter = TablePrinter.OfStringCells(
            _commands
                .Select((command, i) => new[] { i.ToString(), command.Name })
                .Prepend(["ID", "Действие"])
                .ToArray());

        InitStatic();
        if (!TryAskTheme())
        {
            return;
        }

        ConsoleExt.WriteWarningLine("Использовать консоль в ide (как минимум, Rider) не рекомендуется");
        Console.WriteLine("Вы можете вводить CTRL+C или CTRL+D (в зависимости от платформы)");
        Console.WriteLine("для выхода из операций и для отмены подтверждений");

        var exit = false;
        while (!exit)
        {
            Console.WriteLine();
            commandsPrinter.Print();
            Console.WriteLine();

            try
            {
                _commands[ConsoleExt.ReadIntOrThrow()].Invoke();
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
    }

    private bool TryAskTheme()
    {
        Console.WriteLine("Если тема вашей консоли белая, введите что-либо и нажмите <Enter>,\n" +
                          "иначе нажмите <Enter> без ввода");

        var line = Console.ReadLine();
        if (line == null)
        {
            return false;
        }

        ConsoleExt.Init(line == ""
            ? new ConsoleExt.ColorsTheme(ConsoleColor.White, ConsoleColor.Yellow)
            : new ConsoleExt.ColorsTheme(ConsoleColor.Black, ConsoleColor.DarkYellow));

        return true;
    }

    private void InitStatic()
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
}