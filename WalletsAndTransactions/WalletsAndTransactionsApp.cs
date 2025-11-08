using System.Globalization;
using WalletsAndTransactions.Entities;
using WalletsAndTransactions.IO;

namespace WalletsAndTransactions;

public class WalletsAndTransactionsApp
{
    private static readonly string[] WalletFields = ["Id", "Название", "Валюта", "Начальный баланс", "Текущий Баланс"];
    private static readonly string[] TransactionFields = ["Id", "Id кошелька", "Описание", "Дата", "Сумма", "Тип"];

    private readonly List<Wallet> _wallets = [];
    private readonly List<Transaction> _transactions = [];

    public void AskToAddWallet()
    {
        try
        {
            Console.WriteLine("Введите название кошелька:");
            var name = ConsoleExt.Retrying(
                ConsoleExt.ReadLineOrThrow, (
                    failMessage: "Название кошелька не должно быть пустым",
                    check: line => line.Length > 0
                ));

            Console.WriteLine("Введите идентификатор валюты кошелька:");
            var currency = ConsoleExt.Retrying(
                ConsoleExt.ReadLineOrThrow, (
                    failMessage: "Идентификатор валюты не может быть пустым",
                    check: line => line.Length > 0
                ));

            Console.WriteLine("Введите начальный баланс кошелька:");
            var balance = ConsoleExt.Retrying(
                ConsoleExt.ReadDecimalOrThrow,
                formatFailMessage: "Введённое значение не соответствует формату десятичного числа", (
                    failMessage: "Баланс на кошельке не может быть ниже нуля",
                    check: value => value >= 0
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
            var walletId = ConsoleExt.Retrying(
                ConsoleExt.ReadIntOrThrow,
                formatFailMessage: "Вы ввели не целое число", (
                    failMessage: "Кошелька под таким Id не существует",
                    check: id => _wallets.Any(wallet => wallet.Id == id)
                ), (
                    failMessage: "Вы отменили выбор кошелька, повторите вновь",
                    check: IsConfirmingWalletChoice
                ));
            var wallet = _wallets.First(wallet => wallet.Id == walletId);

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
                    check: value => value != 0
                ), (
                    failMessage: "Вы не можете снять больше, чем есть на кошельке в данный момент",
                    check: update => wallet.Balance + update >= 0
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
            ConsoleExt.WriteWarningLine("Вы вышли из операции добавления кошелька");
        }

        return;

        bool IsConfirmingWalletChoice(int id)
        {
            try
            {
                var walletName = _wallets.First(wallet => wallet.Id == id).Name;
                Console.WriteLine($"Вы выбрали кошелёк под названием \"{walletName}\".");
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