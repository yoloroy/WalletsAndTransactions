using System.Globalization;
using System.Text.Json;
using WalletsAndTransactions.IO;
using WalletsAndTransactions.Model;
using WalletsAndTransactions.POCOs;
using WalletsAndTransactions.Util;
using FileDialog = NativeFileDialogCore.Dialog;

namespace WalletsAndTransactions.View;

public class WalletsAndTransactionsApp
{
    private static readonly string[] WalletFields = ["Id", "Название", "Валюта", "Начальный баланс", "Текущий Баланс"];
    private static readonly string[] TransactionFields = ["Id", "Id кошелька", "Описание", "Дата", "Сумма", "Тип"];

    private readonly List<Wallet> _wallets = [];
    private readonly List<Transaction> _transactions = [];

    public void AskToImportData()
    {
        try
        {
            Console.WriteLine("Вам будет открыто окно для выбора файла");
            var result = FileDialog.FileOpen("json");

            if (result.IsCancelled)
            {
                throw new CancellationException();
            }
            if (result.IsError)
            {
                ConsoleExt.WriteWarningLine($"Что-то пошло не так: {result.ErrorMessage!}");
            }

            var path = result.Path ?? AskFilePath();

            ConsoleExt.WriteWarningLine($"TODO: OPEN {path}");

            try
            {
                var imported = JsonSerializerExt.DeserializeBy(File.ReadAllText(path), new
                {
                    Wallets = Array.Empty<WalletPOCO>(),
                    Transactions = Array.Empty<TransactionPOCO>()
                })!;

                _wallets.Clear();
                _wallets.AddRange(imported.Wallets.Select(poco => poco.ToEntity(_transactions)).ToList());
                _transactions.Clear();
                _transactions.AddRange(imported.Transactions.Select(poco => poco.ToEntity()).ToList());
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

    public void AskToAddWallet()
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

            var wallet = new Wallet(_wallets.Count, name, currency, balance, _transactions);
            _wallets.Add(wallet);
            Console.WriteLine("Добавлен новый кошелёк:");
            TablePrinter.Print([WalletFields, wallet]);
        }
        catch (CancellationException)
        {
            ConsoleExt.WriteWarningLine("Вы вышли из операции добавления кошелька");
        }
    }

    public void AskToAddTransaction()
    {
        try
        {
            Console.WriteLine("Введите Id кошелька:");
            Wallet? outWallet = null;
            var walletId = ConsoleExt.Retrying(
                ConsoleExt.ReadIntOrThrow,
                formatFailMessage: "Вы ввели не целое число", (
                    failMessage: "Кошелька под таким Id не существует",
                    check: id => (outWallet = _wallets.FirstOrDefault(w => w.Id == id)) != null
                ), (
                    failMessage: "Вы отменили выбор кошелька, повторите вновь",
                    check: _ => IsConfirmingWalletChoice(outWallet!)
                ));
            var wallet = outWallet!;

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
                ));

            Console.WriteLine("Вы ввели:");
            TablePrinter.Print([
                ["Id кошелька", walletId.ToString()],
                ["Описание транзакции", description ?? "/Пусто/"],
                ["Сумма", decimal.Abs(update).ToString(CultureInfo.CurrentCulture)],
                ["Тип", update > 0 ? "Зачисление" : "Списание"]]);
            Console.WriteLine("Подтвердите ввод, нажатием <Enter>");
            ConsoleExt.ReadLineOrThrow();

            var transaction = new Transaction(
                _transactions.Count,
                walletId,
                DateOnly.FromDateTime(DateTime.Now),
                update,
                description);

            if (!wallet.TryAddTransaction(transaction))
            {
                ConsoleExt.WriteWarningLine("Ошибка при добавлении транзакции");
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
}