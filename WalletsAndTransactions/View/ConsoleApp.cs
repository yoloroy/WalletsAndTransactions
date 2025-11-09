using System.Globalization;
using System.Text.Json;
using NativeFileDialogNET;
using WalletsAndTransactions.IO;
using WalletsAndTransactions.Model;
using WalletsAndTransactions.POCOs;
using WalletsAndTransactions.Util;

namespace WalletsAndTransactions.View;

public class ConsoleApp(Repository repository)
{
    private static readonly string[] WalletFields = ["Id", "Название", "Валюта", "Начальный баланс", "Текущий Баланс"];
    private static readonly string[] TransactionFields = ["Id", "Дата", "Сумма", "Тип", "Описание"];

    private readonly Repository _repository = repository;

    public void OnImportData()
    {
        try
        {
            Console.WriteLine("Вам будет открыто окно для выбора файла");
            new NativeFileDialog()
                .AddFilter("json", "json")
                .SelectFile()
                .Open(out string? result);

            var path = result ?? AskFilePath();

            try
            {
                // Если логика усложнится, то следует вынести в отдельный класс
                var imported = JsonSerializerExt.DeserializeBy(File.ReadAllText(path), new
                {
                    Wallets = Array.Empty<WalletPOCO>(),
                    Transactions = Array.Empty<TransactionPOCO>()
                })!;

                if (_repository.TryLoad(imported.Wallets, imported.Transactions))
                {
                    Console.WriteLine($"Файл {path} был загружен");
                    Console.WriteLine($"Всего кошельков {_repository.Wallets.Count()}");
                    Console.WriteLine($"Всего транзакций {_repository.Transactions.Count()}");
                }
                else
                {
                    ConsoleExt.WriteWarningLine("Ошибка в загружаемых данных");
                }
            }
            catch (JsonException)
            {
                ConsoleExt.WriteWarningLine("Ошибка в формате файла");
            }
        }
        catch (CancellationException)
        {
            ConsoleExt.WriteWarningLine("Вы вышли из операции импорта из файла");
        }
        return;

        string AskFilePath()
        {
            Console.WriteLine("Введите путь к файлу JSON для загрузки данных:");
            return ConsoleExt.Retrying(
                ConsoleExt.ReadLineOrThrow, (
                    failMessage: "Путь к файлу не может быть пустым",
                    check: line => line.Length > 0
                ), (
                    failMessage: "Такого файла не существует",
                    check: Path.Exists
                ));
        }
    }

    public void OnAddWallet()
    {
        try
        {
            Console.WriteLine("Введите название кошелька:");
            var name = ConsoleExt.Retrying(
                ConsoleExt.ReadLineOrThrow, (
                    failMessage: "Название кошелька не должно быть пустым",
                    check: Wallet.NameIsNotEmpty
                ));

            Console.WriteLine("Введите идентификатор валюты кошелька:");
            var currency = ConsoleExt.Retrying(
                ConsoleExt.ReadLineOrThrow, (
                    failMessage: "Идентификатор валюты не может быть пустым",
                    check: Wallet.CurrencyIdIsNotEmpty
                ));

            Console.WriteLine("Введите начальный баланс кошелька:");
            var balance = ConsoleExt.Retrying(
                ConsoleExt.ReadDecimalOrThrow,
                formatFailMessage: "Введённое значение не соответствует формату десятичного числа", (
                    failMessage: "Баланс на кошельке не может быть ниже нуля",
                    check: Wallet.StartingBalanceIsNotNegative
                ));

            Console.WriteLine("Вы ввели:");
            Console.WriteLine($"\tНазвание: {name}");
            Console.WriteLine($"\tВалюта: {currency}");
            Console.WriteLine($"\tНачальный баланс: {balance}");
            Console.WriteLine();
            Console.WriteLine("Подтвердите ввод, нажатием <Enter>");
            ConsoleExt.ReadLineOrThrow();

            var wallet = _repository.AddWallet(name, currency, balance);
            Console.WriteLine("Добавлен новый кошелёк:");
            TablePrinter.Print([WalletFields, wallet]);
        }
        catch (CancellationException)
        {
            ConsoleExt.WriteWarningLine("Вы вышли из операции добавления кошелька");
        }
    }

