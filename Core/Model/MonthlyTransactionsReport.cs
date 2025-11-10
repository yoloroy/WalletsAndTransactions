namespace Core.Model;

/// <summary>
/// Сгруппированные транзакции по типам (Income/Expense),
/// группы отсортированные по общей сумме (по убыванию),
/// в каждой группе транзакции отсортированны по дате (от самых старых к самым новым).
/// </summary>
/// <param name="Incomes">Зачисления</param>
/// <param name="Expenses">Списания</param>
/// <param name="IncomesSum">Абсолютная сумма зачислений</param>
/// <param name="ExpensesSum">Абсолютная сумма списания</param>
public record struct MonthlyTransactionsReport(
    IReadOnlyList<Transaction> Incomes,
    IReadOnlyList<Transaction> Expenses,
    decimal IncomesSum,
    decimal ExpensesSum
);