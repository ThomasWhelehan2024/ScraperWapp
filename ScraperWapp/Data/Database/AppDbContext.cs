using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ScraperWapp.Data.Database;

namespace ScraperWapp.Data;

public class AppDbContext : DbContext
{
    public DbSet<SearchResultDbModel> SearchResults { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var csvPath = Path.Combine(AppContext.BaseDirectory, "land_registry_searches_4weeks.csv");

        if (File.Exists(csvPath))
        {
            var csvLines = File.ReadAllLines(csvPath).Skip(1); 

            int idCounter = 1;
            foreach (var line in csvLines)
            {
                var columns = line.Split(',');

                int rank = int.Parse(columns[0]);
                string url = columns[1];
                string type = columns[2];
                DateTime date = DateTime.Parse(columns[3], CultureInfo.InvariantCulture);

                modelBuilder.Entity<SearchResultDbModel>().HasData(
                    new SearchResultDbModel
                    {
                        Id = idCounter++,
                        Rank = rank,
                        Url = url,
                        Type = type,
                        Date = date
                    }
                );
            }
        }
    }
}