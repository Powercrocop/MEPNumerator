using MEPNumerator.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace MEPNumerator.DataAccess
{
    public class MEPNumeratorDbContext:DbContext
    {
        public MEPNumeratorDbContext()
        { 
        
        }
        public DbSet<Mechanic> Mechanics { get; set; }
        public DbSet<Piping> Pipings { get; set; }
        public DbSet<Electric> Electrics { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite($"Filename={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk\\Revit\\Addins\\2023\\MEPNumerator", "Database.sqlite")}");
        }
    }
}
