using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    internal class CalorieIntakeEntry
    {
        public int EntryId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalCalories { get; set; }
        public decimal TotalProtein { get; set; } // Общие белки за день
        public decimal TotalFat { get; set; } // Общие жиры за день
        public decimal TotalCarbs { get; set; } // Общие углеводы за день
    }
}