    public void OnAddTransaction()
    {
        try
        {
            Console.WriteLine("Введите Id кошелька:");
            Wallet? outWallet = null;
            var walletId = ConsoleExt.Retrying(
                ConsoleExt.ReadIntOrThrow,
                formatFailMessage: "Вы ввели не целое число", (
                    failMessage: "Кошелька под таким Id не существует",
                    check: id => _repository.TryGetWalletById(id, out outWallet)
                ), (
                    failMessage: "Вы отменили выбор кошелька, повторите вновь",
                    check: _ => IsConfirmingWalletChoice(outWallet!)
                ));
            var wallet = outWallet!;

            Console.WriteLine("Введите дату транзакции: \"День Месяц Год\", (Без кавычек)");

            var today = DateOnly.FromDateTime(DateTime.Now);
            DateOnly date = default;
            ConsoleExt.Retrying(
                ConsoleExt.ReadLineOrThrow, (
                    failMessage: "Формат даты: \"День Месяц Год\", (Без кавычек)",
                    check: input => DateOnly.TryParseExact(input, "d M yyyy", out date)
                ), (
                    failMessage: "Вы не можете записать транзакцию из будущего",
                    check: _ => date <= today
                ));

            Console.WriteLine("Введите описание транзакции или пустую строку:");
            string? description = ConsoleExt.ReadLineOrThrow();
            if (description.Length == 0)
            {
                description = null;
            }

            Console.WriteLine(
                "Введите сумму операции:\n" +
                "(отрицательное число будет считаться снятием)\n" +
                "(положительное число будет считаться зачислением)");
            var update = ConsoleExt.Retrying(
                ConsoleExt.ReadDecimalOrThrow,
                formatFailMessage: "Введённое значение не соответствует формату десятичного числа", (
                    failMessage: "Операция без суммы не несёт смысла",
                    check: Transaction.AmountIsNonZero
                ), (
                    failMessage: "Вы не можете снять больше, чем есть на кошельке в данный момент",
                    check: wallet.SupportsTransactionUpdate
                ), (
                    failMessage: "Это операция не могла быть осуществлена, ведь тогда бы баланс ушёл в минус в тот день",
                    check: update => wallet.TransactionStoryWillFitWith(date, update)
                ));

            Console.WriteLine("Вы ввели:");
            TablePrinter.Print([
                ["Id кошелька", walletId.ToString()],
                ["Дата", date.ToString()],
                ["Сумма", decimal.Abs(update).ToString(CultureInfo.CurrentCulture)],
                ["Тип", update > 0 ? "Зачисление" : "Списание"],
                ["Описание транзакции", description ?? "/Пусто/"]]);
            Console.WriteLine("Подтвердите ввод, нажатием <Enter>");
            ConsoleExt.ReadLineOrThrow();

            if (!_repository.TryAddTransaction(
                    walletId, date, update, description,
                    out var transaction))
            {
                Console.WriteLine("Неизвестная ошибка при добавлении транзакции");
                return;
            }

            Console.WriteLine("Добавлена новая транзакция:");
            TablePrinter.Print([TransactionFields, transaction]);
        }
        catch (CancellationException)
        {
            ConsoleExt.WriteWarningLine("Вы вышли из операции добавления транзакции");
        }

        return;

        bool IsConfirmingWalletChoice(Wallet wallet)
        {
            try
            {
                Console.WriteLine($"Вы выбрали кошелёк под названием \"{wallet.Name}\".");
                Console.WriteLine("Подтвердите выбор, нажатием <Enter>");
                ConsoleExt.ReadLineOrThrow();
                return true;
            }
            catch (CancellationException)
            {
                return false;
            }
        }
    }

    public void OnPrintMonthlyTransactionsReport()
    {
        if (_repository.IsEmpty)
        {
            Console.WriteLine("Кошельков нет");
            return;
        }
        if (!_repository.Transactions.Any())
        {
            Console.WriteLine("Транзакций нет");
            return;
        }

        var year = ReadYear();
        var month = ReadMonth();

        var (incomes, expenses, incomesSum, expensesSum) = _repository.GetMonthlyTransactionsReport(year, month);

        if (incomes.Count == 0)
        {
            Console.WriteLine("Зачислений не произодилось");
            Console.WriteLine();
            TablePrinter.Print(expenses.Prepend<object>(TransactionFields).ToArray());
        }
        else if (expenses.Count == 0)
        {
            Console.WriteLine("Списаний не произодилось");
            Console.WriteLine();
            TablePrinter.Print(incomes.Prepend<object>(TransactionFields).ToArray());
        }
        else if (incomesSum >= expensesSum)
        {
            Console.WriteLine($"Сумма зачислений: {incomesSum}");
            TablePrinter.Print(incomes.Prepend<object>(TransactionFields).ToArray());
            Console.WriteLine();
            Console.WriteLine($"Сумма списаний: {expensesSum}");
            TablePrinter.Print(expenses.Prepend<object>(TransactionFields).ToArray());
        }
        else
        {
            Console.WriteLine($"Сумма списаний: {expensesSum}");
            TablePrinter.Print(expenses.Prepend<object>(TransactionFields).ToArray());
            Console.WriteLine();
            Console.WriteLine($"Сумма зачислений: {incomesSum}");
            TablePrinter.Print(incomes.Prepend<object>(TransactionFields).ToArray());
        }
    }

    public void OnPrintTop3Expenses()
    {
        if (_repository.IsEmpty)
        {
            Console.WriteLine("Кошельков нет");
            return;
        }
        if (!_repository.Transactions.Any())
        {
            Console.WriteLine("Транзакций нет");
            return;
        }

        var year = ReadYear();
        var month = ReadMonth();

        var tops = _repository.GetTop3TransactionsByMonth(year, month).ToList();

        var walletsRows = TablePrinter
            .OfAny(tops.Select(t => t.Wallet).Prepend<object>(WalletFields).ToArray())
            .GetLines();

        // Пропустим заголовок
        Console.WriteLine(walletsRows.First());
        var blocks = walletsRows.Skip(1).Zip(tops.Select(top => top.Top3));

        foreach (var (walletLine, transactions) in blocks)
        {
            var transactionLines = TablePrinter
                .OfAny(transactions.Prepend<object>(TransactionFields).ToArray())
                .GetLines();

            Console.WriteLine(walletLine);
            foreach (var transactionLine in transactionLines)
            {
                Console.WriteLine($"  + {transactionLine}");
            }

            Console.WriteLine();
        }
    }

    private static int ReadMonth()
    {
        Console.WriteLine("Введите месяц (1-12):");
        return ConsoleExt.Retrying(
            ConsoleExt.ReadIntOrThrow,
            formatFailMessage: "Вы ввели не целое число", (
                failMessage: "Месяц должен быть от 1 до 12",
                check: month => month is >= 1 and <= 12
            ));
    }

    private static int ReadYear()
    {
        Console.WriteLine("Введите год:");
        return ConsoleExt.Retrying(
            ConsoleExt.ReadIntOrThrow,
            formatFailMessage: "Вы ввели не целое число", (
                failMessage: "Год не может быть отрицательным",
                check: year => year > 0
            ));
    }
}