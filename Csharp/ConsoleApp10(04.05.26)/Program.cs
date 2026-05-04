using System;
using System.Text.RegularExpressions;

namespace SmartHomeHub
{
    // ================= ЧАСТЬ 1: Паттерн Command =================

    /// <summary>
    /// Интерфейс команды
    /// </summary>
    public interface ICommand
    {
        void Execute();
    }

  
    public class SmartDevice
    {
        public void TurnOnLight() => Console.WriteLine("💡 [SmartDevice] Свет включён.");
        public void TurnOffLight() => Console.WriteLine("🌑 [SmartDevice] Свет выключен.");
        public void SetTemperature(double temp) => Console.WriteLine($"🌡️ [SmartDevice] Температура установлена на {temp}°C.");
    }

    
    public class LightCommand : ICommand
    {
        private readonly SmartDevice _device;
        private readonly bool _isOn;

        public LightCommand(SmartDevice device, bool isOn)
        {
            _device = device;
            _isOn = isOn;
        }

        public void Execute()
        {
            if (_isOn) _device.TurnOnLight();
            else _device.TurnOffLight();
        }
    }

    
    public class ClimateCommand : ICommand
    {
        private readonly SmartDevice _device;
        private readonly double _temperature;

        public ClimateCommand(SmartDevice device, double temperature)
        {
            _device = device;
            _temperature = temperature;
        }

        public void Execute() => _device.SetTemperature(_temperature);
    }

    // ================= ЧАСТЬ 2: Chain of Responsibility =================

    /// <summary>
    /// Объект запроса, передаваемый по цепочке
    /// </summary>
    public class Request
    {
        public required string Text { get; set; }
        public required string Token { get; set; }
    }

    /// <summary>
    /// Базовый обработчик цепочки
    /// </summary>
    public abstract class Handler
    {
        protected Handler? _nextHandler;

        public Handler SetNext(Handler next)
        {
            _nextHandler = next;
            return next;
        }

        public abstract void Handle(Request request);
    }

   
    public class LoggingHandler : Handler
    {
        public override void Handle(Request request)
        {
            Console.WriteLine($"📝 [Logging] Получен запрос: \"{request.Text}\"");
            _nextHandler?.Handle(request);
        }
    }

    
    public class SecurityHandler : Handler
    {
        private const string ValidToken = "ADMIN_TOKEN_2024";

        public override void Handle(Request request)
        {
            if (request.Token != ValidToken)
            {
                Console.WriteLine("🔒 [Security] Ошибка: Неверный токен доступа. Запрос отклонён.");
                return; // Останавливаем цепочку
            }
            Console.WriteLine("✅ [Security] Токен проверен успешно.");
            _nextHandler?.Handle(request);
        }
    }

    
    public class ValidationHandler : Handler
    {
        private readonly SmartDevice _device = new SmartDevice();

        public override void Handle(Request request)
        {
            Console.WriteLine("🔍 [Validation] Анализ команды...");
            
            ICommand? command = ParseCommand(request.Text);
            
            if (command == null)
            {
                Console.WriteLine("❌ [Validation] Ошибка: Команда не распознана системой.");
                return;
            }

            Console.WriteLine("🟢 [Validation] Команда валидна. Создание и выполнение...");
        
            command.Execute();
        }

        private ICommand? ParseCommand(string text)
        {
            text = text.ToLower().Trim();

            if (text.Contains("включить свет"))
                return new LightCommand(_device, true);

            if (text.Contains("выключить свет"))
                return new LightCommand(_device, false);

            if (text.StartsWith("установить температуру"))
            {
                
                var match = Regex.Match(text, @"\d+(\.\d+)?");
                if (match.Success && double.TryParse(match.Value, out double temp))
                    return new ClimateCommand(_device, temp);
            }

            return null;
        }
    }

    
    class Program
    {
        static void Main(string[] args)
        {
            // Сборка цепочки: Logging -> Security -> Validation
            var chain = new LoggingHandler();
            chain.SetNext(new SecurityHandler()).SetNext(new ValidationHandler());

            Console.WriteLine("=== ТЕСТ 1: Успешное выполнение ===");
            chain.Handle(new Request { Text = "Включить свет", Token = "ADMIN_TOKEN_2024" });

            Console.WriteLine("\n=== ТЕСТ 2: Неверный токен ===");
            chain.Handle(new Request { Text = "Установить температуру 22", Token = "INVALID_TOKEN" });

            Console.WriteLine("\n=== ТЕСТ 3: Непонятная команда ===");
            chain.Handle(new Request { Text = "Завари мне кофе", Token = "ADMIN_TOKEN_2024" });

            Console.WriteLine("\n=== ТЕСТ 4: Выключение света + дробная температура ===");
            chain.Handle(new Request { Text = "Выключить свет", Token = "ADMIN_TOKEN_2024" });
            chain.Handle(new Request { Text = "Установить температуру 23.5", Token = "ADMIN_TOKEN_2024" });
        }
    }
}