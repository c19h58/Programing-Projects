//Console.WriteLine("Hello, World!");
using System;
using System.Collections.Generic;
using System.Linq;

// Интерфейс компонента 
public interface IMenuItem
{
    string GetName();
    double GetPrice();
}

// Паттерн Composite: Лист (Конкретное блюдо)
public class Dish : IMenuItem
{
    private string _name;
    private double _price;

    public Dish(string name, double price)
    {
        _name = name;
        _price = price;
    }

    public string GetName() => _name;
    public double GetPrice() => _price;
}

// Паттерн Composite: Компоновщик (Набор/Комбо)
public class ComboDeal : IMenuItem
{
    private string _name;
    private List<IMenuItem> _items = new List<IMenuItem>();

    public ComboDeal(string name)
    {
        _name = name;
    }

    public void Add(IMenuItem item) => _items.Add(item);

    public string GetName()
    {
        // Формируем описание состава комбо для наглядности
        var itemsNames = string.Join(", ", _items.Select(i => i.GetName()));
        return $"{_name} ({itemsNames})";
    }

    public double GetPrice()
    {
        // Цена — сумма всех элементов минус скидка 10% 
        double total = _items.Sum(item => item.GetPrice());
        return total * 0.9;
    }
}

// Паттерн Decorator: Базовый класс декоратора
public abstract class DishDecorator : IMenuItem
{
    protected IMenuItem _menuItem;

    public DishDecorator(IMenuItem menuItem)
    {
        _menuItem = menuItem;
    }

    public virtual string GetName() => _menuItem.GetName();
    public virtual double GetPrice() => _menuItem.GetPrice();
}

// Конкретный декоратор: Сыр
public class CheeseDecorator : DishDecorator
{
    public CheeseDecorator(IMenuItem menuItem) : base(menuItem) { }

    public override string GetName() => _menuItem.GetName() + " + Сыр"; 
    public override double GetPrice() => _menuItem.GetPrice() + 50; 
}

// Конкретный декоратор: Грибы
public class MushroomDecorator : DishDecorator
{
    public MushroomDecorator(IMenuItem menuItem) : base(menuItem) { }

    public override string GetName() => _menuItem.GetName() + " + Грибы"; 
    public override double GetPrice() => _menuItem.GetPrice() + 40; 
}

class Program
{
    static void Main(string[] args)
    {
        // 1. Создание одиночной пиццы
        IMenuItem margarita = new Dish("Пицца Маргарита", 500);

        // 2. Обертывание в декораторы (двойной сыр)
        IMenuItem doubleCheesePizza = new CheeseDecorator(new CheeseDecorator(margarita));
        
        // 3. Создание "Мега-Комбо"
        ComboDeal megaCombo = new ComboDeal("Мега-Комбо");

        // Добавляем декорированную пиццу (Маргарита + Сыр + Грибы)
        IMenuItem customPizza = new MushroomDecorator(new CheeseDecorator(new Dish("Пицца Маргарита", 500)));
        megaCombo.Add(customPizza);

        // Добавляем обычный напиток [cite: 24]
        megaCombo.Add(new Dish("Кола", 100));

        // Создаем и добавляем мини-комбо (картофель + соус)
        ComboDeal miniCombo = new ComboDeal("Мини-комбо");
        miniCombo.Add(new Dish("Картофель фри", 150));
        miniCombo.Add(new Dish("Соус", 30));
        
        megaCombo.Add(miniCombo);

        // 4. Вывод итогового дерева заказа
        Console.WriteLine("=== Итоговый заказ ===");
        Console.WriteLine($"Наименование: {megaCombo.GetName()}");
        Console.WriteLine($"Финальная стоимость (со скидкой 10% на комбо): {megaCombo.GetPrice()} руб.");
    }
}