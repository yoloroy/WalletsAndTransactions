from datetime import date, timedelta
from random import choice, uniform
import json
from tkinter import filedialog


WALLETS_N = 20
TRANSACTIONS_N = 50
ACTOR_NAMES = list({chr(i) for i in range(ord('А'), ord('Я'))} - {'Ь', 'Ъ'})
CURRENCIES = ['RUB', 'USD', 'EUR', 'GBP', 'BYN', 'KZT', 'DEN', 'INR', 'AWG', 'KPW']
STARTING_BALANCE_RANGE = 0, 200_000
BALANCE_UPDATE_RANGE = 100, 300_000
WALLET_NAMES = [
    'Сбережения',
    'Зарплатная карта',
    'Основной счёт'
]
DAYS_DELTA = 90

wallets = [{
    'Id': i,
    'Name': choice(WALLET_NAMES) + ' ' + choice(ACTOR_NAMES),
    'CurrencyId': choice(CURRENCIES),
    'StartingBalance': (t := uniform(*STARTING_BALANCE_RANGE)) - t % 0.01
} for i in range(WALLETS_N)]
transactions = [{
    'Id': i,
    'WalletId': choice(wallets)['Id'],
    'Date': (date.today() - timedelta(days=uniform(0, DAYS_DELTA))).isoformat(),
    'SumUpdate': (t := uniform(*BALANCE_UPDATE_RANGE)) - t % 0.01,
    'Description': 'Сия транзакция была сгенерирована в качестве тестовой информации'
} for i in range(TRANSACTIONS_N)]

path = filedialog.askopenfilename()

with open(path, 'w', encoding='utf8') as file:
    print(json.dumps({'Wallets': wallets, 'Transactions': transactions}, ensure_ascii=False), file=file)
