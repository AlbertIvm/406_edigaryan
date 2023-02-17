using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab
{
    public class Calculation
    {
        public int Id { get; set; }
        public double X0 { get; set; }
        public double Y0 { get; set; }
        public double Z0 { get; set; }
        public double A { get; set; }
        public double F { get; set; }
        public double M { get; set; }
        public int NumOfIterations { get; set; }
        public CalculationData Data { get; set; }
    }
}
