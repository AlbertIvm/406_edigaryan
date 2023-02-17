using Microsoft.EntityFrameworkCore;

namespace Lab
{
    public class CalculationContext : DbContext
    {
        public CalculationContext()
        {
            Database.EnsureCreated();
        }

        public DbSet<Calculation> Calculations { get; set; }

        public DbSet<CalculationData> Data { get; set; }

        public void Clear()
        {
            Calculations.RemoveRange(Calculations);
            Data.RemoveRange(Data);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite("Data Source=calculations.db");
        }
    }
}
