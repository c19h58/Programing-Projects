//Console.WriteLine("Hello, World!");
using System;

namespace NeoBankChainOfResponsibility
{
    // 1. Класс транзакции
    public class Transaction
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }

        public Transaction(decimal amount, string currency, string sender, string receiver)
        {
            Amount = amount;
            Currency = currency;
            Sender = sender;
            Receiver = receiver;
        }
    }

    // 2. Интерфейс
    public interface IHandler
    {
        IHandler SetNext(IHandler handler);
        bool Handle(Transaction transaction);
    }

    // 3. Базовый класс обработчика
    public abstract class BaseHandler : IHandler
    {
        private IHandler _nextHandler;

       
        public IHandler SetNext(IHandler handler)
        {
            _nextHandler = handler;
            return handler; 
        }

        
        public virtual bool Handle(Transaction transaction)
        {
            if (_nextHandler != null)
                return _nextHandler.Handle(transaction);
            
            return true; 
        }
    }

    // 4. Конкретные обработчики

    public class ValidationHandler : BaseHandler
    {
        public override bool Handle(Transaction transaction)
        {
            Console.WriteLine("[ValidationHandler] Проверка корректности реквизитов...");
            
            
            if (string.IsNullOrWhiteSpace(transaction.Sender) || 
                string.IsNullOrWhiteSpace(transaction.Receiver) || 
                transaction.Amount <= 0)
            {
                Console.WriteLine("[ValidationHandler] ❌ ОТКЛОНЕНО: Неверные реквизиты или сумма <= 0.\n");
                return false; 
            }

            Console.WriteLine("[ValidationHandler] ✅ Реквизиты корректны.");
            return base.Handle(transaction);
        }
    }

    public class FraudCheckHandler : BaseHandler
    {
        public override bool Handle(Transaction transaction)
        {
            Console.WriteLine("[FraudCheckHandler] Анализ транзакции на мошенничество...");
            
            if (transaction.Amount > 50000)
            {
                Console.WriteLine("[FraudCheckHandler] ⚠️ Сумма > 50,000. Запрашивается дополнительное подтверждение.");
               
            }
            
            Console.WriteLine("[FraudCheckHandler] ✅ Проверка на мошенничество пройдена.");
            return base.Handle(transaction);
        }
    }

    public class CurrencyHandler : BaseHandler
    {
        private const string BaseCurrency = "USD";

        public override bool Handle(Transaction transaction)
        {
            Console.WriteLine("[CurrencyHandler] Проверка валюты счета...");
            
            if (!transaction.Currency.Equals(BaseCurrency, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[CurrencyHandler] 🔄 Валюта {transaction.Currency} отличается от базовой {BaseCurrency}. Выполняется конвертация...");
                
                transaction.Currency = BaseCurrency;
                Console.WriteLine("[CurrencyHandler] ✅ Конвертация выполнена. Валюта приведена к USD.");
            }
            else
            {
                Console.WriteLine("[CurrencyHandler] ✅ Валюта совпадает с базовой. Конвертация не требуется.");
            }
            
            return base.Handle(transaction);
        }
    }

    public class LimitHandler : BaseHandler
    {
        
        private const decimal DailyLimit = 100000;

        public override bool Handle(Transaction transaction)
        {
            Console.WriteLine("[LimitHandler] Проверка суточного лимита пользователя...");
            
            if (transaction.Amount > DailyLimit)
            {
                Console.WriteLine($"[LimitHandler] ❌ ОТКЛОНЕНО: Сумма {transaction.Amount} превышает суточный лимит {DailyLimit}.\n");
                return false; 
            }
            
            Console.WriteLine("[LimitHandler] ✅ Суточный лимит не превышен.");
            return base.Handle(transaction);
        }
    }

    // 5. Точка входа и демонстрация работы конвейера
    class Program
    {
        static void Main(string[] args)
        {
            
            IHandler validation = new ValidationHandler();
            IHandler fraudCheck = new FraudCheckHandler();
            IHandler currencyCheck = new CurrencyHandler();
            IHandler limitCheck = new LimitHandler();

            
            validation.SetNext(fraudCheck)
                      .SetNext(currencyCheck)
                      .SetNext(limitCheck);

            Console.WriteLine("=== ТЕСТ 1: Успешная транзакция ===");
            ProcessTransaction(validation, new Transaction(10000, "EUR", "User1", "User2"));

            Console.WriteLine("\n=== ТЕСТ 2: Ошибка валидации (пустой отправитель, отрицательная сумма) ===");
            ProcessTransaction(validation, new Transaction(-50, "USD", "", "User3"));

            Console.WriteLine("\n=== ТЕСТ 3: Превышение суточного лимита ===");
            ProcessTransaction(validation, new Transaction(150000, "USD", "User4", "User5"));

            Console.WriteLine("\n=== ТЕСТ 4: Крупная сумма (срабатывание проверки на мошенничество) ===");
            ProcessTransaction(validation, new Transaction(75000, "GBP", "User6", "User7"));
        }

        
        static void ProcessTransaction(IHandler firstHandler, Transaction transaction)
        {
            Console.WriteLine($"📄 Транзакция: {transaction.Sender} -> {transaction.Receiver} | {transaction.Amount} {transaction.Currency}");
            Console.WriteLine(new string('-', 50));
            
            bool isSuccess = firstHandler.Handle(transaction);
            
            if (isSuccess)
                Console.WriteLine("🚀 >>> ТРАНЗАКЦИЯ УСПЕШНО ОБРАБОТАНА И ПЕРЕДАНА В ЯДРО СИСТЕМЫ.\n");
            else
                Console.WriteLine("🛑 >>> ОБРАБОТКА ТРАНЗАКЦИИ ПРЕРВАНА.\n");
        }
    }
}
