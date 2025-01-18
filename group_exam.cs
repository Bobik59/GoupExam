using System;

public enum Goal
{
    MinusWeight,
    StopWeight,
    PlusWeight
}

public class CalorieCalculator
{
 
    public static double Calculate_CaloriesConsupshion(int age, double weight, double height, string gender)//подсчет дневного расхода каллорий
                                                                                                            //организмом для поддержания жизни
    {
        if (gender.ToLower() == "мужской")
        {
            return 88.362 + (13.397 * weight) + (4.799 * height) - (5.677 * age);
        }
        else if (gender.ToLower() == "женский")
        {
            return 447.593 + (9.247 * weight) + (3.098 * height) - (4.330 * age);
        }
        else
        {
            throw new ArgumentException("Ошибка, используйте 'мужской' или 'женский'.");
        }
    }

    public static double Calculate_CaloriesNeed(double caloriesconsupshion, double activityLevel) //подсчет потребности каллорий
                                                                                                  //с учетом активности
    {
        return caloriesconsupshion * activityLevel;
    }

    // Расчет суточной нормы калорий в зависимости от цели
    public static double Calculate_CaloriesNorm(double caloriesneed, Goal goal)
    {
        switch (goal)
        {
            case Goal.MinusWeight:
                return caloriesneed - 500; 
            case Goal.StopWeight:
                return caloriesneed; 
            case Goal.PlusWeight:
                return caloriesneed + 500; 
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

   
    public static void CaloriesInfo(int age, double weight, double height, string gender, double activityLevel, Goal goal)
    {
        double caloriesconsupshion = Calculate_CaloriesConsupshion(age, weight, height, gender);
        double caloriesneed = Calculate_CaloriesNeed(caloriesconsupshion, activityLevel);
        double caloriesnorm = Calculate_CaloriesNorm(caloriesneed, goal);

        Console.WriteLine($"Ваш дневной расходкаллорий для поддержания жизнедеятельности : {caloriesconsupshion} ккал/день");
        Console.WriteLine($"Потребность в каллориях с учетом кативности : {caloriesneed} ккал/день");
        Console.WriteLine($"Ваша дневная норма каллорий {goal}: {caloriesnorm} ккал/день");
    }
}

// Пример использования класса
public class Program
{
    public static void Main()
    {
       
        Console.Write("Введите возраст: ");
        int age = int.Parse(Console.ReadLine());

        Console.Write("Введите вес (в кг): ");
        double weight = double.Parse(Console.ReadLine());

        Console.Write("Введите рост (в см): ");
        double height = double.Parse(Console.ReadLine());

        Console.Write("Введите пол (мужской/женский): ");
        string gender = Console.ReadLine().ToLower();

        Console.Write("Введите уровень физической активности (1.2 - сидячий, 1.375 - легкая, 1.55 - умеренная, 1.725 - высокая, 1.9 - очень высокая): ");
        double activityLevel = double.Parse(Console.ReadLine());

        Console.Write("Введите цель (MinusWeight, StopWeight, PlusWeight): ");
        Goal goal = (Goal)Enum.Parse(typeof(Goal), Console.ReadLine(), true);

       
        CalorieCalculator.CaloriesInfo(age, weight, height, gender, activityLevel, goal);
    }
}
