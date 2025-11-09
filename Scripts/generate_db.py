from datetime import date, timedelta
from random import choice, uniform
import json
from tkinter import filedialog


WALLETS_N = 20
TRANSACTIONS_PER_WALLET_N = 50
ACTOR_NAMES = list({chr(i) for i in range(ord('А'), ord('Я'))} - {'Ь', 'Ъ'})
CURRENCIES = ['RUB', 'USD', 'EUR', 'GBP', 'BYN', 'KZT', 'DEN', 'INR', 'AWG', 'KPW']
STARTING_BALANCE_RANGE = 0, 100_000
BALANCE_UPDATE_RANGE = 1, 100_000
WALLET_NAMES = [
    'Сбережения',
    'Зарплатная карта',
    'Основной счёт'
]


switch = 1

wallets = [{
    'Id': i,
    'Name': choice(WALLET_NAMES) + ' ' + choice(ACTOR_NAMES),
    'CurrencyId': choice(CURRENCIES),
    'StartingBalance': round(uniform(*STARTING_BALANCE_RANGE), 2)
} for i in range(WALLETS_N)]

transactions = []
for wallet_i, wallet in enumerate(wallets):
    balance = wallet['StartingBalance']

    for transaction_i in range(
        (wallet_i + 1) * TRANSACTIONS_PER_WALLET_N,
        wallet_i * TRANSACTIONS_PER_WALLET_N,
        -1
    ):
        transaction_date = date.today() - timedelta(days=transaction_i)
        amount = uniform(*BALANCE_UPDATE_RANGE)
        update = -amount if balance - amount >= 0 else +amount
        balance += update

        transactions.append({
            'Id': transaction_i,
            'WalletId': wallet['Id'],
            'Date': transaction_date.isoformat(),
            'SumUpdate': round(update, 2),
            'Description': 'Сия транзакция была сгенерирована в качестве тестовой информации'
        })

print('Asking for file paths')
path = filedialog.askopenfilename()

with open(path, 'w', encoding='utf8') as file:
    print(json.dumps({'Wallets': wallets, 'Transactions': transactions}, ensure_ascii=False), file=file)
