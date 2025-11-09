using NUnit.Framework;
using WalletsAndTransactions.Model;
using System;
using System.Linq;

namespace WalletsAndTransactions.Tests.Model;

[TestFixture]
[TestOf(typeof(Repository))]
public class RepositoryTop3Test
{
    private Repository _repository;
    private Wallet _wallet1;
    private Wallet _wallet2;

    private readonly DateOnly _targetDate = new(2025, 10, 15);
    private readonly DateOnly _targetDateLater = new(2025, 10, 20);
    private readonly DateOnly _otherMonthDate = new(2025, 11, 15);
    private readonly DateOnly _otherYearDate = new(2024, 10, 15);

    [SetUp]
    public void SetUp()
    {
        _repository = new Repository();
        _wallet1 = _repository.AddWallet("Wallet $", "USD", 10000);
        _wallet2 = _repository.AddWallet("Wallet €", "EUR", 10000);
    }

    [Test]
    public void GetTop3_ReturnsOnlyTop3SortedExpenses_WhenMoreThan3Exist()
    {
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, -100, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, -50, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, -200, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, -10, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, -5, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, 5000, "Test", out _);

        var result = _repository.GetTop3TransactionsByMonth(_targetDate.Year, _targetDate.Month).ToList();

        Assert.That(result, Has.Count.EqualTo(1));

        var wallet1Result = result.FirstOrDefault(r => r.Wallet.Id == _wallet1.Id);
        Assert.That(wallet1Result, Is.Not.Null);

        Assert.That(wallet1Result.Top3.Count(), Is.EqualTo(3));

        var amounts = wallet1Result.Top3.Select(t => t.AbsoluteAmount).ToList();
        Assert.That(amounts, Is.EqualTo([200m, 100m, 50m]));
        Assert.That(amounts, Is.Ordered.Descending);
    }

    [Test]
    public void GetTop3_ReturnsOnlyMatchingExpenses_WhenLessThan3Exist()
    {
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, -100, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, -50, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, 500, "Test", out _);

        var result = _repository.GetTop3TransactionsByMonth(_targetDate.Year, _targetDate.Month).ToList();

        Assert.That(result, Has.Count.EqualTo(1));

        var wallet1Result = result.First(r => r.Wallet.Id == _wallet1.Id);
        Assert.That(wallet1Result.Top3.Count(), Is.EqualTo(2));

        var amounts = wallet1Result.Top3.Select(t => t.AbsoluteAmount).ToList();
        Assert.That(amounts, Is.EqualTo([100m, 50m]));
    }

    [Test]
    public void GetTop3_ReturnsEmpty_WhenNoMatchingExpensesExist()
    {
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, 500, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _otherMonthDate, -1000, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _otherYearDate, -2000, "Test", out _);

        var result = _repository.GetTop3TransactionsByMonth(_targetDate.Year, _targetDate.Month).ToList();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetTop3_ReturnsEmpty_WhenRepositoryIsEmpty()
    {
        var emptyRepository = new Repository();

        var result = emptyRepository.GetTop3TransactionsByMonth(_targetDate.Year, _targetDate.Month).ToList();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetTop3_HandlesMultipleWalletsCorrectly()
    {
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, -100, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDate, -50, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDateLater, -200, "Test", out _);
        _repository.TryAddTransaction(_wallet1.Id, _targetDateLater, -10, "Test", out _);

        _repository.TryAddTransaction(_wallet2.Id, _targetDate, -75, "Test", out _);
        _repository.TryAddTransaction(_wallet2.Id, _targetDateLater, -25, "Test", out _);
        _repository.TryAddTransaction(_wallet2.Id, _otherMonthDate, -1000, "Test", out _);

        var resultMap = _repository
            .GetTop3TransactionsByMonth(_targetDate.Year, _targetDate.Month)
            .ToDictionary(r => r.Wallet.Id, r => r.Top3.ToList());

        Assert.That(resultMap, Has.Count.EqualTo(2));
        Assert.That(resultMap.ContainsKey(_wallet1.Id), Is.True);
        Assert.That(resultMap.ContainsKey(_wallet2.Id), Is.True);

        var wallet1Transactions = resultMap[_wallet1.Id];
        Assert.That(wallet1Transactions, Has.Count.EqualTo(3));
        var amounts1 = wallet1Transactions.Select(t => t.AbsoluteAmount).ToList();
        Assert.That(amounts1, Is.EqualTo([200m, 100m, 50m]));

        var wallet2Transactions = resultMap[_wallet2.Id];
        Assert.That(wallet2Transactions, Has.Count.EqualTo(2));
        var amounts2 = wallet2Transactions.Select(t => t.AbsoluteAmount).ToList();
        Assert.That(amounts2, Is.EqualTo([75m, 25m]));
    }
}