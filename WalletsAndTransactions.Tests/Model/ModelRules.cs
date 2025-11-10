using Core;
using Core.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WalletsAndTransactions.Tests.Model;

[TestFixture]
[TestOf(typeof(Wallet))]
[TestOf(typeof(Transaction))]
[TestOf(typeof(Repository))]
public class ModelRules
{
    [Test]
    public void Wallet_TryAddTransaction_WhenTransactionPutsIntoNegative()
    {
        var wallet = new Wallet(0, "test", "TST", 0.0m, []);
        var transaction = new Transaction(0, DateOnly.MinValue, -0.01m);

        Assert.That(wallet.TryAddTransaction(transaction), Is.False);
        Assert.That(wallet.Transactions, Does.Not.Contains(transaction));
    }

    [Test]
    public void Wallet_TryAddTransaction_WhenOk()
    {
        var wallet = new Wallet(0, "test", "TST", 0.0m, []);
        var transaction = new Transaction(0, DateOnly.MinValue, +0.01m);

        Assert.That(wallet.TryAddTransaction(transaction), Is.True);
        Assert.That(wallet.Transactions, Does.Contain(transaction));
    }

    [Test]
    public void Repository_Load_None()
    {
        var wallet = new Wallet(0, "test", "TST", 0.0m, []);
        var transaction = new Transaction(0, DateOnly.MinValue, +0.01m);

        Assert.That(wallet.TryAddTransaction(transaction), Is.True);
        Assert.That(wallet.Transactions, Does.Contain(transaction));
    }

    [Test]
    public void Wallet_TryAddTransaction_FailsWhenPastExpenseInvalidatesFutureBalance()
    {
        var dateDay1 = new DateOnly(2025, 1, 1);
        var dateDay5 = new DateOnly(2025, 1, 5);

        var wallet = new Wallet(0, "test", "TST", 100.0m, new List<Transaction>());

        var futureTransaction = new Transaction(0, dateDay5, -80m);
        Assert.That(wallet.TryAddTransaction(futureTransaction), Is.True);

        var pastTransaction = new Transaction(1, dateDay1, -30m);
        var result = wallet.TryAddTransaction(pastTransaction);

        Assert.That(result, Is.False);
        Assert.That(wallet.Transactions, Does.Not.Contains(pastTransaction));
    }

    [Test]
    public void Repository_TryLoad_ReturnsFalse_WhenTransactionStoryIsInvalid()
    {
        var repository = new Repository();

        // 1. (100) - 90 = 10. (OK)
        // 2. (10) - 110 = -100. (FAIL)
        var result = repository.TryLoad(
            wallets: [new() { Id = 1, Name = "Test", CurrencyId = "USD", StartingBalance = 100 }],
            transactions:
            [
                new() { Id = 101, WalletId = 1, Date = new DateOnly(2025, 1, 5), SumUpdate = -110 },
                new() { Id = 100, WalletId = 1, Date = new DateOnly(2025, 1, 1), SumUpdate = -90 }
            ]);

        Assert.That(result, Is.False);
        Assert.That(repository.IsEmpty, Is.True);
    }

    [Test]
    public void Repository_TryLoad_ReturnsFalse_WhenStartingBalanceIsNegative()
    {
        var repository = new Repository();

        var result = repository
            .TryLoad([new() { Id = 1, Name = "Test", CurrencyId = "USD", StartingBalance = -100 }], []);

        Assert.That(result, Is.False);
        Assert.That(repository.IsEmpty, Is.True);
    }

    [Test]
    public void Repository_TryLoad_ReturnsTrue_WhenDataIsValid()
    {
        var repository = new Repository();

        var result = repository.TryLoad(
            wallets:
            [
                new() { Id = 1, Name = "Test", CurrencyId = "USD", StartingBalance = 100 }
            ],
            transactions:
            [
                new() { Id = 101, WalletId = 1, Date = new DateOnly(2025, 1, 5), SumUpdate = -90 },
                new() { Id = 100, WalletId = 1, Date = new DateOnly(2025, 1, 1), SumUpdate = -10 }
            ]);

        Assert.That(result, Is.True);
        Assert.That(repository.IsEmpty, Is.False);
        Assert.That(repository.Transactions.Count(), Is.EqualTo(2));
    }
}