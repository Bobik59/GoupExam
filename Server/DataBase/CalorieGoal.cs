using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DataBase
{
    public class CalorieGoal
    {
        public int GoalId { get; set; }
        public int UserId { get; set; }
        public decimal DailyCalorieIntake { get; set; } // Норма калорий в день
        public decimal DailyProteinIntake { get; set; } // Норма белков в день
        public decimal DailyFatIntake { get; set; } // Норма жиров в день
        public decimal DailyCarbIntake { get; set; } // Норма углеводов в день
        public GoalType GoalType { get; set; } // Уменьшение, увеличение или поддержание веса
    }
    public enum GoalType
    {
        Loss, // Уменьшение веса
        Gain, // Увеличение веса
        Maintain // Поддержание веса
    }
}
