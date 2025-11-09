using System.Globalization;
using WalletsAndTransactions.Model;

namespace WalletsAndTransactions.IO;

public partial class TablePrinter
{
    // can be made dynamically extendable for library-like usages
    private static readonly Dictionary<Type, Func<object, string[]>> Converters = new()
    {
        [typeof(string[])] = arr => (string[]) arr,

        [typeof(Wallet)] = obj =>
        {
            var wallet = (Wallet) obj;
            return
            [
                wallet.Id.ToString(),
                wallet.Name,
                wallet.CurrencyId,
                wallet.StartingBalance.ToString(CultureInfo.CurrentCulture),
                wallet.Balance.ToString(CultureInfo.CurrentCulture)
            ];
        },

        [typeof(Transaction)] = obj =>
        {
            var transaction = (Transaction) obj;
            return
            [
                transaction.Id.ToString(),
                transaction.WalletId.ToString(),
                transaction.Description ?? "/Пусто/", // TODO locale formatting
                transaction.Date.ToString(CultureInfo.CurrentCulture),
                transaction.AbsoluteAmount.ToString(CultureInfo.CurrentCulture),
                transaction.Type == TransactionType.Income ? "Зачисление" : "Списание" // TODO locale formatting
            ];
        }
    };
}