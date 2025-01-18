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

   
    public static string CaloriesInfo(int age, double weight, double height, string gender, double activityLevel, Goal goal)
    {
        double caloriesconsupshion = Calculate_CaloriesConsupshion(age, weight, height, gender);
        double caloriesneed = Calculate_CaloriesNeed(caloriesconsupshion, activityLevel);
        double caloriesnorm = Calculate_CaloriesNorm(caloriesneed, goal);

        return($"Ваш дневной расходкаллорий для поддержания жизнедеятельности : {caloriesconsupshion} ккал/день" + $"Потребность в каллориях с учетом кативности : {caloriesneed} ккал/день" + $"Ваша дневная норма каллорий {goal}: {caloriesnorm} ккал/день");
    }
}