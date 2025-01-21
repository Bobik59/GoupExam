using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DataBase
{
    public class Dish
    {
        public int DishId { get; set; }
        public string Name { get; set; }
        public decimal Calories { get; set; } // Калории на порцию
        public decimal Protein { get; set; } // Белки на порцию
        public decimal Fat { get; set; } // Жиры на порцию
        public decimal Carbs { get; set; } // Углеводы на порцию
    }
}
