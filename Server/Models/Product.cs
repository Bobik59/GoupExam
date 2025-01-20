using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } // Фрукты, овощи, мясо, и т.д.
        public decimal CaloriesPer100g { get; set; } // Калории на 100 грамм
        public decimal ProteinPer100g { get; set; } // Белки на 100 грамм
        public decimal FatPer100g { get; set; } // Жиры на 100 грамм
        public decimal CarbsPer100g { get; set; } // Углеводы на 100 грамм
    }
}
