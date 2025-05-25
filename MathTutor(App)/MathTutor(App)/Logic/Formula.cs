using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathTutor_App_
{
    // Класс, представляющий формулу
    public class Formula
    {
        public string Name { get; }
        public string Answer { get; }

        public Formula(string nameOfFormula, string rightAnswer)
        {
            Name = nameOfFormula;
            Answer = rightAnswer;
        }
    }
}
